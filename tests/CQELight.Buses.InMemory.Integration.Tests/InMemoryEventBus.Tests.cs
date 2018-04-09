using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.InMemory.Events;
using CQELight.Dispatcher;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading;
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

        private class TestRetryEvent : BaseDomainEvent
        {
            public static int NbTry = 1;

        }
        private class TestRetryEventHandler : IDomainEventHandler<TestRetryEvent>
        {
            public Task HandleAsync(TestRetryEvent domainEvent, IEventContext context = null)
            {
                if (TestRetryEvent.NbTry != 2)
                {
                    TestRetryEvent.NbTry++;
                    throw new Exception("NbTries == 2");
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
        }

        private class TestIfEvent : BaseDomainEvent
        {
            public int Data { get; set; }
        }
        private class TestIfEventHandler : IDomainEventHandler<TestIfEvent>
        {
            public static int Data { get; private set; }
            public Task HandleAsync(TestIfEvent domainEvent, IEventContext context = null)
            {
                Data = domainEvent.Data;
                return Task.CompletedTask;
            }

            internal static void ResetData() => Data = 0;
        }

        private class ParallelEvent : BaseDomainEvent
        {
            public List<int> ThreadsInfos = new List<int>();
            public bool RetryMode = false;
            private SemaphoreSlim sem = new SemaphoreSlim(1);
            public void AddThreadInfos(int i)
            {
                sem.Wait();
                ThreadsInfos.Add(i);
                sem.Release();
            }
        }
        private class ParallelEventHandlerOne : IDomainEventHandler<ParallelEvent>
        {
            public async Task HandleAsync(ParallelEvent domainEvent, IEventContext context = null)
            {
                await Task.Delay(100);
                domainEvent.AddThreadInfos(Thread.CurrentThread.ManagedThreadId);
            }
        }
        private class ParallelEventHandlerTwo : IDomainEventHandler<ParallelEvent>
        {
            private static int NbTries = 0;
            public static void ResetTries() => NbTries = 0;
            public async Task HandleAsync(ParallelEvent domainEvent, IEventContext context = null)
            {
                await Task.Delay(150);
                if (domainEvent.RetryMode && NbTries < 2)
                {
                    NbTries++;
                    throw new InvalidOperationException();
                }
                domainEvent.AddThreadInfos(Thread.CurrentThread.ManagedThreadId);
            }
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

        #region Configuration

        [Fact]
        public async Task InMemoryEventBus_Configuration_RetryStrategy_Callback()
        {
            TestRetryEvent.NbTry = 0;
            bool callbackCalled = false;
            var cfgBuilder =
                new InMemoryEventBusConfigurationBuilder()
                .SetRetryStrategy(100, 1)
                .DefineErrorCallback((e, ctx) => callbackCalled = true);


            var b = new InMemoryEventBus(cfgBuilder.Build());
            await b.RegisterAsync(new TestRetryEvent()).ConfigureAwait(false);

            callbackCalled.Should().BeTrue();

        }

        [Fact]
        public async Task InMemoryEventBus_Configuration_RetryStrategy_CallbackNoInvoked()
        {
            TestRetryEvent.NbTry = 0;
            bool callbackCalled = false;
            var cfgBuilder =
                new InMemoryEventBusConfigurationBuilder()
                .SetRetryStrategy(100, 3)
                .DefineErrorCallback((e, ctx) => callbackCalled = true);


            var b = new InMemoryEventBus(cfgBuilder.Build());
            await b.RegisterAsync(new TestRetryEvent()).ConfigureAwait(false);

            callbackCalled.Should().BeFalse();

        }

        [Fact]
        public async Task InMemoryEventBus_Configuration_RetryStrategy_WhenDispatchParallel()
        {
            ParallelEventHandlerTwo.ResetTries();
            bool callbackCalled = false;
            var cfgBuilder =
                new InMemoryEventBusConfigurationBuilder()
                .SetRetryStrategy(100, 3)
                .DefineErrorCallback((e, ctx) => callbackCalled = true)
                .AllowParallelDispatchFor<ParallelEvent>();

            var b = new InMemoryEventBus(cfgBuilder.Build());

            var evt = new ParallelEvent()
            {
                RetryMode = true
            };
            await b.RegisterAsync(evt).ConfigureAwait(false);

            callbackCalled.Should().BeFalse();
            evt.ThreadsInfos.Should().HaveCount(2);

        }

        [Fact]
        public async Task InMemoryEventBus_Configuration_DispatchIfClause()
        {
            TestIfEventHandler.ResetData();
            var cfgBuilder =
                new InMemoryEventBusConfigurationBuilder()
                .DispatchOnlyIf<TestIfEvent>(e => e.Data > 1);

            var b = new InMemoryEventBus(cfgBuilder.Build());

            TestIfEventHandler.Data.Should().Be(0);

            await b.RegisterAsync(new TestIfEvent { Data = 1 }).ConfigureAwait(false);

            TestIfEventHandler.Data.Should().Be(0);

            await b.RegisterAsync(new TestIfEvent { Data = 10 }).ConfigureAwait(false);

            TestIfEventHandler.Data.Should().Be(10);

        }

        [Fact]
        public async Task InMemoryEventBus_Configuration_ParallelDispatch()
        {
            var cfgBuilder =
                new InMemoryEventBusConfigurationBuilder()
                .AllowParallelDispatchFor<ParallelEvent>();

            var b = new InMemoryEventBus(cfgBuilder.Build());

            var evt = new ParallelEvent();
            await b.RegisterAsync(evt).ConfigureAwait(false);

            evt.ThreadsInfos.Should().HaveCount(2);
        }

        #endregion

    }
}
