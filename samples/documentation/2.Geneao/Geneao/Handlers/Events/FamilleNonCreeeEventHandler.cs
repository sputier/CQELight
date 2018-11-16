using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using Geneao.Events;
using System;
using System.Threading.Tasks;

namespace Geneao.Handlers.Events
{
    class FamilleNonCreeeEventHandler : IDomainEventHandler<FamilleNonCreee>, IAutoRegisterType
    {
        public Task HandleAsync(FamilleNonCreee domainEvent, IEventContext context = null)
        {
            var color = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.DarkRed;

            var raisonText =
                domainEvent.Raison == FamilleNonCreeeRaison.FamilleDejaExistante
                ? "cette famille existe déjà."
                : "le nom de la famille est incorrect.";
            Console.WriteLine($"La famille {domainEvent.NomFamille} n'a pas pu être créée car {raisonText}");

            Console.ForegroundColor = color;

            return Task.CompletedTask;
        }
    }
}
