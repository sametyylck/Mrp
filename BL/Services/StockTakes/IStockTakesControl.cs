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
        Task<string> Insert(StockTakesInsert T,int CompanyId);
        Task<string> InsertItem(List<StockTakeInsertItems> T , int CompanyId);
        Task<string> UpdateItem(StockTakesUpdateItems T, int CompanyId);
        Task<string> DeleteItem(StockTakeDelete T, int CompanyId);

    }
}
