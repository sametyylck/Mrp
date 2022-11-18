using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class StockDTO
    {
        public class Stock
        {
            public int id { get; set; }
            public float? AllStockQuantity { get; set; }

            public int LocationStockId { get; set; }

            public int LocationsStockCount { get; set; }
            public float? RezerveStockCount { get; set; }

            public int StockId { get; set; }
            [Required]
            [StringLength(50)]
            public string Tip { get; set; } = string.Empty;
            public int? ItemId { get; set; }
            public int? CompanyId { get; set; }

        }
        public class StockList
        {
            public int id { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public int? InStock { get; set; }
            public string Tip { get; set; } = string.Empty;
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public int ContactId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public int locationId { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public string VariantCode { get; set; } = string.Empty;
            public float? AverageCost { get; set; }
            public float? Expected { get; set; }
            public float? ValueInStock { get; set; }
            public float? Committed { get; set; }
            public float? Missing { get; set; }


        }
        public class StockListAll
        {
            public int id { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public int? InStock { get; set; }
            public string Tip { get; set; } = string.Empty;
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public int ContactId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public int locationId { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public string VariantCode { get; set; } = string.Empty;
            public float? AverageCost { get; set; }
            public float? MaterialExpected { get; set; }
            public float? ProductExpected { get; set; }
            public float? ValueInStock { get; set; }
            public float? MaterailCommitted { get; set; }
            public float? Missing { get; set; }


        }
    }
}
