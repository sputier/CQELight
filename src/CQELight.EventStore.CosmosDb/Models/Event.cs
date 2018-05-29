using CQELight.Abstractions.Events;
using Newtonsoft.Json;
using System;

namespace CQELight.EventStore.CosmosDb.Models
{
    internal class Event
    {
        #region Properties

        [JsonProperty(PropertyName = "id")]
        public virtual Guid Id { get; set; }
        public virtual Guid? AggregateId { get; set; }
        public virtual string AggregateType { get; set; }
        public virtual string EventData { get; set; }
        public virtual string EventType { get; set; }
        public virtual DateTime EventTime { get; set; }
        public virtual ulong Sequence { get; set; }

        #endregion

    }
}
