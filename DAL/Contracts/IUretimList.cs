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
        Task<IEnumerable<ManufacturingOrderList>> ScheludeOpenList(ManufacturingOrderList T, int CompanyId, int? KAYITSAYISI, int? SAYFA);
        Task<IEnumerable<ManufacturingOrderDoneList>> ScheludeDoneList(ManufacturingOrderDoneList T, int CompanyId, int? KAYITSAYISI, int? SAYFA);
        Task<IEnumerable<ManufacturingTask>> TaskDoneList(ManufacturingTask T, int CompanyId, int? KAYITSAYISI, int? SAYFA);
        Task<IEnumerable<ManufacturingTask>> TaskOpenList(ManufacturingTask T, int CompanyId, int? KAYITSAYISI, int? SAYFA);
        Task<IEnumerable<ManufacturingOrderDetail>> Detail(int CompanyId, int id);
        Task<IEnumerable<ManufacturingOrderItemsIngredientsDetail>> IngredientsDetail(int CompanyId, int id);
        Task<IEnumerable<ManufacturingOrderItemsOperationDetail>> OperationDetail(int CompanyId, int id);
    }
}
