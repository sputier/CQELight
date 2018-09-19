using CQELight.Abstractions.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight_Benchmarks.Models
{
    public class BenchmarkSimpleEvent : BaseDomainEvent
    {

        #region Ctor

        public BenchmarkSimpleEvent(Guid id)
        {
            Id = id;
        }

        #endregion

        #region Properties

        public string StringValue { get; set; }

        public int IntValue { get; set; }

        public DateTime DateTimeValue { get; set; }

        #endregion

    }
}
