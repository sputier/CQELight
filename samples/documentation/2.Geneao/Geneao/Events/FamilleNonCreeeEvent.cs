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
    public sealed class FamilleNonCreeeEvent : BaseDomainEvent
    {

        public string NomFamille { get; private set; }
        public FamilleNonCreeeRaison Raison { get; private set; }

        private FamilleNonCreeeEvent() { }

        internal FamilleNonCreeeEvent(string nomFamille, FamilleNonCreeeRaison raison)
        {
            NomFamille = nomFamille;
            Raison = raison;
        }
    }
}
