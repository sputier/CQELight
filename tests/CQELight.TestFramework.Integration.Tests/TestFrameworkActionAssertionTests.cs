using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Dispatcher;
using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Dispatcher;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.TestFramework.Integration.Tests
{
    public class TestFrameworkActionAssertionTests : BaseUnitTestClass
    {

        #region Ctor & members


        private Mock<IDispatcher> _dispatcherMock = new Mock<IDispatcher>();

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

        #region Command Dispatch

        class Cmd : ICommand { }

        class CommandTest
        {
            public Task DispatchAsync()
            {
                return CoreDispatcher.DispatchCommandAsync(new Cmd());
            }
            public Task DispatchAsync_WithMock(IDispatcher mock)
            {
                return mock.DispatchCommandAsync(new Cmd());
            }
        }

        class MultipleCommandTest
        {
            public async Task DispatchRangeAsync()
            {
                await CoreDispatcher.DispatchCommandAsync(new Cmd());
                await CoreDispatcher.DispatchCommandAsync(new Cmd());
                await CoreDispatcher.DispatchCommandAsync(new Cmd());
            }
            public async Task DispatchRangeAsync_WithMock(IDispatcher mock)
            {
                await mock.DispatchCommandAsync(new Cmd());
                await mock.DispatchCommandAsync(new Cmd());
                await mock.DispatchCommandAsync(new Cmd());
            }
        }

        [Fact]
        public void ThenCommandIsDispatched_AsExpected()
        {
            var cmd = Test.When(() => new CommandTest().DispatchAsync().GetAwaiter().GetResult()).ThenCommandIsDispatched<Cmd>();
            cmd.Should().NotBeNull();

            _dispatcherMock = new Mock<IDispatcher>();
            var cmd2 = Test.When(() =>
                    new CommandTest().DispatchAsync_WithMock(_dispatcherMock.Object).GetAwaiter().GetResult(), _dispatcherMock)
                    .ThenCommandIsDispatched<Cmd>();
            cmd2.Should().NotBeNull();
            _dispatcherMock.Verify(m => m.DispatchCommandAsync(It.IsAny<ICommand>(), It.IsAny<ICommandContext>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void ThenCommandIsDispatched_Should_Throw_If_NoCommandIsDispatched()
        {
            Assert.Throws<TestFrameworkException>(() => Test.When(() => Task.Delay(1).GetAwaiter().GetResult()).ThenCommandIsDispatched<Cmd>());
            Assert.Throws<TestFrameworkException>(() => Test.When(() => Task.Delay(1).GetAwaiter().GetResult()).ThenCommandsAreDispatched());
        }

        [Fact]
        public void ThenNoCommandAreDispatched_AsExpected()
        {
            Test.When(() => Task.Delay(1).GetAwaiter().GetResult()).ThenNoCommandAreDispatched();

            _dispatcherMock = new Mock<IDispatcher>();
            Test.When(() =>
                    Task.Delay(1).GetAwaiter().GetResult(), _dispatcherMock)
                    .ThenNoCommandAreDispatched();
            _dispatcherMock.Verify(m => m.DispatchCommandAsync(It.IsAny<ICommand>(), It.IsAny<ICommandContext>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public void ThenNoCommandAredispatch_Should_Throw_If_Command_IsDispatcher()
        {
            Assert.Throws<TestFrameworkException>(() => Test.When(() => new CommandTest().DispatchAsync().GetAwaiter().GetResult()).ThenNoCommandAreDispatched());

            _dispatcherMock = new Mock<IDispatcher>();
            Assert.Throws<TestFrameworkException>(() => Test.When(() => new CommandTest().DispatchAsync_WithMock(_dispatcherMock.Object).GetAwaiter().GetResult(), _dispatcherMock).ThenNoCommandAreDispatched());
        }

        [Fact]
        public void ThenCommandAreDispatched_AsExpected()
        {
            var cmds = Test.When(() => new MultipleCommandTest().DispatchRangeAsync().GetAwaiter().GetResult()).ThenCommandsAreDispatched();
            cmds.Should().NotBeNull();
            cmds.Should().HaveCount(3);


            _dispatcherMock = new Mock<IDispatcher>();
            var cmds2 = Test.When(() =>
                     new MultipleCommandTest().DispatchRangeAsync_WithMock(_dispatcherMock.Object).GetAwaiter().GetResult(), _dispatcherMock)
                    .ThenCommandsAreDispatched();
            cmds2.Should().NotBeNull();
            cmds.Should().HaveCount(3);
            _dispatcherMock.Verify(m => m.DispatchCommandAsync(It.IsAny<ICommand>(), It.IsAny<ICommandContext>(), It.IsAny<string>()), Times.Exactly(3));
        }

        #endregion

        #region Event Dispatch

        class Evt : BaseDomainEvent { }
        class Evt2 : BaseDomainEvent { }
        class Evt3 : BaseDomainEvent { }

        class EventTest
        {
            public Task PublishAsync()
            {
                return CoreDispatcher.PublishEventAsync(new Evt());
            }
            public Task PublishAsync_WithMock(IDispatcher mock)
            {
                return mock.PublishEventAsync(new Evt());
            }
        }

        class MultipleEventTest
        {
            public async Task PublishEventRangeAsync()
            {
                await CoreDispatcher.PublishEventAsync(new Evt());
                await CoreDispatcher.PublishEventAsync(new Evt2());
                await CoreDispatcher.PublishEventAsync(new Evt3());
            }
            public async Task PublishEventRangeAsync_WithMock(IDispatcher mock)
            {
                await mock.PublishEventAsync(new Evt());
                await mock.PublishEventAsync(new Evt2());
                await mock.PublishEventAsync(new Evt3());
            }
        }

        [Fact]
        public void ThenEventShouldBeRaised_AsExpected()
        {
            var evt = Test.When(() => new EventTest().PublishAsync().GetAwaiter().GetResult()).ThenEventShouldBeRaised<Evt>();
            evt.Should().NotBeNull();

            _dispatcherMock = new Mock<IDispatcher>();
            var evt2 = Test.When(() =>
                new EventTest().PublishAsync_WithMock(_dispatcherMock.Object).GetAwaiter().GetResult(), _dispatcherMock).ThenEventShouldBeRaised<Evt>();
            evt2.Should().NotBeNull();
            _dispatcherMock.Verify(m => m.PublishEventAsync(It.IsAny<IDomainEvent>(), It.IsAny<IEventContext>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void ThenEventShouldBeRaised_Should_Throw_IfNoEvent_Is_Raised()
        {
            Assert.Throws<TestFrameworkException>(() => Test.When(() => Task.Delay(1).GetAwaiter().GetResult()).ThenEventShouldBeRaised<Evt>());
            Assert.Throws<TestFrameworkException>(() => Test.When(() => Task.Delay(1).GetAwaiter().GetResult()).ThenEventsShouldBeRaised());
        }

        [Fact]
        public void ThenNoEventShouldBeRaised_AsExpected()
        {
            Test.When(() => Task.Delay(1).GetAwaiter().GetResult()).ThenNoEventShouldBeRaised();

            _dispatcherMock = new Mock<IDispatcher>();
            Test.When(() => Task.Delay(1).GetAwaiter().GetResult(), _dispatcherMock).ThenNoEventShouldBeRaised();
        }

        [Fact]
        public void ThenNoEventShouldBeRaised_Should_Throw_If_No_Event_AreRaised()
        {
            Assert.Throws<TestFrameworkException>(() => Test.When(() => new EventTest().PublishAsync().GetAwaiter().GetResult()).ThenNoEventShouldBeRaised());

            _dispatcherMock = new Mock<IDispatcher>();
            Assert.Throws<TestFrameworkException>(() => Test.When(() => new EventTest().PublishAsync_WithMock(_dispatcherMock.Object).GetAwaiter().GetResult(), _dispatcherMock).ThenNoEventShouldBeRaised());
        }

        [Fact]
        public void ThenEventsShouldBeRaised_AsExpected()
        {
            var cmds = Test.When(() => new MultipleEventTest().PublishEventRangeAsync().GetAwaiter().GetResult()).ThenEventsShouldBeRaised();
            cmds.Should().NotBeNull();
            cmds.Should().HaveCount(3);

            _dispatcherMock = new Mock<IDispatcher>();
            var cmds2 = Test.When(() => new MultipleEventTest().PublishEventRangeAsync_WithMock(_dispatcherMock.Object).GetAwaiter().GetResult(), _dispatcherMock).ThenEventsShouldBeRaised();
            cmds2.Should().NotBeNull();
            cmds2.Should().HaveCount(3);
            _dispatcherMock.Verify(m => m.PublishEventAsync(It.IsAny<IDomainEvent>(), It.IsAny<IEventContext>(), It.IsAny<string>()), Times.Exactly(3));
        }

        [Fact]
        public void ThenEventShouldBeRaised_PublishRange_AsExpected()
        {
            var evt = Test.When(() => new MultipleEventTest().PublishEventRangeAsync().GetAwaiter().GetResult()).ThenEventShouldBeRaised<Evt>();
            evt.Should().NotBeNull();

            _dispatcherMock = new Mock<IDispatcher>();
            var evt2 = Test.When(() =>
                new EventTest().PublishAsync_WithMock(_dispatcherMock.Object).GetAwaiter().GetResult(), _dispatcherMock).ThenEventShouldBeRaised<Evt>();
            evt2.Should().NotBeNull();
            _dispatcherMock.Verify(m => m.PublishEventAsync(It.IsAny<IDomainEvent>(), It.IsAny<IEventContext>(), It.IsAny<string>()), Times.Once());
        }

        #endregion

    }
}
