using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class StockTakesDTO
    {
        public class StockTakes
        {
            public int id { get; set; }
            public string StockTake { get; set; } = string.Empty;
            public DateTime OlusturmaTarihi { get; set; }
            public string Sebeb { get; set; } = string.Empty;
            public string Bilgi { get; set; } = string.Empty;
            public int DepoId { get; set; }
            public int StokId { get; set; }
            public float? SayilanMiktar { get; set; }
            public float? EksikMiktar { get; set; }

        }
        public class StockTakesUpdate
        {
            public int id { get; set; }
            public string Isim { get; set; } = string.Empty;
            public DateTime OlusturmaTarihi { get; set; }
            public DateTime BaslangıcTarihi { get; set; }
            public DateTime BitisTarihi { get; set; }

            public string Sebeb { get; set; } = string.Empty;
            public string Bilgi { get; set; } = string.Empty;

        }
        public class StockTakesUpdateItems
        {
            public int? StokSayimId { get; set; }
            public int? StokSayimDetayId { get; set; }
            public float? SayilanMiktar { get; set; }
            public string? Bilgi { get; set; }

        }

        public class StockTakesInsert
        {
            public string Isim { get; set; } = string.Empty;
            public DateTime OlusturmaTarihi { get; set; }
            public string Sebeb { get; set; } = string.Empty;
            public string Bilgi { get; set; } = string.Empty;
            public int DepoId { get; set; }

        }
        public class StockTakeList
        {
            public int? id { get; set; }
            public string StockTake { get; set; } = string.Empty;
            public DateTime? OlusturmaTarihi { get; set; }
            public string Sebeb { get; set; } = string.Empty;
            public string Bilgi { get; set; } = string.Empty;
            public int? DepoId { get; set; }
            public string DepoIsmi { get; set; } = string.Empty;
            public int? StokDuzenlemeId { get; set; }
            public string Isim { get; set; } = string.Empty;
            public int? Durum { get; set; }
            public DateTime? BitisTarihi { get; set; }

        }

        public class StockTakeItems
        {
            public int id { get; set; }
            public int StokSayimId { get; set; }
            public int StokId { get; set; }
            public int KategoriId { get; set; }
            public string? Bilgi { get; set; }
            public int InStock { get; set; }
            public float? SayilanMiktar { get; set; }
            public float? EksikMiktar { get; set; }

        }
        public class StockTakeInsertItems
        {
            public int? StokSayimId { get; set; }
            public int? StokId { get; set; }
            public string? Bilgi { get; set; }

        }
        public class StockTakeInsertItemsResponse
        {
            public int id { get; set; }
            public int? StokSayimId { get; set; }
            public string? Isim { get; set; }
            public float InStock { get; set; }
            public int? StokId { get; set; }
            public string? Bilgi { get; set; }

        }
        public class StockTakeDelete
        {
            public int? id { get; set; }
            public int? StokId { get; set; }
        }
        public class StockTakesDetail
        {
            public int id { get; set; }
            public string? StockTake { get; set; }
            public DateTime OlusturmaTarihi { get; set; }
            public string? Sebeb { get; set; }
            public string? Bilgi { get; set; }
            public int DepoId { get; set; }
            public int? StokDuzenlemeId { get; set; }
            public DateTime CompletedDate { get; set; }
            public int Durum { get; set; }
            public int Aktif { get; set; }

        }
        public class StockTakesDone
        {
            public int id { get; set; }
            public int Durum { get; set; }
        }
    }
}
