using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class StockAdjusmentItem
    {
        public int Id { get; set; }
        public int? ItemId { get; set; }
        public double? Adjusment { get; set; }
        public double? CostPerUnit { get; set; }
        public int? StockAdjusmentId { get; set; }
        public double? AdjusmentValue { get; set; }
        public int? CompanyId { get; set; }
        public bool? IsActive { get; set; }

        public virtual Company? Company { get; set; }
        public virtual Item? Item { get; set; }
        public virtual StockAdjusment? StockAdjusment { get; set; }
    }
}
