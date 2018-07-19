using CQELight.Abstractions.Events.Interfaces;
using Geneao.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Geneao.Handlers.Events
{
    class PersonneAjouteeEventHandler : IDomainEventHandler<PersonneAjouteeEvent>
    {
        public Task HandleAsync(PersonneAjouteeEvent domainEvent, IEventContext context = null)
        {
            // Add to database
            return Task.CompletedTask;
        }
    }

}
