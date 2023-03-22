using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class StokHareketDTO
    {
        public int SubeId { get; set; }
        public int DepoId { get; set; }
        public int OlcuId { get; set; }
        public int? ParaBirimiId { get; set; }
        public string EvrakNo { get; set; }
        public int StokId { get; set; }
        public string StokKodu { get; set; }
        public string StokAd { get; set; }
        public float Miktar { get; set; }
        public float BirimFiyat { get; set; }
        public float Tutar { get; set; }
        public float? Kur { get; set; }
        public bool Giris { get; set; }
        public int EvrakTipi { get; set; }
        public float KDVOrani { get; set; }
        public float KDVTutari { get; set; }
        public float AraToplam { get; set; }
    }
}
