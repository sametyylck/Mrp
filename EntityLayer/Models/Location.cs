using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Location
    {
        public Location()
        {
            Contacts = new HashSet<Contact>();
            GeneralDefaultSettingDefaultManufacturingLocations = new HashSet<GeneralDefaultSetting>();
            GeneralDefaultSettingDefaultPurchaseLocations = new HashSet<GeneralDefaultSetting>();
            GeneralDefaultSettingDefaultSalesLocations = new HashSet<GeneralDefaultSetting>();
            LocationStocks = new HashSet<LocationStock>();
            ManufacturingOrders = new HashSet<ManufacturingOrder>();
            Orders = new HashSet<Order>();
            StockAdjusments = new HashSet<StockAdjusment>();
            StockTransferDestinations = new HashSet<StockTransfer>();
            StockTransferOrigins = new HashSet<StockTransfer>();
        }

        public int Id { get; set; }
        public string Tip { get; set; } = null!;
        public string? LocationName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? CityTown { get; set; }
        public string? StateRegion { get; set; }
        public int? ZipPostalCode { get; set; }
        public string? Country { get; set; }
        public string? LegalName { get; set; }
        public bool? Sell { get; set; }
        public bool? Make { get; set; }
        public bool? Buy { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public bool? IsActive { get; set; }
        public int? CompanyId { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? DeletedUser { get; set; }

        public virtual Company? Company { get; set; }
        public virtual ICollection<Contact> Contacts { get; set; }
        public virtual ICollection<GeneralDefaultSetting> GeneralDefaultSettingDefaultManufacturingLocations { get; set; }
        public virtual ICollection<GeneralDefaultSetting> GeneralDefaultSettingDefaultPurchaseLocations { get; set; }
        public virtual ICollection<GeneralDefaultSetting> GeneralDefaultSettingDefaultSalesLocations { get; set; }
        public virtual ICollection<LocationStock> LocationStocks { get; set; }
        public virtual ICollection<ManufacturingOrder> ManufacturingOrders { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<StockAdjusment> StockAdjusments { get; set; }
        public virtual ICollection<StockTransfer> StockTransferDestinations { get; set; }
        public virtual ICollection<StockTransfer> StockTransferOrigins { get; set; }
    }
}
