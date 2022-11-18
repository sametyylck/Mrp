using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Order
    {
        public Order()
        {
            OrdersItems = new HashSet<OrdersItem>();
        }

        public int Id { get; set; }
        public int? SalesOrderId { get; set; }
        public int? SalesOrderItemId { get; set; }
        public int? ManufacturingOrderId { get; set; }
        public int? ManufacturingOrderItemId { get; set; }
        public string? Tip { get; set; }
        public int? ContactId { get; set; }
        public string? OrderName { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? LocationId { get; set; }
        public string? Info { get; set; }
        public double? TotalAll { get; set; }
        public int? CompanyId { get; set; }
        public int? DeliveryId { get; set; }
        public bool? IsActive { get; set; }
        public int? BillingAddressId { get; set; }
        public int? ShippingAddressId { get; set; }
        public DateTime? DeliveryDeadline { get; set; }
        public bool? Quotes { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? DeletedUser { get; set; }

        public virtual Company? Company { get; set; }
        public virtual Contact? Contact { get; set; }
        public virtual Location? Location { get; set; }
        public virtual ICollection<OrdersItem> OrdersItems { get; set; }
    }
}
