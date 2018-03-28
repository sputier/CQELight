using CQELight.Abstractions.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Examples.Console.Events
{
    /// <summary>
    /// A domain event is a simple DTO that carry data of what the system have done, and
    /// by its name, provide information about system state.
    /// </summary>
    public class MessageTreatedEvent : BaseDomainEvent
    {

        #region Properties

        /// <summary>
        /// Like command, event are supposed to be right when hitting handlers.
        /// Events DTO should enforces cohesion with constructor.
        /// </summary>
        public Guid TreatedMessageId { get; private set; }
        public string TreatedMessage { get; private set; }

        #endregion

        #region Ctor

        public MessageTreatedEvent(Guid treatedMessageId, string treatedMessage)
        {
            if (treatedMessageId == Guid.Empty)
            {
                throw new ArgumentException("MessageTreatedEvent.ctor() : The treated message id should be valid.", nameof(treatedMessageId));
            }

            if (string.IsNullOrWhiteSpace(treatedMessage))
            {
                throw new ArgumentException("MessageTreatedEvent.ctor() : The treated message should be provided.", nameof(treatedMessage));
            }

            TreatedMessageId = treatedMessageId;
            TreatedMessage = treatedMessage;
        }

        #endregion

    }
}
