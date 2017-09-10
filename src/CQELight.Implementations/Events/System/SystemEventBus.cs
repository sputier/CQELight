using CQELight.Abstractions.Events.Interfaces;
using CQELight.Implementations.Events.InMemory.Stateless;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Implementations.Events.System
{
    /// <summary>
    /// System-wide bus to dispatch event between clients through a system bus instance.
    /// </summary>
    public class SystemEventBus : IDomainEventBus, IConfigurableBus<SystemEventBusConfiguration>, IDisposable
    {

        #region Members

        /// <summary>
        /// Bus configuration.
        /// </summary>
        private SystemEventBusConfiguration _config;
        /// <summary>
        /// Communication with client.
        /// </summary>
        private static NamedPipeClientStream _outCommunicationStream;
        /// <summary>
        /// Flag that indicates if bus has already be started.
        /// </summary>
        private static bool s_alreadyInitialized;
        /// <summary>
        /// Instance of the bus.
        /// </summary>
        private static SystemEventBus _instance;
        /// <summary>
        /// Thread safety.
        /// </summary>
        private static object s_lockObject = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Current instance
        /// </summary>
        internal static SystemEventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (_instance == null) 
                        {
                            _instance = new SystemEventBus();
                        }
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Flag to indicates if bus is currently running.
        /// </summary>
        public bool Working
        {
            get;
            private set;
        }
        #endregion

        #region Ctor

        /// <summary>
        /// Default ctor.
        /// </summary>
        private SystemEventBus()
        {
            if (s_alreadyInitialized)
            {
                throw new InvalidOperationException("SystemBusClient.ctor() : SystemBus can be instantiate only once.");
            }
            s_alreadyInitialized = true;
        }

        #endregion

        #region IConfigurableBus methods
        
        /// <summary>
        /// Apply the configuration to the bus.
        /// </summary>
        /// <param name="config">Bus configuration.</param>
        public void Configure(SystemEventBusConfiguration config)
            => _config = config ?? throw new ArgumentNullException(nameof(config), "DatabaseEventBus.Configure() : Configuration must be provided.");


        #endregion

        #region IDomainEventBus
        
        /// <summary>
        /// Register synchronously an event to be processed by the bus.
        /// </summary>
        /// <param name="event">Event to register.</param>
        /// <param name="context">Context associated to the event..</param>
        public void Register(IDomainEvent @event, IEventContext context = null)
            => RegisterAsync(@event, context).GetAwaiter().GetResult();
        
        /// <summary>
        /// Register asynchronously an event to be processed by the bus.
        /// </summary>
        /// <param name="event">Event to register.</param>
        /// <param name="context">Context associated to the event..</param>
        public Task RegisterAsync(IDomainEvent @event, IEventContext context = null)
        {
            lock (s_lockObject)
            {
                if (Working)
                {
                    if (_outCommunicationStream == null || !_outCommunicationStream.IsConnected)
                    {
                        throw new InvalidOperationException("SystemBusClient.RegisterAsync() : Bus must be started before.");
                    }
                    var lifetime = _config.TypeLifetime.FirstOrDefault(t => t.Key == @event.GetType()).Value;
                    var evtEnveloppe = new EventEnveloppe
                    {
                        ContextType = context?.GetType()?.AssemblyQualifiedName ?? string.Empty,
                        EventContextData = context != null ? context.ToJson() : string.Empty,
                        EventData = @event.ToJson(),
                        EventTime = @event.EventTime,
                        EventType = @event.GetType().AssemblyQualifiedName,
                        PeremptionDate = DateTime.Now.AddMilliseconds(lifetime != 0 ? lifetime : TimeSpan.FromDays(7).TotalMilliseconds),
                        Sender = _config.Id,
                        Receiver = null // TODO 
                    };
                    evtEnveloppe.ToJson().WriteToStream(_outCommunicationStream);
                    _outCommunicationStream.WaitForPipeDrain();
                    var serverResponse = _outCommunicationStream.ReadString();
                    if (serverResponse != Consts.CONST_SYSTEM_BUS_WELL_RECEIVED_TOKEN)
                    {
                        // todo ?
                    }
                }
            }
            return Task.FromResult(0);
        }
        
        /// <summary>
        /// Start the bus.
        /// </summary>
        public void Start()
        {
            lock (s_lockObject)
            {
                if (Working)
                {
                    return;
                }
                Working = true;
                if (_config == null)
                {
                    Configure(SystemEventBusConfiguration.Default);
                }
                using (var auth = new NamedPipeClientStream(".", Consts.CONST_SYSTEM_BUS_AUTH_PIPE_NAME, PipeDirection.InOut))
                {
                    auth.Connect();
                    auth.ReadMode = PipeTransmissionMode.Message;

                    var authKey = auth.ReadString();
                    if (authKey == Consts.CONST_SYSTEM_BUS_AUTH_KEY) 
                    {
                        if (new ClientInfos { ClientID = _config.Id, ClientName = _config.Name }.ToJson().WriteToStream(auth) > 0)
                        {
                            auth.WaitForPipeDrain();
                            if (auth.ReadString() == Consts.CONST_SYSTEM_BUS_WELL_RECEIVED_TOKEN)
                            {
                                _outCommunicationStream = new NamedPipeClientStream(".", $"{Consts.CONST_SYSTEM_BUS_DEDICATED_PIPE_PREFIX}{_config.Id}", PipeDirection.InOut);
                                _outCommunicationStream.Connect();
                                _outCommunicationStream.ReadMode = PipeTransmissionMode.Message;
                                if (_outCommunicationStream.ReadString() == Consts.CONST_SYSTEM_BUS_READY)
                                {
                                    Task.Run(() => WaitForEvents());
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Stop the bus.
        /// </summary>
        public void Stop()
        {
            Working = false;
            _outCommunicationStream.Dispose();
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        public void Dispose()
        {
            Working = false;
            s_alreadyInitialized = false;
            _outCommunicationStream?.Dispose();
        }


        #endregion

        #region Private methods

        /// <summary>
        /// Create a listening pipe for the server to send us events.
        /// </summary>
        private void WaitForEvents()
        {
            using (var pipeServer = new NamedPipeServerStream($"{Consts.CONST_SYSTEM_BUS_SYSTEM_EVENT_BUS_SERVER_NAME}{_config.Id}", PipeDirection.InOut, -1, PipeTransmissionMode.Message))
            {
                pipeServer.WaitForConnection();
                while (Working)
                {
                    var data = pipeServer.ReadString();
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        var evtData = data.FromJson<EventEnveloppe>();
                        var evt = evtData.EventData.FromJson(Type.GetType(evtData.EventType)) as IDomainEvent;
                        IEventContext ctx = null;
                        if (!string.IsNullOrWhiteSpace(evtData.ContextType) && !string.IsNullOrWhiteSpace(evtData.EventContextData))
                        {
                            ctx = evtData.EventContextData.FromJson(Type.GetType(evtData.ContextType)) as IEventContext;
                        }
                        (new InMemoryStatelessEventBus()).RegisterAsync(evt, ctx);
                    }
                }
            }
        }

        #endregion

    }
}
