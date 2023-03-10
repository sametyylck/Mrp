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
        Task<List<string>> Insert(PurchaseOrderInsert T, int CompanyId);
        Task<List<string>> InsertItem(PurchaseOrderInsertItem T, int CompanyId);
        Task<List<string>> Update(PurchaseOrderUpdate T, int CompanyId);
        Task<List<string>> UpdatePurchaseItem(PurchaseItem T, int CompanyId);
        Task<List<string>> Delete(Delete T,int CompanyId);
        Task<List<string>> DeleteItem(DeleteItems T,int CompanyId);
    }
}
