using CQELight.DAL.Attributes;
using CQELight.DAL.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagementService.Data
{
    [Table]
    public class Product : CustomKeyPersistableEntity
    {
        #region Properties

        [PrimaryKey]
        public int Id { get; set; }

        [Column, MaxLength(256), Required]
        public string Name { get; set; }

        [Column, Required]
        public int Quantity { get; set; }

        #endregion
    }
}
