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
        public string? Tip { get; set; }
        public string? Name { get; set; }
        public int? ItemId { get; set; }
        public DateTime? ProductionDeadline { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public float? PlannedQuantity { get; set; }
        public int LocationId { get; set; }
        public string? Info { get; set; }
        public bool Private { get; set; }
    }
    public class UretimDeleteItems
    {
        public int id { get; set; }
        public int ItemId { get; set; }
        public int ManufacturingOrderId { get; set; }
        public int? Quotes { get; set; }
    }
    public class UretimUpdate
    {
        public int id { get; set; }
        public float PlannedQuantity { get; set; }
        public int? ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "datetime2")]
        public DateTime? ProductionDeadline { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? CreatedDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public int LocationId { get; set; }
        public string Info { get; set; } = string.Empty;
        // (0 = Not Started) (1 = Blocked) (2 = Work In Progress) (3 = Done) 
        public float MaterialCost { get; set; }
        public float OperationCost { get; set; }
        public float TotalCost { get; set; }
        public int? Status { get; set; }
        public float eskiPlanned { get; set; }
        public float eskiLocation { get; set; }
    }
    public class UretimIngredientsInsert
    {
        public int? ManufacturingOrderId { get; set; }
        public int? ItemId { get; set; }
        public string Note { get; set; } = string.Empty;
        public float? Quantity { get; set; }
        public int? LocationId { get; set; }
        public float? Cost { get; set; }
        public int Availability { get; set; }
    }
    public class UretimOperationsInsert
    {
        public int ManufacturingOrderId { get; set; }
        public int OperationId { get; set; }
        public int? ResourceId { get; set; }
        public int? PlannedTime { get; set; }
        public int? Status { get; set; }
        public float? Cost { get; set; }
        public float? CostPerHour { get; set; }
    }
    public class UretimOperationsUpdate
    {
        public int? id { get; set; }
        public int? OrderId { get; set; }
        public int? OperationId { get; set; }
        public int? ResourceId { get; set; }
        public int? PlannedTime { get; set; }
        public int? Status { get; set; }
        public float? CostPerHour { get; set; }
    }
    public class UretimIngredientsUpdate
    {
        public int? id { get; set; }
        public int? OrderId { get; set; }
        public int? ItemId { get; set; }
        public string Note { get; set; } = string.Empty;
        public float? Quantity { get; set; }
        public int? LocationId { get; set; }
        public float? Cost { get; set; }
        public int Availability { get; set; }
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
        public float PlannedQuantity { get; set; }
        public int? ItemId { get; set; }
        public int? ManufacturingOrderItemId { get; set; }

    }
    public class UretimDeleteKontrol
    {
        public int id { get; set; }

    }
    public class PurchaseBuy
    {
        public int? ManufacturingOrderId { get; set; }
        public int? ManufacturingOrderItemId { get; set; }
        public int ItemId { get; set; }
        public float Quantity { get; set; }
        public string Tip { get; set; } = null!;
        public int? ContactId { get; set; }
        public string? OrderName { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public int? LocationId { get; set; }
    }








}
