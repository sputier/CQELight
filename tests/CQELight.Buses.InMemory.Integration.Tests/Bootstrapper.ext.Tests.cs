using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events;
using CQELight.Bootstrapping.Notifications;
using CQELight.Buses.InMemory.Commands;
using CQELight.Buses.InMemory.Events;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Buses.InMemory.Integration.Tests
{
    public class BootstrapperExtensionTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region UseInMemoryCommandBus

        [Fact]
        public void UseInMemoryCommandBus_When_Bootstrapping_Should_Not_CreateNotifications_If_NotStrict_And_NotOptimal()
        {
            var b = new Bootstrapper();
            var notifs = b.UseInMemoryCommandBus().Bootstrapp();

            notifs.Should().BeEmpty();
        }

        private class MultipleHandlerCommand : ICommand { }
        private class FirstHandler : ICommandHandler<MultipleHandlerCommand>
        {
            public Task HandleAsync(MultipleHandlerCommand command, ICommandContext context = null)
                => Task.CompletedTask;
        }
        private class SecondHandler : ICommandHandler<MultipleHandlerCommand>
        {
            public Task HandleAsync(MultipleHandlerCommand command, ICommandContext context = null)
                => Task.CompletedTask;
        }

        [Fact]
        public void UseInMemoryCommandBus_Should_Add_Warning_Notification_If_More_Than_One_Handler_Are_Registered_When_Strict_IsPassed()
        {
            var b = new Bootstrapper(true);
            var notifs = b.UseInMemoryCommandBus().Bootstrapp();

            notifs.Should().HaveCountGreaterOrEqualTo(1);

            var notif = notifs.First(e => e.BootstrapperServiceType == typeof(InMemoryCommandBus));
            notif.Type.Should().Be(BootstrapperNotificationType.Warning);
            notif.BootstrapperServiceType.Should().Be(typeof(InMemoryCommandBus));
            notif.ContentType.Should().Be(BootstapperNotificationContentType.CustomServiceNotification);
        }

        private class AloneCommand : ICommand { }

        [Fact]
        public void UseInMemoryCommandBus_Should_Add_Warning_Notification_If_No_Handlers_Exists_When_Optimal_IsPassed()
        {
            var b = new Bootstrapper(false, true);
            var notifs = b.UseInMemoryCommandBus().Bootstrapp();

            notifs.Should().HaveCountGreaterOrEqualTo(1);

            var notif = notifs.First(e => e.BootstrapperServiceType == typeof(InMemoryCommandBus));
            notif.Type.Should().Be(BootstrapperNotificationType.Warning);
            notif.BootstrapperServiceType.Should().Be(typeof(InMemoryCommandBus));
            notif.ContentType.Should().Be(BootstapperNotificationContentType.CustomServiceNotification);
        }

        [Fact]
        public void UseInMemoryCommandBus_Should_Add_Error_Notification_If_More_Than_One_Handler_Are_Registered_When_Strict_And_Optimal_ArePassed()
        {
            var b = new Bootstrapper(true, true);
            var notifs = b.UseInMemoryCommandBus().Bootstrapp();

            notifs.Should().HaveCountGreaterOrEqualTo(1);

            var notif = notifs.First(e => e.BootstrapperServiceType == typeof(InMemoryCommandBus) && e.Type == BootstrapperNotificationType.Error);
            notif.Type.Should().Be(BootstrapperNotificationType.Error);
            notif.BootstrapperServiceType.Should().Be(typeof(InMemoryCommandBus));
            notif.ContentType.Should().Be(BootstapperNotificationContentType.CustomServiceNotification);
        }

        #endregion

        #region UseInMemoryEventBus

        [Fact]
        public void UseInMemoryEventBus_When_Bootstrapping_Should_Not_CreateNotifications_If_NotStrict_And_NotOptimal()
        {
            var b = new Bootstrapper();
            var notifs = b.UseInMemoryEventBus().Bootstrapp();

            notifs.Should().BeEmpty();
        }

        private class AloneEvent : BaseDomainEvent { }

        [Fact]
        public void UseInMemoryEventBus_Should_Add_Warning_Notification_If_No_Handlers_Exists_When_Optimal_IsPassed()
        {
            var b = new Bootstrapper(false, true);
            var notifs = b.UseInMemoryEventBus().Bootstrapp();

            notifs.Should().HaveCountGreaterOrEqualTo(1);

            var notif = notifs.First(e => e.BootstrapperServiceType == typeof(InMemoryEventBus));
            notif.Type.Should().Be(BootstrapperNotificationType.Warning);
            notif.BootstrapperServiceType.Should().Be(typeof(InMemoryEventBus));
            notif.ContentType.Should().Be(BootstapperNotificationContentType.CustomServiceNotification);
        }

        #endregion
    }
}
