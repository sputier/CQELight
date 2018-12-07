using CQELight.EventStore.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore.Common
{
    class ArchiveEventStoreDbContext : DbContext
    {
        #region Ctor

        public ArchiveEventStoreDbContext(DbContextOptions contextOptions)
            : base(contextOptions)
        {
        }

        #endregion

        #region Overriden methods

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

        #endregion

    }
}
