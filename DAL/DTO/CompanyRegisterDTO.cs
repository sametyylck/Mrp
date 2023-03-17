using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DAL.DTO
{
 
    public class CompanyRegisterDTO
    {

        public string DisplayName { get; set; } = string.Empty;

        public string LegalName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
        public string? Mail { get; set; }

        public string Password { get; set; } = string.Empty;


    }
    public class TokenKontrol
    {
        public int id { get; set; }
        public string DisplayName { get; set; } = string.Empty;

        public string Ad { get; set; } = string.Empty;

        public string Soyisim { get; set; } = string.Empty;

        public string Telefon { get; set; } = string.Empty;
        public string? Mail { get; set; }

        public string Sifre { get; set; } = string.Empty;


    }

    public class CompanyClas
    {
        public int id { get; set; }
        public string DisplayName { get; set; }=string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string CityTown { get; set; } = string.Empty;
        public string StateRegion { get; set; } = string.Empty;
        public int? ZipPostalCode { get; set; }
        public string Country { get; set; } = string.Empty;
        public string Tip { get; set; } = string.Empty;



    }
    public class CompanyInsert
    {
        public string Adres1 { get; set; } = string.Empty;
        public string Adres2 { get; set; } = string.Empty;
        public string Sehir { get; set; } = string.Empty;
        public string Cadde { get; set; } = string.Empty;
        public int? PostaKodu { get; set; }
        public string Ulke { get; set; } = string.Empty;


    }

    public class CompanyUpdate
    {
        public int id { get; set; }
        public string Adres1 { get; set; } = string.Empty;
        public string Adres2 { get; set; } = string.Empty;
        public string Sehir { get; set; } = string.Empty;
        public string Cadde { get; set; } = string.Empty;
        public int? PostaKodu { get; set; }
        public string Ulke { get; set; } = string.Empty;


    }
    public class CompanyUpdateCompany
    {
        public string GorunenIsim { get; set; } = string.Empty;
        public string GercekIsim { get; set; } = string.Empty;



    }
    public class IdControl    {
        public int id { get; set; }
    }
    public class CompanyService
    {
        public int CompanyId { get; set; }
        public int UserId { get; set; }
    }
}
