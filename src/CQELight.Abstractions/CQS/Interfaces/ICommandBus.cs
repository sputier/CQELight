using System.Threading.Tasks;

namespace CQELight.Abstractions.CQS.Interfaces
{
    /// <summary>
    /// Contrat interface for dispatching Commands.
    /// </summary>
    public interface ICommandBus
    {
        /// <summary>
        /// Dispatch command asynchrounously.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="context">Context associated to command.</param>
        Task DispatchAsync(ICommand command, ICommandContext context = null);
        /// <summary>
        /// Dispatch command synchrounously.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="context">Context associated to command.</param>
        void Dispatch(ICommand command, ICommandContext context = null);
    }
}
