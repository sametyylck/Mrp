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
        Task<List<string>> Insert(UretimDTO T);
        Task<List<string>> Update(UretimUpdate T);
        Task<List<string>> DoneStock(UretimTamamlama T);
        Task<List<string>> DeleteItems(UretimDeleteItems T);
        Task<List<string>> IngredientsUpdate(UretimIngredientsUpdate T);
        Task<List<string>> OperationUpdate(UretimOperationsUpdate T);
        Task<List<string>> IngredientInsert(UretimIngredientsInsert T);
        Task<List<string>> OperationsInsert(UretimOperationsInsert T);
        Task<List<string>> PurchaseOrder(PurchaseBuy T);
        Task<List<string>> DeleteKontrol(List<UretimDeleteKontrol> T);



    }
}
