using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight
{
    /// <summary>
    /// Enumeration of bootstrapper service types.
    /// </summary>
    public enum BootstrapperServiceType
    {
        /// <summary>
        /// ioc management service's type
        /// </summary>
        IoC = 0,
        /// <summary>
        /// Bus dispatching service's type
        /// </summary>
        Bus = 1,
        /// <summary>
        /// DAL persistence layer service's type
        /// </summary>
        DAL = 2,
        /// <summary>
        /// Event store service's type
        /// </summary>
        EventStore = 3,
        /// <summary>
        /// Other type of service
        /// </summary>
        Other
    }

    /// <summary>
    /// A contract interface that defines a bootstrapper service.
    /// </summary>
    public interface IBootstrapperService
    {
        /// <summary>
        /// The type of the current service.
        /// </summary>
        BootstrapperServiceType ServiceType { get; }
        /// <summary>
        /// Bootrapping action for this specific service, with a bootstrapping context.
        /// </summary>
        Action<BootstrappingContext> BootstrappAction { get; }
    }
}
