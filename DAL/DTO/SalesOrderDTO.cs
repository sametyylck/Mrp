using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.StockListDTO;

namespace DAL.DTO
{
    public class SalesOrderDTO
    {
        public class SalesOrder
        {

            public string? Tip { get; set; } 
            public int? ContactId { get; set; }
            public string OrderName { get; set; } = string.Empty;
            public DateTime? DeliveryDeadline { get; set; }
            public DateTime? CreateDate { get; set; }
            public int? LocationId { get; set; }
            public string Info { get; set; } = string.Empty;
            public bool? Quotes { get; set; }



        }
        public class SalesOrderResponse
        {
            public int id { get; set; }
            public string? Tip { get; set; }
            public int? ContactId { get; set; }
            public string OrderName { get; set; } = string.Empty;
            public DateTime? DeliveryDeadline { get; set; }
            public DateTime? CreateDate { get; set; }
            public int? LocationId { get; set; }
            public string Info { get; set; } = string.Empty;
            public bool? Quotes { get; set; }


        }

        public class SalesOrderItemResponse
        {
            public int id { get; set; }
            public int? Quotes { get; set; }
            public float? TotalAll { get; set; }
            public int? ItemId { get; set; }
            public string? ItemName { get; set; }
            public int BillingLocationId { get; set; }
            public int? ShippingLocationId { get; set; }
            public float? Quantity { get; set; }
            public float? PricePerUnit { get; set; }
            public float? TotalPrice { get; set; }
            public int TaxId { get; set; }
            public int Rate { get; set; }
            public int SalesItem { get; set; }
            public int Conditions { get; set; }
        }
        public class SalesOrderItem
        {
            public int id { get; set; }
            public int? Quotes { get; set; }
            public int? ItemId { get; set; }
            public int? LocationId { get; set; }
            public int? ContactId { get; set; }
            public int? TaxId { get; set; }
            public float? Quantity { get; set; }
            public int Status { get; set; }
            public int Conditions { get; set; }

        }
        public class SalesOrderCloneAddress
        {
            public int id { get; set; }
            public string? Tip { get; set; }
            public int ContactsId { get; set; }
            public int SalesOrderId { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? CompanyName { get; set; }
            public string? Phone { get; set; }
            public string? AddressLine1 { get; set; }
            public string? AddressLine2 { get; set; }
            public string? CityTown { get; set; }
            public string? StateRegion { get; set; }
            public int ZipPostal { get; set; }
            public string? Country { get; set; }
        }
        public class SalesOrderRezerve
        {
            public int id { get; set; }
            public int SalesOrderId { get; set; }
            public string? Tip { get; set; }
            public int ItemId { get; set; }
            public float RezerveCount { get; set; }
            public int CustomerId { get; set; }
            public int LocationId { get; set; }
            public int Status { get; set; }

            public float LocationStockCount { get; set; }

        }
        public class SalesOrderUpdate
        {
            public int id { get; set; }
            public int ContactId { get; set; }
            public string? DisplayName { get; set; }
            public string? OrderName { get; set; }
            public DateTime DeliveryDeadline { get; set; }
            public DateTime CreateDate { get; set; }
            public int LocationId { get; set; }
            public string? Info { get; set; }
            public float Total { get; set; }

        }
        public class SalesOrderDetail
        {
            public int id { get; set; }
            public int ContactId { get; set; }
            public string? DisplayName { get; set; }
            public string? OrderName { get; set; }
            public int BillingAddressId { get; set; }
            public int? ShippingAddressId { get; set; }
            public DateTime DeliveryDeadline { get; set; }
            public int DeliveryId { get; set; }
            public DateTime CreateDate { get; set; }
            public int LocationId { get; set; }
            public string? LocationName { get; set; }
            public string? Info { get; set; }
            public float Total { get; set; }
            public int Status { get; set; }
            public int Conditions { get; set; }
            public IEnumerable<SatısDetail> detay { get; set; }

        }

