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
    public class InMemoryCommandBus : ICommandBus, IConfigurableCommandBus<InMemoryCommandBusConfiguration>
    {
        #region Private members

        private static IEnumerable<Type> _handlers;
        private InMemoryCommandBusConfiguration _config;
        private readonly IScope _scope;
        private readonly ILogger _logger;

        #endregion

        #region Static initiliazer

        /// <summary>
        /// Default static accessor.
        /// </summary>
        static InMemoryCommandBus()
        {
            _handlers = ReflectionTools.GetAllTypes()
                        .Where(IsCommandHandler).ToList();
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal InMemoryCommandBus(IScopeFactory scopeFactory = null)
        {
            if (scopeFactory != null)
            {
                _scope = scopeFactory.CreateScope();
            }

            _logger =
                _scope?.Resolve<ILoggerFactory>()?.CreateLogger<InMemoryCommandBus>()
                ??
                new LoggerFactory().CreateLogger<InMemoryCommandBus>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Dispatch command and context to all handlers.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="context">Context associated to command</param>
        public Task<Task[]> DispatchAsync(ICommand command, ICommandContext context = null)
        {
            var commandTypeName = command.GetType().FullName;
            _logger.LogInformation($"InMemoryCommandBus : Beginning of dispatching a command of type {commandTypeName}");
            var commandTasks = new List<Task>();
            var handler = TryGetHandlerFromIoCContainer(command);
            if (handler == null)
            {
                _logger.LogInformation($"InMemoryCommandBus : Handler for command type {commandTypeName} not found in Ioc container, trying to get it from CoreDispatcher.");
                handler = TryGetHandlerInstanceFromCoreDispatcher(command);
            }
            if (handler == null)
            {
                _logger.LogInformation($"InMemoryCommandBus : Handler for command type {commandTypeName} not found in CoreDispatcher, trying to instantiate if by reflection.");
                handler = TryGetHandlerInstanceByReflection(command);
            }
            _config = _config ?? InMemoryCommandBusConfiguration.Default;
            try
            {
                if (handler != null)
                {
                    _logger.LogInformation($"InMemoryCommandBus : Invocation of handler of type {handler.GetType().FullName}");
                    var t = (Task)handler.GetType().GetMethod(nameof(ICommandHandler<ICommand>.HandleAsync), new[] { command.GetType(), typeof(ICommandContext) })
                        .Invoke(handler, new object[] { command, context });
                    commandTasks.Add(t);
                }
                else
                {
                    _logger.LogWarning($"InMemoryCommandBus : No handlers for command type {commandTypeName} were found.");
                    _config?.OnNoHandlerFounds(command, context);
                }
            }
            catch (Exception e)
            {
                _logger.LogErrorMultilines($"InMemoryCommandBus.DispatchAsync() : Exception when trying to dispatch command {commandTypeName}",
                    e.ToString());
            }

            var tasks = new List<Task>(commandTasks)
            {
                Task.WhenAll(commandTasks).ContinueWith(a => _scope?.Dispose())
            };

            return Task.FromResult(tasks.ToArray());
        }

        #endregion

        #region IConfigurableCommandBus

        public void Configure(InMemoryCommandBusConfiguration config)
        {
            _config = config;
        }


        #endregion

        #region Private methods

        /// <summary>
        /// Try to retrieve handler from CoreDispatcher.
        /// </summary>
        /// <param name="command">Type of command to get handler.</param>
        /// <returns>Instance of the handler, null if not found.</returns>
        private object TryGetHandlerInstanceFromCoreDispatcher(ICommand command)
         => CoreDispatcher.TryGetHandlerForCommandType(command.GetType());


        /// <summary>
        /// Get an handler instance via reflexion.
        /// </summary>
        /// <param name="command">Type of command to get handler.</param>
        /// <returns>Freshly created instance of handler.</returns>
        private object TryGetHandlerInstanceByReflection(ICommand command)
             => _handlers.Where(h => h.GetInterfaces()
                    .Any(x => x.IsGenericType && x.GenericTypeArguments[0] == command.GetType()))
                    .Select(t => t.CreateInstance()).FirstOrDefault();

        /// <summary>
        /// Get an handler from IoC container.
        /// </summary>
        /// <param name="command">Type of command to get handler.</param>
        /// <returns>Handler instance from IoC container.</returns>
        private object TryGetHandlerFromIoCContainer(ICommand command)
        {
            if (_scope != null)
            {
                var type = typeof(ICommandHandler<>).GetGenericTypeDefinition().MakeGenericType(command.GetType());
                try
                {
                    return _scope.Resolve(type);
                }
                catch (Exception e)
                {
                    _logger.LogErrorMultilines($"InMemoryCommandBus.TryGetHandlerFromIoCContainer() : Cannot resolve handler of type {type.FullName} from IoC container.",
                        e.ToString());
                }
            }
            return null;
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
    }
}
