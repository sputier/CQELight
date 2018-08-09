using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Buses.MSMQ.Client
{
    /// <summary>
    /// Configuration for MSMQ client bus.
    /// </summary>
    public sealed class MSMQClientBusConfiguration
    {

        #region Static members

        /// <summary>
        /// Default configuration that targets localhost for messaging.
        /// </summary>
        public static MSMQClientBusConfiguration Default
            => new MSMQClientBusConfiguration();

        /// <summary>
        /// Collection of relation between event type and lifetime.
        /// </summary>
        public IEnumerable<(Type Type, TimeSpan Expiration)> EventsLifetime { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new client configuration on a MSMQ server.
        /// </summary>
        /// <param name="eventsLifetime">Collection of relation between event type and lifetime. You should fill this collection to 
        /// indicates expiration date for some events. Default value is 7 days.</param>
        public MSMQClientBusConfiguration(IEnumerable<(Type, TimeSpan)> eventsLifetime = null)
        {
            EventsLifetime = eventsLifetime ?? Enumerable.Empty<(Type, TimeSpan)>();
        }

        #endregion

    }
}
