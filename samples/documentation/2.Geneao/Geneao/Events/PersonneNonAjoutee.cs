using CQELight.Abstractions.Events;
using Geneao.Identity;
using System;

namespace Geneao.Events
{

    public enum PersonneNonAjouteeRaison
    {
        PrenomInvalide,
        InformationsDeNaissanceInvalides,
        PersonneExistante
    }

    public sealed class PersonneNonAjoutee : BaseDomainEvent
    {

        public NomFamille NomFamille { get; private set; }
        public string Prenom { get; private set; }
        public string LieuNaissance { get; private set; }
        public DateTime DateNaissance { get; private set; }
        public PersonneNonAjouteeRaison Raison { get; private set; }

        private PersonneNonAjoutee() { }

        internal PersonneNonAjoutee(NomFamille nomFamille, string prenom, string lieuNaissance, DateTime dateNaissance,
            PersonneNonAjouteeRaison raison)
        {
            if (string.IsNullOrWhiteSpace(prenom)) throw new ArgumentException("PersonneAjouteeEvent.Ctor() : Prénom requis.", nameof(prenom));

            if (string.IsNullOrWhiteSpace(lieuNaissance)) throw new ArgumentException("PersonneAjouteeEvent.Ctor() : Lieu naissance requis.", nameof(lieuNaissance));

            DateNaissance = dateNaissance;
            LieuNaissance = lieuNaissance;
            Prenom = prenom;
            NomFamille = nomFamille;
            Raison = raison;
        }

    }
}
