using CQELight.Bootstrapping.Notifications;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests
{
    public class BootstrapperNotificationTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region Ctor

        [Fact]
        public void When_Creating_A_New_BootstrapperNotification_With_Message_It_Should_Set_ContentType_To_CustomService()
        {
            var n = new BootstrapperNotification(BootstrapperNotificationType.Error, "Error message");

            n.ContentType.Should().Be(BootstapperNotificationContentType.CustomServiceNotification);
        }

        [Fact]
        public void When_Creating_A_New_BootstrapperNotification_Type_Should_Be_Provided()
        {
            Assert.Throws<ArgumentNullException>(() => new BootstrapperNotification(BootstrapperNotificationType.Error,
                "error message", null));
        }

        #endregion

    }
}
