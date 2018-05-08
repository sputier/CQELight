using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Abstractions.Saga.Interfaces;
using CQELight.Dispatcher;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions.Saga
{
    /// <summary>
    /// Base class for Saga implementation.
    /// </summary>
    /// <typeparam name="TData">Type of data the saga carries during its life.</typeparam>
    public abstract class Saga<TData> : ISaga
        where TData : class, ISagaData
    {
        #region Members

        private readonly IDispatcher _dispatcher;

        #endregion

        #region Properties

        /// <summary>
        /// Saga's Id.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Saga's data.
        /// </summary>
        public TData Data { get; protected set; }

        /// <summary>
        /// Completion indicator.
        /// </summary>
        public bool Completed { get; private set; }

        #endregion

        #region Ctor

        protected Saga()
        {
            Id = Guid.NewGuid();
            CoreDispatcher.AddHandlerToDispatcher(this);
        }

        protected Saga(TData data)
            : this()
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        protected Saga(TData data, IDispatcher dispatcher)
            : this(data)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        #endregion

        #region Protected methods

        protected Task DispatchCommandAsync(ICommand command)
        {
            if (_dispatcher != null)
            {
                return _dispatcher.DispatchCommandAsync(command, this);
            }
            else
            {
                return CoreDispatcher.DispatchCommandAsync(command, this);
            }
        }

        protected Task DispatchEventAsync(IDomainEvent @event)
        {
            if (_dispatcher != null)
            {
                return _dispatcher.PublishEventAsync(@event, this);
            }
            else
            {
                return CoreDispatcher.PublishEventAsync(@event, this);
            }
        }

        protected virtual void MarkAsComplete()
        {
            Completed = true;
            CoreDispatcher.RemoveHandlerFromDispatcher(this);
            DispatchEventAsync(ToSagaFinishedEvent(this));
        }

        protected IDomainEvent ToSagaFinishedEvent<T>(T saga)
            where T : Saga<TData>
            => (IDomainEvent)typeof(SagaFinishedEvent<>).MakeGenericType(saga.GetType()).CreateInstance(new object[] { saga });

        #endregion

        #region Overriden methods

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is Saga<TData> other)
            {
                return other.Id == Id;
            }
            return false;
        }

        public override int GetHashCode()
            => Id.GetHashCode();

        #endregion

        #region Public methods

        /// <summary>
        /// Start asynchronously the saga.
        /// </summary>
        public abstract Task ExecuteSagaAsync();

        #endregion

    }
}
