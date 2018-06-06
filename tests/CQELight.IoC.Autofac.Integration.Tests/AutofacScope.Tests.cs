using Autofac;
using CQELight.Bootstrapping.Notifications;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CQELight.IoC.Autofac.Integration.Tests
{
    public class AutofacScopeTests : BaseUnitTestClass
    {
        #region Ctor & members

        private interface IScopeTest { string Data { get; } }
        private class ScopeTest : IScopeTest
        {
            public string Data { get; }

            public ScopeTest()
            {
                Data = "ctor";
            }
            public ScopeTest(string data)
            {
                Data = data;
            }
        }

        private interface IParameterResolving { string Data { get; } }
        private class ParameterResolving : IParameterResolving
        {
            public ParameterResolving(string data)
            {
                Data = data;
            }

            public string Data { get; }
        }

        private interface Multiple { }
        private class MultipleOne : Multiple { }
        private class MultipleTwo : Multiple { }

        public AutofacScopeTests()
        {

        }

        #endregion

        #region CreateChildScope

        [Fact]
        public void AutofacScope_CreateChildScope_CustomScopeRegistration_TypeRegistration_AsExpected()
        {
            new Bootstrapper().UseAutofacAsIoC(new ContainerBuilder()).Bootstrapp();

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<IScopeTest>();
                i.Should().BeNull();
                using (var sChild = s.CreateChildScope())
                {
                    i = sChild.Resolve<IScopeTest>();
                    i.Should().BeNull();
                }
                using (var sChild = s.CreateChildScope(e => e.RegisterType<ScopeTest>()))
                {
                    i = sChild.Resolve<IScopeTest>();
                    i.Should().NotBeNull();
                    i.Data.Should().Be("ctor");
                }
            }
        }

        [Fact]
        public void AutofacScope_CreateChildScope_CustomScopeRegistration_InstanceRegistration_AsExpected()
        {
            new Bootstrapper().UseAutofacAsIoC(new ContainerBuilder()).Bootstrapp();

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<IScopeTest>();
                i.Should().BeNull();
                using (var sChild = s.CreateChildScope())
                {
                    i = sChild.Resolve<IScopeTest>();
                    i.Should().BeNull();
                }
                using (var sChild = s.CreateChildScope(e => e.Register(new ScopeTest("instance"))))
                {
                    i = sChild.Resolve<IScopeTest>();
                    i.Should().NotBeNull();
                    i.Data.Should().Be("instance");
                }
            }
        }

        #endregion

        #region Parameters

        [Fact]
        public void AutofacScope_Resolve_TypeParameter()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ParameterResolving>().AsImplementedInterfaces();
            new Bootstrapper().UseAutofacAsIoC(builder).Bootstrapp();

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<IParameterResolving>(new TypeResolverParameter(typeof(string), "test"));
                i.Data.Should().Be("test");
            }
        }

        [Fact]
        public void AutofacScope_Resolve_NameParameter()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ParameterResolving>().AsImplementedInterfaces();
            new Bootstrapper().UseAutofacAsIoC(builder).Bootstrapp();

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<IParameterResolving>(new NameResolverParameter("data", "name_test"));
                i.Data.Should().Be("name_test");
            }
        }

        [Fact]
        public void AutofacScope_ResolveAllInstancesOf_Generic()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MultipleOne>().AsImplementedInterfaces();
            builder.RegisterType<MultipleTwo>().AsImplementedInterfaces();
            new Bootstrapper().UseAutofacAsIoC(builder).Bootstrapp();

            using (var s = DIManager.BeginScope())
            {
                var data = s.ResolveAllInstancesOf<Multiple>();
                data.Should().HaveCount(2);
                data.Any(t => t.GetType() == typeof(MultipleOne)).Should().BeTrue();
                data.Any(t => t.GetType() == typeof(MultipleTwo)).Should().BeTrue();
            }
        }

        [Fact]
        public void AutofacScope_ResolveAllInstancesOf_NonGeneric()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MultipleOne>().AsImplementedInterfaces();
            builder.RegisterType<MultipleTwo>().AsImplementedInterfaces();
            new Bootstrapper().UseAutofacAsIoC(builder).Bootstrapp();

            using (var s = DIManager.BeginScope())
            {
                var data = s.ResolveAllInstancesOf(typeof(Multiple)).Cast<Multiple>();
                data.Should().HaveCount(2);
                data.Any(t => t.GetType() == typeof(MultipleOne)).Should().BeTrue();
                data.Any(t => t.GetType() == typeof(MultipleTwo)).Should().BeTrue();
            }
        }

        #endregion

    }
}
