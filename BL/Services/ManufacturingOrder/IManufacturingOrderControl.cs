using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;

namespace BL.Services.ManufacturingOrder
{
    public interface IManufacturingOrderControl
    {
        Task<string> Insert(ManufacturingOrderA T, int CompanyId);
        Task<string> Update(ManufacturingOrderUpdate T, int CompanyId);
        Task<string> DoneStock(ManufacturingStock T,int CompanyId);
        Task<string> DeleteItems(ManufacturingDeleteItems T, int CompanyId);
        Task<string> IngredientsUpdate(ManufacturingOrderItemsIngredientsUpdate T, int CompanyId);
        Task<string> OperationUpdate(ManufacturingOrderItemsOperationsUpdate T, int CompanyId);
        Task<string> IngredientInsert(ManufacturingOrderItemsIngredientsInsert T, int CompanyId);
        Task<string> OperationsInsert(ManufacturingOrderItemsOperationsInsert T, int CompanyId);
        Task<string> PurchaseOrder(ManufacturingPurchaseOrder T, int CompanyId);



    }
}
