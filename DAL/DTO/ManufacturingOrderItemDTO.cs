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
            public int ProductId { get; set; }
            [Required]
            public int ItemId { get; set; }
            [Required]
            public float PlannedQuantity { get; set; }
            [RegularExpression(RegularExpression, ErrorMessage = "Özel karekter giremezsiniz")]
            [StringLength(50)]
            public string Note { get; set; } = string.Empty;
            public int? CompanyId { get; set; }
            public string Tip { get; set; } = string.Empty;
            public int RezerveId { get; set; }
            public int Status { get; set; }
            public int LocationId { get; set; }
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
            public int? CompanyId { get; set; }
        }

        public class ManufacturingOrderItemsIngredientsDetail
        {
            public int id { get; set; }
            public int? ItemId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Note { get; set; } = string.Empty;
            public float Quantity { get; set; }
            public float Cost { get; set; }
            public int? Availability { get; set; }
            public float Missing { get; set; }

        }
        public class ManufacturingOrderItemsIngredientsUpdate
        {
            public int? id { get; set; }
            public int? SalesOrderId { get; set; }
            public int? SalesOrderItemId { get; set; }
            public int? OrderId { get; set; }
            public int? ItemId { get; set; }
            public string Note { get; set; } = string.Empty;
            public float? Quantity { get; set; }
            public int? LocationId { get; set; }
            public float? Cost { get; set; }
            public int Availability { get; set; }
        }
        public class ManufacturingOrderItemsIngredientsInsert
        {
            public int? SalesOrderId { get; set; }
            public int? SalesOrderItemId { get; set; }
            public int? OrderId { get; set; }
            public int? ItemId { get; set; }
            public string Note { get; set; } = string.Empty;
            public float? Quantity { get; set; }
            public int? LocationId { get; set; }
            public float? Cost { get; set; }
            public int Availability { get; set; }
        }

        public class ManufacturingOrderItemsOperationsUpdate
        {
            public int? id { get; set; }
            public int? OrderId { get; set; }
            public int? OperationId { get; set; }
            public int? ResourceId { get; set; }
            public int? PlannedTime { get; set; }
            public int? Status { get; set; }
            public float? CostPerHour { get; set; }
        }
        public class ManufacturingOrderItemsOperationsInsert
        {
            public int OrderId { get; set; }
            public int OperationId { get; set; }
            public int? ResourceId { get; set; }
            public int? PlannedTime { get; set; }
            public int? Status { get; set; }
            public float? Cost { get; set; }
            public float? CostPerHour { get; set; }
        }


        public class ManufacturingOrderItemsOperationDetail
        {
            public int id { get; set; }
            public int? OperationId { get; set; }
            public string OperationName { get; set; } = string.Empty;
            public int? ResourceId { get; set; }
            public string ResourceName { get; set; } = string.Empty;
            public int? PlannedTime { get; set; }
            public float CostPerHour { get; set; }

            public float Cost { get; set; }
            public int? Status { get; set; }
        }
        public class Quantitys
        {
            public float Quantity { get; set; }
            public float PlannedQuantity { get; set; }

        }
        public class Manufacturing
        {
            public float ToplamCount { get; set; }
            public int LocationStockId { get; set; }
            public float ToplamRezerveCount { get; set; }
            public float AllStockQuantity { get; set; }
            public float RezerveCount { get; set; }
            public int ItemId { get; set; }



        }
        public class ManufacturingPurchaseOrder
        {
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

        }
    }
}
