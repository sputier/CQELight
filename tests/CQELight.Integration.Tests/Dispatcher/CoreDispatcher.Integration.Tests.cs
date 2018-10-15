using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Dispatcher;
using CQELight.Dispatcher.Configuration;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Integration.Tests.Dispatcher
{
    public class CoreDispatcherTests : BaseUnitTestClass
    {
        #region Ctor & members

        private class TestEvent : BaseDomainEvent { }
        private class TestEventHandler : IDomainEventHandler<TestEvent>
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

        private class TestCommand : ICommand { }
        private class TestCommandHandler : ICommandHandler<TestCommand>
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

        public CoreDispatcherTests()
        {
            CoreDispatcher.UseConfiguration(DispatcherConfiguration.Default);
        }

        #endregion

        #region PublicEventAsync

        [Fact]
        public async Task CoreDispatcher_PublishEventAsync_SecurityCritical_On()
        {
            var cfg = new DispatcherConfigurationBuilder();
            cfg.ForEvent<TestEvent>().IsSecurityCritical();
            CoreDispatcher.UseConfiguration(cfg.Build());

            var evt = new TestEvent();
            IDomainEvent callbackEvent = null;

            CoreDispatcher.OnEventDispatched += (s) =>
            {
                callbackEvent = s;
                return Task.CompletedTask;
            };

            await CoreDispatcher.PublishEventAsync(evt).ConfigureAwait(false);
            ReferenceEquals(evt, callbackEvent).Should().BeFalse();
        }

        [Fact]
        public async Task CoreDispatcher_PublishEventAsync_SecurityCritical_Off()
        {
            var evt = new TestEvent();
            IDomainEvent callbackEvent = null;

            var cfg = new DispatcherConfigurationBuilder();
            CoreDispatcher.OnEventDispatched += (s) =>
            {
                callbackEvent = s;
                return Task.CompletedTask;
            };

            await CoreDispatcher.PublishEventAsync(evt).ConfigureAwait(false);
            ReferenceEquals(evt, callbackEvent).Should().BeTrue();
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

            var coreHandler = CoreDispatcher.TryGetHandlersForCommandType(typeof(TestCommand));
            coreHandler.Should().NotBeEmpty();

            CoreDispatcher.RemoveHandlerFromDispatcher(h);

            coreHandler = CoreDispatcher.TryGetHandlersForCommandType(typeof(TestCommand));
            coreHandler.Should().BeEmpty();
        }

        #endregion

    }
}
