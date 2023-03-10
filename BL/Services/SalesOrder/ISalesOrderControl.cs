using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockListDTO;

namespace BL.Services.SalesOrder
{
    public interface ISalesOrderControl
    {
        Task<List<string>> Insert(SatısDTO T, int CompanyId);
        Task<List<string>> Adress(int id, int? ContactId, int CompanyId);
        Task<List<string>> InsertItem(SatısInsertItem T, int CompanyId);
        Task<List<string>> Update(SalesOrderUpdate T, int CompanyId);
        Task<List<string>> DeleteItems(SatısDeleteItems T, int CompanyId);
        Task<List<string>> QuotesDone(QuotesDone T, int CompanyId);
        Task<List<string>> Make(SalesOrderMake T, int CompanyId);
        Task<List<string>> UpdateItem(SatısUpdateItems T, int CompanyId);

    }
}
