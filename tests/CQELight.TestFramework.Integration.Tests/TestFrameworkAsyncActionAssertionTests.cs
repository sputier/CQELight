using CQELight.Abstractions.Dispatcher;
using CQELight.Dispatcher;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.TestFramework.Integration.Tests
{
    public class TestFrameworAsynckActionAssertionTests : BaseUnitTestClass
    {

        #region Ctor & members

        private class MessageOne : IMessage { }
        private class MessageTwo : IMessage { }

        private class Tester
        {
            public async Task TestNoMessage()
            {
                return;
            }

            public Task TestMessageOne()
            {
                return CoreDispatcher.DispatchMessageAsync(new MessageOne());
            }

            public Task TestMessageTwo()
            {
                return CoreDispatcher.DispatchMessageAsync(new MessageTwo());
            }

            public async Task TestBothMessages()
            {
                await CoreDispatcher.DispatchMessageAsync(new MessageOne());
                await CoreDispatcher.DispatchMessageAsync(new MessageTwo());
            }
        }

        #endregion

        #region ThenNoMessageShouldBeRaised

        [Fact]
        public async Task Test_ThenNoMessagesShouldBeRaised_MessageRaised_Should_Throw_TestFramewrokException()
        {
            bool throwExc = false;
            try
            {
                await Test.WhenAsync(() => new Tester().TestMessageOne()).ThenNoMessageShouldBeRaised(200);
            }
            catch (TestFrameworkException)
            {
                throwExc = true;
            }
            throwExc.Should().BeTrue();
        }

        [Fact]
        public async Task Test_ThenNoMessagesShouldBeRaised_NoMessageRaised_AsExpected()
        {
            await Test.WhenAsync(() => new Tester().TestNoMessage()).ThenNoMessageShouldBeRaised(200);
        }

        #endregion

        #region ThenMessageShouldBeRaised

        [Fact]
        public async Task Test_ThenMessageShouldBeRaised_NoMessageRaised_Should_Throw_TestFramewrokException()
        {
            bool throwExc = false;
            try
            {
                await Test.WhenAsync(() => new Tester().TestNoMessage()).ThenMessageShouldBeRaised<MessageOne>(200);
            }
            catch (TestFrameworkException)
            {
                throwExc = true;
            }
            throwExc.Should().BeTrue();
        }

        [Fact]
        public async Task Test_ThenNoMessagesShouldBeRaised_MessageRaised_NotSameType_Should_Throw_TestFramewrokException()
        {
            bool throwExc = false;
            try
            {
                await Test.WhenAsync(() => new Tester().TestMessageTwo()).ThenMessageShouldBeRaised<MessageOne>(200);
            }
            catch (TestFrameworkException)
            {
                throwExc = true;
            }
            throwExc.Should().BeTrue();
        }

        [Fact]
        public async Task Test_ThenNoMessagesShouldBeRaised_MessageRaised_AsExpected()
        {
            var message = await Test.WhenAsync(() => new Tester().TestMessageOne()).ThenMessageShouldBeRaised<MessageOne>(200);
            message.Should().NotBeNull();
            message.Should().BeOfType<MessageOne>();
        }

        #endregion

        #region ThenMessagesShouldBeRaised

        [Fact]
        public async Task Test_ThenMessagesShouldBeRaised_NoMessageRaised_Should_Throw_TestFramewrokException()
        {
            bool throwExc = false;
            try
            {
                await Test.WhenAsync(() => new Tester().TestNoMessage()).ThenMessagesShouldBeRaised(200);
            }
            catch (TestFrameworkException)
            {
                throwExc = true;
            }
            throwExc.Should().BeTrue();
        }

        [Fact]
        public async Task Test_ThenMessagesShouldBeRaised_AsExpected()
        {
            var messages = await Test.WhenAsync(() => new Tester().TestBothMessages()).ThenMessagesShouldBeRaised(200);
            messages.Should().HaveCount(2);
            messages.Any(m => m.GetType() == typeof(MessageOne)).Should().BeTrue();
            messages.Any(m => m.GetType() == typeof(MessageTwo)).Should().BeTrue();
        }

        #endregion

    }
}
