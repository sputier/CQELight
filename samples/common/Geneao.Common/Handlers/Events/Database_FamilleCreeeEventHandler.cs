using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using Geneao.Common.Data.Repositories.Familles;
using Geneao.Domain;
using Geneao.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Geneao.Common.Handlers.Events
{
    class FamilleCreeeEventHandler : IDomainEventHandler<FamilleCreee>, IAutoRegisterType
    {
        private readonly IFamilleRepository _familleRepository;

        public FamilleCreeeEventHandler(IFamilleRepository familleRepository)
        {
            _familleRepository = familleRepository ?? throw new ArgumentNullException(nameof(familleRepository));
        }

        public async Task<Result> HandleAsync(FamilleCreee domainEvent, IEventContext context = null)
        {
            var color = Console.ForegroundColor;
            try
            {
                await _familleRepository.SauverFamilleAsync(new Data.Models.Famille
                {
                    Nom = domainEvent.NomFamille.Value
                }).ConfigureAwait(false);


            }
            catch (Exception e)
            {
                return Result.Fail();
            }
            finally
            {
                Console.ForegroundColor = color;
            }
            return Result.Ok();
        }
    }
}
