using CQELight.Abstractions.DAL.Interfaces;
using CQELight.DAL.Common;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.DAL.EFCore.Adapters
{
    class EFCoreDataWriterAdapter : DisposableObject, IDataWriterAdapter
    {
        #region Members

        private readonly BaseDbContext dbContext;
        private readonly EFCoreOptions options;

        #endregion

        #region Ctor

        public EFCoreDataWriterAdapter(
            BaseDbContext dbContext,
            EFCoreOptions options = null)
        {
            this.dbContext = dbContext;
            this.options = options;
        }

        #endregion

        #region IDataWriterAdapter

        public Task InsertAsync<T>(T entity) where T : class
        {
            dbContext.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync<T>(T entity) where T : class
        {
            bool CheckIfLogicalDeletionIsDisabled()
            {
                return options?.DisableLogicalDeletion == true
                                && entity.GetType().IsInHierarchySubClassOf(typeof(BasePersistableEntity))
                                && (entity as BasePersistableEntity).Deleted
                                && DateTime.UtcNow.Subtract((entity as BasePersistableEntity).DeletionDate ?? DateTime.MinValue).TotalSeconds < 10;
            }
            if (CheckIfLogicalDeletionIsDisabled())
            {
                return DeleteAsync(entity);
            }
            else
            {
                dbContext.Update(entity);
            }
            return Task.CompletedTask;
        }
        
        public Task DeleteAsync<T>(T entity) where T : class
        {
            dbContext.Remove(entity);
            return Task.CompletedTask;
        }
        public Task<int> SaveAsync()
            => dbContext.SaveChangesAsync();

        #endregion

        #region Overriden methods

        protected override void Dispose(bool disposing)
        {
            try
            {
                dbContext.Dispose();
            }
            catch
            {
                //Don't throw on dispose
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
