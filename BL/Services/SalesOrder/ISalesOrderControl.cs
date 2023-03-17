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
        Task<List<string>> Insert(SatısDTO T);
        Task<List<string>> Adress(int id, int? ContactId);
        Task<List<string>> InsertItem(SatısInsertItem T);
        Task<List<string>> Update(SalesOrderUpdate T);
        Task<List<string>> DeleteItems(SatısDeleteItems T);
        Task<List<string>> QuotesDone(QuotesDone T);
        Task<List<string>> Make(SalesOrderMake T);
        Task<List<string>> UpdateItem(SatısUpdateItems T);

    }
}
