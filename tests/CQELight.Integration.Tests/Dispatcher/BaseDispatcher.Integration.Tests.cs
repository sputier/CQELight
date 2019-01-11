using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events;
using CQELight.Dispatcher;
using CQELight.TestFramework;
using CQELight.TestFramework.IoC;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Integration.Tests.Dispatcher
{
    public class BaseDispatcherTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region DispatchEventAsync

        private class TestEvent : BaseDomainEvent { }

        [Fact]
        public async Task BaseDispatcher_DispatchEventAsync_Should_Not_Throw_Exception_If_NoConfiguration_IsDefined_And_LogWarning()
        {
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(m => m.CreateLogger(It.IsAny<string>()))
                .Returns(loggerMock.Object);
            var fakeScopeFactory = new TestScopeFactory(new TestScope(new Dictionary<Type, object>
            {
                {typeof(ILoggerFactory), loggerFactoryMock.Object }
            }));

            bool coreDispatcherCalledWithoutSecurityCritical = false;

            var d = new BaseDispatcher(new CQELight.Dispatcher.Configuration.DispatcherConfiguration(false), fakeScopeFactory);

            var evt = new TestEvent();

            CoreDispatcher.OnEventDispatched += (e) =>
            {
                coreDispatcherCalledWithoutSecurityCritical = object.ReferenceEquals(e, evt);
                return Task.CompletedTask;
            };

            //Shouldn't throw exception
            await d.PublishEventAsync(evt).ConfigureAwait(false);

            loggerMock.Verify(m => m.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
            coreDispatcherCalledWithoutSecurityCritical.Should().BeTrue();
        }

        #endregion

        #region PublishCommandAsync

        private class TestCommand : ICommand { }

        [Fact]
        public async Task BaseDispatcher_PublishCommandAsync_Should_Not_Throw_Exception_If_NoConfiguration_IsDefined_And_LogWarning()
        {

            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(m => m.CreateLogger(It.IsAny<string>()))
                .Returns(loggerMock.Object);
            var fakeScopeFactory = new TestScopeFactory(new TestScope(new Dictionary<Type, object>
            {
                {typeof(ILoggerFactory), loggerFactoryMock.Object }
            }));

            bool coreDispatcherCalledWithoutSecurityCritical = false;
            var d = new BaseDispatcher(new CQELight.Dispatcher.Configuration.DispatcherConfiguration(false), fakeScopeFactory);

            var command = new TestCommand();
            CoreDispatcher.OnCommandDispatched += (c) =>
            {
                coreDispatcherCalledWithoutSecurityCritical = object.ReferenceEquals(c, command);
                return Task.CompletedTask;
            };

            //Shouldn't throw exception
            await d.DispatchCommandAsync(command).ConfigureAwait(false);

            loggerMock.Verify(m => m.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
            coreDispatcherCalledWithoutSecurityCritical.Should().BeTrue();
        }

        #endregion

    }
}
