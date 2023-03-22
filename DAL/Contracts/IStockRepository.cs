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
        Task<IEnumerable<StokListResponse>> MaterialList(StokList T, int KAYITSAYISI, int SAYFA);
         Task<IEnumerable<StokListResponse>> ProductList(StokList T, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<StokListResponse>> SemiProductList(StokList T, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<StokListResponse>> AllItemsList(StokList T, int KAYITSAYISI, int SAYFA);
    }
}
