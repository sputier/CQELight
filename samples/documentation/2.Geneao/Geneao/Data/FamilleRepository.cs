using CQELight.DAL.Interfaces;
using Geneao.Data.Models;
using Geneao.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geneao.Data
{

    interface IFamilleRepository
    {
        Task SauverFamilleAsync(Famille famille);
        Task<Famille> GetFamilleByNomAsync(NomFamille nomFamille);
        Task<IEnumerable<Famille>> GetAllFamillesAsync();
    }

    class FamilleRepository : IFamilleRepository
    {
        private readonly List<Famille> _familles;
        private string _filePath;

        public FamilleRepository(FileInfo jsonFile)
        {
            _filePath = jsonFile.FullName;
            var familles = JsonConvert.DeserializeObject<IEnumerable<Famille>>(File.ReadAllText(_filePath));
            if (familles?.Any() == true)
            {
                _familles = new List<Famille>(familles);
            }
        }

        public Task<IEnumerable<Famille>> GetAllFamillesAsync()
            => Task.FromResult(_familles.AsEnumerable());

        public Task<Famille> GetFamilleByNomAsync(NomFamille nomFamille)
            => Task.FromResult(_familles.Find(f => f.Nom.Equals(nomFamille.Value, StringComparison.OrdinalIgnoreCase)));

        public Task SauverFamilleAsync(Famille famille)
        {
            _familles.Add(famille);
            File.WriteAllText(_filePath, JsonConvert.SerializeObject(_familles));
            return Task.CompletedTask;
        }
    }
}
