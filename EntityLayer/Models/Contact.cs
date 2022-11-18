using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Contact
    {
        public Contact()
        {
            Items = new HashSet<Item>();
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string Tip { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? DisplayName { get; set; }
        public string? Mail { get; set; }
        public string? Phone { get; set; }
        public string? Comment { get; set; }
        public int? BillingLocationId { get; set; }
        public int? ShippingLocationId { get; set; }
        public bool? IsActive { get; set; }
        public int? CompanyId { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? DeletedUser { get; set; }

        public virtual Location? BillingLocation { get; set; }
        public virtual Company? Company { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
