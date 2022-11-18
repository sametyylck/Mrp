using BL.Services.IdControl;
using BL.Services.ManufacturingOrder;
using DAL.Contracts;
using DAL.DTO;
using Dapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Data;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.PurchaseOrderDTO;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ManufacturingOrderController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IManufacturingOrderRepository _manufacturingOrderRepository;
        private readonly IManufacturingOrderItemRepository _manufacturingOrderItemRepository;
        private readonly IDbConnection _db;
        private readonly IValidator<ManufacturingOrderA> _Manufacturinginsert;
        private readonly IValidator<ManufacturingOrderUpdate> _Manufacturinguptade;
        private readonly IValidator<IdControl> _delete;
        private readonly IValidator<ManufacturingStock> _ManufacturingDoneStock;
        private readonly IValidator<ManufacturingTaskDone> _Manufacturingtaskdone;
        private readonly IValidator<ManufacturingDeleteItems> _ManufacturingItemsDelete;
        private readonly IManufacturingOrderControl _manufacturingOrderControl;
        private readonly IIDControl _idcontrol;




        public ManufacturingOrderController(IDbConnection db, IManufacturingOrderRepository manufacturingOrderRepository, IUserService user, IManufacturingOrderItemRepository manufacturingOrderItemRepository, IValidator<ManufacturingOrderA> manufacturinginsert, IValidator<ManufacturingOrderUpdate> manufacturinguptade, IValidator<IdControl> delete, IValidator<ManufacturingStock> manufacturingDoneStock, IValidator<ManufacturingTaskDone> manufacturingtaskdone, IValidator<ManufacturingDeleteItems> manufacturingItemsDelete, IManufacturingOrderControl manufacturingOrderControl, IIDControl idcontrol)
        {
            _db = db;
            _manufacturingOrderRepository = manufacturingOrderRepository;
            _user = user;
            _manufacturingOrderItemRepository = manufacturingOrderItemRepository;
            _Manufacturinginsert = manufacturinginsert;
            _Manufacturinguptade = manufacturinguptade;
            _delete = delete;
            _ManufacturingDoneStock = manufacturingDoneStock;
            _Manufacturingtaskdone = manufacturingtaskdone;
            _ManufacturingItemsDelete = manufacturingItemsDelete;
            _manufacturingOrderControl = manufacturingOrderControl;
            _idcontrol = idcontrol;
        }
        [Route("ScheludeOpenList")]
        [HttpPost]
        public async Task<ActionResult<ManufacturingOrderList>> ScheludeOpenList(ManufacturingOrderList T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _manufacturingOrderRepository.ScheludeOpenList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = await _manufacturingOrderRepository.ScheludeOpenListCount(T, CompanyId);
            return Ok(new { list, count });
        }

        [Route("ScheludeDoneList")]
        [HttpPost]
        public async Task<ActionResult<ManufacturingOrderDoneList>> ScheludeDoneList(ManufacturingOrderDoneList T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _manufacturingOrderRepository.ScheludeDoneList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = await _manufacturingOrderRepository.ScheludeDoneListCount(T, CompanyId);
            return Ok(new { list, count });
        }

        [Route("TaskOpenList")]
        [HttpPost]
        public async Task<ActionResult<ManufacturingTask>> TaskOpenList(ManufacturingTask T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _manufacturingOrderRepository.TaskOpenList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = await _manufacturingOrderRepository.TaskOpenListCount(T, CompanyId);
            return Ok(new { list, count });
        }

        [Route("TaskDoneList")]
        [HttpPost]
        public async Task<ActionResult<ManufacturingTask>> TaskDoneList(ManufacturingTask T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _manufacturingOrderRepository.TaskDoneList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = await _manufacturingOrderRepository.TaskDoneListCount(T, CompanyId);
            return Ok(new { list, count });
        }



        [Route("Insert")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] ManufacturingOrderA T)
        {
            ValidationResult result = await _Manufacturinginsert.ValidateAsync(T);
            if (result.IsValid)
            {
          
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _manufacturingOrderControl.Insert(T, CompanyId);
                if (hata=="true")
                {
                    DynamicParameters prm = new();
                    prm.Add("@CompanyId", CompanyId);

                    //Manufacturing orderi ekleyerek id sini alıyoruz
                    int id = await _manufacturingOrderRepository.Insert(T, CompanyId);
                    var salesorder = 0;
                    //adlığımız id yi ıtems leri eklemek için kullanıyoruz
                    await _manufacturingOrderItemRepository.InsertOrderItems(id, T.SalesOrderId, salesorder, CompanyId);
                    prm.Add("@id", id);
                    var list = await _db.QueryAsync<PurchaseOrderList>($"Select ManufacturingOrder.id,ManufacturingOrder.[Name], ManufacturingOrder.ItemId,ManufacturingOrderItems.Tip, ManufacturingOrder.CustomerId,ManufacturingOrder.ProductionDeadline, ManufacturingOrder.CreatedDate, ManufacturingOrder.PlannedQuantity,ManufacturingOrder.LocationId, ManufacturingOrder.Info, ManufacturingOrder.[Status],ManufacturingOrderItems.ItemId as OrdersItemsId, ManufacturingOrderItems.Notes, ManufacturingOrderItems.PlannedQuantity as ItemsPlannedQuantity , ManufacturingOrderItems.PlannedTime as ItemsPlannedTime , ManufacturingOrderItems.Cost as ItemsCost, ManufacturingOrderItems.[Availability], ManufacturingOrderItems.[Status] as ItemsStatus from ManufacturingOrder LEFT join ManufacturingOrderItems on ManufacturingOrderItems.OrderId =  ManufacturingOrder.id where ManufacturingOrder.CompanyId = @CompanyId and ManufacturingOrder.id = @id ", prm);

                    return Ok(list);
                }
                else
                {
                    return BadRequest(hata);
                }
   
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

     
        }

        [Route("Update")]
        [HttpPut]
        public async Task<ActionResult<ManufacturingOrderUpdate>> Update(ManufacturingOrderUpdate T)
        {
            ValidationResult result = await _Manufacturinguptade.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _manufacturingOrderControl.Update(T, CompanyId);
                if (hata=="true")
                {
                    DynamicParameters prm = new DynamicParameters();
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@id", T.id);
                    prm.Add("@LocationId", T.LocationId);
                    await _manufacturingOrderRepository.Update(T, CompanyId);
                    float eski = T.eskiPlanned;
                    await _manufacturingOrderItemRepository.UpdateOrderItems(T, eski, CompanyId);

                    var list = await _db.QueryAsync<ManufacturingOrderResponse>($"select ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.id,ManufacturingOrderItems.Tip,ISNULL(ManufacturingOrderItems.ItemId,0) as ItemId,Items.Name,ISNULL(ManufacturingOrderItems.PlannedQuantity, 0) as Quantity,ISNULL(Notes, '') as Note, ManufacturingOrderItems.CostPerHour, ManufacturingOrderItems.PlannedTime, ManufacturingOrderItems.Cost, ManufacturingOrderItems.Availability, ManufacturingOrderItems.Status from ManufacturingOrderItems left join ManufacturingOrder on ManufacturingOrder.id = ManufacturingOrderItems.OrderId left join Items on Items.id = ManufacturingOrderItems.ItemId where ManufacturingOrderItems.CompanyId = @CompanyId and ManufacturingOrder.id = @id", prm);

                    return Ok(list);
                }
                else
                {
                    return BadRequest(hata);
                }
   
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
       
        }

        [Route("Detail")]
        [HttpPost]
        public async Task<ActionResult<ManufacturingOrderDetail>> Detail(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var varmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From ManufacturingOrder where CompanyId = {CompanyId} and id = {id}");
            if (varmi.Count() == 0)
            {
                return BadRequest("Böyle Bir Kayıt Yok!");
            }
            var Detail = await _manufacturingOrderRepository.Detail(CompanyId, id);
            return Ok(Detail);
        }


        [Route("DoneStock")]
        [HttpPost]
        public async Task<ActionResult<ManufacturingStock>> DoneStock(ManufacturingStock T)
        {
            ValidationResult result = await _ManufacturingDoneStock.ValidateAsync(T);
            if (result.IsValid)
            {
       
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int UserId = user[1];
                string hata = await _manufacturingOrderControl.DoneStock(T, CompanyId);
                if (hata=="true")
                {
                    await _manufacturingOrderRepository.DoneStock(T, CompanyId, UserId);
                    return Ok();
                }
                else
                {
                    return BadRequest(hata);
                }

            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
 
        }

        //[Route("TaskDone")]
        //[HttpPost]
        //public async Task<ActionResult<ManufacturingStock>> TaskDone(ManufacturingTaskDone T)
        //{
        //    ValidationResult result = await _Manufacturingtaskdone.ValidateAsync(T);
        //    if (result.IsValid)
        //    {
        //        List<int> user = _user.CompanyId();
        //        int CompanyId = user[0];
        //        await _manufacturingOrderRepository.TaskDone(T, CompanyId);
        //        return Ok();
        //    }
        //    else
        //    {
        //        result.AddToModelState(this.ModelState);
        //        return BadRequest(result.ToString());
        //    }

        //}

        [Route("Delete")]
        [HttpDelete]
        public async Task<ActionResult<Delete>> Delete(IdControl T)
        {
            ValidationResult result = await _delete.ValidateAsync(T);
            if (result.IsValid)
            {
              
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int userId = user[1];

                string hata = await _idcontrol.GetControl("ManufacturingOrder", T.id, CompanyId);
                if (hata=="true")
                {
                    await _manufacturingOrderItemRepository.DeleteStockControl(T, CompanyId, userId);
                    return Ok("Silme İşlemi Başarıyla Gerçekleşti");
                }
                else
                {
                    return BadRequest(hata);
                }

         
             
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
         
        }

        [Route("DeleteItems")]
        [HttpDelete]
        public async Task<ActionResult<DeleteItems>> DeleteItems(ManufacturingDeleteItems T)
        {
            ValidationResult result = await _ManufacturingItemsDelete.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata=await _manufacturingOrderControl.DeleteItems(T, CompanyId);
                if (hata=="true")
                {
                    await _manufacturingOrderItemRepository.DeleteItems(T, CompanyId);
                    return Ok("Silme İşlemi Başarıyla Gerçekleşti");
                }
                else
                {
                    return BadRequest(hata);
                }
       
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
         
        }

    }
}
