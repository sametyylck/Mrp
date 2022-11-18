using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class OrdersItem
    {
        public int Id { get; set; }
        public string? Tip { get; set; }
        public int? ItemId { get; set; }
        public double? Quantity { get; set; }
        public double? PricePerUnit { get; set; }
        public int? TaxId { get; set; }
        public double? TaxValue { get; set; }
        public int? OrdersId { get; set; }
        public int? CompanyId { get; set; }
        public double? TotalPrice { get; set; }
        public double? PlusTax { get; set; }
        public double? TotalAll { get; set; }
        public int? MeasureId { get; set; }
        public int? SalesItem { get; set; }
        public int? Ingredients { get; set; }
        public int? Production { get; set; }
        public int? Stance { get; set; }

        public virtual Company? Company { get; set; }
        public virtual Item? Item { get; set; }
        public virtual Order? Orders { get; set; }
    }
}
