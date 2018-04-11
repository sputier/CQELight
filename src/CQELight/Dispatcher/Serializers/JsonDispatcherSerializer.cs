using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Dispatcher;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Events.Serializers
{
    /// <summary>
    /// Serialization for dispatcher as Json data.
    /// </summary>
    public class JsonDispatcherSerializer : IDispatcherSerializer, IAutoRegisterType
    {

        #region IDispatcherSerializer

        /// <summary>
        /// Retrieve the content type of serialized data.
        /// </summary>
        public string ContentType => "application/json";

        /// <summary>
        /// Deserialize a command from Json data.
        /// </summary>
        /// <param name="data">Json data.</param>
        /// <returns>Instance of deserialized command.</returns>
        public ICommand DeserializeCommand(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data), "JsonEventSerializer.DeserializeCommand() : Command data cannot be empty string.");
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ICommand>(data);
        }

        /// <summary>
        /// Deserialize a command from Json data.
        /// </summary>
        /// <typeparam name="T">Type of command to retrieve.</typeparam>
        /// <param name="data">Json data.</param>
        /// <returns>Instance of deserialized command.</returns>
        public T DeserializeCommand<T>(string data) where T : ICommand
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data), "JsonEventSerializer.DeserializeCommand() : Command data cannot be empty string.");
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>
        /// Deserialize an event from Json data.
        /// </summary>
        /// <param name="data">Json data.</param>
        /// <returns>Instance of deserialized event.</returns>
        public IDomainEvent DeserializeEvent(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data), "JsonEventSerializer.DeserializeEvent() : Event data cannot be empty string.");
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<IDomainEvent>(data);
        }

        /// <summary>
        /// Deserialize an event from Json data.
        /// </summary>
        /// <typeparam name="T">Type of event to retrieve.</typeparam>
        /// <param name="data">Json data.</param>
        /// <returns>Instance of deserialized event.</returns>
        public T DeserializeEvent<T>(string data) where T : IDomainEvent
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data), "JsonEventSerializer.DeserializeEvent() : Event data cannot be empty string.");
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>
        /// Serialize a command to a Json string.
        /// </summary>
        /// <param name="command">Command to serialize.</param>
        /// <returns>Command serialized as string.</returns>
        public string SerializeCommand(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "JsonEventSerializer.SerializeCommand() : Command to serialize cannot be null.");
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(command);
        }

        /// <summary>
        /// Serialize an event to a Json string.
        /// </summary>
        /// <param name="event">Event to serialize.</param>
        /// <returns>Event serialized as string.</returns>
        public string SerializeEvent(IDomainEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event), "JsonEventSerializer.SerializeEvent() : Event to serialize cannot be null.");
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(@event);
        }

        #endregion
    }
}
