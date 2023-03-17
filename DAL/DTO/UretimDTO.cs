using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class UretimDTO
    {
        public int? ParentId { get; set; }
        public string? Tip { get; set; }
        public string? Isim { get; set; }
        public int? StokId { get; set; }
        public DateTime? UretimTarihi { get; set; }
        public DateTime? OlusturmTarihi { get; set; }
        public DateTime? BeklenenTarih { get; set; }
        public float? PlananlananMiktar { get; set; }
        public int DepoId { get; set; }
        public string? Bilgi { get; set; }
        public bool Ozel { get; set; }
    }
    public class UretimDeleteItems
    {
        public int id { get; set; }
        public int StokId { get; set; }
        public int UretimId { get; set; }
    }
    public class UretimUpdate
    {
        public int id { get; set; }
        public float PlanlananMiktar { get; set; }
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
    public class UretimIngredientsInsert
    {
        public int? UretimId { get; set; }
        public int? StokId { get; set; }
        public string Bilgi { get; set; } = string.Empty;
        public float? Miktar { get; set; }
        public int? DepoId { get; set; }
        public float? Tutar { get; set; }
        public int MalzemeDurum { get; set; }
    }
    public class UretimOperationsInsert
    {
        public int UretimId { get; set; }
        public int OperasyonId { get; set; }
        public int? KaynakId { get; set; }
        public int? PlanlananZaman { get; set; }
        public int? Durum { get; set; }
        public float? Tutar { get; set; }
        public float? SaatlikUcret { get; set; }
    }
    public class UretimOperationsUpdate
    {
        public int? id { get; set; }
        public int? UretimId { get; set; }
        public int? OperasyonId { get; set; }
        public int? KaynakId { get; set; }
        public int? PlanlananZaman { get; set; }
        public int? Durum { get; set; }
        public float? SaatlikUcret { get; set; }
    }
    public class UretimIngredientsUpdate
    {
        public int? id { get; set; }
        public int? UretimId { get; set; }
        public int? StokId { get; set; }
        public string Bilgi { get; set; } = string.Empty;
        public float? Miktar { get; set; }
        public int? DepoId { get; set; }
        public float? Tutar { get; set; }
        public int MalzemeDurum { get; set; }
    }
    public class ItemKontrol 
    {
        public int? id { get; set; }

    }
    public class UretimTamamlama
    {
        public int id { get; set; }
        public int Status { get; set; }
    }
    public class UretimPurchaseOrder
    {
        public int? ManufacturingOrderId { get; set; }
        public int? ManufacturingOrderItemId { get; set; }

        public string Tip { get; set; } = null!;
        public int? ContactId { get; set; }
        public string? OrderName { get; set; }

        public DateTime? ExpectedDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? LocationId { get; set; }
        public int? ItemId { get; set; }
        public float Quantity { get; set; }

    }
    public class UretimOzelClass
    {
        public int id { get; set; }
        public float PlanlananMiktar { get; set; }
        public int? StokId { get; set; }
        public int? UretimDetayId { get; set; }

    }
    public class UretimDeleteKontrol
    {
        public int id { get; set; }

    }
    public class PurchaseBuy
    {
        public int? UretimId { get; set; }
        public int? UretimDetayId { get; set; }
        public int StokId { get; set; }
        public float Miktar { get; set; }
        public string Tip { get; set; } = null!;
        public int? TedarikciId { get; set; }
        public string? SatinAlmaIsim { get; set; }
        public DateTime? BeklenenTarih { get; set; }
        public int? DepoId { get; set; }
    }
    public class UretimSemiProduct
    {
        public int ParentId { get; set; }
        public string? Name { get; set; }
        public int? ItemId { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public float? PlannedQuantity { get; set; }
        public int LocationId { get; set; }
    }

    public class UretimMake
    {
        public int? id { get; set; }
        public int? OrderId { get; set; }
        public int? ItemId { get; set; }
        public string Note { get; set; } = string.Empty;
        public float? PlannedQuantity { get; set; }
        public int? LocationId { get; set; }

    }







}
