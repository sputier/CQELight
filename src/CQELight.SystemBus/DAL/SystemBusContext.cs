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
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(Implementations.Consts.CONST_CONNECTION_STRING_LOCALDB);
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entEnveloppe = modelBuilder.Entity<EventEnveloppe>();
            entEnveloppe.ToTable("ETR_T_EVENT_TRANSIT");
            entEnveloppe.HasKey(e => e.Id);
            entEnveloppe.Property(e => e.Sender).IsRequired()
                .HasColumnName("ETR_SENDER");
            entEnveloppe.Property(e => e.Receiver).IsRequired(false)
                .HasColumnName("ETR_RECEIVER");
            entEnveloppe.Property(e => e.EventTime).IsRequired()
                .HasColumnName("ETR_TIMESTAMP");
            entEnveloppe.Property(e => e.EventData).IsRequired()
                .HasColumnName("ETR_EVENT_DATA");
            entEnveloppe.Property(e => e.EventType).IsRequired()
                .HasColumnName("ETR_EVENT_TYPE");
            entEnveloppe.Property(e => e.EventContextData).IsRequired(false)
                .HasColumnName("ETR_EVENT_CONTEXT_DATA");
            entEnveloppe.Property(e => e.ContextType).IsRequired(false)
                .HasColumnName("ETR_EVENT_CONTEXT_TYPE");
            entEnveloppe.Property(e => e.PeremptionDate).IsRequired()
                .HasColumnName("ETR_PEREMPTIONDATE");

            var entDispatched = modelBuilder.Entity<DispatchedEvent>();
            entDispatched.ToTable("EDP_T_EVENT_DISPATCHED");
            entDispatched.HasKey(e => e.Id);
            entDispatched.Property(e => e.EventId).IsRequired()
                .HasColumnName("EDP_EVENT_ID");
            entDispatched.Property(e => e.ReceiverId).IsRequired()
                .HasColumnName("EDP_RECEIVER_ID");
        }

        #endregion

    }
}
