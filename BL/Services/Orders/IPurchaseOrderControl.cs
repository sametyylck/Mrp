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
        Task<string> Insert(PurchaseOrderInsert T, int CompanyId);
        Task<string> InsertItem(PurchaseOrderInsertItem T, int CompanyId);
        Task<string> Update(PurchaseOrderUpdate T, int CompanyId);
        Task<string> UpdatePurchaseItem(PurchaseItem T, int CompanyId);
        Task<string> Delete(Delete T,int CompanyId);
        Task<string> DeleteItem(DeleteItems T,int CompanyId);
    }
}
