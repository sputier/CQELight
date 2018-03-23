using CQELight.TestFramework;
using System;
using Xunit;
using CQELight.IoC.Autofac;
using Autofac;
using FluentAssertions;

namespace CQELight.IoC.Autofac.Integration.Tests
{
    public class BootstrapperExtTests : BaseUnitTestClass
    {

        #region Ctor & members

        private interface ITest
        {
            string Data { get; }
        }
        private class Test : ITest
        {

            public Test(string data)
            {
                Data = data;
            }
            public Test()
            {
                Data = "ctor_test";
            }

            public string Data { get; private set; }
        }

        private ContainerBuilder _builder;

        public BootstrapperExtTests()
        {
            _builder = new ContainerBuilder();
        }

        #endregion

        #region InstanceTypeRegistration

        [Fact]
        public void BootstrapperExt_CustomRegistration_InstanceTypeRegistration_AsExpected()
        {
            var b = new Bootstrapper();
            b.AddIoCRegistration(new InstanceTypeRegistration(new Test("test"), typeof(ITest)));
            b.UseAutofacAsIoC(_builder);

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<ITest>();
                i.Data.Should().Be("test");
            }

        }

        #endregion

        #region TypeRegistration

        [Fact]
        public void BootstrapperExt_CustomRegistration_TypeRegistration_AsExpected()
        {
            var b = new Bootstrapper();
            b.AddIoCRegistration(new TypeRegistration(typeof(Test), typeof(ITest)));
            b.UseAutofacAsIoC(_builder);

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<ITest>();
                i.Data.Should().Be("ctor_test");
            }
        }

        #endregion

        #region FactoryRegistration

        [Fact]
        public void BootstrapperExt_CustomRegistration_FactoryRegistration_AsExpected()
        {
            var b = new Bootstrapper();
            b.AddIoCRegistration(new FactoryRegistration(() => new Test("fact_test"), typeof(ITest)));
            b.UseAutofacAsIoC(_builder);

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<ITest>();
                i.Data.Should().Be("fact_test");
            }
        }

        #endregion

    }
}
