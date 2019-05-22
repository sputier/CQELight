using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.DAL.MongoDb
{
    public class MongoDbOptions
    {
        #region Properties

        public MongoUrl Url { get; private set; }

        #endregion

        #region Ctor

        public MongoDbOptions(params string[] serversUrls)
            : this(new MongoUrlBuilder
            {
                Servers = serversUrls.Select(u => new MongoServerAddress(u))
            }.ToMongoUrl())
        {

        }

        public MongoDbOptions(string username, string password, params string[] serversUrls)
            : this(new MongoUrlBuilder
            {
                Servers = serversUrls.Select(u => new MongoServerAddress(u)),
                Username = username,
                Password = password
            }.ToMongoUrl())
        {

        }

        public MongoDbOptions(MongoUrl url)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        #endregion

    }
}
