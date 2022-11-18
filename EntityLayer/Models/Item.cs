using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Item
    {
        public Item()
        {
            Boms = new HashSet<Bom>();
            LocationStocks = new HashSet<LocationStock>();
            ManufacturingOrders = new HashSet<ManufacturingOrder>();
            OrdersItems = new HashSet<OrdersItem>();
            ProductOperationsBoms = new HashSet<ProductOperationsBom>();
            StockAdjusmentItems = new HashSet<StockAdjusmentItem>();
        }

        public int Id { get; set; }
        public string Tip { get; set; } = null!;
        public double? AllStockQuantity { get; set; }
        public string Name { get; set; } = null!;
        public int? CategoryId { get; set; }
        public int? MeasureId { get; set; }
        public int? ContactId { get; set; }
        public string? VariantCode { get; set; }
        public double? DefaultPrice { get; set; }
        public string? Info { get; set; }
        public bool? IsActive { get; set; }
        public int? CompanyId { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? DeletedUser { get; set; }

        public virtual Category? Category { get; set; }
        public virtual Company? Company { get; set; }
        public virtual Contact? Contact { get; set; }
        public virtual MeasureClas? Measure { get; set; }
        public virtual ICollection<Bom> Boms { get; set; }
        public virtual ICollection<LocationStock> LocationStocks { get; set; }
        public virtual ICollection<ManufacturingOrder> ManufacturingOrders { get; set; }
        public virtual ICollection<OrdersItem> OrdersItems { get; set; }
        public virtual ICollection<ProductOperationsBom> ProductOperationsBoms { get; set; }
        public virtual ICollection<StockAdjusmentItem> StockAdjusmentItems { get; set; }
    }
}
