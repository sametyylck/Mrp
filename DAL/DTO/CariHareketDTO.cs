using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class CariHareketDTO
    {
        public int SubeId { get; set; }
        public int DepoId { get; set; }
        public int ParaBirimiId { get; set; }
        public string EvrakNo { get; set; }
        public float Tutar { get; set; }
        public float Kur { get; set; }
        public int EvrakTipi { get; set; }
        public float KDVTutari { get; set; }
        public float AraToplam { get; set; }
        public DateTime? Tarih { get; set; }
        public DateTime? VadeTarihi { get; set; }
        public int CariKod { get; set; }
        public string CariAdSoyad { get; set; }
    }
    public class FaturaDTO
    {
        public int SubeId { get; set; }
        public int DepoId { get; set; }
        public int ParaBirimiId { get; set; }
        public string EvrakNo { get; set; }
        public int EvrakTipi { get; set; }

        public int CariKod { get; set; }
        public string CariAdSoyad { get; set; }
        public float GenelToplam { get; set; }
        public float AraToplam { get; set; }
        public float KDVTutari { get; set; }

        public DateTime FaturaTarihi { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
        public DateTime GuncellemeTarihi { get; set; }


    }

}
