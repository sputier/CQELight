using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher;
using Geneao.Commands;
using Geneao.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Geneao.Handlers.Commands
{
    class AjouterPersonneCommandHandler : ICommandHandler<AjouterPersonneCommand>, IAutoRegisterType
    {
        public async Task<Result> HandleAsync(AjouterPersonneCommand command, ICommandContext context = null)
        {
            var famille = new Famille(command.NomFamille);
            famille.AjouterPersonne(command.Prenom, new InfosNaissance(command.LieuNaissance, command.DateNaissance));
            await famille.PublishDomainEventsAsync();
            return Result.Ok();
        }
    }

}
