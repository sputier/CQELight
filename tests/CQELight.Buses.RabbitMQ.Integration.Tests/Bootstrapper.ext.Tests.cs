using CQELight.IoC;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Buses.RabbitMQ.Integration.Tests
{
    public class BootstrapperExtTests : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region UseRabbitMQClientBus

        [Fact]
        public void UseRabbitMQClientBus_WithIoC_Should_Add_RabbitMQClient_Within_Registrations()
        {
            new Bootstrapper()
                .UseAutofacAsIoC(c => { })
                .UseRabbitMQClientBus(new Client.RabbitMQClientBusConfiguration("test", "localhost", "guest", "guest"))
                .Bootstrapp();

            using(var scope = DIManager.BeginScope())
            {
                var client = scope.Resolve<RabbitMQClient>();
                client.Should().NotBeNull();
                using (var connection = client.GetConnection())
                {
                    connection.IsOpen.Should().BeTrue();
                }
            }
        }

        #endregion

        #region UseRabbitMQServer

        [Fact]
        public void UseRabbitMQServer_WithIoC_Should_Add_RabbitMQClient_Within_Registrations()
        {
            new Bootstrapper()
                .UseAutofacAsIoC(c => { })
                .UseRabbitMQServer(new Server.RabbitMQServerConfiguration("test", "localhost", "guest", "guest", QueueConfiguration.Empty))
                .Bootstrapp();

            using (var scope = DIManager.BeginScope())
            {
                var client = scope.Resolve<RabbitMQClient>();
                client.Should().NotBeNull();
                using (var connection = client.GetConnection())
                {
                    connection.IsOpen.Should().BeTrue();
                }
            }
        }

        #endregion
    }
}
