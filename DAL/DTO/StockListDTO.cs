using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class StockListDTO
    {
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
            public string? ItemName { get; set; }
            public int? InStock { get; set; }
            public string? Tip { get; set; }
            public int CategoryId { get; set; }
            public string? CategoryName { get; set; }
            public int ContactId { get; set; }
            public string? SupplierName { get; set; }
            public int? locationId { get; set; }
            public string? LocationName { get; set; }
            public string? VariantCode { get; set; }
            public float? AverageCost { get; set; }
            public float? MaterialExpected { get; set; }
            public float? ProductExpected { get; set; }
            public float? ValueInStock { get; set; }
            public float? MaterailCommitted { get; set; }
            public float? Missing { get; set; }


        }
    }
}
