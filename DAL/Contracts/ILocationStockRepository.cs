using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockTransferDTO;

namespace DAL.Contracts
{
    public interface ILocationStockRepository
    {
        Task<int> Insert(string Tip, int? ItemId,  int? LocationId);
        Task Delete(int StokId,int LocationStockId);


    }
}
