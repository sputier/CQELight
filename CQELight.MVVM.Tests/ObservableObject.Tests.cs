using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.MVVM.Tests
{
    public class ObservableObjectTests : BaseUnitTestClass
    {

        #region Ctor & members

        private class TestObservable : ObservableObject
        {
            private string _testVal;

            public string TestValue
            {
                get => _testVal;
                set => Set(ref _testVal, value);
            }

        }

        #endregion

        #region Set

        [Fact]
        public void ObservableObject_Set_SameValue_Should_NotRaise_PropertyChanged()
        {
            var o = new TestObservable();
            o.TestValue = "val";
            bool invoked = false;
            o.PropertyChanged += (s, e) => invoked = true;
            o.TestValue = "val";
            invoked.Should().BeFalse();
        }

        [Fact]
        public void ObservableObject_Set_DifferntValue_Should_Raise_PropertyChanged()
        {

            var o = new TestObservable();
            o.TestValue = "val";
            bool invoked = false;
            o.PropertyChanged += (s, e) => invoked = true;
            o.TestValue = "val2";
            invoked.Should().BeTrue();
        }

        #endregion

    }
}
