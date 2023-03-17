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
            public int? SalesOrderId { get; set; }
            public int? SalesOrderItemId { get; set; }
            public string? Name { get; set; } 
            public int? ItemId { get; set; }
            public DateTime? ProductionDeadline { get; set; }
            public DateTime? CreatedDate { get; set; }
            public DateTime? ExpectedDate { get; set; }
            public float? PlannedQuantity { get; set; }
            public int LocationId { get; set; }
            public int? ContactId { get; set; }
            public string? Info { get; set; } 
            public bool Private { get; set; }



        }
        public class SalesOrderMake
        {
            public string? Name { get; set; }
            public string? Tip { get; set; } //MakeOrder mı yoksa Make in Batch mi kontrol için
            public int? SalesOrderId { get; set; }
            public int? SalesOrderItemId { get; set; }

            public int? ItemId { get; set; }

            public DateTime? ProductionDeadline { get; set; }

            public DateTime? CreatedDate { get; set; }

            public DateTime ExpectedDate { get; set; }

            public float? PlannedQuantity { get; set; }

            public int LocationId { get; set; }
            public int? ContactId { get; set; }

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
            public int? SalesOrderId { get; set; }
            public int? SalesOrderItemId { get; set; }
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
            public int ItemId { get; set; }
            public int OrdersId { get; set; }
            public int? Quotes { get; set; }
        }

        public class ManufacturingOrderUpdatePlanned
        {
            public int id { get; set; }
            public float? PlannedQuantity { get; set; }
            public int? ItemId { get; set; }
            public string Name { get; set; } = string.Empty;
            [Column(TypeName = "datetime2")]
            public DateTime? ProductionDeadline { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime? CreatedDate { get; set; }
            public int LocationId { get; set; }
            public string Info { get; set; } = string.Empty;
            // (0 = Not Started) (1 = Blocked) (2 = Work In Progress) (3 = Done) 
            public int? Status { get; set; }
        }
        public class sorgubul
        {
            public float DefaultPrice { get; set; }
            public int LocationStockCount { get; set; }
        }

        public class ManufacturingOrderDetail
        {
            public int id { get; set; }
            public int SalesOrderId { get; set; }
            public int SalesOrderItemId { get; set; }
            public string Name { get; set; } = string.Empty;
            public int? ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public DateTime? ProductionDeadline { get; set; }
            public DateTime? CreatedDate { get; set; }
            public DateTime ExpectedDate { get; set; }
            public float? PlannedQuantity { get; set; }
            public int LocationId { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public int? Status { get; set; }
            public string Info { get; set; } = string.Empty;
            public IEnumerable<ManufacturingOrderItemsIngredientsDetail> IngredientDetail { get; set; }
            public IEnumerable<ManufacturingOrderItemsOperationDetail> OperationDetail { get; set; }

        }
        public class ManufacturingOrderList
        {
            public int id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int? CustomerId { get; set; }
            public string Customer { get; set; } = string.Empty;
            public int? ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public float? PlannedQuantity { get; set; }
            public int? PlannedTime { get; set; }
            public int? Status { get; set; }

            // (0 = Not Started) (1 = Blocked) (2 = Work In Progress) (3 = Done) 
            public string Availability { get; set; } = string.Empty;
            public int? LocationId { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public DateTime? BaslangıcTarih { get; set; }
            public DateTime? SonTarih { get; set; }
        }
        public class ManufacturingOrderListArama
        {
            public string Name { get; set; } = string.Empty;
            public string Customer { get; set; } = string.Empty;
            public string ItemName { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public float? PlannedQuantity { get; set; }
            public int? PlannedTime { get; set; }
            public int? Status { get; set; }

            // (0 = Not Started) (1 = Blocked) (2 = Work In Progress) (3 = Done) 
            public string Availability { get; set; } = string.Empty;
            public int? LocationId { get; set; }
            public DateTime? BaslangıcTarih { get; set; }
            public DateTime? SonTarih { get; set; }
        }

        public class ManufacturingOrderDoneList
        {
            public int id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int? CustomerId { get; set; }
            public string Customer { get; set; } = string.Empty;
            public int? ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public float? PlannedQuantity { get; set; }
            public int? PlannedTime { get; set; }
            public int? Status { get; set; }
            // (0 = Not Started) (1 = Blocked) (2 = Work In Progress) (3 = Done) 
            public int? LocationId { get; set; }
            public string? LocationName { get; set; } = string.Empty;
            public float? MaterialCost { get; set; }
            public float? OperationCost { get; set; }
            public float? TotalCost { get; set; }
            public DateTime? BaslangıcTarih { get; set; }
            public DateTime? SonTarih { get; set; }
        
        }
        public class ManufacturingOrderDoneListArama
        {

            public string Name { get; set; } = string.Empty;
            public string Customer { get; set; } = string.Empty;
            public string ItemName { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public float? PlannedQuantity { get; set; }
            public int? PlannedTime { get; set; }

            public int? LocationId { get; set; }
            public float? MaterialCost { get; set; }
            public float? OperationCost { get; set; }
            public float? TotalCost { get; set; }
            public DateTime? BaslangıcTarih { get; set; }
            public DateTime? SonTarih { get; set; }

        }

        public class ManufacturingOrderResponse
        {
            public int id { get; set; }
            public int? ItemId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Tip { get; set; } = string.Empty;
            public float Quantity { get; set; }
            public string Note { get; set; } = string.Empty;
            public float CostPerHour { get; set; }
            public float PlannedTime { get; set; }
            public float Cost { get; set; }
            public int Availability { get; set; }
            public int Status { get; set; }
        }
        public class ManufacturingOrderOperations
        {
            public int id { get; set; }
            public int ResourceId { get; set; }
            public string ResourceName { get; set; } = string.Empty;
            public int OperationId { get; set; }
            public string OperationName { get; set; } = string.Empty;
            public float CostPerHour { get; set; }
            public float PlannedTime { get; set; }
            public float Cost { get; set; }
            public int OrderId { get; set; }
            public int Status { get; set; }
        }
        public class ManufacturingTask
        {
            public int id { get; set; }
            public int? ManufacturingOrderId { get; set; }
            public string OrderName { get; set; } = string.Empty;
            public int? ResourceId { get; set; }
            public string ResourcesName { get; set; } = string.Empty;
            public int? OperationId { get; set; }
            public string OperationName { get; set; } = string.Empty;
            public int? ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public DateTime ProductionDeadline { get; set; }
            public float? PlannedQuantity { get; set; }
            public float? PlannedTime { get; set; }
            public int? LocationId { get; set; }
            public DateTime CompletedDate { get; set; }
            public int? Status { get; set; }
        }
        public class ManufacturingTaskArama
        {
            public string OrderName { get; set; } = string.Empty;
            public string ResourcesName { get; set; } = string.Empty;
            public string OperationName { get; set; } = string.Empty;
            public string ItemName { get; set; } = string.Empty;
            public DateTime ProductionDeadline { get; set; }
            public float? PlannedQuantity { get; set; }
            public float? PlannedTime { get; set; }
            public int? LocationId { get; set; }
            public DateTime CompletedDate { get; set; }
            public int? Status { get; set; }
        }

    }
}
