using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Events.Interfaces
{
    /// <summary>
    /// Contract interface for event serializer.
    /// </summary>
    public interface IEventSerializer
    {

        /// <summary>
        /// Serialize the event into a string.
        /// </summary>
        /// <param name="event">Event to serialize.</param>
        /// <returns>Event that has been seriazlied into a string.</returns>
        string Serialize(IDomainEvent @event);
        /// <summary>
        /// Deserialize event from string.
        /// </summary>
        /// <param name="data">String data that contains event.</param>
        /// <returns>Instance of event.s</returns>
        IDomainEvent Deserialize(string data);

    }
}
