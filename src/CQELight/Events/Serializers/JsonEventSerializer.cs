using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Events.Serializers
{
    /// <summary>
    /// Serialization of an event into Json.
    /// </summary>
    public class JsonEventSerializer : IEventSerializer, IAutoRegisterType
    {

        #region IEventSerializer
                
        /// <summary>
        /// Deserialize event from string.
        /// </summary>
        /// <param name="data">String data that contains event.</param>
        /// <returns>Instance of event.s</returns>
        public IDomainEvent Deserialize(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data), "JsonEventSerializer.Deserialize() : Event data cannot be empty string.");
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<IDomainEvent>(data);
        }

        /// <summary>
        /// Serialize the event into a string.
        /// </summary>
        /// <param name="event">Event to serialize.</param>
        /// <returns>Event that has been seriazlied into a string.</returns>
        public string Serialize(IDomainEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event), "JsonEventSerializer.Serialize() : Event to serialize cannot be null.");
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(@event);
        }

        #endregion
    }
}
