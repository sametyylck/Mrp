using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class ContactDTO
    {
        public class Contacts
        {
            public int id { get; set; }
            public string Tip { get; set; } = string.Empty;
            public string AddressFirstName { get; set; } = string.Empty;
            public string AddressLastName { get; set; } = string.Empty;
            public string AddressCompany { get; set; } = string.Empty;
            public string AddressPhone { get; set; } = string.Empty;
            public string AddressLine1 { get; set; } = string.Empty;
            public string AddressLine2 { get; set; } = string.Empty;
            public string AddressCityTown { get; set; } = string.Empty;
            public string AddressStateRegion { get; set; } = string.Empty;
            public int AddressZipPostal { get; set; }
            public string AddressCountry { get; set; } = string.Empty;
            public int BillingLocationId { get; set; }
            public int ShippingLocationId { get; set; }
            public int? CompanyId { get; set; }

        }
        public class ContactsUpdateAddress
        {
            public int id { get; set; }
            public string Tip { get; set; } = string.Empty;
            public string AddressFirstName { get; set; } = string.Empty;
            public string AddressLastName { get; set; } = string.Empty;
            public string AddressCompany { get; set; } = string.Empty;
            public string AddressPhone { get; set; } = string.Empty;
            public string AddressLine1 { get; set; } = string.Empty;
            public string AddressLine2 { get; set; } = string.Empty;
            public string AddressCityTown { get; set; } = string.Empty;
            public string AddressStateRegion { get; set; } = string.Empty;
            public int AddressZipPostal { get; set; }
            public string AddressCountry { get; set; } = string.Empty;

        }

        public class ContactsAll
        {

            public int CustomerId { get; set; }
            public string Tip { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            [Required]
            public string DisplayName { get; set; } = string.Empty;
            [EmailAddress]
            public string Mail { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Comment { get; set; } = string.Empty;
            public string BillingFirstName { get; set; } = string.Empty;
            public string BillingLastName { get; set; } = string.Empty;
            public string BillingCompanyName { get; set; } = string.Empty;
            public string BillingPhone { get; set; } = string.Empty;
            public string BillingAddressLine1 { get; set; } = string.Empty;
            public string BillingAddressLine2 { get; set; } = string.Empty;
            public string BillingCityTown { get; set; } = string.Empty;
            public string BillingStateRegion { get; set; } = string.Empty;
            public int BillingZipPostal { get; set; }
            public string BillingCountry { get; set; } = string.Empty;



            public string ShippingFirstName { get; set; } = string.Empty;
            public string ShippingLastName { get; set; } = string.Empty;
            public string ShippingCompanyName { get; set; } = string.Empty;
            public string ShippingPhone { get; set; } = string.Empty;
            public string ShippingAddressLine1 { get; set; } = string.Empty;
            public string ShippingAddressLine2 { get; set; } = string.Empty;
            public string ShippingCityTown { get; set; } = string.Empty;
            public string ShippingStateRegion { get; set; } = string.Empty;
            public int ShippingZipPostal { get; set; }
            public string ShippingCountry { get; set; } = string.Empty;
            public int BillingLocationId { get; set; }
            public int ShippingLocationId { get; set; }
            public int? CompanyId { get; set; }

        }

        public class ContactsList
        {
            public int id { get; set; }
            public string Tip { get; set; } = string.Empty;
            public string DisplayName { get; set; }=string.Empty;
            public string Mail { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Comment { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string? CompanyName { get; set; }


        }
        public class ContactsInsert
        {
            public string Tip { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Mail { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Comment { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string? CompanyName { get; set; }




        }

        public class ContactsItemFilter
        {
            public string? DisplayName { get; set; }
        }
        public class ContactsFilters
        {
            public int id { get; set; }
            public string Tip { get; set; }=string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Mail { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Comment { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string? CompanyName { get; set; }
        }

        public class ContactsDelete
        {
            public int id { get; set; }
            public string Tip { get; set; } = string.Empty;
        }
    }
}
