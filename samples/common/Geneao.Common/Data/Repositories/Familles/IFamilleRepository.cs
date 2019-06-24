using Geneao.Common.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using Geneao.Common.Data.Models;

namespace Geneao.Common.Data.Repositories.Familles
{
    interface IFamilleRepository
    {
        Task SauverFamilleAsync(Famille famille);
        Task<Famille> GetFamilleByNomAsync(NomFamille nomFamille);
        Task<IEnumerable<Famille>> GetAllFamillesAsync();
    }
}
