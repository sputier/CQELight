﻿using CQELight.EventStore.EFCore.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore.Integration.Tests
{
    public class EventStoreDbContextCreator : IDesignTimeDbContextFactory<EventStoreDbContext>
    {
        public EventStoreDbContext CreateDbContext(string[] args)
        {
            return new EventStoreDbContext(new DbContextOptionsBuilder<EventStoreDbContext>()
                        .UseSqlite("FileName=events.db", opts => opts.MigrationsAssembly(typeof(EventStoreDbContextCreator).Assembly.GetName().Name))
                        .Options, SnapshotEventsArchiveBehavior.Delete);
        }
    }
}
