using CQELight.DAL.Common;
using CQELight.DAL.Interfaces;
using CQELight.DAL.MongoDb.Mapping;
using CQELight.Tools.Extensions;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CQELight.DAL.MongoDb
{
    public class MongoRepository<T> : IDatabaseRepository<T>
        where T : IPersistableEntity
    {
        #region Members

        private readonly MappingInfo _mappingInfo;
        private ConcurrentBag<T> _toInsert = new ConcurrentBag<T>();
        private ConcurrentBag<T> _toUpdate = new ConcurrentBag<T>();
        private ConcurrentBag<T> _toDelete = new ConcurrentBag<T>();

        #endregion

        #region Properties

        protected MongoClient MongoClient => MongoDbContext.MongoClient;

        #endregion

        #region Ctor

        public MongoRepository()
        {
            _mappingInfo = MongoDbMapper.GetMapping<T>();
        }

        #endregion

        #region Private methods

        private IMongoCollection<T> GetCollection()
            => MongoDbContext
                .MongoClient
                .GetDatabase(_mappingInfo.DatabaseName)
                .GetCollection<T>(_mappingInfo.CollectionName);

        #endregion

        #region IDatabaseRepository methods

        public void Dispose()
        {
        }

        public IAsyncEnumerable<T> GetAsync(Expression<Func<T, bool>> filter = null,
                                            Expression<Func<T, object>> orderBy = null,
                                            bool includeDeleted = false,
                                            params Expression<Func<T, object>>[] includes)
        {
            var collection = GetCollection();
            FilterDefinition<T> whereFilter = FilterDefinition<T>.Empty;
            FilterDefinition<T> deletedFilter = FilterDefinition<T>.Empty;
            if (filter != null)
            {
                whereFilter = new FilterDefinitionBuilder<T>().Where(filter);
            }
            if (!includeDeleted)
            {
                deletedFilter = new FilterDefinitionBuilder<T>().Eq("Deleted", false);
            }
            var result = collection
                .Find(new FilterDefinitionBuilder<T>().And(whereFilter, deletedFilter));
            IOrderedFindFluent<T, T> sortedResult = null;
            if (orderBy != null)
            {
                sortedResult = result.SortBy(orderBy);
            }
            return (sortedResult ?? result)
                .ToEnumerable()
                .ToAsyncEnumerable();
        }

        public async Task<T> GetByIdAsync<TId>(TId value)
        {
            var collection = GetCollection();
            var filterBuilder = Builders<T>.Filter;
            string idFieldName = string.Empty;
            if (typeof(T).IsInHierarchySubClassOf(typeof(PersistableEntity)))
            {
                idFieldName = nameof(PersistableEntity.Id);
            }
            var filter = filterBuilder.Eq(idFieldName, value);
            return (await collection.FindAsync(filter)).FirstOrDefault();
        }

        public void MarkForDelete(T entityToDelete, bool physicalDeletion = false)
        {
            if (entityToDelete is BasePersistableEntity bpe)
            {
                if (physicalDeletion)
                {
                    _toDelete.Add(entityToDelete);
                }
                else
                {
                    bpe.Deleted = true;
                    bpe.DeletionDate = DateTime.Now;
                }
            }
            else
            {
                if (physicalDeletion)
                {
                    _toDelete.Add(entityToDelete);
                }
                else
                {
                    throw new InvalidOperationException("Soft deletion of custom entities are not supported yet.");
                }
            }
        }

        public void MarkForDeleteRange(IEnumerable<T> entitiesToDelete, bool physicalDeletion = false)
            => entitiesToDelete.DoForEach(e => MarkForDelete(e, physicalDeletion));

        public void MarkForInsert(T entity)
        {
            if (entity is BasePersistableEntity bpe)
            {
                bpe.EditDate = DateTime.Now;
            }
            _toInsert.Add(entity);
        }

        public void MarkForInsertRange(IEnumerable<T> entities)
            => entities.DoForEach(MarkForInsert);

        public void MarkForUpdate(T entity)
        {
            if (entity is BasePersistableEntity bpe)
            {
                bpe.EditDate = DateTime.Now;
            }
            _toUpdate.Add(entity);
        }

        public void MarkForUpdateRange(IEnumerable<T> entities)
            => entities.DoForEach(MarkForUpdate);

        public void MarkIdForDelete<TId>(TId id, bool physicalDeletion = false)
        {
            var entity = GetByIdAsync(id).GetAwaiter().GetResult();
            MarkForDelete(entity, physicalDeletion);
        }

        public async Task<int> SaveAsync()
        {
            var collection = GetCollection();
            var actions = _toInsert.Count + _toDelete.Count + _toUpdate.Count;

            await collection.InsertManyAsync(_toInsert);

            _toInsert = new ConcurrentBag<T>();
            _toDelete = new ConcurrentBag<T>();
            _toUpdate = new ConcurrentBag<T>();

            return actions;
        }

        #endregion
    }
}
