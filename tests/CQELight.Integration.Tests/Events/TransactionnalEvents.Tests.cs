using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Dispatcher;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Integration.Tests.Events
{
    public class Event1 : BaseDomainEvent
    {
        public string Data { get; set; }
    }
    public class Event2 : BaseDomainEvent
    {
        public string Data { get; set; }
    }
    public class Event3 : BaseDomainEvent
    {
        public string Data { get; set; }
    }

    public class TransactionEvent : BaseTransactionnalEvent
    {
        public TransactionEvent(params IDomainEvent[] events)
            : base(events)
        {

        }
    }

    public class TransactionEventHandler : BaseTransactionEventHandler<TransactionEvent>
    {
        public string DataParsed { get; private set; }
        public string BeforeData { get; private set; }
        public string AfterData { get; private set; }

        public TransactionEventHandler()
        {
        }

        protected override Task AfterTreatEventsAsync()
        {
            AfterData = "AFTER";
            return base.AfterTreatEventsAsync();
        }

        protected override Task BeforeTreatEventsAsync()
        {
            BeforeData = "BEFORE";
            return base.BeforeTreatEventsAsync();
        }

        protected override Task TreatEventAsync(IDomainEvent evt)
        {
            if (evt is Event1 evt1)
            {
                DataParsed += "|1:" + evt1.Data;
            }
            else if (evt is Event2 evt2)
            {
                DataParsed += "|2:" + evt2.Data;
            }
            else if (evt is Event3 evt3)
            {
                DataParsed += "|3:" + evt3.Data;
            }
            return Task.CompletedTask;
        }
    }

    public class TransactionEventTests : BaseUnitTestClass
    {

        #region Ctor & members

        public TransactionEventTests()
        {
        }

        #endregion

        #region BaseTransactionnalEvent ctor

        [Fact]
        public void BaseTransactionnalEvent_Ctor_ParamsTests()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionEvent(null));
            Assert.Throws<ArgumentException>(() => new TransactionEvent());
            Assert.Throws<ArgumentException>(() => new TransactionEvent(new Event1 { Data = "Data1" }));
        }

        [Fact]
        public void BaseTransactionnalEvent_Ctor_AsExpected()
        {
            var evt = new TransactionEvent(
                new Event1 { Data = "Data1" },
                new Event2 { Data = "Data2" },
                new Event3 { Data = "Data3" }
                );

            evt.Should().NotBeNull();
            evt.Events.Should().HaveCount(3);
            evt.Events.First().Should().BeOfType<Event1>();
            evt.Events.Skip(1).First().Should().BeOfType<Event2>();
            evt.Events.Skip(2).First().Should().BeOfType<Event3>();
        }

        #endregion

        #region Dispatch & handle

        [Fact]
        public async Task BaseTransactionnalEvent_Handle_AsExpected()
        {
            var h = new TransactionEventHandler();

            var evt = new TransactionEvent(
                new Event1 { Data = "Data1" },
                new Event2 { Data = "Data2" },
                new Event3 { Data = "Data3" }
                );

            await h.HandleAsync(evt).ConfigureAwait(false);
            
            h.DataParsed.Should().Be("|1:Data1|2:Data2|3:Data3");
        }


        #endregion

        #region BeforeTreatEvents & AfterTreatEvents

        [Fact]
        public async Task BaseTransactionnalEvent_Handle_Before_After_AsExpected()
        {
            var h = new TransactionEventHandler();
            var evt = new TransactionEvent(
                new Event1 { Data = "Data1" },
                new Event2 { Data = "Data2" },
                new Event3 { Data = "Data3" }
                );

            await h.HandleAsync(evt).ConfigureAwait(false);

            h.DataParsed.Should().Be("|1:Data1|2:Data2|3:Data3");

            h.BeforeData.Should().Be("BEFORE");
            h.AfterData.Should().Be("AFTER");
        }

        #endregion

    }
}
