using CQELight.Abstractions.Events.Interfaces;
using Geneao.Events;
using System;
using System.Threading.Tasks;

namespace Geneao.Handlers.Events
{
    class FamilleCreeeEventHandler : IDomainEventHandler<FamilleCreeeEvent>
    {
        public Task HandleAsync(FamilleCreeeEvent domainEvent, IEventContext context = null)
        {
            var color = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine($"La famille {domainEvent.NomFamille.Value} a correctement été créée dans le système.");

            Console.ForegroundColor = color;

            return Task.CompletedTask;
        }
    }
}
