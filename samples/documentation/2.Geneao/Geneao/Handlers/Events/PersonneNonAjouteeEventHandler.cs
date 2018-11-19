using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using Geneao.Events;
using System;
using System.Threading.Tasks;

namespace Geneao.Handlers.Events
{
    class PersonneNonAjouteeEventHandler : IDomainEventHandler<PersonneNonAjoutee>, IAutoRegisterType
    {
        public Task HandleAsync(PersonneNonAjoutee domainEvent, IEventContext context = null)
        {
            var color = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.DarkRed;

            string raisonText = string.Empty;
            switch (domainEvent.Raison)
            {
                case PersonneNonAjouteeRaison.InformationsDeNaissanceInvalides:
                    raisonText = "les informations de naissances sont invalides.";
                    break;
                case PersonneNonAjouteeRaison.PersonneExistante:
                    raisonText = "une personne avec le même prénom et les mêmes informations de naissance existe déjà.";
                        break;
                case PersonneNonAjouteeRaison.PrenomInvalide:
                    raisonText = "le prénom est invalide.";
                    break;
            }
            Console.WriteLine($"La création de la personne {domainEvent.Prenom} a échouée car {raisonText}");

            Console.ForegroundColor = color;

            return Task.CompletedTask;
        }
    }
}
