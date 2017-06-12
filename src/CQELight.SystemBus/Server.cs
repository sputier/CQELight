using CQELight.SystemBus.DAL;
using CQELight.SystemBus.DAL.Models;
using System;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CQELight.Implementations.Events.System;
using CQELight.Tools.Extensions;

namespace CQELight.SystemBus
{
    /// <summary>
    /// Server class to manage system-wide bus.
    /// </summary>
    public class Server
    {

        #region Properties

        /// <summary>
        /// Flag to indicate working.
        /// </summary>
        public bool Working { get; private set; }

        #endregion

        #region Members

        /// <summary>
        /// Cancel token source.
        /// </summary>
        private CancellationTokenSource _cancelSource;
        /// <summary>
        /// Association of Guid with communication as client.
        /// </summary>
        private ConcurrentDictionary<Guid, NamedPipeClientStream> _clientStreams =
            new ConcurrentDictionary<Guid, NamedPipeClientStream>();
        /// <summary>
        /// Collection of current dispatched events.
        /// </summary>
        private ConcurrentBag<DispatchedEvent> _dispatchedEvents = new ConcurrentBag<DispatchedEvent>();

        #endregion

        #region Public methods

        /// <summary>
        /// Begin server work.
        /// </summary>>
        public async Task Run()
        {
            _cancelSource = new CancellationTokenSource();
            Working = true;
            await Task.Run(() =>
            {
                Console.WriteLine("Working");
                while (Working)
                {
                    using (var pipeServer = new NamedPipeServerStream(Implementations.Consts.CONST_SYSTEM_BUS_AUTH_PIPE_NAME, PipeDirection.InOut, -1, PipeTransmissionMode.Message))
                    {
                        pipeServer.WaitForConnection();
                        Console.WriteLine("Client connected");
                        try
                        {
                            Implementations.Consts.CONST_SYSTEM_BUS_AUTH_KEY.WriteToStream(pipeServer); // Connexion
                            pipeServer.WaitForPipeDrain();
                            //Récupération des infos clients
                            var clientInfos = pipeServer.ReadString();

                            if (!string.IsNullOrWhiteSpace(clientInfos))
                            {
                                var infos = clientInfos.FromJson<ClientInfos>();
                                Console.WriteLine($"Client {infos.ClientName}, ID {infos.ClientID} authenticates");
                                Task.Run(() => HandleClient(infos), _cancelSource.Token).ConfigureAwait(false); // démarrage sur task dédiée.
                                Task.Run(() => SendAllEventsToClient(infos), _cancelSource.Token).ConfigureAwait(false);
                                Implementations.Consts.CONST_SYSTEM_BUS_WELL_RECEIVED_TOKEN.WriteToStream(pipeServer);
                                pipeServer.WaitForPipeDrain();
                                CreateOutConnectionWithClient(infos);
                            }
                            else
                            {
                                Console.WriteLine("Client send empty auth values...");
                            }
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine("ERROR: {0}", e.Message);
                        }
                    }
                }

            }, _cancelSource.Token).ConfigureAwait(false);
        }


        /// <summary>
        /// Arrêt du fonctionnement du serveur.
        /// </summary>
        public void Stop()
        {
            _cancelSource.Cancel();
            Working = false;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Méthode de gestion des flux d'un client donné.
        /// </summary>
        private void HandleClient(ClientInfos infos)
        {
            Console.WriteLine($"Creating communication thread with client {infos.ClientName}");
            var pipeServer = new NamedPipeServerStream($"{Implementations.Consts.CONST_SYSTEM_BUS_DEDICATED_PIPE_PREFIX}{infos.ClientID}", PipeDirection.InOut, -1, PipeTransmissionMode.Message);
            pipeServer.WaitForConnection();
            Implementations.Consts.CONST_SYSTEM_BUS_READY.WriteToStream(pipeServer);
            pipeServer.WaitForPipeDrain();
            while (Working)
            {
                try
                {
                    //Récupération des infos clients
                    var data = pipeServer.ReadString();
                    Console.WriteLine($"Received data: {data}");
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        Implementations.Consts.CONST_SYSTEM_BUS_WELL_RECEIVED_TOKEN.WriteToStream(pipeServer);
                        var evtData = data.FromJson<EventEnveloppe>();
                        if (evtData == null)
                        {
                            Console.WriteLine("Client send data that was not TransitEvent !");
                        }
                        else
                        {
                            PersistData(evtData);
                            SendEventToClients(evtData);
                        }
                    }
                    else
                    {
                        if (!pipeServer.IsConnected)
                        {
                            Console.WriteLine($"Client {infos.ClientID} disconnected...");
                            if (_clientStreams.TryRemove(infos.ClientID, out NamedPipeClientStream val))
                            {
                                break;
                            }
                            else
                            {
                                Console.WriteLine($"Cannot remove client {infos.ClientID}!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Client send empty data...");
                        }
                    }

                }
                catch (IOException e)
                {
                    Console.WriteLine("ERROR: {0}", e.Message);
                    if (_clientStreams.TryRemove(infos.ClientID, out NamedPipeClientStream val))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Cannot remove client {infos.ClientID}!");
                    }
                }
            }
        }

