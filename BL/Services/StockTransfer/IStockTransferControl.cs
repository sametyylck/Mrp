using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockTransferDTO;

namespace BL.Services.StockTransfer
{
    public interface IStockTransferControl
    {
        Task<string> Insert(StockTransferInsert T,int CompanyId);
        Task<string> InsertItem(StockTransferInsertItem T, int CompanyId);
        Task<string> UpdateItems(int? ItemId, int? StockTransferId, int? id, int CompanyId);
    }
}
