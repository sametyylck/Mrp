using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockTakesDTO;

namespace BL.Services.StockTakes
{
    public interface IStockTakesControl
    {
        Task<List<string>> Insert(StockTakesInsert T);
        Task<List<string>> InsertItem(List<StockTakeInsertItems> T);
        Task<List<string>> UpdateItem(StockTakesUpdateItems T);
        Task<List<string>> DeleteItem(StockTakeDelete T);
        Task<List<string>> StockTakesDone(StockTakesDone T);

        }
}
