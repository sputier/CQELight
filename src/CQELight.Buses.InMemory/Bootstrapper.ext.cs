using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.InMemory.Commands;
using CQELight.Buses.InMemory.Events;
using CQELight.Dispatcher;
using CQELight.IoC;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.InMemory
{
    public static class BootstrapperExt
    {

        /// <summary>
        /// Configure the bootstrapper to use InMemory buses for dispatching events.
        /// </summary>
        /// <param name="bootstrapper">Instance of boostrapper.</param>
        /// <param name="configuration">Configuration to use for in memory event bus.</param>
        public static Bootstrapper UseInMemoryEventBus(this Bootstrapper bootstrapper, InMemoryEventBusConfiguration configuration = null)
        {
            var config = configuration ?? InMemoryEventBusConfiguration.Default;
            bootstrapper.AddIoCRegistration(new FactoryRegistration(() => {
                var bus = new InMemoryEventBus();
                bus.Configure(config);
                return bus;
            }, typeof(IDomainEventBus), typeof(InMemoryEventBus)));
            return bootstrapper;
        }
        /// <summary>
        /// Configure the bootstrapper to use InMemory buses for dispatching commands.
        /// </summary>
        /// <param name="bootstrapper">Instance of boostrapper.</param>
        /// <param name="configuration">Configuration to use for in memory command bus.</param>
        public static Bootstrapper UseInMemoryCommandBus(this Bootstrapper bootstrapper, InMemoryCommandBusConfiguration configuration = null)
        {
            var config = configuration ?? InMemoryCommandBusConfiguration.Default;
            bootstrapper.AddIoCRegistration(new FactoryRegistration(() => {
                var bus = new InMemoryCommandBus();
                bus.Configure(config);
                return bus;
            }, typeof(ICommandBus), typeof(InMemoryCommandBus)));
            return bootstrapper;
        }

    }
}
