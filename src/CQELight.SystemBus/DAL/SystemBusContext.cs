using CQELight.Implementations.Events.System;
using CQELight.SystemBus.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.SystemBus.DAL
{
    /// <summary>
    /// EF DbContext for the system bus app.
    /// </summary>
    public class SystemBusContext : DbContext
    {

        #region Overidden methods

        /// <summary>
        /// Configuration du context.
        /// </summary>
        /// <param name="optionsBuilder">Builder d'option.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(Implementations.Consts.CONST_CONNECTION_STRING_LOCALDB);
        }

        /// <summary>
        /// Création du modèle.
        /// </summary>
        /// <param name="modelBuilder">Builder du modèle.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entEnveloppe = modelBuilder.Entity<EventEnveloppe>();
            entEnveloppe.ToTable("ETR_T_EVENT_TRANSIT");
            entEnveloppe.HasKey(e => e.Id);
            entEnveloppe.Property(e => e.Sender).IsRequired()
                .ForSqliteHasColumnName("ETR_SENDER")
                .ForSqlServerHasColumnName("ETR_SENDER");
            entEnveloppe.Property(e => e.Receiver).IsRequired(false)
                .ForSqliteHasColumnName("ETR_RECEIVER")
                .ForSqlServerHasColumnName("ETR_RECEIVER");
            entEnveloppe.Property(e => e.EventTime).IsRequired()
                .ForSqliteHasColumnName("ETR_TIMESTAMP")
                .ForSqlServerHasColumnName("ETR_TIMESTAMP");
            entEnveloppe.Property(e => e.EventData).IsRequired()
                .ForSqliteHasColumnName("ETR_EVENT_DATA")
                .ForSqlServerHasColumnName("ETR_EVENT_DATA");
            entEnveloppe.Property(e => e.EventType).IsRequired()
                .ForSqliteHasColumnName("ETR_EVENT_TYPE")
                .ForSqlServerHasColumnName("ETR_EVENT_TYPE");
            entEnveloppe.Property(e => e.EventContextData).IsRequired(false)
                .ForSqliteHasColumnName("ETR_EVENT_CONTEXT_DATA")
                .ForSqlServerHasColumnName("ETR_EVENT_CONTEXT_DATA");
            entEnveloppe.Property(e => e.ContextType).IsRequired(false)
                .ForSqliteHasColumnName("ETR_EVENT_CONTEXT_TYPE")
                .ForSqlServerHasColumnName("ETR_EVENT_CONTEXT_TYPE");
            entEnveloppe.Property(e => e.PeremptionDate).IsRequired()
                .ForSqliteHasColumnName("ETR_PEREMPTIONDATE")
                .ForSqlServerHasColumnName("ETR_PEREMPTIONDATE");

            var entDispatched = modelBuilder.Entity<DispatchedEvent>();
            entDispatched.ToTable("EDP_T_EVENT_DISPATCHED");
            entDispatched.HasKey(e => e.Id);
            entDispatched.Property(e => e.EventId).IsRequired()
                .ForSqliteHasColumnName("EDP_EVENT_ID")
                .ForSqlServerHasColumnName("EDP_EVENT_ID");
            entDispatched.Property(e => e.ReceiverId).IsRequired()
                .ForSqliteHasColumnName("EDP_RECEIVER_ID")
                .ForSqlServerHasColumnName("EDP_RECEIVER_ID");
        }

        #endregion

    }
}
