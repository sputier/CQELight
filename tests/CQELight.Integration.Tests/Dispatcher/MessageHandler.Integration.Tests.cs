using Autofac;
using CQELight.Abstractions.Dispatcher;
using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Dispatcher;
using CQELight.IoC.Autofac;
using CQELight.TestFramework;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Integration.Tests.Dispatcher
{
    public class MessageHandlerTests : BaseUnitTestClass
    {

        #region Ctor & members

        class TestMessage : IMessage
        {

        }

        class TestMessageHandler : IMessageHandler<TestMessage>
        {

            public static bool IsHandled;

            public static void ResetFlag() => IsHandled = false;

            public TestMessageHandler()
            {
                ResetFlag();
            }

            public Task HandleMessageAsync(TestMessage message)
            {
                IsHandled = true;
                return Task.CompletedTask;
            }
        }
        
        private ContainerBuilder GetBasicBuilder()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new LoggerFactory()).AsImplementedInterfaces();
            return builder;
        }

        #endregion

        #region Ctor

        public MessageHandlerTests()
        {
            TestMessageHandler.ResetFlag();
            new Bootstrapper().UseAutofacAsIoC(GetBasicBuilder());
            CleanRegistrationInDispatcher();
        }

        #endregion

        #region HandleMessageAsync

        [Fact]
        public async Task Dispatcher_MessageHandler_NoHandling_IfNotAdded()
        {
            CleanRegistrationInDispatcher();
            var message = new TestMessage();

            TestMessageHandler.IsHandled.Should().BeFalse();

            await CoreDispatcher.DispatchMessageAsync(message);

            TestMessageHandler.IsHandled.Should().BeFalse();
        }

        [Fact]
        public async Task Dispatcher_MessageHandler_Handling_AsExpected()
        {
            CleanRegistrationInDispatcher();
            var message = new TestMessage();

            CoreDispatcher.AddHandlerToDispatcher(new TestMessageHandler());

            TestMessageHandler.IsHandled.Should().BeFalse();

            await CoreDispatcher.DispatchMessageAsync(message);

            TestMessageHandler.IsHandled.Should().BeTrue();
        }

        [Fact]
        public async Task Dispatcher_MessageHandler_Should_Not_BeAble_ToHandle_AfterRemove()
        {
            CleanRegistrationInDispatcher();
            var message = new TestMessage();

            var h = new TestMessageHandler();
            CoreDispatcher.AddHandlerToDispatcher(h);

            TestMessageHandler.IsHandled.Should().BeFalse();

            await CoreDispatcher.DispatchMessageAsync(message);

            TestMessageHandler.IsHandled.Should().BeTrue();

            TestMessageHandler.ResetFlag();

            CoreDispatcher.RemoveHandlerFromDispatcher(h);

            TestMessageHandler.IsHandled.Should().BeFalse();

            await CoreDispatcher.DispatchMessageAsync(message);

            TestMessageHandler.IsHandled.Should().BeFalse();
        }

        #endregion

    }
}
