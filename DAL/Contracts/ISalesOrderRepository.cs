using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.SalesOrderDTO;

namespace DAL.Contracts
{
    public interface ISalesOrderRepository
    {
        Task<IEnumerable<SalesOrderList>> SalesOrderList(SalesOrderList T, int CompanyId, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<SalesOrderList>> QuotesList(SalesOrderList T, int CompanyId, int KAYITSAYISI, int SAYFA);
        Task<int> SalesOrderCount(SalesOrderList T, int CompanyId);
         Task<int> QuotesCount(SalesOrderList T, int CompanyId);
        Task<int> Insert(SalesOrder T, int CompanyId);
        Task<int> InsertAddress(SalesOrderCloneAddress A, int CompanyId, int id);
        Task UpdateAddress(SalesOrderCloneAddress A, int id, int CompanyId);
        Task Update(SalesOrderUpdate T, int id, int CompanyId);
        Task DeleteStockControl(SalesDelete T, int CompanyId,int user);
        Task DeleteItems(SalesDeleteItems T, int CompanyId);

        Task QuotesDone(Quotess T, int CompanyId);
    }
}
