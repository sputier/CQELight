using System;
using System.Collections.Generic;
using System.Text;
using CQELight.TestFramework;
using FluentAssertions;
using Xunit;

namespace CQELight.MVVM.Tests
{
    public class MessageDialogServiceOptionsTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region Ctor

        [Fact]
        public void MessageDialogServiceOptions_Ctor_DialogStyle_Should_Be_Info_By_Default()
        {
            var o = new MessageDialogServiceOptions();
            o.DialogStyle.Should().Be(AlertType.Info);
        }

        #endregion

    }
}
