using DAL.DTO;
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
        Task<List<string>> Insert(UretimDTO T, int CompanyId);
        Task<List<string>> Update(UretimUpdate T, int CompanyId);
        Task<List<string>> DoneStock(UretimTamamlama T,int CompanyId);
        Task<List<string>> DeleteItems(UretimDeleteItems T, int CompanyId);
        Task<List<string>> IngredientsUpdate(UretimIngredientsUpdate T, int CompanyId);
        Task<List<string>> OperationUpdate(UretimOperationsUpdate T, int CompanyId);
        Task<List<string>> IngredientInsert(UretimIngredientsInsert T, int CompanyId);
        Task<List<string>> OperationsInsert(UretimOperationsInsert T, int CompanyId);
        Task<List<string>> PurchaseOrder(PurchaseBuy T, int CompanyId);
        Task<List<string>> DeleteKontrol(List<UretimDeleteKontrol> T, int CompanyId);



    }
}