        public class SalesDone
        {
            public int id { get; set; }
            public int DeliveryId { get; set; }
        }
        public class SalesOrderUpdateItems
        {
            public int id { get; set; }
            public string? Tip { get; set; }
            public int Quotes { get; set; }
            public int OrderItemId { get; set; }
           public int ManufacturingOrderId { get; set; }
            public int ContactId { get; set; }
            public int LocationId { get; set; }
            public int ItemId { get; set; }
            public float Quantity { get; set; }
            public float PricePerUnit { get; set; }
            public int TaxId { get; set; }
            public int Conditions { get; set; }
            public DateTime DeliveryDeadline { get; set; }
            public string? Note { get; set; }
        }
        public class SalesOrderUpdateMakeBatchItems
        {
            public int id { get; set; }
            public int OrderItemId { get; set; }
            public int ItemId { get; set; }
            public float Quantity { get; set; }
            public float PlannedQuantity { get; set; }
            public string? Note { get; set; }

        }
        public class SalesOrderList
        {
            public int? id { get; set; }
            public int Quotes { get; set; }
            public int? LocationId { get; set; }
            public string? OrderName { get; set; }
            public int? ContactId { get; set; }
            public string? CustomerName { get; set; }
            public float? TotalAll { get; set; }
            public DateTime DeliveryDeadline { get; set; }
            public int? SalesItem { get; set; }
            public int? Ingredients { get; set; }
            public int? Production { get; set; }
            public int? DeliveryId { get; set; }
            public string? LocationName { get; set; }
            public DateTime? BaslangıcTarih { get; set; }
            public DateTime? SonTarih { get; set; }

            public IEnumerable<ManufacturingOrderDetail>? MOList { get; set; }
            public IEnumerable<SalesOrderItemDetail>? MissingList { get; set; }
        }
        public class SalesOrderItemDetail
        {
            public int id { get; set; }
            public string? Tip { get; set; }

            public int ItemId { get; set; }
            public string? ItemName { get; set; }
            public float Quantity { get; set; }
            public float PricePerUnit { get; set; }
            public int TaxId { get; set; }
            public string? TaxName { get; set; }
            public float Rate { get; set; }
            public float? TotalAll { get; set; }
            public int? SalesItem { get; set; }
            public int? Ingredients { get; set; }
            public int? Production { get; set; }
            public int Missing { get; set; }

            public IEnumerable<ManufacturingOrderDetail>? MOList { get; set; }

        }
        public class SalesOrderSellSomeList
        {
            public int SalesOrderId { get; set; }
            public int SalesOrderItemId { get; set; }
            public int ItemId { get; set; }
            public int? ContactId { get; set; }
            public string? Customer { get; set; }
            public int Rezerve { get; set; }
            public IEnumerable<SellSomeList>? ManufacturingList { get; set; }
            public IEnumerable<SellSomeList>? Missing { get; set; }

        }
        public class SellSomeList
        {
            public int id { get; set; }
            public int? ContactId { get; set; }
            public string? Customer { get; set; }
            public string? OrderName { get; set; }
            public int? Quantity { get; set; }
            public int? Rezerve { get; set; }
            public int? Missing { get; set; }



        }
        public class MissingCount
        { 
            [Required]
            public int id { get; set; }
            public int LocationId { get; set; }
            public int SalesOrderItemId { get; set; }
            [Required]
            public int ProductId { get; set; }
            public int MaterialId { get; set; }
            public string? MaterialName { get; set; }
            public int Missing { get; set; }
            public List<PurchaseOrdersItemDetails>? PurchaseOrderList { get; set; }


        }
        public class SalesDelete
        {
            public int id { get; set; }
            public string? Tip { get; set; }
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


        public class SalesDeleteItems
        {
            public int id { get; set; }
            public int ItemId { get; set; }
            public int OrdersId { get; set; }
            public int? Quotes { get; set; }
        }
    }
}
