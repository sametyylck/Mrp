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
            public string AdresAd { get; set; } = string.Empty;
            public string AdresSoyisim { get; set; } = string.Empty;
            public string AdresSirket { get; set; } = string.Empty;
            public string AdresTelefon { get; set; } = string.Empty;
            public string Adres1 { get; set; } = string.Empty;
            public string Adres2 { get; set; } = string.Empty;
            public string AdresSehir { get; set; } = string.Empty;
            public string AdresCadde { get; set; } = string.Empty;
            public int AdresPostaKodu { get; set; }
            public string AdresUlke { get; set; } = string.Empty;

        }

        public class ContactsAll
        {

            public int CariKod { get; set; }
            public string Tip { get; set; } = string.Empty;
            public string AdSoyad { get; set; } = string.Empty;
            public string VergiDairesi { get; set; } = string.Empty;
            public string VergiNumarası { get; set; } = string.Empty;
            public int CariTipId { get; set; }
            public string Mail { get; set; } = string.Empty;
            public string Telefon { get; set; } = string.Empty;
            public string FaturaAdresIdAd { get; set; } = string.Empty;
            public string FaturaAdresIdSoyIsim { get; set; } = string.Empty;
            public string FaturaAdresIdSirketIsmi { get; set; } = string.Empty;
            public string FaturaAdresIdTelefon { get; set; } = string.Empty;
            public string FaturaAdresIdAdres1 { get; set; } = string.Empty;
            public string FaturaAdresIdAdres2 { get; set; } = string.Empty;
            public string FaturaAdresIdSehir { get; set; } = string.Empty;
            public string FaturaAdresIdCadde { get; set; } = string.Empty;
            public int FaturaAdresIdPostaKodu { get; set; }
            public string FaturaAdresIdUlke { get; set; } = string.Empty;

            public string KargoAdresIdAdresIdAd { get; set; } = string.Empty;
            public string KargoAdresIdAdresIdSoyIsim { get; set; } = string.Empty;
            public string KargoAdresIdAdresIdSirketIsmi { get; set; } = string.Empty;
            public string KargoAdresIdAdresIdTelefon { get; set; } = string.Empty;
            public string KargoAdresIdAdresIdAdres1 { get; set; } = string.Empty;
            public string KargoAdresIdAdresIdAdres2 { get; set; } = string.Empty;
            public string KargoAdresIdAdresIdSehir { get; set; } = string.Empty;
            public string KargoAdresIdAdresIdCadde { get; set; } = string.Empty;
            public int KargoAdresIdPostaKodu { get; set; }
            public string KargoAdresIdUlke { get; set; } = string.Empty;
            public int FaturaAdresiId { get; set; }
            public int KargoAdresiId { get; set; }

        }

        public class ContactsList
        {
            public int CariKod { get; set; }
            public int CariTipId { get; set; }
            public string GorunenIsim { get; set; }=string.Empty;
            public string Mail { get; set; } = string.Empty;
            public string Telefon { get; set; } = string.Empty;
            public string AdSoyad { get; set; } = string.Empty;
            public string? SirketIsmi { get; set; }


        }
        public class ContactsInsert
        {
            public string AdSoyad { get; set; } = string.Empty;
            public string VergiDairesi { get; set; } = string.Empty;
            public string VergiNumarası { get; set; } = string.Empty;
            public int CariTipId { get; set; }
            public string Mail { get; set; } = string.Empty;
            public string Telefon { get; set; } = string.Empty;
            public int? ParaBirimiId { get; set; }




        }
        public class CariUpdate
        {
            public int CariKod { get; set; }
            public string AdSoyad { get; set; } = string.Empty;
            public string VergiDairesi { get; set; } = string.Empty;
            public string VergiNumarası { get; set; } = string.Empty;
            public int CariTipId { get; set; }
            public string Mail { get; set; } = string.Empty;
            public string Telefon { get; set; } = string.Empty;
            public int? ParaBirimiId { get; set; }




        }


        public class ContactsItemFilter
        {
            public string? GorunenIsim { get; set; }
        }
        public class CariTip
        {
            public int id { get; set; }
            public string KartTipi { get; set; }
        }
        public class ContactsFilters
        {
            public int CariKod { get; set; }
            public string AdSoyad { get; set; } = string.Empty;
            public string VergiDairesi { get; set; } = string.Empty;
            public string VergiNumarası { get; set; } = string.Empty;
            public int CariTipId { get; set; }
            public string Mail { get; set; } = string.Empty;
            public string Telefon { get; set; } = string.Empty;
            public int? ParaBirimiId { get; set; }

        }

        public class ContactsDelete
        {
            public int id { get; set; }
        }
    }
}
