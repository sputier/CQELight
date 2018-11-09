using CQELight.Abstractions.Dispatcher;
using CQELight.Configuration;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses
{
    /// <summary>
    /// Base class to hold data for sending messages to third party message queues.
    /// </summary>
    public class Enveloppe
    {
        #region Properties

        /// <summary>
        /// Serialized data of the message.
        /// </summary>
        public string Data { get; private set; }
        /// <summary>
        /// Assembly qualified type of the serialized data.
        /// </summary>
        public string AssemblyQualifiedDataType { get; private set; }
        /// <summary>
        /// Flag that indicates if the message is persistent or not.
        /// </summary>
        public bool PersistentMessage { get; private set; }
        /// <summary>
        /// Expiration time span to let know message queue system when a message is ready for 
        /// deletion.
        /// </summary>
        public TimeSpan Expiration { get; private set; }
        /// <summary>
        /// The AppId of the emiter.
        /// </summary>
        public AppId Emiter { get; private set; }

        #endregion

        #region Ctor

        internal Enveloppe() { }

        /// <summary>
        /// Create a new enveloppe that contains data,
        /// which is persistent and have one day expiration.
        /// </summary>
        /// <param name="data">Serialized data object to be inserted into the enveloppe.</param>
        /// <param name="dataType">Type of serialized data</param>
        /// <param name="emiter">AppId of the emiter</param>
        public Enveloppe(string data, Type dataType, AppId emiter)
            : this(data, dataType, emiter, true, TimeSpan.FromDays(1))
        { }

        /// <summary>
        /// Create a new enveloppe that contains data,
        /// with a defined expiration.
        /// </summary>
        /// <param name="data">Serialized data object to be inserted into the enveloppe.</param>
        /// <param name="dataType">Type of serialized data</param>
        /// <param name="emiter">AppId of the emiter</param>
        /// <param name="expiration">Time before message expires.</param>
        public Enveloppe(string data, Type dataType, AppId emiter, TimeSpan expiration)
            : this(data, dataType, emiter, true, expiration)
        { }

        /// <summary>
        /// Create a new enveloppe that contains data,
        /// which have one day expiration.
        /// when any
        /// </summary>
        /// <param name="data">Serialized data object to be inserted into the enveloppe.</param>
        /// <param name="dataType">Type of serialized data</param>
        /// <param name="emiter">AppId of the emiter</param>
        /// <param name="persistent">Persistent flag, that indicates if message should be 
        /// automatically deleted when anyone ack it.</param>
        public Enveloppe(string data, Type dataType, AppId emiter, bool persistent)
            : this(data, dataType, emiter, persistent, TimeSpan.FromDays(1))
        { }

        /// <summary>
        /// Create a new enveloppe with defined informations.
        /// </summary>
        /// <param name="data">Serialized data object to be inserted into the enveloppe.</param>
        /// <param name="dataType">Type of serialized data</param>
        /// <param name="emiter">AppId of the emiter</param> 
        /// <param name="expiration">Time before message expires.</param>
        /// <param name="persistent">Persistent flag, that indicates if message should be 
        /// automatically deleted when anyone ack it.</param>
        public Enveloppe(string data, Type dataType, AppId emiter, bool persistent, TimeSpan expiration)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data));
            }
            Data = data;
            AssemblyQualifiedDataType = dataType.AssemblyQualifiedName;
            PersistentMessage = persistent;
            Expiration = expiration;
            Emiter = emiter;
        }

        #endregion

    }
}
