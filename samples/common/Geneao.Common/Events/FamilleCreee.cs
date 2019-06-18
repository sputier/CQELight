using CQELight.Abstractions.Events;
using Geneao.Common.Identity;
using Geneao.Domain;

namespace Geneao.Events
{
    public sealed class FamilleCreee : BaseDomainEvent
    {

        public NomFamille NomFamille { get; private set; }

        private FamilleCreee() { }

        internal FamilleCreee(NomFamille nomFamille)
        {
            NomFamille = nomFamille;
            AggregateId = nomFamille;
            AggregateType = typeof(Famille);
        }

    }
}
