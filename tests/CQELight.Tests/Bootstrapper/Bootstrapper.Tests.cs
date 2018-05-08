using CQELight.Bootstrapping.Notifications;
using CQELight.IoC;
using CQELight.TestFramework;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CQELight.Tests
{
    public class BootstrapperTests : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region AddIoCRegistration

        [Fact]
        public void Bootstrapper_AddIoCRegistration_TestParams()
        {
            Assert.Throws<ArgumentNullException>(() => new Bootstrapper().AddIoCRegistration(null));

            var b = new Bootstrapper();
            b.AddIoCRegistration(new TypeRegistration(typeof(DateTime), typeof(DateTime)));
            b.IoCRegistrations.Should().HaveCount(2);
            b.IoCRegistrations.First().Should().BeOfType<TypeRegistration>();
        }

        #endregion

        #region ConfigureDispatcher

        [Fact]
        public void Bootstrapper_ConfigureDispatcher_TestParams()
        {
            Assert.Throws<ArgumentNullException>(() => new Bootstrapper().ConfigureCoreDispatcher(null));
        }

        #endregion

        #region AddService

        [Fact]
        public void Bootstrapper_AddService_TestParams()
        {
            Assert.Throws<ArgumentNullException>(() => new Bootstrapper().AddService(null));
        }

        [Fact]
        public void Bootstrapper_AddService_StrictMode_AlreadExists()
        {
            var m1 = new Mock<IBootstrapperService>();
            m1.Setup(m => m.ServiceType).Returns(BootstrapperServiceType.IoC);
            var m2 = new Mock<IBootstrapperService>();
            m2.Setup(m => m.ServiceType).Returns(BootstrapperServiceType.IoC);

            Assert.Throws<InvalidOperationException>(() => new Bootstrapper(true).AddService(m1.Object).AddService(m2.Object));
        }

        #endregion

        #region Bootstrapp

        [Fact]
        public void Bootstrapper_Bootstrapp_Non_Optimal_Mode()
        {
            var b = new Bootstrapper();
            b.Bootstrapp(out List<BootstrapperNotification> notifs);
            notifs.Should().BeEmpty();
        }

        [Fact]
        public void Bootstrapper_Bootstrapp_Optimal_Mode()
        {
            var b = new Bootstrapper(checkOptimal: true);
            b.Bootstrapp(out List<BootstrapperNotification> notifs);
            notifs.Should().HaveCount(4);
            notifs.All(n => n.Type == BootstrapperNotificationType.Warning).Should().BeTrue();
            notifs.Any(s => s.ContentType == BootstapperNotificationContentType.BusServiceMissing).Should().BeTrue();
            notifs.Any(s => s.ContentType == BootstapperNotificationContentType.DALServiceMissing).Should().BeTrue();
            notifs.Any(s => s.ContentType == BootstapperNotificationContentType.EventStoreServiceMissing).Should().BeTrue();
            notifs.Any(s => s.ContentType == BootstapperNotificationContentType.IoCServiceMissing).Should().BeTrue();
        }

        #endregion

    }
}
