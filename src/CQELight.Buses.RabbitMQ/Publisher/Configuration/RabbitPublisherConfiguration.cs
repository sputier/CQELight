using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Configuration.Publisher
{
    public class RabbitPublisherConfiguration
    {
        #region Public static methods

        public static RabbitPublisherConfiguration GetDefault(string emiter) =>
            new RabbitPublisherConfiguration
            {
                EventsConfiguration = new[] {
                    new RabbitPublisherConfigurationBuilder()
                        .ForAllEvents()
                        .UseExchange(emiter + "_events")
                },
                CommandsConfiguration = new[] {
                    new RabbitPublisherConfigurationBuilder()
                        .ForAllCommands()
                        .UseExchange(emiter + "_commands")
                }
            };

        #endregion

        #region Properties

        public IEnumerable<RabbitPublisherEventConfiguration> EventsConfiguration { get; internal set; }

        public IEnumerable<RabbitPublisherCommandConfiguration> CommandsConfiguration { get; internal set; }

        #endregion
    }
}
