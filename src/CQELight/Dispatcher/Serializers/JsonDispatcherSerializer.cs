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

        public ICommand DeserializeCommand(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data), "JsonEventSerializer.DeserializeCommand() : Command data cannot be empty string.");
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ICommand>(data);
        }

        public T DeserializeCommand<T>(string data) where T : ICommand
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data), "JsonEventSerializer.DeserializeCommand() : Command data cannot be empty string.");
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }

        public IDomainEvent DeserializeEvent(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data), "JsonEventSerializer.DeserializeEvent() : Event data cannot be empty string.");
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<IDomainEvent>(data);
        }

        public T DeserializeEvent<T>(string data) where T : IDomainEvent
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data), "JsonEventSerializer.DeserializeEvent() : Event data cannot be empty string.");
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }

        public string SerializeCommand(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "JsonEventSerializer.SerializeCommand() : Command to serialize cannot be null.");
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(command);
        }

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
