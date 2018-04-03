using CQELight.MVVM.Interfaces;
using CQELight.TestFramework;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.MVVM.Tests
{
    public class BaseViewModelTests : BaseUnitTestClass
    {

        #region Ctor & members

        private Mock<IView> _viewMock;
private class TestViewModel : BaseViewModel
        {
            public TestViewModel(IView view)
                :base(view)
            {

            }
        }
        

        public BaseViewModelTests()
        {
            _viewMock = new Mock<IView>();
        }

        #endregion

        #region Cancel

        [Fact]
        public void BaseViewModel_Cancel_Should_Close_Window()
        {
            var vm = new TestViewModel(_viewMock.Object);

            vm.Cancel();

            _viewMock.Verify(v => v.Close(), Times.Once());
        }

        [Fact]
        public void BaseViewModel_Cancel_Should_Close_Window_Through_Command()
        {
            var vm = new TestViewModel(_viewMock.Object);

            vm.CancelCommand.Execute(null);

            _viewMock.Verify(v => v.Close(), Times.Once());
        }

        #endregion

        #region OnCloseAsync

        [Fact]
        public async Task BaseViewModel_OnCloseAsync_Should_ReturnsTrue_ByDefault()
        {
            var vm = new TestViewModel(_viewMock.Object);
            (await vm.OnCloseAsync().ConfigureAwait(false)).Should().BeTrue();
        }

        #endregion

    }
}