        /// <summary>
        /// Envoies les évenements actuellement en BDD au client donné.
        /// </summary>
        private void SendAllEventsToClient(ClientInfos infos)
        {
            using (var dbCtx = new SystemBusContext())
            {
                foreach (var evt in dbCtx.Set<EventEnveloppe>().Where(c => c.PeremptionDate > DateTime.Now).ToList())
                {
                    SendEventToClientStream(evt, infos.ClientID);
                }
            }
        }

        /// <summary>
        /// Crée une connexion sortante vers un client afin de lui envoyer les events.
        /// </summary>
        /// <param name="cli">Client.</param>
        private void CreateOutConnectionWithClient(ClientInfos cli)
        {
            var connection = new NamedPipeClientStream(".", $"{Implementations.Consts.CONST_SYSTEM_BUS_SYSTEM_EVENT_BUS_SERVER_NAME}{cli.ClientID}", PipeDirection.InOut);
            connection.Connect();
            connection.ReadMode = PipeTransmissionMode.Message;

            if (connection.IsConnected)
            {
                _clientStreams.AddOrUpdate(cli.ClientID, connection, (id, co) => co);
            }
        }

        /// <summary>
        /// Envoie l'évenement aux clients.
        /// </summary>
        /// <param name="evtData">Données de l'évenement à envoyer.</param>
        private void SendEventToClients(EventEnveloppe evtData)
        {
            if (evtData.PeremptionDate <= DateTime.Today)
            {
                Console.WriteLine($"Event id {evtData.Id} was perempted. Deletion.");
                RemoveEvent(evtData);
                return;
            }
            if (evtData.Receiver.HasValue)
            {
                SendEventToClientStream(evtData, evtData.Receiver.Value);
                Console.WriteLine($"Event id {evtData.Id} send to receiver. Deletion.");
                RemoveEvent(evtData);
            }
            else // Tout les clients
            {
                _clientStreams.DoForEach(c => SendEventToClientStream(evtData, c.Key));
            }
        }

        /// <summary>
        /// Envoi de l'évenement sur le stream du client
        /// </summary>
        /// <param name="evtData">Event à envoyer.</param>
        /// <param name="clientId">Id du client.</param>
        private void SendEventToClientStream(EventEnveloppe evtData, Guid clientId)
        {
            if (evtData.Sender == clientId || _dispatchedEvents.Any(d => d.ReceiverId == clientId && d.EventId == evtData.Id))
            {
                return;
            }
            var client = _clientStreams.FirstOrDefault(c => c.Key == clientId);
            if (client.Value != null)
            {
                try
                {
                    var dispatchValue = new DispatchedEvent { DispatchTimestamp = DateTime.Now, EventId = evtData.Id, ReceiverId = clientId };
                    evtData.ToJson().WriteToStream(client.Value);
                    _dispatchedEvents.Add(dispatchValue);
                    PersistData(dispatchValue);
                }
                catch
                {
                    _clientStreams.TryRemove(clientId, out NamedPipeClientStream val);
                }
            }
            else
            {
                _clientStreams.TryRemove(clientId, out NamedPipeClientStream val);
            }
        }

        /// <summary>
        /// Supprimes l'évenement de la base de données.
        /// </summary>
        /// <param name="evtData">Données de l'évenement à supprimer.</param>
        private void RemoveEvent(EventEnveloppe evtData)
        {
            using (var dbCtx = new SystemBusContext())
            {
                dbCtx.RemoveRange(dbCtx.Set<DispatchedEvent>().Where(d => d.EventId == evtData.Id));
                dbCtx.Remove(evtData);
                dbCtx.SaveChanges();
            }
        }

        /// <summary>
        /// Sauvegarde une donnée en base de données.
        /// </summary>
        /// <param name="obj">Evénement ou infos de dispatch.</param>
        private void PersistData(object obj)
        {
            using (var dbCtx = new SystemBusContext())
            {
                dbCtx.Add(obj);
                dbCtx.SaveChanges();
            }
        }

        #endregion

    }
}
