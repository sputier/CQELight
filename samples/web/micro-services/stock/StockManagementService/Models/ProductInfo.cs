using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagementService.Models
{
    public class ProductInfo
    {
        #region Properties

        public int Id { get; set; }

        [Required, MinLength(1), MaxLength(128)]
        public string Name { get; set; }

        public int Quantity { get; set; }

        #endregion
    }
}
