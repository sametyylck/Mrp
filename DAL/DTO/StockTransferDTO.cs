using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class StockTransferDTO
    {
        public class StokAktarimDetay
        {
            public int id { get; set; }
            public int StokId { get; set; }
            public float Miktar { get; set; }
            public int StokAktarimId { get; set; }
        }
        public class StockTransferAll
        {
            public int id { get; set; }
            public string AktarimIsmi { get; set; } = string.Empty;
            public DateTime AktarmaTarihi { get; set; }
            public int? BaslangicDepo { get; set; }

            public int? HedefDepo { get; set; }
            public string Bilgi { get; set; } = string.Empty;
            public int? StokId { get; set; }
            public float? Miktar { get; set; }
        }
        public class StockTransferInsert
        {
            public string AktarimIsmi { get; set; } = string.Empty;
            public DateTime AktarmaTarihi { get; set; }
            public int BaslangicDepo { get; set; }
            public int HedefDepo { get; set; }
            public string Bilgi { get; set; } = string.Empty;
            public int StokId { get; set; }
            public float Miktar { get; set; }
            public int SubeId { get; set; }

        }
        public class StockTransferInsertItem
        {
            public int StokId { get; set; }
            public float Miktar { get; set; }
            public int StokAktarimId { get; set; }
        }
        public class StockUpdate
        {
            public int id { get; set; }
            public string AktarimIsmi { get; set; }
            public float Toplam { get; set; }
            public DateTime AktarmaTarihi { get; set; }
            public string Bilgi { get; set; }
        }
        public class StockTransferList
        {
            public int id { get; set; }
            public string AktarimIsmi { get; set; } = string.Empty;
            public DateTime AktarmaTarihi { get; set; }

            public int HedefDepo { get; set; }
            public string HedefDepoIsmi { get; set; } = string.Empty;
            public int BaslangicDepo { get; set; }
            public string BaslangicDepoIsmi { get; set; } = string.Empty;
            public string Bilgi { get; set; } = string.Empty;
            public string Toplam { get; set; } = string.Empty;
        }

        public class StockTransferDetails
        {
            public int id { get; set; }
            public string AktarimIsmi { get; set; } = string.Empty;
            public DateTime AktarimTarihi { get; set; }

            public int HedefDepo { get; set; }
            public string HedefDepoIsmi { get; set; } = string.Empty;
            public int BaslangicDepo { get; set; }
            public string BaslangıcDepoIsmi { get; set; } = string.Empty;
            public string Bilgi { get; set; } = string.Empty;
            public string Toplam { get; set; } = string.Empty;
            public IEnumerable<StockTransferDetailsItems> detay { get; set; }
        }
        public class StockTransferDelete
        {
            public int id { get; set; }
        }
        public class StockTransferDeleteItems
        {
            public int id { get; set; }
            public int StokAktarimId { get; set; }

            public int StokId { get; set; }
        }

        public class StockMergeSql
        {
            public int VarsayilanFiyat { get; set; }
            public string Tip { get; set; } = string.Empty;
            public int BaslangicDepo { get; set; }
            public float BaslangicDepoStokAdeti { get; set; }
            public int HedefDepo { get; set; }
            public float HedefDepoStokAdeti { get; set; }
            public float? Miktar { get; set; }
            public int StokId { get; set; }
            public int originvarmi { get; set; }
            public int stockCountOrigin { get; set; }
            public int destinationvarmı { get; set; }
            public int? DestinationStockCounts { get; set; }
            public float? RezerveCountOrigin { get; set; }
            public float? RezerveCountDestination { get; set; }

        }

        public class StockTransferDetailsItems
        {
            public int id { get; set; }
            public int StokId { get; set; }
            public string UrunIsmi { get; set; } = string.Empty;
            public float Miktar { get; set; }
            public int BaslangıcDepo { get; set; }
            public string BaslangıcDepoIsmi { get; set; } = string.Empty;
            public float BaslangıcDepoStokAdeti { get; set; }
            public int HedefDepo { get; set; }
            public float BirimFiyat { get; set; }
            public string HedefDepoIsim { get; set; } = string.Empty;
            public float HedefDepoStokAdeti { get; set; }

            public float TransferUcreti { get; set; }
        }
        public class StockTransferDetailsResponse
        {
            public int id { get; set; }
            public int StokId { get; set; }
            public string UrunIsmi { get; set; } = string.Empty;
            public float Miktar { get; set; }
            public int BaslangicDepo { get; set; }
            public int HedefDepo { get; set; }
            public float VarsayilanFiyat { get; set; }
            public string StokKodu { get; set; }
            public string Tip { get; set; }
            public int BaslangicStokAdeti { get; set; }
            public int HedefStokAdeti { get; set; }
            public int OlcuId { get; set; }
            public int SubeId { get; set; }
            public string EvrakNo { get; set; }

        }


        public class StockTransferItems
        {
            public int? id { get; set; }
            public int StokAktarimId { get; set; }
            public int? StokId { get; set; }
            public float Miktar { get; set; }
        }
    }
}
