using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Company
    {
        public Company()
        {
            Boms = new HashSet<Bom>();
            Categories = new HashSet<Category>();
            Contacts = new HashSet<Contact>();
            GeneralDefaultSettings = new HashSet<GeneralDefaultSetting>();
            Items = new HashSet<Item>();
            LocationStocks = new HashSet<LocationStock>();
            Locations = new HashSet<Location>();
            ManufacturingOrderItems = new HashSet<ManufacturingOrderItem>();
            ManufacturingOrders = new HashSet<ManufacturingOrder>();
            Measures = new HashSet<MeasureClas>();
            Operations = new HashSet<Operation>();
            Orders = new HashSet<Order>();
            OrdersItems = new HashSet<OrdersItem>();
            ProductOperationsBoms = new HashSet<ProductOperationsBom>();
            Resources = new HashSet<Resource>();
            StockAdjusmentItems = new HashSet<StockAdjusmentItem>();
            StockAdjusments = new HashSet<StockAdjusment>();
            StockTransferItems = new HashSet<StockTransferItem>();
            StockTransfers = new HashSet<StockTransfer>();
            Taxes = new HashSet<Tax>();
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string? LegalName { get; set; }
        public int? LocationId { get; set; }

        public virtual ICollection<Bom> Boms { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<Contact> Contacts { get; set; }
        public virtual ICollection<GeneralDefaultSetting> GeneralDefaultSettings { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<LocationStock> LocationStocks { get; set; }
        public virtual ICollection<Location> Locations { get; set; }
        public virtual ICollection<ManufacturingOrderItem> ManufacturingOrderItems { get; set; }
        public virtual ICollection<ManufacturingOrder> ManufacturingOrders { get; set; }
        public virtual ICollection<MeasureClas> Measures { get; set; }
        public virtual ICollection<Operation> Operations { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<OrdersItem> OrdersItems { get; set; }
        public virtual ICollection<ProductOperationsBom> ProductOperationsBoms { get; set; }
        public virtual ICollection<Resource> Resources { get; set; }
        public virtual ICollection<StockAdjusmentItem> StockAdjusmentItems { get; set; }
        public virtual ICollection<StockAdjusment> StockAdjusments { get; set; }
        public virtual ICollection<StockTransferItem> StockTransferItems { get; set; }
        public virtual ICollection<StockTransfer> StockTransfers { get; set; }
        public virtual ICollection<Tax> Taxes { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
