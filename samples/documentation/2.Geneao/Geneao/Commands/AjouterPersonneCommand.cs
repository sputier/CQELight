using CQELight.Abstractions.CQS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geneao.Commands
{
    public sealed class AjouterPersonneCommand : ICommand
    {
        public string Nom { get; private set; }
        public string Prenom { get; private set; }
        public string LieuNaissance { get; private set; }
        public DateTime DateNaissance { get; private set; }

        private AjouterPersonneCommand() { }

        public AjouterPersonneCommand(string nom, string prenom, string lieuNaissance, DateTime dateNaissance)
        {
            if (string.IsNullOrWhiteSpace(nom)) throw new ArgumentException("AjouterPersonneCommand.Ctor() : Nom requis.", nameof(nom));

            if (string.IsNullOrWhiteSpace(prenom)) throw new ArgumentException("AjouterPersonneCommand.Ctor() : Prénom requis.", nameof(prenom));

            if (string.IsNullOrWhiteSpace(lieuNaissance)) throw new ArgumentException("AjouterPersonneCommand.Ctor() : Lieu naissance requis.", nameof(lieuNaissance));

            DateNaissance = dateNaissance;
            LieuNaissance = lieuNaissance;
            Prenom = prenom;
            Nom = nom;
        }
    }

}
