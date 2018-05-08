using CQELight.Configuration;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Common
{
    internal class Enveloppe
    {
        #region Properties

        public string Data { get; set; }
        public string AssemblyQualifiedDataType { get; set; }
        public bool PersistentMessage { get; set; }
        public TimeSpan Expiration { get; set; }
        public AppId Emiter { get; set; }

        #endregion

        #region Ctor

        public Enveloppe() { }

        public Enveloppe(object data, AppId emiter, bool persistent = false, TimeSpan? expiration = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            Data = data.ToJson();
            AssemblyQualifiedDataType = data.GetType().AssemblyQualifiedName;
            PersistentMessage = persistent;
            Expiration = expiration ?? TimeSpan.FromDays(1);
            Emiter = emiter;
        }

        #endregion

    }
}
