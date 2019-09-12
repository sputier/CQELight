using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Configuration.Publisher
{
    public class RabbitPublisherConfigurationBuilder
    {
        #region Members

        private List<Type> _commandTypes;
        private List<Type> _eventTypes;

        private List<RabbitPublisherCommandConfiguration> _commandConfigurations = new List<RabbitPublisherCommandConfiguration>();
        private List<RabbitPublisherEventConfiguration> _eventsConfiguration = new List<RabbitPublisherEventConfiguration>();

        #endregion

        #region Ctor

        public RabbitPublisherConfigurationBuilder()
        {
            var allTypes = ReflectionTools.GetAllTypes().ToList();
            _eventTypes = allTypes.Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract).ToList();
            _commandTypes = allTypes.Where(t => typeof(ICommand).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract).ToList();
        }

        #endregion

        #region Public methods

        #region Command configuration

        public RabbitPublisherCommandConfiguration ForAllCommands()
        {
            var config = new RabbitPublisherCommandConfiguration(_commandTypes.ToArray());
            _commandConfigurations.Add(config);
            return config;
        }

        public RabbitPublisherCommandConfiguration ForCommand<T>() where T : ICommand
        {
            var config = new RabbitPublisherCommandConfiguration(typeof(T));
            _commandConfigurations.Add(config);
            return config;
        }

        public RabbitPublisherCommandConfiguration ForCommands(params Type[] commandsTypes)
        {
            var config = new RabbitPublisherCommandConfiguration(commandsTypes);
            _commandConfigurations.Add(config);
            return config;
        }

        public RabbitPublisherCommandConfiguration ForCommandsInAssembly(string assemblyName)
        {
            var config = new RabbitPublisherCommandConfiguration(_commandTypes.Where(t => t.Assembly.GetName().Name == assemblyName).ToArray());
            _commandConfigurations.Add(config);
            return config;
        }

        public RabbitPublisherCommandConfiguration ForAllOtherCommands()
        {
            var remainingCommandTypes = _commandTypes.Where(t => !_commandConfigurations.SelectMany(c => c.Types).Contains(t));
            var config = new RabbitPublisherCommandConfiguration(remainingCommandTypes.ToArray());
            _commandConfigurations.Add(config);
            return config;
        }

        #endregion

        #region Event configuration

        public RabbitPublisherEventConfiguration ForAllEvents()
        {
            var config = new RabbitPublisherEventConfiguration(_eventTypes.ToArray());
            _eventsConfiguration.Add(config);
            return config;
        }

        public RabbitPublisherEventConfiguration ForEvent<T>() where T : IDomainEvent
        {
            var config = new RabbitPublisherEventConfiguration(typeof(T));
            _eventsConfiguration.Add(config);
            return config;
        }

        public RabbitPublisherEventConfiguration ForEvents(params Type[] eventTypes)
        {
            var config = new RabbitPublisherEventConfiguration(eventTypes);
            _eventsConfiguration.Add(config);
            return config;
        }

        public RabbitPublisherEventConfiguration ForEventsInAssembly(string assemblyName)
        {
            var config = new RabbitPublisherEventConfiguration(_eventTypes.Where(t => t.Assembly.GetName().Name == assemblyName).ToArray());
            _eventsConfiguration.Add(config);
            return config;
        }

        public RabbitPublisherEventConfiguration ForAllOtherEvents()
        {
            var remainingEventTypes = _eventTypes.Where(t => !_eventsConfiguration.SelectMany(c => c.Types).Contains(t));
            var config = new RabbitPublisherEventConfiguration(remainingEventTypes.ToArray());
            _eventsConfiguration.Add(config);
            return config;
        }

        #endregion

        public RabbitPublisherConfiguration GetConfiguration()
            => new RabbitPublisherConfiguration
            {
                CommandsConfiguration = _commandConfigurations.ToList(),
                EventsConfiguration = _eventsConfiguration.ToList()
            };

        #endregion
    }
}
