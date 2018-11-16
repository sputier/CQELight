using CQELight.Abstractions.CQS.Interfaces;
using Geneao.Data;
using Geneao.Queries.Models.Out;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geneao.Queries
{
    public interface IRecupererListeFamille : IQuery<IEnumerable<FamilleListItem>> { }
    class RecupererListeFamille : IRecupererListeFamille
    {

        private static ConcurrentBag<string> s_Cache
            = new ConcurrentBag<string>();

        private readonly IFamilleRepository _familleRepository;

        public RecupererListeFamille(IFamilleRepository familleRepository)
        {
            _familleRepository = familleRepository ?? throw new ArgumentNullException(nameof(familleRepository));
        }

        public async Task<IEnumerable<FamilleListItem>> ExecuteQueryAsync()
        {
            if (s_Cache.IsEmpty)
            {
                var allFamilles = (await _familleRepository.GetAllFamillesAsync().ConfigureAwait(false)).Select(f => f.Nom);
            }
            return s_Cache.Select(v => new FamilleListItem { Nom = v });
        }
    }
}
