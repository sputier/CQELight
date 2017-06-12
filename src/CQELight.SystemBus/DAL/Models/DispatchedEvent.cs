using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.SystemBus.DAL.Models
{
    /// <summary>
    /// Model for representing a dispatched event.
    /// </summary>
    public class DispatchedEvent
    {

        #region Properties

        /// <summary>
        /// Object Id.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Id of the event.
        /// </summary>
        public Guid EventId { get; set; }
        /// <summary>
        /// Id of the receiver.
        /// </summary>
        public Guid ReceiverId { get; set; }
        /// <summary>
        /// Time when event was dispatched.
        /// </summary>
        public DateTime DispatchTimestamp { get; set; }

        #endregion

    }
}
