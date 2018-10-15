using CQELight.Abstractions.Dispatcher;
using CQELight.Dispatcher;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CQELight.TestFramework.Integration.Tests
{
    public class TestFrameworkActionAssertionTests : BaseUnitTestClass
    {

        #region Ctor & members

        private class MessageOne : IMessage { }
        private class MessageTwo : IMessage { }

        private class Tester
        {
            public void TestNoMessage()
            {
                return;
            }

            public void TestMessageOne()
            {
                CoreDispatcher.DispatchMessageAsync(new MessageOne()).GetAwaiter().GetResult();
            }

            public void TestMessageTwo()
            {
                CoreDispatcher.DispatchMessageAsync(new MessageTwo()).GetAwaiter().GetResult();
            }

            public void TestBothMessages()
            {
                CoreDispatcher.DispatchMessageAsync(new MessageOne()).GetAwaiter().GetResult();
                CoreDispatcher.DispatchMessageAsync(new MessageTwo()).GetAwaiter().GetResult();
            }
        }

        #endregion

        #region ThenNoMessageShouldBeRaised

        [Fact]
        public void Test_ThenNoMessagesShouldBeRaised_MessageRaised_Should_Throw_TestFramewrokException()
        {
            bool throwExc = false;
            try
            {
                Test.When(() => new Tester().TestMessageOne()).ThenNoMessageShouldBeRaised(200);
            }
            catch (TestFrameworkException)
            {
                throwExc = true;
            }
            throwExc.Should().BeTrue();
        }

        [Fact]
        public void Test_ThenNoMessagesShouldBeRaised_NoMessageRaised_AsExpected()
        {
            Test.When(() => new Tester().TestNoMessage()).ThenNoMessageShouldBeRaised(200);
        }

        #endregion

        #region ThenMessageShouldBeRaised

        [Fact]
        public void Test_ThenMessageShouldBeRaised_NoMessageRaised_Should_Throw_TestFramewrokException()
        {
            bool throwExc = false;
            try
            {
                Test.When(() => new Tester().TestNoMessage()).ThenMessageShouldBeRaised<MessageOne>(200);
            }
            catch (TestFrameworkException)
            {
                throwExc = true;
            }
            throwExc.Should().BeTrue();
        }

        [Fact]
        public void Test_ThenNoMessagesShouldBeRaised_MessageRaised_NotSameType_Should_Throw_TestFramewrokException()
        {
            bool throwExc = false;
            try
            {
                Test.When(() => new Tester().TestMessageTwo()).ThenMessageShouldBeRaised<MessageOne>(200);
            }
            catch (TestFrameworkException)
            {
                throwExc = true;
            }
            throwExc.Should().BeTrue();
        }

        [Fact]
        public void Test_ThenNoMessagesShouldBeRaised_MessageRaised_AsExpected()
        {
            var message = Test.When(() => new Tester().TestMessageOne()).ThenMessageShouldBeRaised<MessageOne>(200);
            message.Should().NotBeNull();
            message.Should().BeOfType<MessageOne>();
        }

        #endregion

        #region ThenMessagesShouldBeRaised

        [Fact]
        public void Test_ThenMessagesShouldBeRaised_NoMessageRaised_Should_Throw_TestFramewrokException()
        {
            bool throwExc = false;
            try
            {
                Test.When(() => new Tester().TestNoMessage()).ThenMessagesShouldBeRaised(200);
            }
            catch (TestFrameworkException)
            {
                throwExc = true;
            }
            throwExc.Should().BeTrue();
        }

        [Fact]
        public void Test_ThenMessagesShouldBeRaised_AsExpected()
        {
            var messages = Test.When(() => new Tester().TestBothMessages()).ThenMessagesShouldBeRaised(200);
            messages.Should().HaveCount(2);
            messages.Any(m => m.GetType() == typeof(MessageOne)).Should().BeTrue();
            messages.Any(m => m.GetType() == typeof(MessageTwo)).Should().BeTrue();
        }

        #endregion

    }
}
