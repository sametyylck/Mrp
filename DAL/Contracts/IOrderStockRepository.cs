using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.PurchaseOrderDTO;

namespace DAL.Contracts
{
    public interface IOrderStockRepository
    {
        Task StockUpdate(PurchaseOrderId T, int CompanyId,int user);
         Task<IEnumerable<PurchaseOrderLogsList>> List(PurchaseOrderLogsList T, int CompanyId, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<PurchaseOrderLogsList>> DoneList(PurchaseOrderLogsList T, int CompanyId, int KAYITSAYISI, int SAYFA);
        Task<int> DoneCount(PurchaseOrderLogsList T, int CompanyId);
        Task<int> Count(PurchaseOrderLogsList T,int CompanyId);
    }
}
