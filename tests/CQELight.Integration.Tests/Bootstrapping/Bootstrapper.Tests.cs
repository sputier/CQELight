using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Dispatcher;
using CQELight.IoC;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CQELight.Integration.Tests.Bootstrapping
{
    public class BootstrapperTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region Ctor

        [Fact]
        public void Bootstrapper_Ctor_Should_Have_BaseDispatcher_In_IoCRegistrations()
        {
            var b = new Bootstrapper();
            b.IoCRegistrations.Should().HaveCount(1);
            b.IoCRegistrations.First().Should().BeOfType<TypeRegistration>();

            b.IoCRegistrations.First().As<TypeRegistration>().InstanceType.Should().Be(typeof(BaseDispatcher));
            b.IoCRegistrations.First().As<TypeRegistration>().Types.First().Should().Be(typeof(IDispatcher));
        }

        #endregion

    }
}
