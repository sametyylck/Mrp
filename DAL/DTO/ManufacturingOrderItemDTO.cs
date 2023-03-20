using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class ManufacturingOrderItemDTO
    {
        public class DoneStock
        {
            private const string RegularExpression = @"^[a-zA-Z0-9 ğĞçÇşŞüÜöÖıİ'' ']+$";

            public int id { get; set; }
            [Required]
            public int MamulId { get; set; }
            [Required]
            public int StokId { get; set; }
            [Required]
            public int? ParentId { get; set; }
            public float PlanlananMiktar { get; set; }
            [RegularExpression(RegularExpression, ErrorMessage = "Özel karekter giremezsiniz")]
            [StringLength(50)]
            public string Bilgi { get; set; } = string.Empty;
            public string Tip { get; set; } = string.Empty;
            public int RezerveId { get; set; }
            public int Durum { get; set; }
            public int DepoId { get; set; }
            public int SatisId { get; set; }
            public int SatisDetayId { get; set; }
        }

        public class ManufacturingOrderItems
        {
            public int id { get; set; }
            [StringLength(50)]
            public string Tip { get; set; } = string.Empty;
            public int? OrderId { get; set; }
            public int? ItemId { get; set; }
            [StringLength(50)]
            public string Notes { get; set; } = string.Empty;
            public double? PlannedQuantity { get; set; }
            public double? Cost { get; set; }
            //(0 = Not Available) (1 = Expected) (2 = InStock)
            public int? Availability { get; set; }
            public int? OperationId { get; set; }
            [StringLength(10)]
            public string ResourceId { get; set; } = string.Empty;
            public int? PlannedTime { get; set; }
            // (0 = Not Started) (1 = In Progress) (2 = Paused) (3 = Completed) (4 = Blocked)
            public int? Status { get; set; }
            public int? PurchaseOrderId { get; set; }
        }
        public class BuyKontrol
        {
            public int OlcuId { get; set; }
            public int VergiId { get; set; }

        }


        public class ManufacturingOrderItemsIngredientsDetail
        {
            public int id { get; set; }
            public int? StokId { get; set; }
            public string Isim { get; set; } = string.Empty;
            public string Bilgi { get; set; } = string.Empty;
            public float PlanlananMiktar { get; set; }
            public float Tutar { get; set; }
            public int? MalzemeDurum { get; set; }
            public float Kayip { get; set; }

        }
        public class ManufacturingOrderItemsIngredientsUpdate
        {
            public int? id { get; set; }
            public int? SatisId { get; set; }
            public int? SatisDetayId { get; set; }
            public int? UretimId { get; set; }
            public int? StokId { get; set; }
            public float? Miktar { get; set; }
            public int? DepoId { get; set; }
            public float? Tutar { get; set; }
            public int MalzemeDurumu { get; set; }
            public string Bilgi { get; set; }
        }
        public class ManufacturingOrderItemsIngredientsInsert
        {
            public int? SatisId { get; set; }
            public int? SatisDetayId { get; set; }
            public int? UretimId { get; set; }
            public int? StokId { get; set; }
            public float? Miktar { get; set; }
            public int? DepoId { get; set; }
            public float? Tutar { get; set; }
            public int MalzemeDurumu { get; set; }
            public string Bilgi { get; set; }
        }

        public class ManufacturingOrderItemsOperationsUpdate
        {
            public int? id { get; set; }
            public int? UretimId { get; set; }
            public int? OperasyonId { get; set; }
            public int? KaynakId { get; set; }
            public int? PlanlananZaman { get; set; }
            public int? Durum { get; set; }
            public float? SaatlikUcret { get; set; }
        }
        public class ManufacturingOrderItemsOperationsInsert
        {
            public int UretimId { get; set; }
            public int OperasyonId { get; set; }
            public int? KaynakId { get; set; }
            public int? PlanlananZaman { get; set; }
            public int? Durum { get; set; }
            public float? Tutar { get; set; }
            public float? SaatlikUcret { get; set; }
        }


        public class ManufacturingOrderItemsOperationDetail
        {
            public int id { get; set; }
            public int? OperasyonId { get; set; }
            public string OperasyonIsmi { get; set; } = string.Empty;
            public int? KaynakId { get; set; }
            public string KaynakIsmi { get; set; } = string.Empty;
            public int? PlanlananZaman { get; set; }
            public float SaatlikUcret { get; set; }

            public float Tutar { get; set; }
            public int? Durum { get; set; }
        }
        public class Quantitys
        {
            public float Miktar { get; set; }
            public float PlanlananMiktar { get; set; }

        }
        public class Manufacturing
        {
            public float ToplamCount { get; set; }
            public int DepoStokOd { get; set; }
            public float ToplamRezerveCount { get; set; }
            public float RezerveAdet { get; set; }
            public int StokId { get; set; }



        }
        public class ManufacturingPurchaseOrder
        {
            public int? SatisId { get; set; }
            public int? SatisDetayId { get; set; }
            public int? UretimId { get; set; }
            public int? UretimDetayId { get; set; }

            public string Tip { get; set; } = null!;
            public int? CariKod { get; set; }
            public string? SatinAlimIsmi { get; set; }

            public DateTime? BeklenenTarih { get; set; }
            public DateTime? OlusturmaTarihi { get; set; }
            public int? DepoId { get; set; }
            public int? StokId { get; set; }
            public float Miktar { get; set; }

        }
        public class UretimDeleteKontrolClas
        {
            public int? SatisId { get; set; }
            public int? SatisDetayId { get; set; }

            public string? Isim { get; set; }

        }


    }
}
