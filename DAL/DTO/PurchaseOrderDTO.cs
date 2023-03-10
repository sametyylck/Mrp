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
            public int? SalesOrderId { get; set; }
            public int? SalesOrderItemId { get; set; }
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
            public float? PricePerUnit { get; set; }
            public float? TotalAll { get; set; }
            public float? TotalPrice { get; set; }
            public int MeasureId { get; set; }
            public float PlusTax { get; set; }
            public int TaxId { get; set; }
            public int TaxValue { get; set; }
            public string Info { get; set; } = string.Empty;
            public int DeliveryId { get; set; }
            public float Missing { get; set; }
        }
        public class PurchaseOrderInsertItem
        {
            public int OrderId { get; set; }
            public int? ItemId { get; set; }
            public float Quantity { get; set; }
            public float? PricePerUnit { get; set; }
            public int MeasureId { get; set; }
            public int TaxId { get; set; }
        }
        public class PurchaseOrderInsert
        {
            public int? SalesOrderId { get; set; }
            public int? SalesOrderItemId { get; set; }
            public int? ManufacturingOrderId { get; set; }
            public int? ManufacturingOrderItemId { get; set; }
            public int TaxId { get; set; }
            public int ItemId { get; set; }
            public float Quantity { get; set; }
            public int MeasureId { get; set; }
            public string Tip { get; set; } = null!;
            public int? ContactId { get; set; }
            public string? OrderName { get; set; }
            public float? TotalAll { get; set; }
            public DateTime? ExpectedDate { get; set; }
            public DateTime? CreateDate { get; set; }
            public int? LocationId { get; set; }
            public string Info { get; set; } = string.Empty;
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
            public int? ContactId { get; set; }
            public string OrderName { get; set; } = string.Empty;
            public DateTime? ExpectedDate { get; set; }
            public float TotalAll { get; set; }
            public DateTime? CreateDate { get; set; }
            public int? LocationId { get; set; }
            public string Info { get; set; } = string.Empty;
        }
        public class PurchaseItemControl
        {
            public int? id { get; set; }
            public int? ItemId { get; set; }
            public int? TaxId { get; set; }
            public float? TaxValue { get; set; }
            public float? PlusTax { get; set; }
            public float? Quantity { get; set; }
            public int? MeasureId { get; set; }
            public float? PricePerUnit { get; set; }
            public int? OrdersId { get; set; }
            public string? Tip { get; set; }
            public int? LocationId { get; set; }
            public int? ContactId { get; set; }
            public string? ContactTip { get; set; }
            public int? SalesOrderId { get; set; }
            public int? SalesOrderItemId { get; set; }
            public int? IdControl { get; set; }

        }


        public class PurchaseItem
        {
            public int id { get; set; }
            public int? ItemId { get; set; }
            public int? TaxId { get; set; }
            public float? Quantity { get; set; }
            public int MeasureId { get; set; }
            public float? PricePerUnit { get; set; }
            public int OrdersId { get; set; }
        }
        public class PurchaseOrderUpdateItemResponse
        {
            public int id { get; set; }
            public int? ItemId { get; set; }
            public string? ItemName { get; set; }
            public int? TaxId { get; set; }
            public string? TaxName { get; set; }
            public float? TaxValue { get; set; }
            public float? Quantity { get; set; }
            public float? TotalAll { get; set; }
            public int MeasureId { get; set; }
            public string? MeasureName { get; set; }
            public float? PricePerUnit { get; set; }
            public int OrdersId { get; set; }
        }
        public class PurchaseDetails
        {
            public int id { get; set; }
            public string Tip { get; set; } = string.Empty;
            public string Info { get; set; } = string.Empty;
            public int? ContactId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public string OrderName { get; set; } = string.Empty;
            public DateTime? ExpectedDate { get; set; }
            public int DeliveryId { get; set; }
            public DateTime? CreateDate { get; set; }
            public int? LocationId { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public IEnumerable<PurchaseOrdersItemDetails> detay { get; set; }

        }

        public class PurchaseOrdersItemDetails
        {
            public int id { get; set; }
            public int ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public float Quantity { get; set; }
            public float PricePerUnit { get; set; }
            public int TaxId { get; set; }
            public float TaxValue { get; set; }
            public string TaxName { get; set; } = string.Empty;
            public float ItemTotalAll { get; set; }
            public int MeasureId { get; set; }
            public string MeasureName { get; set; } = string.Empty;
            public float ItemTotalPrice { get; set; }
            public float ItemPlusTax { get; set; }
            public int OrdersId { get; set; }

        }


        public class PurchaseOrderControls
        {
            public int varmi { get; set; }
        }

        public class PurchaseOrderLogsList
        {
            public int? id { get; set; }
            public string OrderName { get; set; } = string.Empty;
            public string? Tip { get; set; }

            public string SupplierName { get; set; } = string.Empty;
            public float? TotalAll { get; set; }
            public int? LocationId { get; set; }
            public DateTime? ExpectedDate { get; set; }

        }
      
        public class PurchaseOrderId
        {
            public int id { get; set; }
            public int DeliveryId { get; set; }
            public int OldDeliveryId { get; set; }
        }

        public class Delete
        {
            public int id { get; set; }
            public string Tip { get; set; } = string.Empty;
            public int? Quotes { get; set; }
        }
  

        public class Quotess
        {
            public int id { get; set; }
            public int? Quotes { get; set; }
            public int ItemId { get; set; }
            public int LocationId { get; set; }
            public int ContactId { get; set; }
            public int Quantity { get; set; }
            public int? Status { get; set; }
            public int? Conditions { get; set; }
        }


        public class DeleteItems
        {
            public int id { get; set; }
            public int ItemId { get; set; }
            public int OrdersId { get; set; }
            public int? Quotes { get; set; }
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
