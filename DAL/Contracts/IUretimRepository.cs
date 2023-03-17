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
        Task InsertOrderItems(int id, int? ItemId, int LocationId,float? adet, int? SalesOrderId, int? SalesOrderItemId);
        Task Update(UretimUpdate T);
        Task DeleteItems(UretimDeleteItems T);
        Task UpdateOrderItems(int id, int LocationId, float adetbul, float eski);
        Task<int> IngredientsInsert(UretimIngredientsInsert T);
        Task<int> OperationsInsert(UretimOperationsInsert T);
        Task OperationsUpdate(UretimOperationsUpdate T);
        Task IngredientsUpdate(UretimIngredientsUpdate T);
        Task Delete(List<UretimDeleteKontrol> T, int UserId);
        Task DoneStock(UretimTamamlama T,int UserId);
        Task BuyStockControl(PurchaseOrderInsert T, int? missing);
    }
}
