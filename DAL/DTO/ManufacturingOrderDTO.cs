using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderItemDTO;

namespace DAL.DTO
{
    public class ManufacturingOrderDTO
    {
        public class ManufacturingOrderA
        {
            public string? Tip { get; set; } //MakeOrder mı yoksa Make in Batch mi kontrol için
            public int? SatisId { get; set; }
            public int? SatisDetayId { get; set; }
            public string? Isim { get; set; } 
            public int? StokId { get; set; }
            public DateTime? UretimTarihi { get; set; }
            public DateTime? OlusturmaTarihi { get; set; }
            public DateTime? BeklenenTarih { get; set; }
            public float? PlanlananMiktar { get; set; }
            public int DepoId { get; set; }
            public int? CariKod { get; set; }
            public string? Bilgi { get; set; } 
            public bool Ozel { get; set; }



        }
        public class SalesOrderMake
        {
            public string? Isim { get; set; }
            public string? Tip { get; set; } //MakeOrder mı yoksa Make in Batch mi kontrol için
            public int? SatisId { get; set; }
            public int? SatisDetayId { get; set; }

            public int? StokId { get; set; }

            public DateTime? UretimTarihi { get; set; }

            public DateTime? OlusturmaTarihi { get; set; }

            public DateTime BeklenenTarih { get; set; }

            public float? PlanlananMiktar { get; set; }

            public int DepoId { get; set; }
            public int? CariId { get; set; }

        }
        public class ManufacturingOrderUpdate
        {
            public int id { get; set; }
            public int SatisId { get; set; }
            public int SatisDetayId { get; set; }
            public float? PlanlananMiktar { get; set; }
            public int? StokId { get; set; }
            public string Isim { get; set; } = string.Empty;
            [Column(TypeName = "datetime2")]
            public DateTime? UretimTarihi { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime? OlusturmaTarihi { get; set; }
            public DateTime? BeklenenTarih { get; set; }
            public int DepoId { get; set; }
            public string Bilgi { get; set; } = string.Empty;
            // (0 = Not Started) (1 = Blocked) (2 = Work In Progress) (3 = Done) 
            public float MalzemeTutarı { get; set; }
            public float OperasyonTutarı { get; set; }
            public float ToplamTutar { get; set; }
            public int? Durum { get; set; }
            public float eskiPlanned { get; set; }
            public float eskiLocation { get; set; }
        }
        public class ManufacturingStock
        {
            public int id { get; set; }
            public int? SatisId { get; set; }
            public int? SatisDetayId { get; set; }
            public int Status { get; set; }
        }
        public class ManufacturingTaskDone
        {
            public int id { get; set; }
            public int UretimId { get; set; }
            public int Durum { get; set; }
        }

        public class ManufacturingDeleteItems
        {
            public int id { get; set; }
            public int StokId { get; set; }
            public int OrdersId { get; set; }
            public int? Quotes { get; set; }
        }

        public class ManufacturingOrderUpdatePlanned
        {
            public int id { get; set; }
            public float? PlannedQuantity { get; set; }
            public int? StokId { get; set; }
            public string Isim { get; set; } = string.Empty;
            [Column(TypeName = "datetime2")]
            public DateTime? UretimTarihi { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime? OlusturmaTarihi { get; set; }
            public int DepoId { get; set; }
            public string Bilgi { get; set; } = string.Empty;
            // (0 = Not Started) (1 = Blocked) (2 = Work In Progress) (3 = Done) 
            public int? Status { get; set; }
        }
        public class sorgubul
        {
            public float VarsayilanFiyat { get; set; }
            public int LocationStockCount { get; set; }
        }

        public class ManufacturingOrderDetail
        {
            public int id { get; set; }
            public int SatisId { get; set; }
            public int SatisDetayId { get; set; }
            public string Isim { get; set; } = string.Empty;
            public int? StokId { get; set; }
            public string UrunIsmi { get; set; } = string.Empty;
            public DateTime? UretimTarihi { get; set; }
            public DateTime? OlusturmaTarihi { get; set; }
            public DateTime BeklenenTarih { get; set; }
            public float? PlanlananMiktar { get; set; }
            public int DepoId { get; set; }
            public string DepoIsmi { get; set; } = string.Empty;
            public int? Durum { get; set; }
            public string Bilgi { get; set; } = string.Empty;
            public IEnumerable<ManufacturingOrderItemsIngredientsDetail> IngredientDetail { get; set; }
            public IEnumerable<ManufacturingOrderItemsOperationDetail> OperationDetail { get; set; }

        }
        public class ManufacturingOrderList
        {
            public int id { get; set; }
            public string Isim { get; set; } = string.Empty;
            public int? CustomerId { get; set; }
            public string Customer { get; set; } = string.Empty;
            public int? StokId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public float? PlannedQuantity { get; set; }
            public int? PlannedTime { get; set; }
            public int? Status { get; set; }

