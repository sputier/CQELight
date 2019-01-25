using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore.Common
{
    class EventArchiveBehaviorInfos
    {

        #region Properties

        public SnapshotEventsArchiveBehavior ArchiveBehavior { get; set; }
        public DbContextOptions<ArchiveEventStoreDbContext> ArchiveDbContextOptions { get; set; }

        #endregion

    }
}
