using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class ManufacturingOrderItem
    {
        public int Id { get; set; }
        public string? Tip { get; set; }
        public int? OrderId { get; set; }
        public int? ItemId { get; set; }
        public string? Notes { get; set; }
        public double? PlannedQuantity { get; set; }
        public double? CostPerHour { get; set; }
        public double? Cost { get; set; }
        public int? Availability { get; set; }
        public int? OperationId { get; set; }
        public string? ResourceId { get; set; }
        public double? PlannedTime { get; set; }
        public int? Status { get; set; }
        public int? CompanyId { get; set; }

        public virtual Company? Company { get; set; }
        public virtual ManufacturingOrder? Order { get; set; }
    }
}
