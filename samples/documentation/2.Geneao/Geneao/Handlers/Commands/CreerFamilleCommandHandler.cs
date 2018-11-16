using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher;
using Geneao.Commands;
using Geneao.Data;
using Geneao.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace Geneao.Handlers.Commands
{
    class CreerFamilleCommandHandler : ICommandHandler<CreerFamilleCommand>, IAutoRegisterType
    {
        private readonly IFamilleRepository _familleRepository;

        public CreerFamilleCommandHandler(IFamilleRepository familleRepository)
        {
            _familleRepository = familleRepository ?? throw new System.ArgumentNullException(nameof(familleRepository));
        }

        public async Task HandleAsync(CreerFamilleCommand command, ICommandContext context = null)
        {
            Famille._nomFamilles = (await _familleRepository.GetAllFamillesAsync().ConfigureAwait(false)).Select(f => new Identity.NomFamille(f.Nom)).ToList();
            var events = Famille.CreerFamille(command.Nom);
            await CoreDispatcher.PublishEventsRangeAsync(events);
        }
    }
}
