using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.Dispatcher
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
                             /// <summary>
                             /// Awaiter of event after command was send into buses.
                             /// </summary>
    public class DispatcherAwaiter : IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
    {
        #region Nested class

        /// <summary>
        /// Lock class type.
        /// </summary>
        private class TypeLock
        {
            /// <summary>
            /// Type.
            /// </summary>
            public Type Type { get; }

            /// <summary>
            /// Ctor.
            /// </summary>
            /// <param name="type">Type</param>
            public TypeLock(Type type)
            {
                Type = type;
            }
        }

        #endregion

        #region Members

        /// <summary>
        /// List of lock by types.
        /// </summary>
        private static readonly ConcurrentBag<TypeLock> s_TypeLocks = new ConcurrentBag<TypeLock>();

        /// <summary>
        /// List of handlers tasks
        /// </summary>
        private readonly IEnumerable<Task> _handlerTasks;
        /// <summary>
        /// Lambda for dispatcher.
        /// </summary>
        private readonly Func<IDomainEvent, Task> _lambda;
        /// <summary>
        /// Result event.
        /// </summary>
        private readonly IList<IDomainEvent> _results = new List<IDomainEvent>();

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DispatcherAwaiter()
        {
            _lambda = (e) =>
            {
                _results.Add(e);
                return Task.CompletedTask;
            };
            CoreDispatcher.OnEventDispatched += _lambda;
        }

        /// <summary>
        /// Constructor with some command handler tasks
        /// </summary>
        /// <param name="commandHandlerTasks">List of running tasks from command handler.</param>
        public DispatcherAwaiter(IList<Task> commandHandlerTasks)
            : this()
        {
            _handlerTasks = commandHandlerTasks;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~DispatcherAwaiter()
        {
            Dispose(false);
        }

        #endregion

        #region IDisposable methods

        /// <summary>
        /// Cleaning up memory
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Cleaning up memory
        /// </summary>
        /// <param name="disposing">Flag to indicate if coming from dispose method or not.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
#pragma warning disable S3971 // "GC.SuppressFinalize" should not be called
                GC.SuppressFinalize(this);
#pragma warning restore S3971 // "GC.SuppressFinalize" should not be called
            }

            CoreDispatcher.OnEventDispatched -= _lambda;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Wait for a specific time to all handlers to perform.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        internal ulong WaitForHandlers(ulong timeout = 1000)
        {
            ulong elapsedTime = 0;
            if (_handlerTasks != null && _handlerTasks.Any())
            {
                DateTime start = DateTime.Now;
                Task.WaitAll(_handlerTasks.ToArray(), TimeSpan.FromMilliseconds(timeout));
                DateTime end = DateTime.Now;
                elapsedTime += Convert.ToUInt64((end - start).TotalMilliseconds);
            }
            return elapsedTime;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Wait for a specific time to all handlers to perform.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public void WaitForCompletion(ulong timeout = 1000)
            => WaitForHandlers(timeout);


        #endregion

    }
}
