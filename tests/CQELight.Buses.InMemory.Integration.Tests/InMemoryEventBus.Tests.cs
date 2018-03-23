using Autofac;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.InMemory.Events;
using CQELight.Dispatcher;
using CQELight.IoC.Autofac;
using CQELight.TestFramework;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Buses.InMemory.Integration.Tests
{
    public class InMemoryEventBusTests : BaseUnitTestClass
    {

        #region Ctor & members

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

        private ContainerBuilder GetBasicBuilder()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new LoggerFactory()).AsImplementedInterfaces();
            return builder;
        }

        #endregion

        #region RegisterAsync

        [Fact]
        public async Task InMemoryEventBus_RegisterAsync_ContextAsHandler()
        {
            new Bootstrapper().UseAutofacAsIoC(GetBasicBuilder());

            CleanRegistrationInDispatcher();
            var b = new InMemoryEventBus();
            await b.RegisterAsync(new TestEvent { Data = "to_ctx" }, new TestEventContextHandler(0));

            TestEventContextHandler.Data.Should().Be("to_ctx");
            TestEventContextHandler.Dispatcher.Should().Be(0);
        }

        [Fact]
        public async Task InMemoryEventBus_RegisterAsync_ExceptionInHandler()
        {
            var builder = GetBasicBuilder();
            new Bootstrapper().UseAutofacAsIoC(builder);

            CleanRegistrationInDispatcher();
            bool errorInvoked = false;
            var c = new InMemoryEventBusConfiguration(3, 10, (e, ctx) => errorInvoked = true);
            var b = new InMemoryEventBus();
            b.Configure(c);
            await b.RegisterAsync(new TestEvent { Data = "err" }, null);

            errorInvoked.Should().BeTrue();

        }

        [Fact]
        public async Task InMemoryEventBus_RegisterAsync_HandlerInDispatcher()
        {
            new Bootstrapper().UseAutofacAsIoC(GetBasicBuilder());

            CleanRegistrationInDispatcher();
            CoreDispatcher.AddHandlerToDispatcher(new TestEventContextHandler(1));
            var b = new InMemoryEventBus();
            await b.RegisterAsync(new TestEvent { Data = "to_ctx" });

            TestEventContextHandler.Data.Should().Be("to_ctx");
            TestEventContextHandler.Dispatcher.Should().Be(1);

        }

        #endregion

    }
}
