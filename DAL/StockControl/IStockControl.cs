using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.StockControl
{
    public interface IStockControl
    {
        Task<int> Count(int? ItemId,int CompanyId,int? LocationId);
    }
}
