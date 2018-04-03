using Autofac;
using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Dispatcher;
using CQELight.Dispatcher.Configuration;
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

        class TestCommand : ICommand { }
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

        public CoreDispatcherTests()
        {
            CoreDispatcher.UseConfiguration(CoreDispatcherConfiguration.Default);
        }

        #endregion

        #region PublicEventAsync

        [Fact]
        public async Task CoreDispatcher_PublishEventAsync_SecurityCritical_On()
        {
            var cfg = new CoreDispatcherConfigurationBuilder();
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

            var cfg = new CoreDispatcherConfigurationBuilder();
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

            var coreHandler = CoreDispatcher.TryGetHandlerForCommandType(typeof(TestCommand));
            coreHandler.Should().NotBeNull();

            CoreDispatcher.RemoveHandlerFromDispatcher(h);

            coreHandler = CoreDispatcher.TryGetHandlerForCommandType(typeof(TestCommand));
            coreHandler.Should().BeNull();
        }

        #endregion

    }
}
