using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using Geneao.Common.Data.Repositories.Familles;
using Geneao.Common.Identity;
using Geneao.Common.Queries.Models.Out;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geneao.Common.Queries
{
    public interface IRecupererListePersonnes : IQuery<IEnumerable<PersonneDetails>, NomFamille> { }
    public class RecupererListePersonnes : IRecupererListePersonnes, IAutoRegisterType
    {
        private readonly IFamilleRepository _familleRepository;

        public RecupererListePersonnes(IFamilleRepository familleRepository)
        {
            _familleRepository = familleRepository ?? throw new ArgumentNullException(nameof(familleRepository));
        }

        public async Task<IEnumerable<PersonneDetails>> ExecuteQueryAsync(NomFamille param)
        {
            var famille = await _familleRepository.GetFamilleByNomAsync(param);
            if (famille != null)
            {
                return famille.Personnes.Select(p => new PersonneDetails
                {
                    Prenom = p.Prenom,
                    LieuNaissance = p.LieuNaissance,
                    DateNaissance = p.DateNaissance
                }).ToList();
            }
            return Enumerable.Empty<PersonneDetails>();
        }
    }
}
