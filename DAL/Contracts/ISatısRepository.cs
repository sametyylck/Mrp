using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.SalesOrderDTO;

namespace DAL.Contracts
{
    public interface ISatısRepository
    {
        Task<int> Insert(SatısDTO T, int CompanyId);
        Task<int> InsertPurchaseItem(SatısInsertItem T, int CompanyId);
        Task Update(SalesOrderUpdate T,int CompanyId);
        Task UpdateItems(SatısUpdateItems T,int CompanyId);
        Task UpdateAddress(SalesOrderCloneAddress A, int id, int CompanyId);
        Task DeleteItems(SatısDeleteItems T, int CompanyId);
        Task DeleteStockControl(List<SatısDelete> T, int CompanyId, int User);
        Task<int> Make(SalesOrderMake T, int CompanyId);
        Task DoneSellOrder(SalesDone T, int CompanyId, int UserId);
        Task<int> Control(SatısInsertItem T, int OrdersId, string? Tip, int CompanyId);
        Task IngredientsControl(SatısInsertItem T, int OrdersId, int CompanyId);
        Task<int> InsertAddress(SalesOrderCloneAddress A,int id);

    }
}
