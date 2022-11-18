using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Tax
    {
        public Tax()
        {
            GeneralDefaultSettingDefaultTaxPurchaseOrders = new HashSet<GeneralDefaultSetting>();
            GeneralDefaultSettingDefaultTaxSalesOrders = new HashSet<GeneralDefaultSetting>();
        }

        public int Id { get; set; }
        public double Rate { get; set; }
        public string? TaxName { get; set; }
        public int? CompanyId { get; set; }

        public virtual Company? Company { get; set; }
        public virtual ICollection<GeneralDefaultSetting> GeneralDefaultSettingDefaultTaxPurchaseOrders { get; set; }
        public virtual ICollection<GeneralDefaultSetting> GeneralDefaultSettingDefaultTaxSalesOrders { get; set; }
    }
}
