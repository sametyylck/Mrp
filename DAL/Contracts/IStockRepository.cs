using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockListDTO;

namespace DAL.Contracts
{
    public interface IStockRepository
    {
        Task<IEnumerable<StockList>> MaterialList(StockList T, int CompanyId, int KAYITSAYISI, int SAYFA);
         Task<IEnumerable<StockList>> ProductList(StockList T, int CompanyId, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<StockList>> SemiProductList(StockList T, int CompanyId, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<StockListAll>> AllItemsList(StockListAll T, int CompanyId, int KAYITSAYISI, int SAYFA);
    }
}
