using CQELight.Abstractions.Events;
using CQELight.Abstractions.Saga.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Saga
{
    /// <summary>
    /// Specific event for saga completion.
    /// This is event is published when a saga call the MarkAsComplete method.
    /// By handling this particular event, you're sure that you're in a final state of the saga, which
    /// may results of differents kind of end-events.
    /// </summary>
    /// <typeparam name="T">Type de saga complétée.</typeparam>
    public class SagaFinishedEvent<T> : BaseDomainEvent
        where T : class, ISaga
    {

        #region Properties

        /// <summary>
        /// Instance of ended saga.
        /// </summary>
        public T Saga { get; protected set; }

        #endregion

        #region Ctor

        public SagaFinishedEvent(T saga)
        {
            Saga = saga ?? throw new ArgumentNullException(nameof(saga));
        }

        #endregion

    }
}
