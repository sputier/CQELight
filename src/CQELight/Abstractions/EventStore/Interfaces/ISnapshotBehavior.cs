using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions.EventStore.Interfaces
{
    /// <summary>
    /// Contract interface for making snapshot upon an event store.
    /// </summary>
    public interface ISnapshotBehavior
    {

        /// <summary>
        /// Generate a new snapshot based on the aggregate id and the aggregate type.
        /// </summary>
        /// <param name="aggregateId">Id of the aggregate.</param>
        /// <param name="newSequence">New value of sequence.</param>
        /// <param name="aggregateType">Type of the aggregate.</param>
        /// <returns>A new snapshot instance and the new sequence for events.</returns>
        Task<(ISnapshot Snapshot, int NewSequence)> GenerateSnapshotAsync(Guid aggregateId, Type aggregateType);

        /// <summary>
        /// Get the info if a snapshot is needed, based on the aggregate id and the aggregate type.
        /// </summary>
        /// <param name="aggregateId">Id of the aggregate.</param>
        /// <param name="aggregateType">Type of the aggregate.</param>
        /// <returns>True if a snapshot should be created, false otherwise.</returns>
        Task<bool> IsSnapshotNeededAsync(Guid aggregateId, Type aggregateType);
    }
}
