using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.PurchaseOrderDTO;

namespace DAL.Contracts
{
    public interface IManufacturingOrderItemRepository
    {
        Task InsertOrderItems(int id, int? SalesOrderId, int? SalesOrderItemId);
        Task UpdateOrderItems(ManufacturingOrderUpdate T, float eski);
        Task<IEnumerable<ManufacturingOrderItemsIngredientsDetail>> IngredientsDetail(int id);
        Task<IEnumerable<ManufacturingOrderItemsOperationDetail>> OperationDetail( int id);
        Task IngredientsUpdate(ManufacturingOrderItemsIngredientsUpdate T);
        Task OperationsUpdate(ManufacturingOrderItemsOperationsUpdate T);
        Task<int> IngredientsInsert(ManufacturingOrderItemsIngredientsInsert T);
        Task<int> OperationsInsert(ManufacturingOrderItemsOperationsInsert T);
        Task DeleteStockControl(IdControl T, int UserId);
        Task DeleteItems(ManufacturingDeleteItems T);
        Task BuyStockControl(ManufacturingPurchaseOrder T, int? missing);
    }
}
