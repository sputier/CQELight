using CQELight.EventStore.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore.Common
{
    internal class EventStoreDbContext : DbContext
    {
        #region Ctor

        public EventStoreDbContext(DbContextOptions contextOptions)
            :base(contextOptions)
        {
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
        }

        #endregion

    }
}
