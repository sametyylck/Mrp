using DAL.DTO;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockAdjusmentDTO;
using static DAL.DTO.StockTakesDTO;

namespace DAL.Contracts
{
    public interface IStockAdjusmentRepository
    {
        Task<int> Insert(StockAdjusmentInsert t);
        Task<int> InsertItem(StockAdjusmentInsertItem T, int StockAdjusmentId,int user);
        Task Update(StockAdjusmentUpdate T);
        Task UpdateStockAdjusmentItem(StockAdjusmentUpdateItems T,int User);
        Task Delete(IdControl T,int UserId);
        Task DeleteItems(StockAdjusmentItemDelete T,int User);
        Task<IEnumerable<StockAdjusmentList>> List(StockAdjusmentList T,int KAYITSAYISI, int SAYFA);
         Task<IEnumerable<StockAdjusmentClas>> Detail(int id);
    }
}
