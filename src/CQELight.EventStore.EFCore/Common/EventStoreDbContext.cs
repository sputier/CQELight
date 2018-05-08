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

        private readonly DbContextConfiguration _contextConfiguration;

        #endregion

        #region Ctor

        public EventStoreDbContext(DbContextConfiguration contextConfiguration)
        {
            _contextConfiguration = contextConfiguration ?? throw new ArgumentNullException(nameof(contextConfiguration));
        }

        #endregion

        #region Overriden methods

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            switch (_contextConfiguration.ConfigType)
            {
                case ConfigurationType.SQLite:
                    optionsBuilder.UseSqlite(_contextConfiguration.ConnectionString);
                    break;
                case ConfigurationType.SQLServer:
                default:
                    optionsBuilder.UseSqlServer(_contextConfiguration.ConnectionString);
                    break;
            }
            base.OnConfiguring(optionsBuilder);
        }

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
        }

        #endregion

    }
}
