using CQELight.Abstractions.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQELight_Benchmarks.Models
{
    public class AggregateEvent : BaseDomainEvent
    {

        #region Properties

        public int AggregateIntValue { get; set; }
        public string AggregateStringValue { get; set; }

        #endregion

        #region Ctor

        public AggregateEvent(Guid id, Guid aggId)
        {
            Id = id;
            AggregateId = aggId;
        }

        #endregion

    }
}