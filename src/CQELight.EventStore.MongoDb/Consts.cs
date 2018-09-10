using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.MongoDb
{
    internal static class Consts
    {
        #region Consts

        public const string CONST_DB_NAME = "CQELight_Events";
        public const string CONST_ARCHIVE_DB_NAME = "CQELight_Events_Archive";
        public const string CONST_EVENTS_COLLECTION_NAME = "events";
        public const string CONST_SNAPSHOT_COLLECTION_NAME = "snapshots";
        public const string CONST_ID_FIELD = "_id";

        #endregion

    }
}
