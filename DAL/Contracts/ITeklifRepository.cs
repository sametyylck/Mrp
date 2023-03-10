using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockListDTO;

namespace DAL.Contracts
{
    public interface ITeklifRepository
    {
        Task<int> Insert(SatısDTO T, int CompanyId);
        Task<int> InsertPurchaseItem(TeklifInsertItem T, int CompanyId);
        Task Update(SalesOrderUpdate T, int CompanyId);
        Task UpdateItems(TeklifUpdateItems T, int CompanyId);
        Task<IEnumerable<SalesOrderDetail>> Detail(int CompanyId, int id);
        Task<IEnumerable<SatısList>> SalesOrderList(SatısListFiltre T, int CompanyId, int? KAYITSAYISI, int? SAYFA);
        Task DeleteItems(SatısDeleteItems T, int CompanyId);
        Task DeleteStockControl(List<SatısDelete> A, int CompanyId, int User);
        Task QuotesDone(QuotesDone T, int CompanyId);



    }
}
