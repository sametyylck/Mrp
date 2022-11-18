using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class StockAdjusment
    {
        public StockAdjusment()
        {
            StockAdjusmentItems = new HashSet<StockAdjusmentItem>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Reason { get; set; }
        public DateTime? Date { get; set; }
        public int? LocationId { get; set; }
        public int? CompanyId { get; set; }
        public bool? IsActive { get; set; }
        public string? Info { get; set; }
        public double? Total { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? DeletedUser { get; set; }

        public virtual Company? Company { get; set; }
        public virtual Location? Location { get; set; }
        public virtual ICollection<StockAdjusmentItem> StockAdjusmentItems { get; set; }
    }
}
