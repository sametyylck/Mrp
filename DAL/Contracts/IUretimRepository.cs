using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;

namespace DAL.Contracts
{
    public interface IUretimRepository
    {
        Task<int> Insert(UretimDTO T, int CompanyId);
        Task InsertOrderItems(int id, int? ItemId, int LocationId,float? adet, int CompanyId, int? SalesOrderId, int? SalesOrderItemId);
        Task Update(UretimUpdate T, int CompanyId);
        Task DeleteItems(UretimDeleteItems T, int CompanyId);
        Task UpdateOrderItems(int id, int LocationId, float adetbul, float eski, int CompanyId);
        Task<int> IngredientsInsert(UretimIngredientsInsert T, int CompanyId);
        Task<int> OperationsInsert(UretimOperationsInsert T, int CompanyId);
        Task OperationsUpdate(UretimOperationsUpdate T, int CompanyId);
        Task IngredientsUpdate(UretimIngredientsUpdate T, int CompanyId);
        Task Delete(List<UretimDeleteKontrol> T, int CompanyId, int UserId);
        Task DoneStock(UretimTamamlama T, int CompanyId, int UserId);
        Task BuyStockControl(PurchaseOrderInsert T, int? missing, int CompanyId);
    }
}
