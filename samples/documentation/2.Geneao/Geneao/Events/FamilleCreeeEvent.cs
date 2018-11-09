using CQELight.Abstractions.Events;
using Geneao.Identity;

namespace Geneao.Events
{
    public sealed class FamilleCreeeEvent : BaseDomainEvent
    {

        public NomFamille NomFamille { get; private set; }

        private FamilleCreeeEvent() { }

        internal FamilleCreeeEvent(NomFamille nomFamille)
        {
            NomFamille = nomFamille;
        }

    }
}
