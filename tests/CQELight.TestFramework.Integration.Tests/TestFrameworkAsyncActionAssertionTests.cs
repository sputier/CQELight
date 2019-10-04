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
    public class TestFrameworAsynckActionAssertionTests : BaseUnitTestClass
    {

        #region Ctor & members

        private Mock<IDispatcher> _dispatcherMock = new Mock<IDispatcher>();
        private class MessageOne : IMessage { }
        private class MessageTwo : IMessage { }

        private class Tester
        {
            public Task TestNoMessage()
            {
                return Task.CompletedTask;
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

        #region ThenCommandIsDispatched

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
        public async Task ThenCommandIsDispatched_AsExpected()
        {
            var cmd = await Test.WhenAsync(() => new CommandTest().DispatchAsync()).ThenCommandIsDispatched<Cmd>();
            cmd.Should().NotBeNull();

            _dispatcherMock = new Mock<IDispatcher>();
            var cmd2 = await Test.WhenAsync(() =>
                    new CommandTest().DispatchAsync_WithMock(_dispatcherMock.Object), _dispatcherMock)
                    .ThenCommandIsDispatched<Cmd>();
            cmd2.Should().NotBeNull();
            _dispatcherMock.Verify(m => m.DispatchCommandAsync(It.IsAny<ICommand>(), It.IsAny<ICommandContext>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task ThenCommandIsDispatched_Should_Throw_If_NoCommandIsDispatched()
        {
            await Assert.ThrowsAsync<TestFrameworkException>(() => Test.WhenAsync(() => Task.Delay(1)).ThenCommandIsDispatched<Cmd>());
            await Assert.ThrowsAsync<TestFrameworkException>(() => Test.WhenAsync(() => Task.Delay(1)).ThenCommandsAreDispatched());
        }


        [Fact]
        public async Task ThenNoCommandAreDispatched_AsExpected()
        {
            await Test.WhenAsync(() => Task.Delay(1)).ThenNoCommandAreDispatched();

            _dispatcherMock = new Mock<IDispatcher>();
            await Test.WhenAsync(() =>
                    Task.Delay(1), _dispatcherMock)
                    .ThenNoCommandAreDispatched();
            _dispatcherMock.Verify(m => m.DispatchCommandAsync(It.IsAny<ICommand>(), It.IsAny<ICommandContext>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task ThenNoCommandAredispatch_Should_Throw_If_Command_IsDispatcher()
        {
            await Assert.ThrowsAsync<TestFrameworkException>(() => Test.WhenAsync(() => new CommandTest().DispatchAsync()).ThenNoCommandAreDispatched());

            _dispatcherMock = new Mock<IDispatcher>();
            await Assert.ThrowsAsync<TestFrameworkException>(() => Test.WhenAsync(() => new CommandTest().DispatchAsync_WithMock(_dispatcherMock.Object), _dispatcherMock).ThenNoCommandAreDispatched());
        }

        [Fact]
        public async Task ThenCommandAreDispatched_AsExpected()
        {
            var cmds = await Test.WhenAsync(() => new MultipleCommandTest().DispatchRangeAsync()).ThenCommandsAreDispatched();
            cmds.Should().NotBeNull();
            cmds.Should().HaveCount(3);

            _dispatcherMock = new Mock<IDispatcher>();
            var cmds2 = await Test.WhenAsync(() =>
                     new MultipleCommandTest().DispatchRangeAsync_WithMock(_dispatcherMock.Object), _dispatcherMock)
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
        public async Task ThenEventShouldBeRaised_AsExpected()
        {
            var evt = await Test.WhenAsync(() => new EventTest().PublishAsync()).ThenEventShouldBeRaised<Evt>();
            evt.Should().NotBeNull();

            _dispatcherMock = new Mock<IDispatcher>();
            var evt2 = await Test.WhenAsync(() =>
                new EventTest().PublishAsync_WithMock(_dispatcherMock.Object), _dispatcherMock).ThenEventShouldBeRaised<Evt>();
            evt2.Should().NotBeNull();
            _dispatcherMock.Verify(m => m.PublishEventAsync(It.IsAny<IDomainEvent>(), It.IsAny<IEventContext>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task ThenEventShouldBeRaised_Should_Throw_IfNoEvent_Is_Raised()
        {
            await Assert.ThrowsAsync<TestFrameworkException>(() => Test.WhenAsync(() => Task.Delay(1)).ThenEventShouldBeRaised<Evt>());
            await Assert.ThrowsAsync<TestFrameworkException>(() => Test.WhenAsync(() => Task.Delay(1)).ThenEventsShouldBeRaised());
        }

        [Fact]
        public async Task ThenNoEventShouldBeRaised_AsExpected()
        {
            await Test.WhenAsync(() => Task.Delay(1)).ThenNoEventShouldBeRaised();

            _dispatcherMock = new Mock<IDispatcher>();
            await Test.WhenAsync(() => Task.Delay(1), _dispatcherMock).ThenNoEventShouldBeRaised();
        }

        [Fact]
        public async Task ThenNoEventShouldBeRaised_Should_Throw_If_No_Event_AreRaised()
        {
            await Assert.ThrowsAsync<TestFrameworkException>(() => Test.WhenAsync(() => new EventTest().PublishAsync()).ThenNoEventShouldBeRaised());

            _dispatcherMock = new Mock<IDispatcher>();
            await Assert.ThrowsAsync<TestFrameworkException>(() => Test.WhenAsync(() => new EventTest().PublishAsync_WithMock(_dispatcherMock.Object), _dispatcherMock).ThenNoEventShouldBeRaised());
        }

        [Fact]
        public async Task ThenEventsShouldBeRaised_AsExpected()
        {
            var cmds = await Test.WhenAsync(() => new MultipleEventTest().PublishEventRangeAsync()).ThenEventsShouldBeRaised();
            cmds.Should().NotBeNull();
            cmds.Should().HaveCount(3);

            _dispatcherMock = new Mock<IDispatcher>();
            var cmds2 = await Test.WhenAsync(() => new MultipleEventTest().PublishEventRangeAsync_WithMock(_dispatcherMock.Object), _dispatcherMock).ThenEventsShouldBeRaised();
            cmds2.Should().NotBeNull();
            cmds2.Should().HaveCount(3);
            _dispatcherMock.Verify(m => m.PublishEventAsync(It.IsAny<IDomainEvent>(), It.IsAny<IEventContext>(), It.IsAny<string>()), Times.Exactly(3));
        }

        #endregion

    }
}
