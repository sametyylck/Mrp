using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class PurchaseOrderDTO
    {
        public class OrdersResponse
        {
            public int id { get; set; }
            public int SalesOrderItemId { get; set; }
            public int SalesOrderId { get; set; }
            public int ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public float? Quantity { get; set; }
            public int TaxId { get; set; }
            public string TaxName { get; set; } = string.Empty;
            public int TaxValue { get; set; }
            public int MeasureId { get; set; }
            public string MeasureName { get; set; } = string.Empty;
            public float? PricePerUnit { get; set; }
            public float? TotalPrice { get; set; }
            public float? TotalAll { get; set; }
            public int SalesItem { get; set; }
            public int Ingredients { get; set; }
            public int Production { get; set; }

        }
        public class PurchaseOrder
        {
            public int id { get; set; }
            public int? SatisId { get; set; }
            public int? SatisDetayId { get; set; }
            public int? UretimId { get; set; }
            public int? UretimDetayId { get; set; }
            public int SubeId { get; set; }
            public string Tip { get; set; } = null!;
            public int? TedarikciId { get; set; }
            public string? SatinAlmaIsmi { get; set; } 

            public DateTime? BeklenenTarih { get; set; }
            public DateTime? OlusturmaTarihi { get; set; }
            public int DepoId { get; set; }
            public int? StokId { get; set; }
            public float Miktar { get; set; }
            public float? BirimFiyat { get; set; }
            public float? TumToplam { get; set; }
            public float? ToplamTutar { get; set; }
            public int OlcuId { get; set; }
            public float VergiMiktari { get; set; }
            public int VergiId { get; set; }
            public int VergiDegeri { get; set; }
            public string Bilgi { get; set; } = string.Empty;
            public int DurumBelirteci { get; set; }
            public float Kayip { get; set; }

        }
        public class PurchaseOrderInsertItem
        {
            public int SatinAlmaId { get; set; }
            public int? StokId { get; set; }
            public float Miktar { get; set; }
            public float? BirimFiyat { get; set; }
            public int OlcuId { get; set; }
            public int VergiId { get; set; }
        }
        public class PurchaseOrderInsert
        {
            public int? SatisId { get; set; }
            public int? SatisDetayId { get; set; }
            public int? UretimId { get; set; }
            public int? UretimDetayId { get; set; }
            public int VergiId { get; set; }
            public int StokId { get; set; }
            public float Miktar { get; set; }
            public int OlcuId { get; set; }
            public string Tip { get; set; } = null!;
            public int? TedarikciId { get; set; }
            public string? SatinAlmaIsmi { get; set; }
            public DateTime? BeklenenTarih { get; set; }
            public DateTime? OlusturmaTarihi { get; set; }
            public int SubeId { get; set; }
            public int? DepoId { get; set; }
            public string Bilgi { get; set; } = string.Empty;
        }
        public class PurchaseOrderList
        {
            public int id { get; set; }
            public string Tip { get; set; } = string.Empty;
            public int? ContactId { get; set; } 
            public string OrderName { get; set; } = string.Empty;
            public DateTime? ExpectedDate { get; set; }

            public DateTime? CreateDate { get; set; }
            public float? OrdersTotalAll { get; set; }
            public float? OrdersTotalPrice { get; set; }
            public int? LocationId { get; set; }
            public int? ItemId { get; set; }
            public string? Name { get; set; }
            public float? Quantity { get; set; }
            public int MeasureId { get; set; }
            public string? MeasureName { get; set; }
            public int TaxId { get; set; }
            public int TaxValue { get; set; }
            public string Info { get; set; } = string.Empty;
        }
        public class PurchaseOrderUpdate
        {
            public int id { get; set; }
            public int SubeId { get; set; }
            public int? TedarikciId { get; set; }
            public string SatinAlmaIsmi { get; set; } = string.Empty;
            public DateTime? BeklenenTarih { get; set; }
            public float TumTutar { get; set; }
            public DateTime? OlusturmaTarihi { get; set; }
            public int? DepoId { get; set; }
            public string Bilgi { get; set; } = string.Empty;
        }
        public class PurchaseItemControl
        {
            public int? id { get; set; }
            public int? StokId { get; set; }
            public int? VergiId { get; set; }
            public float? VergiDegeri { get; set; }
            public float? VergiMiktari { get; set; }
            public float? Miktar { get; set; }
            public int? OlcuId { get; set; }
            public float? BirimFiyat { get; set; }
            public int? SatinAlmaId { get; set; }
            public string? Tip { get; set; }
            public int? DepoId { get; set; }
            public int? CariId { get; set; }
            public int? SatisId { get; set; }
            public int? SatisDetayId { get; set; }
            public int? IdControl { get; set; }

        }
        public class PurchaseItemControl2
        {
            public int? id { get; set; }
            public int? StokId { get; set; }
            public int? VergiId { get; set; }
            public float? VergiDegeri { get; set; }
            public float? VergiMiktari { get; set; }
            public float? Miktar { get; set; }
            public int? OlcuId { get; set; }
            public float? BirimFiyat { get; set; }
            public int? SatinAlmaId { get; set; }
            public string? Tip { get; set; }
            public int? DepoId { get; set; }
            public int? ContactId { get; set; }
            public string? CariTip { get; set; }
            public int? SatisId { get; set; }
            public int? SatisDetayId { get; set; }
            public int? IdControl { get; set; }
            public int? UretimId { get; set; }
            public int? UretimDetayId { get; set; }

        }


        public class PurchaseItem
        {
            public int id { get; set; }
            public int? StokId { get; set; }
            public int? VergiId { get; set; }
            public float Miktar { get; set; }
            public int OlcuId { get; set; }
            public float? BirimFiyat { get; set; }
            public int SatinAlmaId { get; set; }
        }
        public class SatinAlmaDetays
        {
            public int id { get; set; }
            public int? StokId { get; set; }
            public int? VergiId { get; set; }
            public float VergiDegeri { get; set; }
            public float VergiMiktari { get; set; }
            public string StokKodu { get; set; }
            public string StokAd { get; set; }
            public float Miktar { get; set; }
            public int OlcuId { get; set; }
            public float BirimFiyat { get; set; }
            public int SatinAlmaId { get; set; }
            public float TumToplam { get; set; }
            public float AraToplam { get; set; }
        }

        public class PurchaseOrderUpdateItemResponse
        {
            public int id { get; set; }
            public int? StokId { get; set; }
            public string? UrunIsmi { get; set; }
            public int? VergiId { get; set; }
            public string? VergiIsim { get; set; }
            public float? VergiDegeri { get; set; }
            public float? Miktar { get; set; }
            public float? TumToplam { get; set; }
            public int OlcuId { get; set; }
            public string? OlcuIsmi { get; set; }
            public float? BirimFiyat { get; set; }
            public int SatinAlmaId { get; set; }
        }
        public class PurchaseDetails
        {
            public int id { get; set; }
            public string Tip { get; set; } = string.Empty;
            public string Bilgi { get; set; } = string.Empty;
            public int? TedarikciId { get; set; }
            public string TedarikciIsmi { get; set; } = string.Empty;
            public string SatinAlmaIsmi { get; set; } = string.Empty;
            public DateTime? BeklenenTarih { get; set; }
            public int DurumBelirteci { get; set; }
            public DateTime? OlusturmaTarihi { get; set; }
            public int? DepoId { get; set; }
            public int SubeId { get; set; }
            public string DepoIsmi { get; set; } = string.Empty;
            public IEnumerable<PurchaseOrdersItemDetails> detay { get; set; }

        }

        public class PurchaseOrdersItemDetails
        {
            public int id { get; set; }
            public int StokId { get; set; }
            public string UrunIsmi { get; set; } = string.Empty;
            public float Miktar { get; set; }
            public float BirimFiyat { get; set; }
            public int VergiId { get; set; }
            public float VergiDegeri { get; set; }
            public string VergiIsim { get; set; } = string.Empty;
            public float TumToplam { get; set; }
            public int OlcuId { get; set; }
            public string OlcuIsmi { get; set; } = string.Empty;
            public float ToplamTutar { get; set; }
            public float VergiMiktari { get; set; }
            public int SatinAlmaId { get; set; }

        }


        public class PurchaseOrderControls
        {
            public int varmi { get; set; }
        }

        public class PurchaseOrderLogsList
        {
            public int? id { get; set; }
            public string SatinAlmaIsmi { get; set; } = string.Empty;
            public string? Tip { get; set; }

            public string TedarikciIsmi { get; set; } = string.Empty;
            public float? TumToplam { get; set; }
            public int? DepoId { get; set; }
            public DateTime? BeklenenTarih { get; set; }

        }
      
        public class PurchaseOrderId
        {
            public int id { get; set; }
            public int DurumBelirteci { get; set; }
            public int OldDeliveryId { get; set; }
        }

        public class Delete
        {
            public int id { get; set; }
            public string Tip { get; set; } = string.Empty;
        }
  

        public class Quotess
        {
            public int id { get; set; }
            public int StokId { get; set; }
            public int DepoId { get; set; }
            public int Tedarikci { get; set; }
            public int Miktar { get; set; }
            public int? Durum { get; set; }
            public int? Conditions { get; set; }
        }


        public class DeleteItems
        {
            public int id { get; set; }
            public int StokId { get; set; }
            public int SatinAlmaId { get; set; }
        }
        public class PurchaseBuy
        {
            public int id { get; set; }
            public int SalesOrderItemId { get; set; }
            public int SalesOrderId { get; set; }
            public int? ManufacturingOrderId { get; set; }
            public int? ManufacturingOrderItemId { get; set; }
            public int ItemId { get; set; }
            public float? Quantity { get; set; }
            public int DeliveryId { get; set; }
        }
    }
}
