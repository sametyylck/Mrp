using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.PurchaseOrderDTO;

namespace BL.Services.Orders
{
    public interface IPurchaseOrderControl
    {
        Task<List<string>> Insert(PurchaseOrderInsert T);
        Task<List<string>> InsertItem(PurchaseOrderInsertItem T);
        Task<List<string>> Update(PurchaseOrderUpdate T);
        Task<List<string>> UpdatePurchaseItem(PurchaseItem T);
        Task<List<string>> Delete(Delete T);
        Task<List<string>> DeleteItem(DeleteItems T);
    }
}
