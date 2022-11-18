using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Rezerve
    {
        public int Id { get; set; }
        public int? SalesOrderId { get; set; }
        public int? SalesOrderItemId { get; set; }
        public int? ManufacturingOrderId { get; set; }
        public int? ManufacturingOrderItemId { get; set; }
        public string? Tip { get; set; }
        public int? ItemId { get; set; }
        public double? RezerveCount { get; set; }
        public int? CustomerId { get; set; }
        public int? LocationId { get; set; }
        public int? Status { get; set; }
        public double? LocationStockCount { get; set; }
        public int? CompanyId { get; set; }
    }
}
