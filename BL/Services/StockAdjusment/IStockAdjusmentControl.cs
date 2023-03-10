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
        Task<List<string>> Insert(StockAdjusmentInsert T, int CompanyId);
        Task<List<string>> InsertItem(StockAdjusmentInsertItem T, int CompanyId);
        Task<List<string>> Update(StockAdjusmentUpdate T, int CompanyId);
        Task<List<string>> UpdateStockAdjusment(StockAdjusmentUpdateItems T, int CompanyId);
        Task<List<string>> DeleteItems(StockAdjusmentItemDelete T, int CompanyId);


    }
}
