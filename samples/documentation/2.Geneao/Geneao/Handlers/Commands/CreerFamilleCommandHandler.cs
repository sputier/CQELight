using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.EventStore.Interfaces;
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
        private readonly IEventStore _eventStore;

        public CreerFamilleCommandHandler(IFamilleRepository familleRepository,
            IEventStore eventStore)
        {
            _familleRepository = familleRepository ?? throw new System.ArgumentNullException(nameof(familleRepository));
            _eventStore = eventStore ?? throw new System.ArgumentNullException(nameof(eventStore));
        }

        public async Task<Result> HandleAsync(CreerFamilleCommand command, ICommandContext context = null)
        {
            Famille._nomFamilles = (await _familleRepository.GetAllFamillesAsync().ConfigureAwait(false)).Select(f => new Identity.NomFamille(f.Nom)).ToList();
            var events = Famille.CreerFamille(command.Nom);
            await CoreDispatcher.PublishEventsRangeAsync(events);
            return Result.Ok();
        }
    }
}
