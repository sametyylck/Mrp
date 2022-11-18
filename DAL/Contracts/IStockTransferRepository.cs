using DAL.DTO;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockTransferDTO;

namespace DAL.Contracts
{
    public interface IStockTransferRepository
    {
        Task<int> Insert(StockTransferInsert T, int CompanyId);
        Task<int> Count(StockTransferList T, int CompanyId);
        Task<int> InsertStockTransferItem(StockTransferInsertItem T, int? id, int CompanyId, int UserId);
        Task Update(StockUpdate T, int CompanyId);
        Task UpdateStockTransferItem(StockTransferItems T, int CompanyId, int UserId);
        Task<IEnumerable<StockTransferList>> List(StockTransferList T, int CompanyId, int KAYITSAYISI, int sayfa);
        Task Delete(IdControl T, int CompanyId, int UserId);
        Task DeleteItems(StockTransferDeleteItems T, int CompanyId, int UserId);
       Task<IEnumerable<StockTransferList>> Details(int id, int CompanyId);
        Task<IEnumerable<StockTransferDetailsItems>> ItemDetails(int id, int CompanyId);
    }
}
