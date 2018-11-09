using CQELight.DAL.Attributes;
using CQELight.DAL.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geneao.Data.Models
{
    [Table("Familles")]
    public class Famille : CustomKeyPersistableEntity
    {
        [PrimaryKey]
        public string Nom { get; set; }
        public ICollection<Personne> Personnes { get; set; }

    }
}
