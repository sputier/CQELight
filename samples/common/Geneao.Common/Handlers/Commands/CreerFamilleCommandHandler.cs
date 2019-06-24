using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher;
using Geneao.Common.Commands;
using Geneao.Domain;
using Geneao.Events;
using Geneao.Common.Identity;
using System.Linq;
using System.Threading.Tasks;
using Geneao.Common.Data.Repositories.Familles;

namespace Geneao.Common.Handlers.Commands
{
    class CreerFamilleCommandHandler : ICommandHandler<CreerFamilleCommand>, IAutoRegisterType
    {
        private readonly IFamilleRepository _familleRepository;

        public CreerFamilleCommandHandler(IFamilleRepository familleRepository)
        {
            _familleRepository = familleRepository ?? throw new System.ArgumentNullException(nameof(familleRepository));
        }

        public async Task<Result> HandleAsync(CreerFamilleCommand command, ICommandContext context = null)
        {
            Famille._nomFamilles = (await _familleRepository.GetAllFamillesAsync().ConfigureAwait(false)).Select(f => new NomFamille(f.Nom)).ToList();
            var result = Famille.CreerFamille(command.Nom);
            if (result && result is Result<NomFamille> resultFamille)
            {
                await CoreDispatcher.PublishEventAsync(new FamilleCreee(resultFamille.Value));
                return Result.Fail();
            }
            return result;
        }
    }
}
