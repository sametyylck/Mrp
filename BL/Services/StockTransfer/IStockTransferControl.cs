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
        Task<List<string>> Insert(StockTransferInsert T);
        Task<List<string>> InsertItem(StockTransferInsertItem T);
        Task<List<string>> UpdateItems(int? ItemId, int? StockTransferId, int? id);
        Task Kontrol(int? Id,int? ItemId, int? StockTransferId);
        Task<List<string>> AdresStokKontrol(int? Id,int? ItemId, int? StockTransferId, float? Quantity);
    }
}
