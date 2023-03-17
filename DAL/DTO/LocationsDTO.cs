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
        public string Isim { get; set; } = string.Empty;
        public string Adres1 { get; set; } = string.Empty;
        public string Adres2 { get; set; } = string.Empty;
        public string Sehir { get; set; } = string.Empty;
        public string Cadde { get; set; } = string.Empty;
        public int? PostaKodu { get; set; }
        public string Ulke { get; set; } = string.Empty;
        public string GercekIsim { get; set; } = string.Empty;
        public bool? Satis { get; set; }
        public bool? Uretim { get; set; }
        public bool? SatinAlma { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyisim { get; set; } = string.Empty;
        public string SirketIsmi { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
    }
    public class LocationsInsert
    {
        public string Tip { get; set; } = string.Empty;
        public string Isim { get; set; } = string.Empty;
        public string Adres1 { get; set; } = string.Empty;
        public string Adres2 { get; set; } = string.Empty;
        public string Sehir { get; set; } = string.Empty;
        public string Cadde { get; set; } = string.Empty;
        public int? PostaKodu { get; set; }
        public string Ulke { get; set; } = string.Empty;
        public string GercekIsim { get; set; } = string.Empty;
        public bool? Satis { get; set; }
        public bool? Uretim { get; set; }
        public bool? SatinAlma { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyisim { get; set; } = string.Empty;
        public string SirketIsmi { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
    }
}
