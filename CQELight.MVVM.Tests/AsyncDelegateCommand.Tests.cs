using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.MVVM.Tests
{
    public class AsyncDelegateCommandTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region Ctor

        [Fact]
        public void AsyncDelegateCommand_Ctor_TestParams()
        {
            Assert.Throws<ArgumentNullException>(() => new AsyncDelegateCommand(null));
        }

        #endregion

        #region CanExecute

        [Fact]
        public void AsyncDelegateCommand_CanExecute_NotSpecified_Should_Returns_True()
        {
            var c = new AsyncDelegateCommand(_ => Task.CompletedTask);

            c.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void AsyncDelegateCommand_CanExecute_Should_Invoke_Lambad()
        {
            bool invoked = false;
            var c = new AsyncDelegateCommand(_ => Task.CompletedTask, _ =>
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
        public async Task AsyncDelegateCommand_Execute_AsExpected()
        {
            bool invoked = false;
            var c = new AsyncDelegateCommand(_ =>
            { invoked = true; return Task.CompletedTask; });

            c.Execute(null);
            await Task.Delay(50);
            invoked.Should().BeTrue();
        }

        #endregion

        #region RaiseCanExecuteChanged

        [Fact]
        public void AsyncDelegateCommand_RaiseCanExecuteChanged_Should_Invoke_Listeners()
        {
            bool invoked = false;
            var c = new AsyncDelegateCommand(_ => Task.CompletedTask);
            c.CanExecuteChanged += (s, e) => invoked = true;

            c.RaiseCanExecuteChanged();
            invoked.Should().BeTrue();
        }

        #endregion

    }
}
