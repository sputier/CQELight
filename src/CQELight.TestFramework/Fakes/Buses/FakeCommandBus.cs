using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.DDD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.TestFramework.Fakes.Buses
{
    /// <summary>
    /// Fake command bus used to test commands.
    /// </summary>
    public class FakeCommandBus : ICommandBus
    {
        #region Static members

        internal static FakeCommandBus Instance;

        #endregion

        #region Members

        internal IList<ICommand> _commands = new List<ICommand>();

        #endregion

        #region Properties

        /// <summary>
        /// List of all commands.
        /// </summary>
        public IEnumerable<ICommand> Commands => _commands.AsEnumerable();

        #endregion

        #region Ctor

        public FakeCommandBus(Result expectedResult)
        {
            Instance = this;
        }

        #endregion

        #region ICommandBus

        /// <summary>
        /// Dispatch command asynchrounously.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="context">Context associated to command.</param>
        /// <returns>List of launched tasks from handler.</returns>
        public Task<Result> DispatchAsync(ICommand command, ICommandContext context = null)
        {
            _commands.Add(command);
            return Task.FromResult(Result.Ok());
        }

        #endregion

    }
}
