using CQELight.TestFramework.Extensions;
using FluentAssertions;
using System;
using Xunit;

namespace CQELight.TestFramework.MVVM.Tests.Extensions
{
    public class MVVMExtensionsTests
    {

        #region Ctor & members

        [Fact]
        public void GetStandardWindowMock_PerformOnUIThread_Should_ApplyAction()
        {
            var moq = MVVMExtensions.GetStandardWindowMock();

            bool b = false;

            moq.Object.PerformOnUIThread(() => b = true);

            b.Should().BeTrue();
        }

        #endregion

    }
}
