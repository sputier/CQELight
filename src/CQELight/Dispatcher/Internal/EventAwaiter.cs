using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Dispatcher.Internal
{
    /// <summary>
    /// Helping class to allow waiting on a particular event.
    /// </summary>
    /// <typeparam name="T">Type of event to wait.</typeparam>
    internal class EventAwaiter<T> : IEventAwaiter, IDomainEventHandler<T> where T : class, IDomainEvent
    {

        #region Members

        /// <summary>
        /// Current instance of awaiter.
        /// </summary>
        private static EventAwaiter<T> _instance;

        #endregion

        #region Properties

        /// <summary>
        /// Event that we're waiting for.
        /// </summary>
        public T Event { get; private set; }
        /// <summary>
        /// Context associated to the event.
        /// </summary>
        public IEventContext Context { get; private set; }
        /// <summary>
        /// Thread safety object.
        /// </summary>
        private static object s_lockObject = new object();
        /// <summary>
        /// Current singleton instance.
        /// </summary>
        internal static EventAwaiter<T> Instance
        {
            get
            {
                return _instance;
            }
            set
            { _instance = value; }
        }

        #endregion

        #region ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal EventAwaiter()
        {
            Instance = this;
        }

        #endregion

        #region IDomainEventHandler
        
        /// <summary>
        /// Handle the domain event.
        /// </summary>
        /// <param name="domainEvent">Domain event to handle.</param>
        /// <param name="context">Associated context.</param>
        public Task HandleAsync(T domainEvent, IEventContext context = null)
        {
            lock (s_lockObject)
            {
                Event = domainEvent;
                Context = context;
            }
            return Task.FromResult(0);
        }

        #endregion
    }
}
