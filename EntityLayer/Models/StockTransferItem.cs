using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class StockTransferItem
    {
        public int id { get; set; }
        public int? ItemId { get; set; }
        public double? CostPerUnit { get; set; }
        public double? Quantity { get; set; }
        public double? TransferValue { get; set; }
        public int? StockTransferId { get; set; }
        public int? CompanyId { get; set; }
        public bool? IsActive { get; set; }

        public virtual Company? Company { get; set; }
        public virtual StockTransfer? StockTransfer { get; set; }
    }
}
