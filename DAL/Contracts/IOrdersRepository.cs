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
        Task<int> Insert(PurchaseOrderInsert T, int UserId);
        Task<int> InsertPurchaseItem(PurchaseOrderInsertItem T, int OrdersId);
        Task Update(PurchaseOrderUpdate T);
        Task UpdatePurchaseItem(PurchaseItem T);
        Task Delete(List<Delete> A,int user);
        Task DeleteItems(DeleteItems T);
        Task<IEnumerable<PurchaseDetails>> Details(int id);
    }
}
