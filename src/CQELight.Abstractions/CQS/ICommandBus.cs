using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions
{
    /// <summary>
    /// Contrat interface for dispatching Commands.
    /// </summary>
    public interface ICommandBus
    {
        /// <summary>
        /// Dispatch events asynchronously to all listening handlers.
        /// </summary>
        /// <param name="events">Events to dispatch.</param>
        Task DispatchRangeAsync(IEnumerable<ICommand> events);
        /// <summary>
        /// Dispatch events synchronously to all listening handlers.
        /// </summary>
        /// <param name="events">Events to dispatch.</param>
        void DispatchRagne(IEnumerable<IDomainEvent> events);
        /// <summary>
        /// Dispatch an eveny asynchronously.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task DispatchAsync(IDomainEvent @event);
        /// <summary>
        /// Dispatch an event synchronously.
        /// </summary>
        /// <param name="event"></param>
        void Dispatch(IDomainEvent @event);
    }
}
