using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Dispatcher;
using Geneao.Commands;
using Geneao.Domain;
using System.Threading.Tasks;

namespace Geneao.Handlers.Commands
{
    public class CreerFamilleCommandHandler : ICommandHandler<CreerFamilleCommand>
    {
        public async Task HandleAsync(CreerFamilleCommand command, ICommandContext context = null)
        {
            var events = Famille.CreerFamille(command.Nom);
            await CoreDispatcher.PublishEventsRangeAsync(events);
        }
    }
}
