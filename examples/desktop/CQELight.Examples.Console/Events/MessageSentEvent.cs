using CQELight.Abstractions.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Examples.ConsoleApp.Events
{
    /// <summary>
    /// Simple event when a message has been sent within the system.
    /// </summary>
    public class MessageSentEvent : BaseDomainEvent
    {

        #region Properties

        /// <summary>
        /// Message that has been sent.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Message's sender.
        /// </summary>
        public Guid Sender { get; set; }
        /// <summary>
        /// Name of the sender.
        /// </summary>
        public string SenderName { get; set; }

        #endregion

    }
}
