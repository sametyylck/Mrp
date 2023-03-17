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
        Task<IEnumerable<SalesOrderList>> SalesOrderList(SalesOrderList T,int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<SalesOrderList>> QuotesList(SalesOrderList T, int KAYITSAYISI, int SAYFA);
        Task<int> SalesOrderCount(SalesOrderList T, int CompanyId);
         Task<int> QuotesCount(SalesOrderList T, int CompanyId);
        Task<int> Insert(SalesOrder T);
        Task<int> InsertAddress(SalesOrderCloneAddress A, int id);
        Task UpdateAddress(SalesOrderCloneAddress A, int id);
        Task Update(SalesOrderUpdate T, int id);
        Task DeleteStockControl(SalesDelete T,int user);
        Task DeleteItems(SalesDeleteItems T);

        Task QuotesDone(Quotess T);
    }
}
