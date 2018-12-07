using CQELight.EventStore.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore.Common
{
    internal class EventStoreDbContext : DbContext
    {

        #region Members

        private readonly SnapshotEventsArchiveBehavior _behavior;

        #endregion

        #region Ctor

        public EventStoreDbContext(DbContextOptions contextOptions)
            : base(contextOptions)
        {
            _behavior = SnapshotEventsArchiveBehavior.StoreToNewTable;
        }

        public EventStoreDbContext(DbContextOptions contextOptions, SnapshotEventsArchiveBehavior behavior)
            : base(contextOptions)
        {
            _behavior = behavior;
        }

        #endregion

        #region Overriden methods

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var evtModel = modelBuilder.Entity<Event>();

            evtModel.HasIndex(e => new { e.AggregateId, e.AggregateType });

            evtModel.ToTable("EVT_T_EVENT");
            evtModel.HasKey(e => e.Id);
            evtModel.Property(e => e.Id).HasColumnName("EVT_ID");
            evtModel.Property(e => e.AggregateId).HasColumnName("EVT_AGG_ID");
            evtModel.Property(e => e.AggregateType).HasColumnName("EVT_AGG_TYPE").HasMaxLength(1024);
            evtModel.Property(e => e.EventData).HasColumnName("EVT_DATA").IsRequired();
            evtModel.Property(e => e.EventType).HasColumnName("EVT_TYPE").IsRequired().HasMaxLength(1024);
            evtModel.Property(e => e.EventTime).HasColumnName("EVT_TIMESTAMP").IsRequired();
            evtModel.Property(e => e.Sequence).HasColumnName("EVT_SEQUENCE").IsRequired();

            var snapModel = modelBuilder.Entity<Snapshot>();

            snapModel.HasIndex(e => new { e.AggregateId, e.AggregateType });

            snapModel.ToTable("SNP_T_SNAPSHOT");
            snapModel.HasKey(e => e.Id);
            snapModel.Property(e => e.Id).HasColumnName("SNP_ID");
            snapModel.Property(e => e.AggregateId).HasColumnName("SNP_AGG_ID");
            snapModel.Property(e => e.AggregateType).HasColumnName("SNP_AGG_TYPE").HasMaxLength(1024);
            snapModel.Property(e => e.SnapshotData).HasColumnName("SNP_DATA").IsRequired();
            snapModel.Property(e => e.SnapshotTime).HasColumnName("SNP_TIME").IsRequired();
            snapModel.Ignore(e => e.AggregateState);

            if (_behavior == SnapshotEventsArchiveBehavior.StoreToNewTable)
            {
                var evtArchiveModel = modelBuilder.Entity<ArchiveEvent>();

                evtArchiveModel.HasIndex(e => new { e.AggregateId, e.AggregateType });

                evtArchiveModel.ToTable("EVA_T_EVENT_ARCHIVE");
                evtArchiveModel.HasKey(e => e.Id);
                evtArchiveModel.Property(e => e.Id).HasColumnName("EVA_ID");
                evtArchiveModel.Property(e => e.AggregateId).HasColumnName("EVA_AGG_ID");
                evtArchiveModel.Property(e => e.AggregateType).HasColumnName("EVA_AGG_TYPE").HasMaxLength(1024);
                evtArchiveModel.Property(e => e.EventData).HasColumnName("EVA_DATA").IsRequired();
                evtArchiveModel.Property(e => e.EventType).HasColumnName("EVA_TYPE").IsRequired().HasMaxLength(1024);
                evtArchiveModel.Property(e => e.EventTime).HasColumnName("EVA_TIMESTAMP").IsRequired();
                evtArchiveModel.Property(e => e.Sequence).HasColumnName("EVA_SEQUENCE").IsRequired();
            }
        }

        #endregion

    }
}
