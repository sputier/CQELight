using CQELight.Abstractions.Events;
using Geneao.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geneao.Events
{

    public enum FamilleNonCreeeRaison
    {
        NomIncorrect,
        FamilleDejaExistante
    }
    public sealed class FamilleNonCreee : BaseDomainEvent
    {

        public string NomFamille { get; private set; }
        public FamilleNonCreeeRaison Raison { get; private set; }

        private FamilleNonCreee() { }

        internal FamilleNonCreee(string nomFamille, FamilleNonCreeeRaison raison)
        {
            NomFamille = nomFamille;
            Raison = raison;
        }
    }
}
