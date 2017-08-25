using CQELight.TestFramework;
using CQELight.Tools.Extensions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tools.Tests.Extensions
{
    public class TypeExtensionsTests : BaseUnitTestClass
    {

        #region Nested classes

        class A { }
        class B : A { }
        class C : B { }
        class D : C { }
        class E { }
        class F
        {
            public string Param { get; set; }
            public F(string param)
            {
                Param = param;
            }
        }

        #endregion

        #region IsInHierarchySubClassOf

        [Fact]
        public void TypeExtensions_IsInHierarchySubClassOf_NotIn()
        {
            typeof(E).IsInHierarchySubClassOf(typeof(A)).Should().BeFalse();
        }

        [Fact]
        public void TypeExtensions_IsInHierarchySubClassOf_AsExpected()
        {
            typeof(D).IsInHierarchySubClassOf(typeof(A)).Should().BeTrue();
        }

        #endregion

        #region CreateInstance

        [Fact]
        public void TypeExtensions_CreateInstance_NoParametersCtor()
        {
            var instance = (typeof(E)).CreateInstance();

            instance.Should().NotBeNull();
            instance.Should().BeOfType<E>();
        }

        [Fact]
        public void TypeExtensions_CreateInstance_MismatchParametersCtor()
        {
            var instance = (typeof(F)).CreateInstance();

            instance.Should().BeNull();
        }

        [Fact]
        public void TypeExtensions_CreateInstance_WithParametersCtor()
        {
            var instance = (typeof(F)).CreateInstance("testParam");

            instance.Should().NotBeNull();
            instance.Should().BeOfType<F>();
            (instance as F).Param.Should().Be("testParam");
        }

        #endregion

    }
}
