using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockAdjusmentDTO;

namespace BL.Services.StockAdjusment
{
    public interface IStockAdjusmentControl
    {
        Task<List<string>> Insert(StockAdjusmentInsert T);
        Task<List<string>> InsertItem(StockAdjusmentInsertItem T);
        Task<List<string>> Update(StockAdjusmentUpdate T);
        Task<List<string>> UpdateStockAdjusment(StockAdjusmentUpdateItems T);
        Task<List<string>> DeleteItems(StockAdjusmentItemDelete T);


    }
}
