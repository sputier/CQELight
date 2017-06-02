using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Dispatcher.Configuration.Internal
{
    /// <summary>
    /// Helping class to help building configuration.
    /// </summary>
    internal class EventDispatchConfigurationBuilder
    {

        #region Properties

        /// <summary>
        /// Types of bus concerned by this configuration.
        /// </summary>
        public Type[] BusTypes { get; set; }
        /// <summary>
        /// Type of event serializer.
        /// </summary>
        public Type SerializerType { get; set; }
        /// <summary>
        /// Handler to fire in case of exception.
        /// </summary>
        public Action<Exception> ErrorHandler { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Build properties into a collection of configuration for each bus.
        /// </summary>
        /// <param name="scope">IoC Scope to use to retrieve bus instance from type.</param>
        /// <returns>Collection of configuration.</returns>
        public IEnumerable<EventDispatchConfiguration> Build(IScope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope), "EventDispatchConfigurationBuilder.Build() : Scope need to be provided to build configuration.");
            return BusTypes.Select(b =>
                  new EventDispatchConfiguration
                  {
                      Bus = (IDomainEventBus)scope.Resolve(b)
                  });
        }

        #endregion

    }
}
