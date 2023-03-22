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
        Task<int> Insert(StockTransferInsert T);
        Task<int> InsertStockTransferItem(StockTransferInsertItem T, int id, int UserId);
        Task Update(StockUpdate T);
        Task<int> UpdateStockTransferItem(StokAktarimDetay T, int UserId);
        Task<IEnumerable<StockTransferList>> List(StockTransferList T, int KAYITSAYISI, int sayfa);
        Task Delete(IdControl T,int UserId);
        Task DeleteItems(StockTransferDeleteItems T,int UserId);
       Task<IEnumerable<StockTransferDetails>> Details(int id);
    }
}
