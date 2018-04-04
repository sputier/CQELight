using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.InMemory.Events;
using CQELight.Dispatcher;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Buses.InMemory.Integration.Tests
{
    public class InMemoryEventBusTests : BaseUnitTestClass
    {

        #region Ctor & members

        private class Event1 : BaseDomainEvent
        {
            public string Data { get; set; }
        }
        private class Event2 : BaseDomainEvent
        {
            public string Data { get; set; }
        }
        private class Event3 : BaseDomainEvent
        {
            public string Data { get; set; }
        }

        private class TransactionEvent : BaseTransactionnalEvent
        {
            public TransactionEvent(params IDomainEvent[] events)
                : base(events)
            {

            }
        }

        private class TransactionEventHandler : BaseTransactionEventHandler<TransactionEvent>
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

        private class TestEventContextHandler : IDomainEventHandler<TestEvent>, IEventContext
        {
            public static string Data { get; private set; }
            public static int Dispatcher { get; private set; }

            public static void ResetData()
            => Data = string.Empty;
            public TestEventContextHandler(int dispatcher)
            {
                ResetData();
                Dispatcher = dispatcher;
            }
            public Task HandleAsync(TestEvent domainEvent, IEventContext context = null)
            {
                Data = domainEvent.Data;
                return Task.CompletedTask;
            }
        }
        private class ExceptionHandler : IDomainEventHandler<TestEvent>
        {
            public Task HandleAsync(TestEvent domainEvent, IEventContext context = null)
            {
                throw new NotImplementedException();
            }
        }
        private class TestEvent : BaseDomainEvent
        {
            public string Data { get; set; }
        }

        public InMemoryEventBusTests()
        {
            TestEventContextHandler.ResetData();
        }

        #endregion

        #region RegisterAsync

        [Fact]
        public async Task InMemoryEventBus_RegisterAsync_ContextAsHandler()
        {
            CleanRegistrationInDispatcher();
            var b = new InMemoryEventBus();
            await b.RegisterAsync(new TestEvent { Data = "to_ctx" }, new TestEventContextHandler(0)).ConfigureAwait(false);

            TestEventContextHandler.Data.Should().Be("to_ctx");
            TestEventContextHandler.Dispatcher.Should().Be(0);
        }

        [Fact]
        public async Task InMemoryEventBus_RegisterAsync_ExceptionInHandler()
        {

            CleanRegistrationInDispatcher();
            bool errorInvoked = false;
            var c = new InMemoryEventBusConfigurationBuilder()
                .DefineErrorCallback((e, ctx) => errorInvoked = true)
                .SetRetryStrategy(3, 10)
                .Build();
            var b = new InMemoryEventBus(c);
            await b.RegisterAsync(new TestEvent { Data = "err" }, null).ConfigureAwait(false);

            errorInvoked.Should().BeTrue();

        }

        [Fact]
        public async Task InMemoryEventBus_RegisterAsync_HandlerInDispatcher()
        {
            CleanRegistrationInDispatcher();
            CoreDispatcher.AddHandlerToDispatcher(new TestEventContextHandler(1));
            var b = new InMemoryEventBus();
            await b.RegisterAsync(new TestEvent { Data = "to_ctx" }).ConfigureAwait(false);

            TestEventContextHandler.Data.Should().Be("to_ctx");
            TestEventContextHandler.Dispatcher.Should().Be(1);

        }

        #endregion

        #region TransactionnalEvent

        [Fact]
        public async Task InMemoryEventBus_RegisterAsync_TransactionnalEvent()
        {
            CleanRegistrationInDispatcher();
            var h = new TransactionEventHandler();
            CoreDispatcher.AddHandlerToDispatcher(h);

            var evt = new TransactionEvent(
                new Event1 { Data = "Data1" },
                new Event2 { Data = "Data2" },
                new Event3 { Data = "Data3" }
                );
            
            var b = new InMemoryEventBus();
            await b.RegisterAsync(evt).ConfigureAwait(false);

            h.DataParsed.Should().Be("|1:Data1|2:Data2|3:Data3");

        }

        #endregion

    }
}
