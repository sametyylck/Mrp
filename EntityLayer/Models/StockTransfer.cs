using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class StockTransfer
    {
        public StockTransfer()
        {
            StockTransferItems = new HashSet<StockTransferItem>();
        }

        public int Id { get; set; }
        public string? StockTransferName { get; set; }
        public DateTime? TransferDate { get; set; }
        public int? OriginId { get; set; }
        public int? DestinationId { get; set; }
        public string? Info { get; set; }
        public double? Total { get; set; }
        public int? CompanyId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? DeletedUser { get; set; }

        public virtual Company? Company { get; set; }
        public virtual Location? Destination { get; set; }
        public virtual Location? Origin { get; set; }
        public virtual ICollection<StockTransferItem> StockTransferItems { get; set; }
    }
}
