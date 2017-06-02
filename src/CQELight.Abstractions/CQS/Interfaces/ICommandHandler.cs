using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions.CQS.Interfaces
{
    /// <summary>
    /// Contract interface for command's handlers.
    /// </summary>
    /// <typeparam name="T">Type of command to handle.</typeparam>
    public interface ICommandHandler<T> where T : ICommand
    {

        /// <summary>
        /// Handle a specific command instance with its context.
        /// </summary>
        /// <param name="command">Command to handle.</param>
        /// <param name="context">Linked context.</param>
        Task HandleAsync(T command, ICommandContext context = null);

    }
}
