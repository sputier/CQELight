using System;
using System.Collections.Generic;
using System.Text;

namespace Geneao.Common.Queries.Models.Out
{
    public class PersonneDetails
    {
        public string Prenom { get; internal set; }
        public string LieuNaissance { get; internal set; }
        public DateTime DateNaissance { get; internal set; }

        internal PersonneDetails() { }
    }
}
