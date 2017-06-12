using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Implementations.Events.System
{
    /// <summary>
    /// Enveloppe to carry event data through system bus.
    /// </summary>
    public class EventEnveloppe
    {

        #region Properties

        /// <summary>
        /// Id of the current record.
        /// </summary>
        public virtual Guid Id { get; set; }
        /// <summary>
        /// Sender id.
        /// </summary>
        public virtual Guid Sender { get; set; }
        /// <summary>
        /// Receiver id if accurate.
        /// </summary>
        public virtual Guid? Receiver { get; set; }
        /// <summary>
        /// Time when event was created in system.
        /// </summary>
        public DateTime EventTime { get; set; }
        /// <summary>
        /// Event data.
        /// </summary>
        public virtual string EventData { get; set; }
        /// <summary>
        /// Event type.
        /// </summary>
        public virtual string EventType { get; set; }
        /// <summary>
        /// Event context data.
        /// </summary>
        public virtual string EventContextData { get; set; }
        /// <summary>
        /// Event context type.
        /// </summary>
        public virtual string ContextType { get; set; }
        /// <summary>
        /// Peremption date, aka when event is no more accurate for the system.
        /// </summary>
        public virtual DateTime PeremptionDate { get; set; }

        #endregion

    }
}
