using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher;
using CQELight.IoC;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQELight.Buses.InMemory.Commands
{
    /// <summary>
    /// Bus for dispatching commands.
    /// State is not handle in this bus, by definition, commands are stateless. If the system fail in any unexpected ways,
    /// the use wouldn't want its action to be replayed when system is up again.
    /// </summary>
    public class InMemoryCommandBus : ICommandBus
    {
        #region Private members

        private static IEnumerable<Type> _handlers;
        private InMemoryCommandBusConfiguration _config;
        private readonly IScope _scope;
        private readonly ILogger _logger;

        #endregion
        
        #region Ctor

        internal InMemoryCommandBus()
            : this(null, null)
        {

        }

        internal InMemoryCommandBus(InMemoryCommandBusConfiguration configuration = null, IScopeFactory scopeFactory = null)
        {
            if (scopeFactory != null)
            {
                _scope = scopeFactory.CreateScope();
            }

            _logger =
                _scope?.Resolve<ILoggerFactory>()?.CreateLogger<InMemoryCommandBus>()
                ??
                new LoggerFactory().CreateLogger<InMemoryCommandBus>();
            _config = configuration ?? InMemoryCommandBusConfiguration.Default;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Dispatch command and context to all handlers.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="context">Context associated to command</param>
        public async Task<Task[]> DispatchAsync(ICommand command, ICommandContext context = null)
        {
            var commandTypeName = command.GetType().FullName;
            _logger.LogInformation($"InMemoryCommandBus : Beginning of dispatching a command of type {commandTypeName}");
            var commandTasks = new List<Task>();
            _config = _config ?? InMemoryCommandBusConfiguration.Default;
            var ifClause = _config.IfClauses.FirstOrDefault(i => i.Key == command.GetType()).Value;
            if (ifClause?.Invoke(command) == false)
            {
                _logger.LogInformation($"InMemoryCommandBus : If condition for command type {commandTypeName} has returned false.");

                return new[] { Task.CompletedTask };
            }
            var handlers = TryGetHandlerFromIoCContainer(command);
            if (!handlers.Any())
            {
                _logger.LogInformation($"InMemoryCommandBus : Handler for command type {commandTypeName} not found in Ioc container, trying to get it from CoreDispatcher.");
                handlers = TryGetHandlersInstancesFromCoreDispatcher(command);
            }
            if (!handlers.Any())
            {
                _logger.LogInformation($"InMemoryCommandBus : Handler for command type {commandTypeName} not found in CoreDispatcher, trying to instantiate if by reflection.");
                handlers = TryGetHandlersInstancesByReflection(command);
            }
            if (!handlers.Any())
            {
                _logger.LogWarning($"InMemoryCommandBus : No handlers for command type {commandTypeName} were found.");
                _config.OnNoHandlerFounds?.Invoke(command, context);
                return new[] { Task.CompletedTask };
            }
            bool manyHandlersAndShouldWait = false;
            if (handlers.Skip(1).Any())
            {
                if (!_config.CommandAllowMultipleHandlers.Any(t => new TypeEqualityComparer().Equals(t.Type, command.GetType())))
                {
                    throw new InvalidOperationException($"InMemoryCommandBus : the command of type {commandTypeName} have multiple handler within the same process. " +
                        "If this is expected, you should add it in configuration to allow multiple handlers, altough this is not recommended.");
                }
                else
                {
                    manyHandlersAndShouldWait
                        = _config.CommandAllowMultipleHandlers.FirstOrDefault(t => new TypeEqualityComparer().Equals(t.Type, command.GetType())).ShouldWait;
                }
            }
            foreach (var item in handlers)
            {
                _logger.LogInformation($"InMemoryCommandBus : Invocation of handler of type {handlers.GetType().FullName}");
                var method = item.GetType().GetMethods()
                        .First(m => m.Name == nameof(ICommandHandler<ICommand>.HandleAsync));
                try
                {
                    if (manyHandlersAndShouldWait)
                    {
                        await ((Task)method.Invoke(item, new object[] { command, context })).ConfigureAwait(false);
                    }
                    else
                    {
                        var t = (Task)method.Invoke(item, new object[] { command, context });
                        commandTasks.Add(t);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogErrorMultilines($"InMemoryCommandBus.DispatchAsync() : Exception when trying to dispatch command {commandTypeName} to handler {item.GetType().FullName}",
                        e.ToString());
                }
            }

            if (!manyHandlersAndShouldWait)
            {
                var tasks = new List<Task>();
                tasks.AddRange(commandTasks);
                tasks.Add(Task.WhenAll(commandTasks).ContinueWith(a => _scope?.Dispose()));
                return tasks.ToArray();
            }
            else
            {
                _scope?.Dispose();
                return new[] { Task.CompletedTask };
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Try to retrieve handlers from CoreDispatcher.
        /// </summary>
        /// <param name="command">Type of command to get handler.</param>
        /// <returns>Collection of found handlers.</returns>
        private IEnumerable<object> TryGetHandlersInstancesFromCoreDispatcher(ICommand command)
         => CoreDispatcher.TryGetHandlersForCommandType(command.GetType());

        /// <summary>
        /// Get a bunch of handlers instances via reflexion.
        /// </summary>
        /// <param name="command">Type of command to get handler.</param>
        /// <returns>Freshly created instances of handlers.</returns>
        private IEnumerable<object> TryGetHandlersInstancesByReflection(ICommand command)
             => _handlers.Where(h => h.GetInterfaces()
                    .Any(x => x.IsGenericType && x.GenericTypeArguments[0] == command.GetType()))
                    .Select(t => t.CreateInstance());

        /// <summary>
        /// Get an handler from IoC container.
        /// </summary>
        /// <param name="command">Type of command to get handler.</param>
        /// <returns>Handler instance from IoC container.</returns>
        private IEnumerable<object> TryGetHandlerFromIoCContainer(ICommand command)
        {
            if (_scope != null)
            {
                var type = typeof(ICommandHandler<>).GetGenericTypeDefinition().MakeGenericType(command.GetType());
                try
                {
                    return _scope.ResolveAllInstancesOf(type).Cast<object>();
                }
                catch (Exception e)
                {
                    _logger.LogErrorMultilines($"InMemoryCommandBus.TryGetHandlerFromIoCContainer() : Cannot resolve handler of type {type.FullName} from IoC container.",
                        e.ToString());
                }
            }
            return Enumerable.Empty<object>();
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Check if type match all pre-requesites to be a commandHandler.
        /// </summary>
        /// <param name="x">Type to check.</param>
        /// <returns>True if it's a command handler, false otherwise.</returns>
        private static bool IsCommandHandler(Type x)
            => x.GetInterfaces().Any(y => y.IsGenericType
                                           && y.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                                           && x.IsClass;

        #endregion

        #region Internal static methods
        
        internal static void InitHandlersCollection(string[] excludedDLLs)
        {
            _handlers = ReflectionTools.GetAllTypes(excludedDLLs)
                        .Where(IsCommandHandler).ToList();
        }

        #endregion

    }
}
