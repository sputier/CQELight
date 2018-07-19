using CQELight.Abstractions.DDD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geneao.Domain
{
    class Famille : AggregateRoot<Guid>
    {

        public IEnumerable<Personne> Personnes => _state.Personnes.AsEnumerable();

        private FamilleState _state;

        private class FamilleState : AggregateState
        {

            public List<Personne> Personnes { get; set; }

            public FamilleState()
            {
                Personnes = new List<Personne>();
            }
        }

        public Famille()
        {
            _state = new FamilleState();
        }

        public void AjouterPersonne(string nom, string prenom, InfosNaissance infosNaissance)
        {
            if (!_state.Personnes.Any(p => p.Nom == nom && p.Prenom == prenom && p.InfosNaissance == infosNaissance))
            {
                _state.Personnes.Add(Personne.DeclarerNaissance(nom, prenom, infosNaissance));
            }
        }

    }

}
