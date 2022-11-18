using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class StockTakesDTO
    {
        public class StockTakes
        {
            public int id { get; set; }
            public string StockTake { get; set; } = string.Empty;
            public DateTime CreadtedDate { get; set; }
            public string Reason { get; set; } = string.Empty;
            public string Info { get; set; } = string.Empty;
            public int LocationId { get; set; }
            public int CompanyId { get; set; }
            public string Notes { get; set; } = string.Empty;
            public int ItemId { get; set; }
            public float? CountedQuantity { get; set; }
            public float? Discrepancy { get; set; }

        }
        public class StockTakesUpdate
        {
            public int id { get; set; }
            public string StockTake { get; set; } = string.Empty;
            public DateTime CreadtedDate { get; set; }
            public DateTime StartedDate { get; set; }
            public DateTime CompletedDate { get; set; }

            public string Reason { get; set; } = string.Empty;
            public string Info { get; set; } = string.Empty;

        }
        public class StockTakesUpdateItems
        {
            public int? StockTakesId { get; set; }
            public int? StockTakesItemId { get; set; }
            public float? CountedQuantity { get; set; }
            public string? Note { get; set; }

        }

        public class StockTakesInsert
        {
            public string StockTake { get; set; } = string.Empty;
            public DateTime CreadtedDate { get; set; }
            public string Reason { get; set; } = string.Empty;
            public string Info { get; set; } = string.Empty;
            public int LocationId { get; set; }
            public string Notes { get; set; } = string.Empty;

        }
        public class StockTakeList
        {
            public int? id { get; set; }
            public string StockTake { get; set; } = string.Empty;
            public DateTime? CreadtedDate { get; set; }
            public string Reason { get; set; } = string.Empty;
            public string Info { get; set; } = string.Empty;
            public int? LocationId { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public int? StockAdjusmentId { get; set; }
            public string Name { get; set; } = string.Empty;
            public int? CompanyId { get; set; }
            public int? Status { get; set; }
            public DateTime? CompletedDate { get; set; }

        }

        public class StockTakeItems
        {
            public int id { get; set; }
            public int StockTakesId { get; set; }
            public int ItemId { get; set; }
            public int CategoryId { get; set; }
            public string? Notes { get; set; }
            public int InStock { get; set; }
            public float? CountedQuantity { get; set; }
            public float? Discrepancy { get; set; }
            public int CompanyId { get; set; }

        }
        public class StockTakeInsertItems
        {
            public int? StockTakesId { get; set; }
            public int? ItemId { get; set; }
            public string? Notes { get; set; }

        }
        public class StockTakeInsertItemsResponse
        {
            public int id { get; set; }
            public int? StockTakesId { get; set; }
            public string? Name { get; set; }
            public float InStock { get; set; }
            public int? ItemId { get; set; }
            public string? Note { get; set; }

        }
        public class StockTakeDelete
        {
            public int? id { get; set; }
            public int? ItemId { get; set; }
        }
        public class StockTakesDetail
        {
            public int id { get; set; }
            public string? StockTake { get; set; }
            public DateTime CreadtedDate { get; set; }
            public string? Reason { get; set; }
            public string? Info { get; set; }
            public int LocationId { get; set; }
            public int CompanyId { get; set; }
            public int? StockAdjusmentId { get; set; }
            public DateTime CompletedDate { get; set; }
            public int Status { get; set; }
            public int IsActive { get; set; }

        }
        public class StockTakesDone
        {
            public int id { get; set; }
            public int Status { get; set; }
        }
    }
}
