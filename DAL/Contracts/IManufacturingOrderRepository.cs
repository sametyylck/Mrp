using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;

namespace DAL.Contracts
{
    public interface IManufacturingOrderRepository
    {
        Task<int> Insert(ManufacturingOrderA T, int CompanyId);
        Task Update(ManufacturingOrderUpdate T, int CompanyId);
        Task<IEnumerable<ManufacturingOrderDetail>> Detail(int CompanyId, int id);
        Task DoneStock(ManufacturingStock T, int CompanyId,int UserId);
        Task TaskDone(ManufacturingTaskDone T, int CompanyId);
        Task<IEnumerable<ManufacturingOrderList>> ScheludeOpenList(ManufacturingOrderList T, int CompanyId, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<ManufacturingOrderDoneList>> ScheludeDoneList(ManufacturingOrderDoneList T, int CompanyId, int KAYITSAYISI, int SAYFA);
        Task<int> ScheludeDoneListCount(ManufacturingOrderDoneList T, int CompanyId);
        Task<int> ScheludeOpenListCount(ManufacturingOrderList T, int CompanyId);
        Task<IEnumerable<ManufacturingTask>> TaskDoneList(ManufacturingTask T, int CompanyId, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<ManufacturingTask>> TaskOpenList(ManufacturingTask T, int CompanyId, int KAYITSAYISI, int SAYFA);
        Task<int> TaskOpenListCount(ManufacturingTask T, int CompanyId);
        Task<int> TaskDoneListCount(ManufacturingTask T, int CompanyId);
    }
}
