using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class LocationStock
    {
        public int Id { get; set; }
        public string? Tip { get; set; }
        public int? LocationId { get; set; }
        public int? ItemId { get; set; }
        public double? StockCount { get; set; }
        public double? RezerveStockCount { get; set; }
        public bool? IsActive { get; set; }
        public int? CompanyId { get; set; }

        public virtual Company? Company { get; set; }
        public virtual Item? Item { get; set; }
        public virtual Location? Location { get; set; }
    }
}
