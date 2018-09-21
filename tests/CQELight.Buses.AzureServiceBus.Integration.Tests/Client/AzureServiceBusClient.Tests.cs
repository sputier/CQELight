using CQELight.Abstractions.Configuration;
using CQELight.Abstractions.Events;
using CQELight.Buses.AzureServiceBus.Client;
using CQELight.Configuration;
using CQELight.TestFramework;
using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Moq;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Buses.AzureServiceBus.Integration.Tests.Client
{
    public class AzureServiceBusClientTests : BaseUnitTestClass
    {

        #region Nested class

        private class AzureEvent : BaseDomainEvent
        {
            public string Data { get; set; }
        }

        #endregion

        #region Ctor & members

        private Mock<IAppIdRetriever> _appIdMock;
        private IConfiguration _configuration;

        public AzureServiceBusClientTests()
        {
            _appIdMock = new Mock<IAppIdRetriever>();
            _appIdMock.Setup(c => c.GetAppId()).Returns(AppId.Generate());
            _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        }

        #endregion

        #region PublishEventAsync

        [Fact]
        public async Task AzureServiceBusClient_PublishEvent_AsExpected()
        {
            var queueClient = new QueueClient(_configuration["ConnectionString"], "cqelight");

            var client = new AzureServiceBusClient(_appIdMock.Object, queueClient, new AzureServiceBusClientConfiguration(
                _configuration["ConnectionString"], null));

            await client.PublishEventAsync(new AzureEvent { Data = "test event data" });

            bool hasCorrectlyReceived = false;

            queueClient.RegisterMessageHandler((m, c) =>
            {
                hasCorrectlyReceived = m.ContentType == typeof(AzureEvent).AssemblyQualifiedName
                && m.Body != null;

                var evt = Encoding.UTF8.GetString(m.Body).FromJson<AzureEvent>();

                hasCorrectlyReceived &= evt != null && evt.Data == "test event data";

                return Task.CompletedTask;
            }, new MessageHandlerOptions(e =>
            {
                hasCorrectlyReceived = false;
                return Task.CompletedTask;
            }));

            int elapsedTime = 0;
            while (!hasCorrectlyReceived && elapsedTime < 2000)
            {
                elapsedTime += 50;
                await Task.Delay(50);
            }

            hasCorrectlyReceived.Should().BeTrue();
        }

        #endregion

    }
}
