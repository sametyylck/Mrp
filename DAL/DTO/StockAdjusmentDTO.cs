using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class StockAdjusmentDTO
    {
        public class StockAdjusmentClas
        {
            public int id { get; set; }
            public string Isim { get; set; } = string.Empty;
            public string Sebeb { get; set; } = string.Empty;
            public DateTime Tarih { get; set; }
            public int DepoId { get; set; }
            public string DepoIsmi { get; set; } = string.Empty;
            public string Bilgi { get; set; } = string.Empty;
            public IEnumerable<StockAdjusmentItems> detay { get; set; }
        }
        public class StockAdjusmentUpdate
        {
            public int id { get; set; }
            public string Isim { get; set; } = string.Empty;
            public string Sebeb { get; set; } = string.Empty;
            public DateTime Tarih { get; set; }
            public int DepoId { get; set; }
            public string DepoIsmi { get; set; } = string.Empty;
            public string Bilgi { get; set; } = string.Empty;
        }
        public class StockAdjusmentItems
        {
            public int id { get; set; }
            public int StokId { get; set; }
            public string UrunIsmi { get; set; } = string.Empty;
            public float Miktar { get; set; }
            public float BirimFiyat { get; set; }
            public int StokDuzenlemeId { get; set; }
            public float Toplam { get; set; }
            public int StokMiktar { get; set; }
        }
        public class StockAdjusmentUpdateItems
        {
            public int? id { get; set; }
            public int StokId { get; set; }
            public float Miktar { get; set; }
            public float BirimFiyat { get; set; }
            public int StokDuzenlemeId { get; set; }
        }
        public class StockAdjusmentAll
        {
            public int id { get; set; }
            public int? StokSayimId { get; set; }
            public string Isim { get; set; } = string.Empty;
            public string Sebeb { get; set; } = string.Empty;
            public DateTime Tarih { get; set; }
            public int DepoId { get; set; }
            public bool Aktif { get; set; }
            public string Bilgi { get; set; } = string.Empty;
            public float? Toplam { get; set; }
            public int StokId { get; set; }
            public float Miktar { get; set; }
            public float BirimFiyat { get; set; }
            public int StokDuzenlemeId { get; set; }

        }
        public class StockAdjusmentInsert
        {
            public int? StokSayimId { get; set; }
            public string Isim { get; set; } = string.Empty;
            public string Sebeb { get; set; } = string.Empty;
            public DateTime Tarih { get; set; }
            public int? DepoId { get; set; }
            public string Bilgi { get; set; } = string.Empty;

        }
        public class StockAdjusmentInsertItem
        {
            public int? DepoId { get; set; }
            public int StokId { get; set; }
            public float? Miktar { get; set; }
            public float? BirimFiyat { get; set; }
            public int StokDuzenlemeId { get; set; }

        }
        public class StockAdjusmentList
        {
            public int id { get; set; }
            public string Isim { get; set; } = string.Empty;
            public string Sebeb { get; set; } = string.Empty;
            public float? Toplam { get; set; }
            public string DepoIsmi { get; set; } = string.Empty;

        }

        public class StockAdjusmentInsertResponse
        {
            public int id { get; set; }
            public int StokDuzenelemeId { get; set; }
            public string Isim { get; set; } = string.Empty;
            public string Sebeb { get; set; } = string.Empty;
            public DateTime Tarih { get; set; }
            public int DepoId { get; set; }
            public string DepoIsmi { get; set; } = string.Empty;
            public string Bilgi { get; set; } = string.Empty;
       }
        public class StockAdjusmentItemDelete
        {
            public int id { get; set; }
            public int StokId { get; set; }
            public int StokDuzenelemeId { get; set; }

        }

        public class StockAdjusmentStockUpdate
        {
            public int id { get; set; }
            public float? Miktar { get; set; }
            public float UretimMiktari { get; set; }
            public int DepoStokId { get; set; }
            public int StokAdeti { get; set; }
            public int StokId { get; set; }
            public string Tip { get; set; } = string.Empty;
            public float? RezerveDeger { get; set; }
        }
        public class StockAdjusmentSql
        {
            public int id { get; set; }
            public float? Miktar { get; set; }
            public float PlanlananMiktar { get; set; }
            public int DepoStokId { get; set; }
           public int StokAdeti { get; set; }
            public int StokId { get; set; }
            public string Tip { get; set; } = string.Empty;
            public float? RezerveDeger { get; set; }
        }
        public class LocaVarmı
        {
            public int id { get; set; }
            public int RezerveId { get; set; }
            public int MalzemeDurum { get; set; }
            public float DepoStoklar { get; set; }
            public float DepoStokId { get; set; }
            public string Tip { get; set; } = string.Empty;
            public float VarsayilanFiyat { get; set; }
            public int? RezerveStockCount { get; set; }
            public float? RezerveDeger { get; set; }
            public float OrdersItemCount { get; set; }
            public int StokId { get; set; }
            public int DepoId { get; set; }
            public float Miktari { get; set; }
            public int CariKod { get; set; }
            public int StokMiktar { get; set; }
            public float PlanlananMiktar { get; set; }
            public int VergiId { get; set; }
            public int VergiDeger { get; set; }
            public int Durum { get; set; }
            public float Kayıp { get; set; }



        }
        public class DeleteClas
        {
            public int id { get; set; }
            public string Tip { get; set; } = string.Empty;
        }
    }
}
