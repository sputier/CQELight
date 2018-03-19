using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.MVVM.Tests
{
    public class DelegateCommandTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region Ctor

        [Fact]
        public void DelegateCommand_Ctor_TestParams()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateCommand(null));
        }

        #endregion

        #region CanExecute

        [Fact]
        public void DelegateCommand_CanExecute_NotSpecified_Should_Returns_True()
        {
            var c = new DelegateCommand(_ => { });

            c.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void DelegateCommand_CanExecute_Should_Invoke_Lambad()
        {
            bool invoked = false;
            var c = new DelegateCommand(_ => { }, _ =>
            {
                invoked = true;
                return true;
            });

            c.CanExecute(null).Should().BeTrue();
            invoked.Should().BeTrue();
        }

        #endregion

        #region Execute

        [Fact]
        public void DelegateCommand_Execute_AsExpected()
        {
            bool invoked = false;
            var c = new DelegateCommand(_ => invoked = true);

            c.Execute(null);
            invoked.Should().BeTrue();
        }

        #endregion

        #region RaiseCanExecuteChanged

        [Fact]
        public void DelegateCommand_RaiseCanExecuteChanged_Should_Invoke_Listeners()
        {
            bool invoked = false;
            var c = new DelegateCommand(_ => { });
            c.CanExecuteChanged += (s, e) => invoked = true;

            c.RaiseCanExecuteChanged();
            invoked.Should().BeTrue();
        }

        #endregion

    }
}
