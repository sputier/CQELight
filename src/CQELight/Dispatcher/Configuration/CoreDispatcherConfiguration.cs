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

        #region Overriden methods

        /// <summary>
        /// Express the whole configuration as string for debug purposes.
        /// </summary>
        /// <returns>Configuration as string.</returns>
        public override string ToString()
        {
            StringBuilder config = new StringBuilder();
            foreach (var configData in EventDispatchersConfiguration)
            {
                config.Append($"Event of type {configData.Key.FullName} : ");
                var i = 0;
                foreach (var dispatchData in configData.Value)
                {
                    i++;
                    config.AppendLine($" --- Configuration n° {i} ----");
                    try
                    {
                        config.AppendLine($" -> Dispatch on {dispatchData.BusType.FullName}");
                        config.AppendLine($" -> Error handler defined ? {(dispatchData.ErrorHandler != null ? "yes" : "no")}");
                        config.AppendLine($" -> Serialize events with : {dispatchData.Serializer?.GetType().FullName}");
                    }
                    catch
                    {
                        //Exception ignored because no need to handle it when expressing if it happens
                    }
                }
            }
            return config.ToString();
        }

        #endregion

    }
}
