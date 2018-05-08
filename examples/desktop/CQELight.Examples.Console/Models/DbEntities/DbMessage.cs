using CQELight.DAL.Attributes;
using CQELight.DAL.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Examples.Console.Models.DbEntities
{
    [Table("MES_T_MESSAGE")]
    internal class DbMessage : DbEntity
    {
        [Column("MES_MESSAGE")]
        public virtual string Message { get; set; }

        private DbMessage()
        {

        }
        public DbMessage(Guid id)
        {
            Id = id;
        }
    }
}
