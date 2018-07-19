using CQELight.Abstractions.CQS.Interfaces;
using Geneao.Commands;
using Geneao.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Geneao.Handlers.Commands
{
    class AjouterPersonneCommandHandler : ICommandHandler<AjouterPersonneCommand>
    {
        public async Task HandleAsync(AjouterPersonneCommand command, ICommandContext context = null)
        {
            var famille = new Famille();
            famille.AjouterPersonne(command.Nom, command.Prenom, new InfosNaissance(command.LieuNaissance, command.DateNaissance));
            await famille.DispatchDomainEventsAsync();
        }
    }

}
