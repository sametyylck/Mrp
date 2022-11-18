using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.PurchaseOrderDTO;

namespace DAL.Contracts
{
    public interface IOrdersRepository
    {
        Task<int> Insert(PurchaseOrderInsert T, int CompanyId);
        Task<int> InsertPurchaseItem(PurchaseOrderInsertItem T, int OrdersId, int CompanyId);
        Task Update(PurchaseOrderUpdate T, int CompanyId);
        Task UpdatePurchaseItem(PurchaseItem T, int CompanyId);
        Task Delete(Delete T, int CompanyId,int user);
        Task DeleteItems(DeleteItems T, int CompanyId);
        Task<IEnumerable<PurchaseDetails>> Details(int id, int CompanyId);
        Task<IEnumerable<PurchaseOrdersItemDetails>> PurchaseOrderDetailsItem(int id, int CompanyId);
    }
}
