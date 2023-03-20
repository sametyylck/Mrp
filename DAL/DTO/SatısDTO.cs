using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class SatısDTO
    {


        public string? Tip { get; set; }
        public int? CariId { get; set; }
        public string SatisIsmi { get; set; } = string.Empty;
        public DateTime? TeslimSuresi { get; set; }
        public DateTime? OlusturmaTarihi { get; set; }
        public int? DepoId { get; set; }
        public string Bilgi { get; set; } = string.Empty;

    }
    public class SatısInsertItem
    {
        public int SatisId { get; set; }
        public int? StokId { get; set; }
        public int? DepoId { get; set; }
        public int? CariId { get; set; }
        public int? VergiId { get; set; }
        public float? Miktar { get; set; }
        public int Durum { get; set; }
        public int Conditions { get; set; }

    }
    public class TeklifInsertItem
    {
        public int SatisId { get; set; }
        public int? StokId { get; set; }
        public int? DepoId { get; set; }
        public int? CariId { get; set; }
        public int? VergiId { get; set; }
        public float? Miktar { get; set; }

    }
    public class TeklifUpdateItems
    {
        public int id { get; set; }
        public int SatisId { get; set; }
        public int CariId { get; set; }
        public int DepoId { get; set; }
        public int StokId { get; set; }
        public float Miktar { get; set; }
        public float BirimFiyat { get; set; }
        public int VergiId { get; set; }
        public DateTime TeslimSuresi { get; set; }
        public string? Bilgi { get; set; }
    }


    public class SatısDelete
    {
        public int id { get; set; }
    }
    public class SatısDeleteItems
    {
        public int id { get; set; }
        public int StokId { get; set; }
        public int OrdersId { get; set; }
    }
    public class SatısUpdateItems
    {
        public int id { get; set; }
        public string? Tip { get; set; }
        public int SatisId { get; set; }
        public int UretimId { get; set; }
        public int CariId { get; set; }
        public int DepoId { get; set; }
        public int StokId { get; set; }
        public float Miktar { get; set; }
        public float BirimFiyat { get; set; }
        public int VergiId { get; set; }
        public int Conditions { get; set; }
        public DateTime TeslimSuresi { get; set; }
        public string? Bilgi { get; set; }
    }
    public class SatisDetay
    {
        public int DepoId { get; set; }
        public int StokId { get; set; }
        public int id { get; set; }
        public int SatisId { get; set; }
        public float Miktar { get; set; }
    }




}
