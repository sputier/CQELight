using CQELight.Abstractions.CQS.Interfaces;
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
using System.Windows.Input;

namespace CQELight.Buses.InMemory.Commands
{
    /// <summary>
    /// Bus for dispatching commands.
    /// State is not handle in this bus, by definition, commands are stateless. If the system fail in any unexpected ways,
    /// the use wouldn't want its action to be replayed when system is up again.
    /// </summary>
    public class InMemoryCommandBus
    {
        #region Private members

        /// <summary>
        /// Commands handlers.
        /// </summary>
        private static IEnumerable<Type> _handlers;
        /// <summary>
        /// IoC Scope.
        /// </summary>
        private readonly IScope _scope;
        /// <summary>
        /// Logger for the bus.
        /// </summary>
        private readonly ILogger _logger;

        #endregion

        #region Static initiliazer

        /// <summary>
        /// Accesseur statique par défaut.
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
        internal InMemoryCommandBus()
        {
            _scope = DIManager.BeginScope();
            _logger = _scope.Resolve<ILoggerFactory>()?.CreateLogger<InMemoryCommandBus>();
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
            try
            {
                if (handler != null)
                {
                    _logger.LogInformation($"InMemoryCommandBus : Invocation du handler de type {handler.GetType().FullName}");
                    var t = (Task)handler.GetType().GetMethod("HandleAsync", new[] { command.GetType(), typeof(ICommandContext) })
                        .Invoke(handler, new object[] { command, context });
                    commandTasks.Add(t);
                }
                else
                {
                    _logger.LogWarning($"InMemoryCommandBus : No handlers for command type {commandTypeName} were found.");
                }
            }
            catch (Exception e)
            {
                _logger.LogErrorMultilines($"InMemoryCommandBus.DispatchAsync() : Exception when trying to dispatch command {commandTypeName} on handler {handler.GetType().FullName}",
                    e.ToString());
            }
            var tasks = new List<Task>(commandTasks);
            tasks.Add(Task.WhenAll(commandTasks).ContinueWith(a => _scope.Dispose()));

            return Task.FromResult(tasks.ToArray());
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
                    .Select(t => _scope.Resolve(t) ?? t.CreateInstance()).ToList().FirstOrDefault();

        /// <summary>
        /// Get an handler from IoC container.
        /// </summary>
        /// <param name="command">Type of command to get handler.</param>
        /// <returns>Handler instance from IoC container.</returns>
        private object TryGetHandlerFromIoCContainer(ICommand command)
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
