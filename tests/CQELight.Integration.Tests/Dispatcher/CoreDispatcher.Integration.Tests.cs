using Autofac;
using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
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

namespace CQELight.Integration.Tests.Dispatcher
{
    public class CoreDispatcherTests : BaseUnitTestClass
    {

        #region Ctor & members

        class TestEvent : BaseDomainEvent { }
        class TestEventHandler : IDomainEventHandler<TestEvent>
        {
            public static bool IsHandled;
            public static void ResetFlag() => IsHandled = false;
            public TestEventHandler()
            {
                ResetFlag();
            }
            public Task HandleAsync(TestEvent domainEvent, IEventContext context = null)
            {
                return Task.CompletedTask;
            }
        }

        class TestCommand: ICommand { }
        class TestCommandHandler : ICommandHandler<TestCommand>
        {
            public static bool IsHandled;
            public static void ResetFlag() => IsHandled = false;
            public TestCommandHandler()
            {
                ResetFlag();
            }
            public Task HandleAsync(TestCommand command, ICommandContext context = null)
            {
                return Task.CompletedTask;
            }
        }

        private ContainerBuilder GetBasicBuilder()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new LoggerFactory()).AsImplementedInterfaces();
            return builder;
        }

        public CoreDispatcherTests()
        {
            new Bootstrapper().UseAutofacAsIoC(GetBasicBuilder());
        }

        #endregion

        #region RemoveHandlerFromDispatcher

        [Fact]
        public void CoreDispatcher_RemoveHandlerFromDispatcher_ParamsTests()
        {
            Assert.Throws<ArgumentNullException>(() => CoreDispatcher.RemoveHandlerFromDispatcher(null));
        }

        [Fact]
        public void CoreDispatcher_RemoveHandlerFromDispatcher_EventHandler()
        {
            var h = new TestEventHandler();
            CoreDispatcher.AddHandlerToDispatcher(h);

            var all = CoreDispatcher.TryGetHandlersForEventType(typeof(TestEvent));
            all.Should().HaveCount(1);

            CoreDispatcher.RemoveHandlerFromDispatcher(h);

            all = CoreDispatcher.TryGetHandlersForEventType(typeof(TestEvent));
            all.Should().HaveCount(0);
        }

        [Fact]
        public void CoreDispatcher_RemoveHandlerFromDispatcher_CommandHandler()
        {

            var h = new TestCommandHandler();
            CoreDispatcher.AddHandlerToDispatcher(h);

            var coreHandler = CoreDispatcher.TryGetHandlerForCommandType(typeof(TestCommand));
            coreHandler.Should().NotBeNull();

            CoreDispatcher.RemoveHandlerFromDispatcher(h);

            coreHandler = CoreDispatcher.TryGetHandlerForCommandType(typeof(TestCommand));
            coreHandler.Should().BeNull();
        }

        #endregion

    }
}
