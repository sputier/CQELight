using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using Geneao.Common.Data.Models;
using Geneao.Common.Data.Repositories.Familles;
using Geneao.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Geneao.Common.Handlers.Events
{
    class PersonneAjouteeEventHandler : IDomainEventHandler<PersonneAjoutee>, IAutoRegisterType
    {
        private readonly IFamilleRepository _familleRepository;

        public PersonneAjouteeEventHandler(IFamilleRepository familleRepository)
        {
            _familleRepository = familleRepository ?? throw new ArgumentNullException(nameof(familleRepository));
        }
        public async Task<Result> HandleAsync(PersonneAjoutee domainEvent, IEventContext context = null)
        {
            var famille = await _familleRepository.GetFamilleByNomAsync(domainEvent.NomFamille);
            if (famille != null)
            {
                famille.Personnes.Add(new Personne
                {
                    DateNaissance = domainEvent.DateNaissance,
                    LieuNaissance = domainEvent.LieuNaissance,
                    Prenom = domainEvent.Prenom
                });
                await _familleRepository.SauverFamilleAsync(famille);
                return Result.Ok();
            }
            return Result.Fail();
        }
    }

}
