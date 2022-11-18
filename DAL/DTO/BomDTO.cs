using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.PurchaseOrderDTO;

namespace DAL.DTO
{
    public class BomDTO
    {
        public class BOMInsert
        {
          
            public int ProductId { get; set; }
           
            public int MaterialId { get; set; }
      
            public float Quantity { get; set; }
         
            public string? Note { get; set; }
            public string? Tip { get; set; }
        }
        public class BOMUpdate
        {

            public int id { get; set; }

            public int ProductId { get; set; }

            public int MaterialId { get; set; }

            public float Quantity { get; set; }

            public string? Note { get; set; }
            public string? Tip { get; set; }


        }
        public class BOM
        {

            public int id { get; set; }

            public int ProductId { get; set; }

            public int MaterialId { get; set; }

            public float Quantity { get; set; }

            public string? Note { get; set; }
            public int CompanyId { get; set; }


        }

        public class BOMDelete
        {
            public int id { get; set; }
        }

        public class ListBomMaterial
        {

            public int? id { get; set; }
            public string Name { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }
        public class ListBOM
        {
            public int id { get; set; }
            public int? ProductId { get; set; }
            public int? MaterialId { get; set; }
            public string MaterialName { get; set; } = string.Empty;
            public int? Quantity { get; set; }
            public string Note { get; set; } = string.Empty;
            public float? StockCost { get; set; }
            public bool IsActive { get; set; }
            public int? CompanyId { get; set; }
        }
        public class MissingCount
        {
            [Required]
            public int id { get; set; }
            public int LocationId { get; set; }
            public int SalesOrderItemId { get; set; }
            [Required]
            public int ProductId { get; set; }
            public int MaterialId { get; set; }
            public string MaterialName { get; set; } = string.Empty;
            public int Missing { get; set; }
            public List<PurchaseOrdersItemDetails> PurchaseOrderList { get; set; } 


        }
    }
}
