using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.SalesOrderDTO;

namespace DAL.DTO
{
    public class StockListDTO
    {
        public class StockList
        {
            public int id { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public int? InStock { get; set; }
            public string Tip { get; set; } = string.Empty;
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public int ContactId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public int locationId { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public string VariantCode { get; set; } = string.Empty;
            public float? AverageCost { get; set; }
            public float? Expected { get; set; }
            public float? ValueInStock { get; set; }
            public float? Committed { get; set; }
            public float? Missing { get; set; }


        }
        public class StockListAll
        {
            public int id { get; set; }
            public string? ItemName { get; set; }
            public int? InStock { get; set; }
            public string? Tip { get; set; }
            public int CategoryId { get; set; }
            public string? CategoryName { get; set; }
            public int ContactId { get; set; }
            public string? SupplierName { get; set; }
            public int? locationId { get; set; }
            public string? LocationName { get; set; }
            public string? VariantCode { get; set; }
            public float? AverageCost { get; set; }
            public float? MaterialExpected { get; set; }
            public float? ProductExpected { get; set; }
            public float? ValueInStock { get; set; }
            public float? MaterailCommitted { get; set; }
            public float? Missing { get; set; }


        }

        public class SatısList
        {
            public int? id { get; set; }
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
        }
        public class SatısListFiltre
        {
            public int? LocationId { get; set; }
            public string? OrderName { get; set; }
            public string? CustomerName { get; set; }
            public float? TotalAll { get; set; }
            public int? SalesItem { get; set; }
            public int? Ingredients { get; set; }
            public int? Production { get; set; }
            public int? DeliveryId { get; set; }
            public string? BaslangıcTarih { get; set; }
            public string? SonTarih { get; set; }
        }

        public class SatısDetail
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

        }

        public class IngredientMis
        {
            [Required]
            public int id { get; set; }
            public int LocationId { get; set; }
            public int SalesOrderItemId { get; set; }
            [Required]
            public int ProductId { get; set; }


        }
        public class QuotesDone
        {
            public int id { get; set; }
            public int? Quotes { get; set; }
            public int LocationId { get; set; }
            public int ContactId { get; set; }

        }


    }
}
