using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class GeneralDefaultSetting
    {
        public int Id { get; set; }
        public int? CurrencyId { get; set; }
        public int? DefaultSalesOrder { get; set; }
        public int? DefaultPurchaseOrder { get; set; }
        public int? DefaultTaxSalesOrderId { get; set; }
        public int? DefaultTaxPurchaseOrderId { get; set; }
        public int? DefaultSalesLocationId { get; set; }
        public int? DefaultPurchaseLocationId { get; set; }
        public int? DefaultManufacturingLocationId { get; set; }
        public int? CompanyId { get; set; }

        public virtual Company? Company { get; set; }
        public virtual Currency? Currency { get; set; }
        public virtual Location? DefaultManufacturingLocation { get; set; }
        public virtual Location? DefaultPurchaseLocation { get; set; }
        public virtual Location? DefaultSalesLocation { get; set; }
        public virtual Tax? DefaultTaxPurchaseOrder { get; set; }
        public virtual Tax? DefaultTaxSalesOrder { get; set; }
    }
}
