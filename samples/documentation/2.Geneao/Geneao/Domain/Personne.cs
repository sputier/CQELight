using CQELight.Abstractions.DDD;
using Geneao.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geneao.Domain
{
    class Personne : Entity<PersonneId>
    {

        #region Properties
        
        public string Prenom { get; private set; }
        public InfosNaissance InfosNaissance { get; private set; }

        #endregion

        #region Ctor

        private Personne(PersonneId id)
        {
            Id = id;
        }

        #endregion

        #region Public static methods

        public static Personne DeclarerNaissance(string prenom, InfosNaissance infosNaissance)
        {
            if (string.IsNullOrWhiteSpace(prenom)) throw new ArgumentException("Personne.DeclarerNaissance() : Prénom requis", nameof(prenom));

            if (infosNaissance == null) throw new ArgumentNullException(nameof(infosNaissance));

            return new Personne(PersonneId.Generate())
            {
                Prenom = prenom,
                InfosNaissance = infosNaissance
            };
        }

        #endregion

    }

}
