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
        Task<int> Count(StockAdjusmentList T, int CompanyId);
        Task<int> Insert(StockAdjusmentInsert t,int CompanyId);
        Task<int> InsertItem(StockAdjusmentInsertItem T, int StockAdjusmentId, int CompanyId,int user);
        Task Update(StockAdjusmentUpdate T, int CompanyId);
        Task UpdateStockAdjusmentItem(StockAdjusmentUpdateItems T, int CompanyId,int User);
        Task Delete(IdControl T, int CompanyId,int UserId);
        Task DeleteItems(StockAdjusmentItemDelete T, int CompanyId,int User);
        Task<IEnumerable<StockAdjusmentList>> List(StockAdjusmentList T, int CompanyId, int KAYITSAYISI, int SAYFA);
         Task<IEnumerable<StockAdjusmentClas>> Detail(int CompanyId, int id);
    }
}
