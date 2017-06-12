using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Implementations.Events.System
{
    /// <summary>
    /// Configuration class for the system bus.
    /// </summary>
    public class SystemEventBusConfiguration : IDomainEventBusConfiguration
    {

        #region Static properties

        /// <summary>
        /// Defaut configuration for the bus.
        /// </summary>
        public static SystemEventBusConfiguration Default =>
#if DEBUG
            new SystemEventBusConfiguration(Guid.NewGuid(), "DEFAULT_NAME");
#else

#endif

        #endregion

        #region Properties

        /// <summary>
        /// Id of memory client for system bus.
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// Name of memory client for system bus.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Association of event type and max lifetime in milliseconds.
        /// </summary>
        public ConcurrentDictionary<Type, ulong> TypeLifetime = new ConcurrentDictionary<Type, ulong>();

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="id">Id of the client.</param>
        /// <param name="name">Name of the client.</param>
        public SystemEventBusConfiguration(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Define life time for a specific event type.
        /// </summary>
        /// <typeparam name="T">Type of event to configure.</typeparam>
        /// <param name="lifetime">Milliseconds lifetime for this event type.</param>
        /// <returns>Current configuration.</returns>
        public SystemEventBusConfiguration HasLifetime<T>(ulong lifetime) where T : IDomainEvent
        {
            TypeLifetime.AddOrUpdate(typeof(T), lifetime, (t, u) => u);
            return this;
        }

        #endregion

    }
}
