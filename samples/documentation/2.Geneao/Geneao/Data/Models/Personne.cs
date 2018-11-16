using CQELight.DAL.Attributes;
using CQELight.DAL.Common;
using System;

namespace Geneao.Data.Models
{
    [Table("Personnes")]
    public class Personne : PersistableEntity
    {
        [Column]
        public string Prenom { get; set; }
        [Column]
        public string LieuNaissance { get; set; }
        [Column]
        public DateTime DateNaissance { get; set; }
        [ForeignKey]
        public Famille Famille { get; set; }
        [Column("NomFamille"), KeyStorageOf(nameof(Famille))]
        public string Famille_Id { get; set; }
    }
}
