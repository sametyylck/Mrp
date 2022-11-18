using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockTakesDTO;

namespace DAL.Contracts
{
    public interface IStockTakesRepository
    {
        Task<int> Insert(StockTakesInsert t,int CompanyId);
        Task<int> StockTakesCount(StockTakeList T, int CompanyId);
        Task<int> InsertItem(List<StockTakeInsertItems> T, int CompanyId);
        Task Update(StockTakesUpdate T, int id, int CompanyId);
        Task UpdateItems(StockTakesUpdateItems T, int CompanyId);
        Task Delete(IdControl T, int CompanyId,int user);
        Task DeleteItems(StockTakeDelete T, int CompanyId);
        Task<IEnumerable<StockTakeList>> StockTakesList(StockTakeList T, int CompanyId, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<StockTakes>> Detail(int CompanyId, int id);
        Task<IEnumerable<StockTakeItems>> ItemDetail(int CompanyId, int id);
        Task StockTakesDone(StockTakesDone T, int CompanyId, int UserId);

    }       
}
