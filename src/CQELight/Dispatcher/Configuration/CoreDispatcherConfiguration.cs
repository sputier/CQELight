using CQELight.Dispatcher.Configuration.Internal;
using CQELight.Events.Serializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Dispatcher.Configuration
{
    /// <summary>
    /// Configuration instance to use for the dispatcher.
    /// </summary>
    public class CoreDispatcherConfiguration
    {

        #region Members

        /// <summary>
        /// Default instance of the configuration.
        /// </summary>
        private static CoreDispatcherConfiguration _default;

        #endregion

        #region Properties

        /// <summary>
        /// Internal collection of configuration associated to a type.
        /// </summary>
        internal ConcurrentDictionary<Type, ICollection<EventDispatchConfiguration>> EventDispatchersConfiguration { get; } =
            new ConcurrentDictionary<Type, ICollection<EventDispatchConfiguration>>();


        #endregion

        #region Public static members

        /// <summary>
        /// Default instance of the configuration.
        /// This default configuration map every events within the system with all available bus, use Json as default serializer,
        /// simply ignores errors.
        /// </summary>
        public static CoreDispatcherConfiguration Default
        {
            get
            {
                if (_default == null)
                {
                    var builder = new CoreDispatcherConfigurationBuilder();
                    builder.ForAllEvents().UseAllAvailableBuses().SerializeWith<JsonEventSerializer>();
                    _default = builder.Build();
                }
                return _default;
            }
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Constructeur internal.
        /// </summary>
        internal CoreDispatcherConfiguration()
        {

        }

        #endregion
    }
}
