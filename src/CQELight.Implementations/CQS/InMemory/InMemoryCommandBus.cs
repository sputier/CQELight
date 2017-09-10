using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher;
using CQELight.IoC;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CQELight.Implementations.CQS.InMemory
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
        private IScope _scope;

        #endregion

        #region Static initiliazer

        /// <summary>
        /// Accesseur statique par défaut.
        /// </summary>
        static InMemoryCommandBus()
        {
            _handlers = ReflectionTools.GetAllTypes()
                .Where(x => x.GetInterfaces()
                           .Any(y => y.IsGenericType &&
                                     y.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                    && x.IsClass).Distinct().ToList();
        }

        #endregion
        #region Public methods

        /// <summary>
        /// Dispatch des commandes aux listeners.
        /// </summary>
        /// <param name="command">Command à dispatcher.</param>
        /// <param name="context">Contexte de la commande.</param>
        public Task<Task[]> DispatchAsync(ICommand command, ICommandContext context = null)
        {
            _scope = CoreDispatcher._scope?.CreateChildScope() ?? DIManager.BeginScope();
            var commandTasks = new List<Task>();
            System.Collections.IEnumerable handlers = GetHandlersFromIoCContainer(command);
            if (handlers?.Any() == false)
            {
                handlers = GetHandlersInstanceForCommand(command);
            }
            foreach (var handler in handlers)
            {
                try
                {
                    if (handler != null)
                    {
                        var t = (Task)handler.GetType().GetMethod("HandleAsync", new[] { command.GetType(), typeof(ICommandContext) })
                            .Invoke(handler, new object[] { command, context });
                        t.ConfigureAwait(false);
                        commandTasks.Add(t);
                    }
                    else
                    {
                        // TODO ?
                    }
                }
                catch
                { //TODO logger une erreur de dispatch sur handler 
                }
            }
            var tasks = new List<Task>(commandTasks);
            tasks.Add(Task.WhenAll(commandTasks).ContinueWith(a => _scope.Dispose()));

            return Task.FromResult(tasks.ToArray());
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Récupère une liste d'objet handler à partir d'une commande en fonction des types connus.
        /// </summary>
        /// <param name="command">Commande dont on veut les handlers.</param>
        /// <returns>List d'handlre sous forme d'objet générés.</returns>
        private IEnumerable<object> GetHandlersInstanceForCommand(ICommand command)
             => _handlers.Where(h => h.GetInterfaces()
                    .Any(x => x.IsGenericType && x.GenericTypeArguments[0] == command.GetType()))
                    .Select(t => _scope.Resolve(t) ?? t.CreateInstance()).ToList();

        /// <summary>
        /// Récupères les handlers qui ont été enregistrés dans le container IoC.
        /// </summary>
        /// <param name="command">Command dont on veut les handlers.</param>
        /// <returns>List d'handlers depuis le container IoC.</returns>
        private System.Collections.IEnumerable GetHandlersFromIoCContainer(ICommand command)
        {
            var type = typeof(ICommandHandler<>).GetGenericTypeDefinition().MakeGenericType(command.GetType());
            var collection = typeof(IEnumerable<>).GetGenericTypeDefinition().MakeGenericType(type);
            // Utilisation des types enregistrés 
            try
            {
                return (System.Collections.IEnumerable)_scope.Resolve(collection);
            }
            catch
            {
                //TODO Log
                try
                {
                    return new List<object> { _scope.Resolve(type) } as System.Collections.IEnumerable;
                }
                catch
                {
                    //TODO handle
                }
            }
            return new List<object>() as System.Collections.IEnumerable;
        }

        #endregion
    }
}
