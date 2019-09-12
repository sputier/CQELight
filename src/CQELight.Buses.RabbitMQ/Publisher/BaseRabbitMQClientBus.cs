using CQELight.Abstractions.Dispatcher;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using RabbitMQ.Client;

namespace CQELight.Buses.RabbitMQ.Publisher
{
    /// <summary>
    /// Abstract base class for RabbitMQPublisher bus.
    /// </summary>
    public abstract class BaseRabbitMQPublisherBus
    {
        #region Members

        protected RabbitPublisherBusConfiguration Configuration { get; }

        protected ILogger Logger { get; }

        protected IDispatcherSerializer Serializer { get; }

        #endregion

        #region Ctor

        public BaseRabbitMQPublisherBus(
            IDispatcherSerializer serializer,
            RabbitPublisherBusConfiguration configuration,
            ILoggerFactory loggerFactory = null)
        {
            if (loggerFactory == null)
            {
                loggerFactory = new LoggerFactory();
                loggerFactory.AddProvider(new DebugLoggerProvider());
            }
            Logger = loggerFactory.CreateLogger<BaseRabbitMQPublisherBus>();
            Configuration = configuration;
            Serializer = serializer;
        }

        #endregion

        #region Protected methods

        protected IBasicProperties GetBasicProperties(IModel channel, Enveloppe env)
        {
            IBasicProperties props = channel.CreateBasicProperties();
            props.ContentType = "text/json";
            props.DeliveryMode = (byte)(env.PersistentMessage ? 2 : 1);
            props.Type = env.AssemblyQualifiedDataType;
            return props;
        }

        protected IConnection GetConnection() => Configuration.ConnectionFactory.CreateConnection();

        #endregion

    }
}
