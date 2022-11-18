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
        Task InsertOrderItems(int id, int? SalesOrderId, int? SalesOrderItemId, int CompanyId);
        Task UpdateOrderItems(ManufacturingOrderUpdate T, float eski, int CompanyId);
        Task<IEnumerable<ManufacturingOrderItemsIngredientsDetail>> IngredientsDetail(int CompanyId, int id);
        Task<IEnumerable<ManufacturingOrderItemsOperationDetail>> OperationDetail(int CompanyId, int id);
        Task IngredientsUpdate(ManufacturingOrderItemsIngredientsUpdate T, int CompanyId);
        Task OperationsUpdate(ManufacturingOrderItemsOperationsUpdate T, int CompanyId);
        Task<int> IngredientsInsert(ManufacturingOrderItemsIngredientsInsert T, int CompanyId);
        Task<int> OperationsInsert(ManufacturingOrderItemsOperationsInsert T, int CompanyId);
        Task DeleteStockControl(IdControl T, int CompanyId,int UserId);
        Task DeleteItems(ManufacturingDeleteItems T, int CompanyId);
        Task BuyStockControl(ManufacturingPurchaseOrder T, int? missing, int CompanyId);
    }
}
