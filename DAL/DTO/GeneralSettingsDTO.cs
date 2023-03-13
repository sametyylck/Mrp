using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class GeneralSettingsDTO
    {
        public class GeneralDefaultSettings
        {
            public int id { get; set; }
      
            public int? CurrencyId { get; set; }
     
            public int? DefaultSalesOrder { get; set; }
      
            public int? DefaultPurchaseOrder { get; set; }
     
            public int? DefaultTaxSalesOrderId { get; set; }
        
            public int? DefaultTaxPurchaseOrderId { get; set; }
       
            public int? DefaultSalesLocationId { get; set; }
         
            public int? DefaultPurchaseLocationId { get; set; }
       
            public int? DefaultManufacturingLocationId { get; set; }

        }
        public class DefaultSettingList
        {
            public int id { get; set; }
            public int CurrencyId { get; set; }
            public string CurrencyName { get; set; } = string.Empty;
            public int SalesOrderDate { get; set; }
            public int PurchaseOrderDate { get; set; }
            public int TaxSalesOrderId { get; set; }
            public string TaxSalesOrderName { get; set; } = string.Empty;
            public float TaxSalesRate { get; set; }
            public int TaxPurchaseOrderId { get; set; }
            public string TaxPurchaseOrderName { get; set; } = string.Empty;
            public float TaxPurchaseRate { get; set; }
            public int SalesLocationId { get; set; }
            public string SalesLocationName { get; set; } = string.Empty;
            public int PurchaseLocationId { get; set; }
            public string PurchaseLocationName { get; set; } = string.Empty;
            public int ManufacturingLocationId { get; set; }
            public string ManufacturingLocationName { get; set; } = string.Empty;
        }
    }
}
