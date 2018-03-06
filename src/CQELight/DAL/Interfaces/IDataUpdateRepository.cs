using CQELight.DAL.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.DAL.Interfaces
{
    /// <summary>
    /// Contrat interface for repositories that performs data updates.
    /// </summary>
    public interface IDataUpdateRepository<T>
        where T : BaseDbEntity
    {
        /// <summary>
        /// Asynchronously saves modifications into repository.
        /// </summary>
        /// <returns>Number of modifications performed.</returns>
        Task<int> SaveAsync();
        /// <summary>
        /// Mark an entity for insertion.
        /// </summary>
        /// <param name="entity">Entity to insert.</param>
        void MarkForInsert(T entity);
        /// <summary>
        /// Mark an entity for update.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        void MarkForUpdate(T entity);
        /// <summary>
        /// Mark an entity for deletion.
        /// </summary>
        /// <param name="entityToDelete">Entity to delete.</param>
        /// <param name="physicalDeletion">Flag to use physical deletion.</param>
        void MarkForDelete(T entityToDelete, bool physicalDeletion = false);
    }
}
