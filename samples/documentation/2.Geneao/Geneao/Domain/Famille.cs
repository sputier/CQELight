using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using Geneao.Events;
using Geneao.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geneao.Domain
{
    class Famille : AggregateRoot<NomFamille>
    {
        private static List<NomFamille> _nomFamilles = new List<NomFamille>();

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

        public Famille(NomFamille nomFamille)
        {
            if (!_nomFamilles.Any(f => f.Value.Equals(nomFamille.Value, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Famille.Ctor() : Impossible de créer une famille qui n'a pas été d'abord créée dans le système.");
            }
            Id = nomFamille;
            _state = new FamilleState();
        }

        public static IEnumerable<IDomainEvent> CreerFamille(string nom, IEnumerable<Personne> personnes = null)
        {
            NomFamille nomFamille = new NomFamille();
            try
            {
                nomFamille = new NomFamille(nom);
            }
            catch
            {
                return new IDomainEvent[] { new FamilleNonCreeeEvent(nom, FamilleNonCreeeRaison.NomIncorrect) };
            }
            if (_nomFamilles.Any(f => f.Value.Equals(nom, StringComparison.OrdinalIgnoreCase)))
            {
                return new IDomainEvent[] { new FamilleNonCreeeEvent(nom, FamilleNonCreeeRaison.FamilleDejaExistante) };
            }
            _nomFamilles.Add(nomFamille);
            return new IDomainEvent[] { new FamilleCreeeEvent(nomFamille) };
        }

        public void AjouterPersonne(string prenom, InfosNaissance infosNaissance)
        {
            PersonneNonAjouteeEvent CreateErrorEvent(PersonneNonAjouteeRaison raison)
            {
                return new PersonneNonAjouteeEvent(Id, prenom, infosNaissance.Lieu, infosNaissance.DateNaissance, raison);
            }
            if (string.IsNullOrWhiteSpace(prenom))
            {
                AddDomainEvent(CreateErrorEvent(PersonneNonAjouteeRaison.PrenomInvalide));
            }
            else
            {
                if (!_state.Personnes.Any(p => p.Prenom == prenom && p.InfosNaissance == infosNaissance))
                {
                    _state.Personnes.Add(Personne.DeclarerNaissance(prenom, infosNaissance));
                    AddDomainEvent(new PersonneAjouteeEvent(Id, prenom, infosNaissance.Lieu, infosNaissance.DateNaissance));
                }
                else
                {
                    AddDomainEvent(CreateErrorEvent(PersonneNonAjouteeRaison.PersonneExistante));
                }
            }
        }
    }

}
