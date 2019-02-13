using CQELight.Abstractions.Events.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CQELight.EventStore.GregYoungsEventStore.Common
{
    internal static class EventSerializationExtension
    {
        public static string SerializeToJson(this IDomainEvent @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            return JsonConvert.SerializeObject(@event);
        }

        public static string SerializeMetadataToJson(this IDomainEvent @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            return JsonConvert.SerializeObject(ReadType(@event.GetType()));
        }

        private static IEnumerable<object> ReadType(Type type)
        {
            return type.GetProperties().Select(a => new
            {
                PropertyName = a.Name,
                Type = a.PropertyType.Name,
                IsPrimitive = a.PropertyType.IsPrimitive && a.PropertyType != typeof(string),
                Properties = (a.PropertyType.IsPrimitive && a.PropertyType != typeof(string)) ? null : ReadType(a.PropertyType)
            }).ToList();
        }
    }
}
