using CQELight.TestFramework;
using CQELight.Tools.Extensions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CQELight.Tools.Tests.Extensions
{
    public class TypeExtensionsTests : BaseUnitTestClass
    {
        #region Nested classes

        private class A { }
        private class B : A { }
        private class C : B { }
        private class D : C { }
        private class E : D { }
        private class F : E
        {
            public string Param { get; set; }
            public F(string param)
            {
                Param = param;
            }
        }

        private class Props
        {
            private string A { get; set; }
            private int B { get; set; }
            private DateTime C { get; set; }
            private D D { get; set; }
        }

        #endregion

        #region IsInHierarchySubClassOf

        [Fact]
        public void TypeExtensions_IsInHierarchySubClassOf_NotIn()
        {
            typeof(DateTime).IsInHierarchySubClassOf(typeof(A)).Should().BeFalse();
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

        #region GetAllProperties

        [Fact]
        public void TypeExtensions_GetAllProperties_AsExpected()
        {
            var props = typeof(Props).GetAllProperties();
            props.Should().NotBeNull();
            props.Should().HaveCount(4);
            props.Any(p => p.PropertyType == typeof(string) && p.Name == "A").Should().BeTrue();
            props.Any(p => p.PropertyType == typeof(int) && p.Name == "B").Should().BeTrue();
            props.Any(p => p.PropertyType == typeof(DateTime) && p.Name == "C").Should().BeTrue();
            props.Any(p => p.PropertyType == typeof(D) && p.Name == "D").Should().BeTrue();
        }

        #endregion

        #region NameExistsInHierarchy

        [Fact]
        public void NameExistsInHierarchy_NotFound_Should_BeFalse()
        {
            var bType = typeof(DateTime);

            bType.NameExistsInHierarchy("A").Should().BeFalse();
        }

        [Fact]
        public void NameExistsInHierarchy_First_Parent_Should_BeTrue()
        {
            var bType = typeof(B);

            bType.NameExistsInHierarchy("A").Should().BeTrue();
        }

        [Fact]
        public void NameExistsInHierarchy_SecondParent_Should_BeTrue()
        {
            var bType = typeof(C);

            bType.NameExistsInHierarchy("A").Should().BeTrue();
        }

        [Fact]
        public void NameExistsInHierarchy_DeepParent_Should_BeTrue()
        {
            var bType = typeof(F);

            bType.NameExistsInHierarchy("A").Should().BeTrue();
        }

        #endregion

    }
}
