using CQELight.Abstractions.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Examples.ConsoleApp.Events
{
    /// <summary>
    /// Simple event that pops up when a client connect.
    /// </summary>
    public class ClientConnectedEvent : BaseDomainEvent
    {

        #region Properties

        /// <summary>
        /// Friendly name of the client.
        /// </summary>
        public string FriendlyName { get; set; }
        /// <summary>
        /// Unique Id of the client.
        /// </summary>
        public Guid Id { get; set; }


        #endregion

    }
}
