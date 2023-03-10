using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockListDTO;

namespace DAL.Contracts
{
    public interface ISatısListRepository
    {
        Task<IEnumerable<SatısList>> SalesOrderList(SatısListFiltre T, int CompanyId, int? KAYITSAYISI, int? SAYFA);
        Task<IEnumerable<SatısList>> SalesOrderDoneList(SatısListFiltre T, int CompanyId, int? KAYITSAYISI, int? SAYFA);
        Task<IEnumerable<SalesOrderDetail>> Detail(int CompanyId, int id);
        Task<IEnumerable<MissingCount>> IngredientsMissingList(IngredientMis T, int CompanyId);
        Task<IEnumerable<SalesOrderSellSomeList>> SalesManufacturingList(int SalesOrderId, int SalesOrderItemId, int CompanyId);
    }
}
