using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Buses.InMemory.Commands
{
    /// <summary>
    /// In memory command bus configuration.
    /// </summary>
    public class InMemoryCommandBusConfiguration
    {

        #region Static properties

        /// <summary>
        /// Default configuration.
        /// </summary>
        public static InMemoryCommandBusConfiguration Default
            => new InMemoryCommandBusConfiguration(null);


        #endregion

        #region Members

        internal Dictionary<Type, Func<ICommand, bool>> _ifClauses = new Dictionary<Type, Func<ICommand, bool>>();

        #endregion

        #region Properties

        /// <summary>
        /// Callback when no handler for a specific command is found in the same process.
        /// </summary>
        public Action<ICommand, ICommandContext> OnNoHandlerFounds { get; internal set; }
        /// <summary>
        /// Collection of dispatch clauses.
        /// </summary>
        public IEnumerable<KeyValuePair<Type, Func<ICommand, bool>>> IfClauses => _ifClauses.AsEnumerable();

        #endregion

        #region Ctor

        internal InMemoryCommandBusConfiguration()
        {

        }

        private InMemoryCommandBusConfiguration(Action<ICommand, ICommandContext> onNoHandlerFounds)
        {
            OnNoHandlerFounds = onNoHandlerFounds;
        }

        #endregion
    }
}
