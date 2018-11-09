using CQELight.DAL.Attributes;
using CQELight.DAL.Common;
using System;

namespace Geneao.Data.Models
{
    [Table("Personnes")]
    public class Personne : PersistableEntity
    {
        [Column("Prenom")]
        public string Prenom { get; set; }
        [Column("LieuNaissance")]
        public string LieuNaissance { get; set; }
        [Column("DateNaissance")]
        public DateTime DateNaissance { get; set; }
        [ForeignKey]
        public Famille Famille { get; set; }
        [Column("NomFamille"), KeyStorageOf(nameof(Famille))]
        public string Famille_Id { get; set; }
    }
}
