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
        /// Configuration à utiliser.
        /// </summary>
        private SystemEventBusConfiguration _config;
        /// <summary>
        /// Stream de communication avec le serveur système en sortie.
        /// </summary>
        private static NamedPipeClientStream _outCommunicationStream;
        /// <summary>
        /// Flag indiquant si le bus a déjà été initialisé.
        /// </summary>
        private static bool s_alreadyInitialized;
        /// <summary>
        /// Instance du bus.
        /// </summary>
        private static SystemEventBus _instance;
        /// <summary>
        /// Objet de thread safety.
        /// </summary>
        private static object s_lockObject = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Instance du bus.
        /// </summary>
        internal static SystemEventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (_instance == null) // Le premier entrant l'initialise
                        {
                            _instance = new SystemEventBus();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Flag d'indication de fonctionnement.
        /// </summary>
        public bool Working
        {
            get;
            private set;
        }
        #endregion

        #region Ctor

        /// <summary>
        /// Consturcteur par défaut.
        /// </summary>
        private SystemEventBus()
        {
            if (s_alreadyInitialized)
            {
                throw new InvalidOperationException("SystemBusClient.ctor() : Le bus client System ne peut être instancié qu'une seule fois.");
            }
            s_alreadyInitialized = true;
        }

        #endregion

        #region IConfigurableBus methods

        /// <summary>
        /// Configure le bus avec la configuration donnée.
        /// </summary>
        /// <param name="config">Configuration de bus.</param>
        public void Configure(SystemEventBusConfiguration config)
            => _config = config ?? throw new ArgumentNullException(nameof(config), "DatabaseEventBus.Configure() : La configuration doit être renseignée.");


        #endregion

        #region IDomainEventBus

        /// <summary>
        /// Enregistre un event de façon synchrone dans le bus pour son dispatch selon les règles du bus.
        /// </summary>
        /// <param name="event">Event a dispatcher.</param>
        /// <param name="context">Contexte de l'évement.</param>
        public void Register(IDomainEvent @event, IEventContext context = null)
            => RegisterAsync(@event, context).GetAwaiter().GetResult();

        /// <summary>
        /// Enregistre un event de façon asynchrone dans le bus pour son dispatch selon les règles du bus.
        /// </summary>
        /// <param name="event">Event à enregistrer.</param>
        /// <param name="context">Contexte de l'évement.</param>
        public Task RegisterAsync(IDomainEvent @event, IEventContext context = null)
        {
            lock (s_lockObject)
            {
                if (Working)
                {
                    if (_outCommunicationStream == null || !_outCommunicationStream.IsConnected)
                    {
                        throw new InvalidOperationException("SystemBusClient.RegisterAsync() : Le bus doit d'abord être démarré.");
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
        /// Démarrage du bus.
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
                //Authenficiation avec le bus système.
                using (var auth = new NamedPipeClientStream(".", Consts.CONST_SYSTEM_BUS_AUTH_PIPE_NAME, PipeDirection.InOut))
                {
                    auth.Connect();
                    auth.ReadMode = PipeTransmissionMode.Message;

                    var authKey = auth.ReadString();
                    if (authKey == Consts.CONST_SYSTEM_BUS_AUTH_KEY) // C'est le bon serveur système.
                    {
                        //On envoie ses infos client
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
        /// Arrêt du bus.
        /// </summary>
        public void Stop()
        {
            Working = false;
            _outCommunicationStream.Dispose();
        }

        /// <summary>
        /// Nettoyage des données.
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
        /// Méthode d'attente et d'écoute du serveur.
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
                        InMemoryStatelessEventBus.Instance.Register(evt, ctx);
                    }
                }
            }
        }

        #endregion

    }
}
