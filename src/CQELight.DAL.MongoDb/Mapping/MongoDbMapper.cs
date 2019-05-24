using CQELight.DAL.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.DAL.MongoDb.Mapping
{
    internal static class MongoDbMapper
    {

        #region Static properties

        static ConcurrentBag<MappingInfo> _mappings = new ConcurrentBag<MappingInfo>();

        #endregion

        #region Static methods

        public static MappingInfo GetMapping<T>()
            where T : IPersistableEntity
        {
            var mapping = _mappings.FirstOrDefault(m => m.EntityType == typeof(T));
            if(mapping == null)
            {
                mapping = new MappingInfo(typeof(T));
            }
            return mapping;
        }

        #endregion

    }
}
