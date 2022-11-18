using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class ManufacturingOrder
    {
        public ManufacturingOrder()
        {
            ManufacturingOrderItems = new HashSet<ManufacturingOrderItem>();
        }

        public int Id { get; set; }
        public int? SalesOrderId { get; set; }
        public int? SalesOrderItemId { get; set; }
        public string? Name { get; set; }
        public int? ItemId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime? ProductionDeadline { get; set; }
        public DateTime? CreatedDate { get; set; }
        public double? PlannedQuantity { get; set; }
        public int? LocationId { get; set; }
        public double? MaterialCost { get; set; }
        public double? OperationCost { get; set; }
        public double? TotalCost { get; set; }
        public string? Info { get; set; }
        public int? Status { get; set; }
        public int? CompanyId { get; set; }
        public DateTime? DoneDate { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? DeleteUser { get; set; }

        public virtual Company? Company { get; set; }
        public virtual Item? Item { get; set; }
        public virtual Location? Location { get; set; }
        public virtual ICollection<ManufacturingOrderItem> ManufacturingOrderItems { get; set; }
    }
}
