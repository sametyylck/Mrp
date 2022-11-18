using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.SalesOrderDTO;

namespace BL.Services.SalesOrder
{
    public interface ISalesOrderControl
    {
        Task<string> Insert(SalesOrderDTO.SalesOrder T, int CompanyId);
        Task<string> Adress(int id,int? ContactId ,int CompanyId);
        Task<string> InsertItem(SalesOrderItem T, int CompanyId);
        Task<string> Update(SalesOrderUpdate T, int CompanyId);
        Task<string> DeleteItems(SalesDeleteItems T , int CompanyId) ;
        Task<string> QuotesDone(Quotess T, int CompanyId);
        Task<string> Make(SalesOrderMake T, int CompanyId);
        Task<string> UpdateItem(SalesOrderUpdateItems T, int CompanyId);

    }
}
