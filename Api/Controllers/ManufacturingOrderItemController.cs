using BL.Services.ManufacturingOrder;
using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using Dapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.ComponentModel.Design;
using System.Data;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ManufacturingOrderItemController : ControllerBase
    {

        private readonly IUserService _user;
        private readonly IDbConnection _db;
        private readonly IManufacturingOrderItemRepository _manufacturingitem;
        private readonly IOrdersRepository _order;
        private readonly IValidator<ManufacturingOrderItemsIngredientsUpdate> _ManuItemsIngUpdate;
        private readonly IValidator<ManufacturingOrderItemsIngredientsInsert> _ManuItemIngInsert;
        private readonly IValidator<ManufacturingOrderItemsOperationsInsert> _ManuItemOperInsert;
        private readonly IValidator<ManufacturingOrderItemsOperationsUpdate> _ManuItemOperUpdate;
        private readonly IValidator<ManufacturingPurchaseOrder> _ManuItemPurchaseOrder;
        private readonly IManufacturingOrderControl _manufacturingOrderControl;

        public ManufacturingOrderItemController(IUserService user, IManufacturingOrderItemRepository manufacturingitem, IDbConnection db, IOrdersRepository order, IValidator<ManufacturingOrderItemsIngredientsUpdate> manuItemsIngUpdate, IValidator<ManufacturingOrderItemsIngredientsInsert> manuItemIngInsert, IValidator<ManufacturingOrderItemsOperationsInsert> manuItemOperInsert, IValidator<ManufacturingOrderItemsOperationsUpdate> manuItemOperUpdate, IValidator<ManufacturingPurchaseOrder> manuItemPurchaseOrder, IManufacturingOrderControl manufacturingOrderControl)
        {
            _user = user;
            _manufacturingitem = manufacturingitem;
            _db = db;
            _order = order;
            _ManuItemsIngUpdate = manuItemsIngUpdate;
            _ManuItemIngInsert = manuItemIngInsert;
            _ManuItemOperInsert = manuItemOperInsert;
            _ManuItemOperUpdate = manuItemOperUpdate;
            _ManuItemPurchaseOrder = manuItemPurchaseOrder;
            _manufacturingOrderControl = manufacturingOrderControl;
        }
        [Route("IngredientsDetail")]
        [HttpGet]
        public async Task<ActionResult<ManufacturingOrderItemsIngredientsDetail>> IngredientsDetail(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var varmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From ManufacturingOrderItems where CompanyId = {CompanyId} and OrderId = {id}");
            if (varmi.Count() == 0)
            {
                return BadRequest("Böyle Bir Kayıt Bulunamadı!");
            }
            var IngredientsDetail = await _manufacturingitem.IngredientsDetail(CompanyId, id);
            return Ok(IngredientsDetail);
        }

        [Route("OperationDetail")]
        [HttpGet]
        public async Task<ActionResult<ManufacturingOrderItemsIngredientsDetail>> OperationDetail(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var varmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From ManufacturingOrderItems where CompanyId = {CompanyId} and OrderId = {id}");
            if (varmi.Count() == 0)
            {
                return BadRequest("Böyle Bir Kayıt Bulunamadı!");
            }
            var IngredientsDetail = await _manufacturingitem.OperationDetail(CompanyId, id);
            return Ok(IngredientsDetail);
        }

        [Route("IngredientsUpdate")]
        [HttpPut]
        public async Task<ActionResult<ManufacturingOrderItemsIngredientsUpdate>> IngredientsUpdate(ManufacturingOrderItemsIngredientsUpdate T)
        {
            ValidationResult result = await _ManuItemsIngUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _manufacturingOrderControl.IngredientsUpdate(T, CompanyId);
                if (hata!="true")
                {
                    return BadRequest(hata);
                }
                else
                {
                    DynamicParameters prm = new DynamicParameters();
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@id", T.id);
                    prm.Add("@OrderId", T.OrderId);
                    prm.Add("@ItemId", T.ItemId);
                    string sqld = $@"select
                (select ISNULL(id,0) from ManufacturingOrderItems where OrderId=@OrderId and id=@id and ItemId=@ItemId )as id,
                (select ItemId from ManufacturingOrderItems where OrderId=@OrderId and id=@id)as ItemId ";
                    var ItemIddegistimi = await _db.QueryAsync<ManufacturingOrderItemsIngredientsUpdate>(sqld, prm);
                    prm.Add("@MaterialId", ItemIddegistimi.First().ItemId);//Item değiştimi kontrol ve rezerve geri eklemeleri

                    if (ItemIddegistimi.First().id == 0 || ItemIddegistimi.First().id == null)
                    {
                        List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"select  (select id from Rezerve where ManufacturingOrderId = @OrderId and CompanyId = @CompanyId and ItemId = @MaterialId and ManufacturingOrderItemId=@id and Status=1)as RezerveId", prm)).ToList();

                        var rezerveid = sorgu.First().RezerveId;
                        prm.Add("@rezerveid", rezerveid);
                        prm.Add("@Status", 4);
                        await _db.ExecuteAsync($"Update Rezerve set Status=@Status where ManufacturingOrderId=@OrderId and CompanyId=@CompanyId and ItemId=@MaterialId and ManufacturingOrderItemId=@id and id=@rezerveid", prm);
                    }
                    string sql = $@"Update ManufacturingOrderItems Set ItemId=@ItemId where CompanyId=@CompanyId and OrderId=OrderId and id=@id  ";
                    await _db.ExecuteAsync(sql, prm);
                    await _manufacturingitem.IngredientsUpdate(T, CompanyId);


                    var list = await _db.QueryAsync<ManufacturingOrderResponse>($"select ManufacturingOrderItems.id,ManufacturingOrderItems.Tip,ISNULL(ManufacturingOrderItems.ItemId,0) as ItemId,Items.Name,ISNULL(ManufacturingOrderItems.PlannedQuantity,0) as Quantity,ISNULL(Notes,'') as Note, ManufacturingOrderItems.Cost, ManufacturingOrderItems.Availability from ManufacturingOrderItems left join ManufacturingOrder on ManufacturingOrder.id = ManufacturingOrderItems.OrderId left join Items on Items.id = ManufacturingOrderItems.ItemId where ManufacturingOrderItems.CompanyId = @CompanyId and ManufacturingOrderItems.id = @id and ManufacturingOrder.id = @OrderId", prm);

                    return Ok(list);
                }
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

            
        }
        [Route("OperationsUpdate")]
        [HttpPut]
        public async Task<ActionResult<ManufacturingOrderItemsOperationsUpdate>> OperationsUpdate(ManufacturingOrderItemsOperationsUpdate T)
        {
            ValidationResult result = await _ManuItemOperUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _manufacturingOrderControl.OperationUpdate(T, CompanyId);
                if (hata!="true")
                {
                    return BadRequest(hata);
                }
                else
                {
                    await _manufacturingitem.OperationsUpdate(T, CompanyId);
                    DynamicParameters prm = new DynamicParameters();
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@id", T.id);
                    prm.Add("@OrderId", T.OrderId);
                    var list = await _db.QueryAsync<ManufacturingOrderOperations>($"select ManufacturingOrderItems.id,ManufacturingOrderItems.OperationId,Operations.[Name] as OperationName,ManufacturingOrderItems.ResourceId, Resources.[Name] as ResourceName, ManufacturingOrderItems.CostPerHour,ManufacturingOrderItems.PlannedTime, ManufacturingOrderItems.Cost, ManufacturingOrderItems.Status from ManufacturingOrderItems left join ManufacturingOrder on ManufacturingOrder.id = ManufacturingOrderItems.OrderId left join Resources on Resources.id = ManufacturingOrderItems.ResourceId left join Operations on Operations.id = ManufacturingOrderItems.OperationId left join Items on Items.id = ManufacturingOrderItems.ItemId where ManufacturingOrderItems.CompanyId = @CompanyId and ManufacturingOrderItems.id = @id", prm);

                    return Ok(list);


                }
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
     
        }
        [Route("IngredientsInsert")]
        [HttpPost]
        public async Task<ActionResult<ManufacturingOrderItemsIngredientsInsert>> IngredientsInsert(ManufacturingOrderItemsIngredientsInsert T)
        {
            ValidationResult result = await _ManuItemIngInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata=await _manufacturingOrderControl.IngredientInsert(T, CompanyId);
                if (hata=="true")
                {
                    int id = await _manufacturingitem.IngredientsInsert(T, CompanyId);
                    string sql = $@"Select moi.id,moi.Tip,ItemId,Items.Name,ISNULL(Notes,'')AS Note,moi.PlannedQuantity as Quantity,moi.Cost,moi.Availability From ManufacturingOrderItems moi
                                left join Items on Items.id = moi.ItemId
                                where moi.CompanyId = {CompanyId} and moi.OrderId = {T.OrderId} and moi.id = {id}";
                    var eklenen = await _db.QueryAsync<ManufacturingOrderItemsIngredientsDetail>(sql);
                    return Ok(eklenen);
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

        [Route("OperationsInsert")]
        [HttpPost]
        public async Task<ActionResult<ManufacturingOrderItemsOperationsInsert>> OperationsInsert(ManufacturingOrderItemsOperationsInsert T)
        {
            ValidationResult result = await _ManuItemOperInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _manufacturingOrderControl.OperationsInsert(T, CompanyId);
                if (hata=="true")
                {
                    int id = await _manufacturingitem.OperationsInsert(T, CompanyId);
                    DynamicParameters prm = new DynamicParameters();
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@id", id);

                    string sql = $@"select ManufacturingOrderItems.id,ManufacturingOrderItems.OperationId,Operations.[Name] as OperationName,ManufacturingOrderItems.ResourceId, Resources.[Name] as ResourceName, ManufacturingOrderItems.CostPerHour,ManufacturingOrderItems.PlannedTime, ManufacturingOrderItems.Cost, ManufacturingOrderItems.Status from ManufacturingOrderItems left join ManufacturingOrder on ManufacturingOrder.id = ManufacturingOrderItems.OrderId left join Resources on Resources.id = ManufacturingOrderItems.ResourceId left join Operations on Operations.id = ManufacturingOrderItems.OperationId left join Items on Items.id = ManufacturingOrderItems.ItemId where ManufacturingOrderItems.CompanyId = @CompanyId and ManufacturingOrderItems.id = @id";
                    var eklenen = await _db.QueryAsync<ManufacturingOrderOperations>(sql, prm);
                    return Ok(eklenen);
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

        [Route("PurchaseOrderBuy")]
        [HttpPost]
        public async Task<ActionResult<ManufacturingPurchaseOrder>> PurchaseOrderBuy(ManufacturingPurchaseOrder T)
        {
            ValidationResult result = await _ManuItemPurchaseOrder.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];

                string hata = await _manufacturingOrderControl.PurchaseOrder(T, CompanyId);
                if (hata=="true")
                {

                    DynamicParameters prm = new DynamicParameters();
                    prm.Add("@OrderId", T.ManufacturingOrderId);
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@id", T.ManufacturingOrderItemId);
                    prm.Add("@ItemId", T.ItemId);
                    prm.Add("@LocationId", T.LocationId);

                    string sqlsorgu = $@"
				 select  
              (select
            (ISNULL(LocationStock.StockCount,0)-ISNULL(SUM(Rezerve.RezerveCount),0))-ISNULL(moi.PlannedQuantity,0)+ISNULL((rez.RezerveCount),0)as Missing
            
            from ManufacturingOrder mo
			    left join ManufacturingOrderItems moi on mo.id=moi.OrderId 
            left join Items on Items.id=moi.ItemId 
			LEFT join Rezerve on  Rezerve.ItemId=Items.id and Rezerve.Status=1 AND Rezerve.LocationId=@LocationId
		   LEFT join Rezerve rez on rez.ManufacturingOrderId=mo.id and rez.ManufacturingOrderItemId=moi.id and Rezerve.Status=1 AND rez.LocationId=131
            left join LocationStock on LocationStock.ItemId=Items.id and LocationStock.LocationId=@LocationId
            where  moi.CompanyId = @CompanyId and moi.id = @id and moi.Tip='Ingredients'  and mo.id=@OrderId and  mo.Status!=3
            Group by moi.id,moi.Tip,moi.ItemId,Items.Name,Notes,moi.Cost,moi.Availability
            ,LocationStock.StockCount,moi.PlannedQuantity,rez.RezerveCount)+
			(select ISNULL(SUM(OrdersItem.Quantity),0) from OrdersItem 
			left join Orders on Orders.id=OrdersItem.OrdersId
			where ManufacturingOrderId=@OrderId and ManufacturingOrderItemId=@id and Orders.DeliveryId=1 and Orders.IsActive=1 and Orders.LocationId=@LocationId)as missing";
                    int? missingdeger = await _db.QueryFirstAsync<int>(sqlsorgu, prm);

                    PurchaseOrderInsert A = new PurchaseOrderInsert();
                    A.ContactId = T.ContactId;
                    A.OrderName = T.OrderName;
                    A.LocationId = T.LocationId;
                    A.SalesOrderId = T.SalesOrderId;
                    A.SalesOrderItemId = T.SalesOrderItemId;
                    A.ManufacturingOrderId = T.ManufacturingOrderId;
                    A.ManufacturingOrderItemId = T.ManufacturingOrderItemId;
                    A.Tip = T.Tip;
                    A.ExpectedDate = T.ExpectedDate;
                    A.CreateDate = T.CreateDate;

                    int id = await _order.Insert(A, CompanyId);
                    prm.Add("@id", id);
                    string sql = $@"Update Orders Set ManufacturingOrderId=@OrderId where CompanyId=@CompanyId and id=@id ";
                    await _db.ExecuteAsync(sql, prm);

                    PurchaseOrderInsertItem B = new PurchaseOrderInsertItem();
                    B.ItemId = T.ItemId;
                    B.Quantity = T.Quantity;
                    B.TaxId= await _db.QueryFirstAsync<int>("select DefaultTaxPurchaseOrderId from GeneralDefaultSettings where CompanyId=@CompanyId", prm);
                    B.MeasureId= await _db.QueryFirstAsync<int>($"select MeasureId from Items where id={T.ItemId} ", prm);
                    await _order.InsertPurchaseItem(B, id, CompanyId);
                    await _manufacturingitem.BuyStockControl(T, missingdeger, CompanyId);

                    string sqllist = $@"
                     select Orders.id,OrderName,OrdersItem.ItemId,SalesOrderId,SalesOrderItemId,ManufacturingOrderId,ManufacturingOrderItemId,DeliveryId 
                     from Orders 
                     left join OrdersItem on OrdersItem.OrdersId=Orders.id
                     where Orders.CompanyId=@CompanyId and Orders.Tip='PurchaseOrder' and Orders.id=@id and IsActive=1";
                    var list = await _db.QueryAsync<PurchaseBuy>(sqllist, prm);
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






    }
}
