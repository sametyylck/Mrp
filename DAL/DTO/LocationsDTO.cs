using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class LocationsDTO
    {
        public int id { get; set; }
        public string Tip { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string CityTown { get; set; } = string.Empty;
        public string StateRegion { get; set; } = string.Empty;
        public int? ZipPostalCode { get; set; }
        public string Country { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public bool? Sell { get; set; }
        public bool? Make { get; set; }
        public bool? Buy { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? CompanyId { get; set; }
    }
    public class LocationsInsert
    {
        public string Tip { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string CityTown { get; set; } = string.Empty;
        public string StateRegion { get; set; } = string.Empty;
        public int? ZipPostalCode { get; set; }
        public string Country { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public bool? Sell { get; set; }
        public bool? Make { get; set; }
        public bool? Buy { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
