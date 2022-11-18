using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.SalesOrderDTO;

namespace DAL.Contracts
{
    public interface ISalesOrderItemRepository
    {
        Task<int> InsertPurchaseItem(SalesOrderItem T, int CompanyId);
        Task UpdateItems(SalesOrderUpdateItems T, int id, int CompanyId);
        Task<int> Control(SalesOrderItem T, int OrdersId, int CompanyId);
        Task IngredientsControl(SalesOrderItem T, int OrdersId, int CompanyId);
        Task UpdateIngredientsControl(SalesOrderUpdateItems T, int OrdersId, int CompanyId);
        Task UpdateMakeItems(SalesOrderUpdateItems T, float eski, int CompanyId);
        Task UpdateMakeBatchItems(SalesOrderUpdateItems T, int CompanyId,float eskiQuantity);
        Task<IEnumerable<MissingCount>> IngredientsMissingList(MissingCount T, int CompanyId);
        Task StockControl(ManufacturingOrderA T, float? rezervecount, int CompanyId);
        Task<IEnumerable<SalesOrderDetail>> Detail(int CompanyId, int id);
        Task<IEnumerable<SalesOrderItemDetail>> ItemDetail(int CompanyId, int id);
        Task DoneSellOrder(SalesDone T, int CompanyId,int UserId);
        Task<IEnumerable<SalesOrderSellSomeList>> SellSomeList(SalesOrderSellSomeList T, int CompanyId);
        Task DoneStock(int id, int SalesOrderId, int SalesOrderItemId, int Status, int CompanyId,int UserId);
    }
}
