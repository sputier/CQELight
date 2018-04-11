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
            b.IoCRegistrations.Should().HaveCount(1);
            b.IoCRegistrations.First().Should().BeOfType<TypeRegistration>();
        }

        #endregion

        #region ConfigureDispatcher

        [Fact]
        public void Bootstrapper_ConfigureDispatcher_TestParams()
        {
            Assert.Throws<ArgumentNullException>(() => new Bootstrapper().ConfigureDispatcher(null));
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

    }
}
