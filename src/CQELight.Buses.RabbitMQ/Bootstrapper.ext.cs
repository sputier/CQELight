using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.RabbitMQ.Client;
using CQELight.IoC;
using System.Linq;

namespace CQELight.Buses.RabbitMQ
{
    public static class BootstrapperExtensions
    {

        #region Public static methods

        public static Bootstrapper UseRabbitMQClientBus(this Bootstrapper bootstrapper, RabbitMQClientBusConfiguration configuration = null)
        {
            var service = RabbitMQBootstrappService.Instance;

            service.BootstrappAction += () =>
            {
                bootstrapper.AddIoCRegistrations(
                    new TypeRegistration(typeof(RabbitMQClientBus), typeof(IDomainEventBus)),
                    new TypeRegistration(typeof(RabbitMQClientBus), typeof(ICommandBus)),
                    new InstanceTypeRegistration(configuration ?? RabbitMQClientBusConfiguration.Default,
                        typeof(RabbitMQClientBusConfiguration)));
            };

            if (!bootstrapper.RegisteredServices.Any(s => s == service))
            {
                bootstrapper.AddService(service);
            }
            return bootstrapper;
        }
        
        #endregion

    }
}
