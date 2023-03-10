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
        Task<List<string>> Insert(StockTransferInsert T,int CompanyId);
        Task<List<string>> InsertItem(StockTransferInsertItem T, int CompanyId);
        Task<List<string>> UpdateItems(int? ItemId, int? StockTransferId, int? id, int CompanyId);
        Task Kontrol(int? Id,int? ItemId, int? StockTransferId, int CompanyId);
        Task<List<string>> AdresStokKontrol(int? Id,int? ItemId, int? StockTransferId, float? Quantity, int CompanyId);
    }
}