            // (0 = Not Started) (1 = Blocked) (2 = Work In Progress) (3 = Done) 
            public string Availability { get; set; } = string.Empty;
            public int? DepoId { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public DateTime? BaslangıcTarih { get; set; }
            public DateTime? SonTarih { get; set; }
        }
        public class ManufacturingOrderListArama
        {
            public string Isim { get; set; } = string.Empty;
            public string CariAdSoyAd { get; set; } = string.Empty;
            public string UrunIsmi { get; set; } = string.Empty;
            public string KategoriIsmi { get; set; } = string.Empty;
            public float? PlanlananMiktar { get; set; }
            public int? PlanlananZaman { get; set; }
            public int? Durum { get; set; }

            // (0 = Not Started) (1 = Blocked) (2 = Work In Progress) (3 = Done) 
            public string MalzemeDurumu { get; set; } = string.Empty;
            public int? DepoId { get; set; }
            public DateTime? BaslangıcTarih { get; set; }
            public DateTime? SonTarih { get; set; }
        }

        public class ManufacturingOrderDoneList
        {
            public int id { get; set; }
            public string Isim { get; set; } = string.Empty;
            public int? CariId { get; set; }
            public string CariAdSoyad { get; set; } = string.Empty;
            public int? StokId { get; set; }
            public string UrunIsmi { get; set; } = string.Empty;
            public string KategoriIsmi { get; set; } = string.Empty;
            public float? PlanlananMiktar { get; set; }
            public int? PlanlananZaman { get; set; }
            public int? Durum { get; set; }
            // (0 = Not Started) (1 = Blocked) (2 = Work In Progress) (3 = Done) 
            public int? DepoId { get; set; }
            public string? DepoIsmi { get; set; } = string.Empty;
            public float? MalzemeFiyati { get; set; }
            public float? OperasyonFiyati { get; set; }
            public float? TotalCost { get; set; }
            public DateTime? BaslangıcTarih { get; set; }
            public DateTime? SonTarih { get; set; }
        
        }
        public class ManufacturingOrderDoneListArama
        {

            public string Isim { get; set; } = string.Empty;
            public string CariAdSoyad { get; set; } = string.Empty;
            public string UrunIsmi { get; set; } = string.Empty;
            public string KategoriIsmi { get; set; } = string.Empty;
            public float? PlanlananMiktar { get; set; }
            public int? PlanlananZaman { get; set; }

            public int? DepoId { get; set; }
            public float? MalzemeFiyati { get; set; }
            public float? OperasyonFiyati { get; set; }
            public float? ToplamTutar { get; set; }
            public DateTime? BaslangıcTarih { get; set; }
            public DateTime? SonTarih { get; set; }

        }

        public class ManufacturingOrderResponse
        {
            public int id { get; set; }
            public int? StokId { get; set; }
            public string Isim { get; set; } = string.Empty;
            public string Tip { get; set; } = string.Empty;
            public float Miktar { get; set; }
            public string Note { get; set; } = string.Empty;
            public float SaatlikUcret { get; set; }
            public float PlanlananZaman { get; set; }
            public float Tutar { get; set; }
            public int MalzemeDurum { get; set; }
            public int Durum { get; set; }
        }
        public class ManufacturingOrderOperations
        {
            public int id { get; set; }
            public int KaynakId { get; set; }
            public string KaynakIsmi { get; set; } = string.Empty;
            public int OperasyonId { get; set; }
            public string OperasyonIsmi { get; set; } = string.Empty;
            public float SaatlikUcret { get; set; }
            public float PlanlananZaman { get; set; }
            public float Tutar { get; set; }
            public int UretimId { get; set; }
            public int Durum { get; set; }
        }
        public class ManufacturingTask
        {
            public int id { get; set; }
            public int? UretimId { get; set; }
            public string UretimIsim { get; set; } = string.Empty;
            public int? KaynakId { get; set; }
            public string KaynakIsmi { get; set; } = string.Empty;
            public int? OperasyonId { get; set; }
            public string OperasyonIsmi { get; set; } = string.Empty;
            public int? StokId { get; set; }
            public string UrunIsim { get; set; } = string.Empty;
            public DateTime UretimTarihi { get; set; }
            public float? PlanlananMiktar { get; set; }
            public float? PlanlananZaman { get; set; }
            public int? DepoId { get; set; }
            public DateTime TamamlamaTarihi { get; set; }
            public int? Durum { get; set; }
        }
        public class ManufacturingTaskArama
        {
            public string UretimIsmi { get; set; } = string.Empty;
            public string KaynakIsmi { get; set; } = string.Empty;
            public string OperasyonIsmi { get; set; } = string.Empty;
            public string UrunIsmi { get; set; } = string.Empty;
            public DateTime UretimTarihi { get; set; }
            public float? PlanlananMiktar { get; set; }
            public float? PlanlananZaman { get; set; }
            public int? DepoId { get; set; }
            public DateTime TamamlamaTarihi { get; set; }
            public int? Durum { get; set; }
        }

    }
}
