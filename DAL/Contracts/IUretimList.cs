using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;

namespace DAL.Contracts
{
    public interface IUretimList
    {
        Task<IEnumerable<ManufacturingOrderList>> ScheludeOpenList(ManufacturingOrderListArama T, int CompanyId, int? KAYITSAYISI, int? SAYFA);
        Task<IEnumerable<ManufacturingOrderDoneList>> ScheludeDoneList(ManufacturingOrderDoneListArama T, int CompanyId, int? KAYITSAYISI, int? SAYFA);
        Task<IEnumerable<ManufacturingTask>> TaskDoneList(ManufacturingTaskArama T, int CompanyId, int? KAYITSAYISI, int? SAYFA);
        Task<IEnumerable<ManufacturingTask>> TaskOpenList(ManufacturingTaskArama T, int CompanyId, int? KAYITSAYISI, int? SAYFA);
        Task<IEnumerable<ManufacturingOrderDetail>> Detail(int CompanyId, int id);
        Task<IEnumerable<ManufacturingOrderItemsIngredientsDetail>> IngredientsDetail(int CompanyId, int id);
        Task<IEnumerable<ManufacturingOrderItemsOperationDetail>> OperationDetail(int CompanyId, int id);
    }
}
