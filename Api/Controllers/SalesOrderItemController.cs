using BL.Services.SalesOrder;
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
using System.Data;
using System.Security.Claims;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesOrderItemController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly ISalesOrderItemRepository _salesOrderItemRepository;
        private readonly IDbConnection _db;
        private readonly IManufacturingOrderItemRepository _manufacturingOrderItemRepository;
        private readonly IManufacturingOrderRepository _manufacturingOrder;
        private readonly IValidator<SalesOrderMake> _SalesOrderMake;
        private readonly IValidator<SalesOrderUpdateItems> _SalesOrderUpdateItems;
        private readonly ISalesOrderControl _salescontrol;

        public SalesOrderItemController(IUserService user, ISalesOrderItemRepository salesOrderItemRepository, IDbConnection db, IManufacturingOrderItemRepository manufacturingOrderItemRepository, IManufacturingOrderRepository manufacturingOrder, IValidator<SalesOrderMake> salesOrderMake, IValidator<SalesOrderUpdateItems> salesOrderUpdateItems, ISalesOrderControl salescontrol)
        {
            _user = user;
            _salesOrderItemRepository = salesOrderItemRepository;
            _db = db;
            _manufacturingOrderItemRepository = manufacturingOrderItemRepository;
            _manufacturingOrder = manufacturingOrder;
            _SalesOrderMake = salesOrderMake;
            _SalesOrderUpdateItems = salesOrderUpdateItems;
            _salescontrol = salescontrol;
        }



        [Route("Detail")]
        [HttpGet, Authorize]
        public async Task<ActionResult<SalesOrderDetail>> Detail(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            var varmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From Orders where CompanyId = {CompanyId} and id = {id}");
            if (varmi.First() == 0)
            {
                return BadRequest("Böyle Bir Kayıt Bulunamadı!");
            }
            var Detail = await _salesOrderItemRepository.Detail(CompanyId, id);
            var billingid = Detail.First().BillingAddressId;
            var shipping = Detail.First().ShippingAddressId;
            prm.Add("@billing", billingid);
            prm.Add("@shipping", shipping);
            List<LocationsDTO> billingadress = (await _db.QueryAsync<LocationsDTO>($"Select * from Locations where id=@billing and CompanyId=@CompanyId", prm)).ToList();
            List<LocationsDTO> shippingaddress = (await _db.QueryAsync<LocationsDTO>($"Select * from Locations where id=@shipping and CompanyId=@CompanyId", prm)).ToList();
            return Ok(new { Detail, billingadress, shippingaddress });

        }


        [Route("ItemDetail")]
        [HttpGet, Authorize]
        public async Task<ActionResult<SalesOrderItemDetail>> ItemDetail(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var varmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From Orders where CompanyId = {CompanyId} and id = {id}");
            if (varmi.First() == 0)
            {
                return BadRequest("Böyle Bir Kayıt Bulunamadı!");
            }
            var OrdersItem = await _db.QueryAsync<int>($"Select Count(*) as varmi From OrdersItem where CompanyId = {CompanyId} and OrdersId = {id}");
            if (OrdersItem.Count() == 0)
            {
                return Ok();
            }

            var ItemDetail = await _salesOrderItemRepository.ItemDetail(CompanyId, id);
            return Ok(ItemDetail);

        }


        [Route("IngredientsMissingList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<MissingCount>> IngredientsMissingList(MissingCount T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var IngredientMissingList = await _salesOrderItemRepository.IngredientsMissingList(T, CompanyId);
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@OrderItemId", T.SalesOrderItemId);
            prm.Add("@CompanyId", CompanyId);
            string sqlc = $@"select OrdersItem.id ,ItemId,Items.Name as ItemName,Orders.ContactId,Contacts.DisplayName,Quantity,Orders.id as OrdersId from OrdersItem 
                          left join Orders on Orders.id=OrdersItem.OrdersId
                          left join Contacts on Contacts.id=Orders.ContactId
                          left join Items on Items.id=OrdersItem.ItemId
                          where Orders.Tip='PurchaseOrder' and Orders.SalesOrderId=@id and Orders.SalesOrderItemId=@OrderItemId and DeliveryId=1";
            var PurchaseOrder = await _db.QueryAsync<PurchaseOrdersItemDetails>(sqlc, prm);

            return Ok(new { IngredientMissingList, PurchaseOrder });

        }

        [Route("SellSomeList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SalesOrderSellSomeList>> SellSomeList(SalesOrderSellSomeList T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            var varmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From Orders where CompanyId = {CompanyId} and id = {T.SalesOrderId}");
            if (varmi.First() == 0)
            {
                return BadRequest("Böyle Bir Kayıt Bulunamadı!");
            }
            var List = await _salesOrderItemRepository.SellSomeList(T, CompanyId);

            return Ok(List);

        }

        [Route("Make")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SalesOrderMake>> Make(SalesOrderMake T)
        {
            ValidationResult result = await _SalesOrderMake.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _salescontrol.Make(T, CompanyId);
                if (hata=="true")
                {
                    DynamicParameters prm = new DynamicParameters();
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@ItemId", T.ItemId);
                    prm.Add("@SalesOrderId", T.SalesOrderId);
                    prm.Add("@SalesOrderItemId", T.SalesOrderItemId);
                    prm.Add("@CustomerId", T.ContactId);
                    prm.Add("@location", T.LocationId);
                    var ItemId = T.ItemId;

                    ManufacturingOrderA A = new ManufacturingOrderA();
                    A.ItemId = T.ItemId;
                    A.PlannedQuantity = T.PlannedQuantity;
                    A.SalesOrderId = T.SalesOrderId;
                    A.SalesOrderItemId = T.SalesOrderItemId;
                    A.ContactId = T.ContactId;
                    A.LocationId = T.LocationId;
                    A.Tip = T.Tip;
                    A.ExpectedDate = T.ExpectedDate;
                    A.CreatedDate = T.CreatedDate;
                    A.ProductionDeadline = T.ProductionDeadline;
                    if (T.Tip == "MakeBatch")
                    {


                        int id = await _manufacturingOrder.Insert(A, CompanyId);
                        await _manufacturingOrderItemRepository.InsertOrderItems(id, T.SalesOrderId, T.SalesOrderItemId, CompanyId);

                    }
                    else if (T.Tip == "MakeOrder")
                    {

                        List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select (Select ISNULL(id, 0) from LocationStock where ItemId =  @ItemId  and LocationId = @location and CompanyId = @CompanyId)   as LocationStockId, (select ISNULL(DefaultPrice, 0) From Items where CompanyId = @CompanyId and id = @ItemId)as  DefaultPrice,(select Rezerve.RezerveCount from Rezerve where CompanyId=@CompanyId and ItemId=@ItemId and CustomerId=@CustomerId and LocationId=@location and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId)as RezerveCount", prm)).ToList();
                        float? rezervecount = sorgu.First().RezerveCount;
                        float LocationStockId = sorgu.First().LocationStockId;
                        prm.Add("@LocationStockId", LocationStockId);
                        var make = await _db.QueryAsync<SalesOrderRezerve>($"select mo.id from ManufacturingOrder mo where SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and CompanyId=@CompanyId  and IsActive=1 and Status!=3", prm);

                        foreach (var item in make)
                        {
                            prm.Add("@manuid", item.id);
                            prm.Add("@Null", null);
                            await _db.ExecuteAsync($"Update ManufacturingOrder set SalesOrderId=@Null , SalesOrderItemId=@Null , CustomerId=@Null where id=@manuid and  CompanyId=@CompanyId", prm);

                        }



                        if (rezervecount != null)
                        {
                            prm.Add("@Status", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Status=@Status where SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and  CompanyId=@CompanyId", prm);

                            int id = await _manufacturingOrder.Insert(A, CompanyId);
                            await _manufacturingOrderItemRepository.InsertOrderItems(id, T.SalesOrderId, T.SalesOrderItemId, CompanyId);
                        }
                        else
                        {
                            int id = await _manufacturingOrder.Insert(A, CompanyId);
                            await _manufacturingOrderItemRepository.InsertOrderItems(id, T.SalesOrderId, T.SalesOrderItemId, CompanyId);
                        }


                    }
                    var sales = await _db.QueryAsync<int>($@"select SalesItem from OrdersItem where CompanyId=@CompanyId and id=@SalesOrderItemId", prm);
                    int salesId = sales.First();
                    if (salesId != 1)
                    {
                        if (salesId == 2)
                        {
                            prm.Add("@ProductionId", 1);
                            await _db.ExecuteAsync($"Update OrdersItem set Production=@ProductionId where CompanyId=@CompanyId and id=@SalesOrderItemId", prm);
                        }
                        else if (salesId == 3)
                        {
                            prm.Add("@ProductionId", 4);
                            await _db.ExecuteAsync($"Update OrdersItem set Production=@ProductionId where CompanyId=@CompanyId and id=@SalesOrderItemId", prm);
                        }
                    }
                    else if (salesId == 1)
                    {
                        prm.Add("@ProductionId", 0);
                        await _db.ExecuteAsync($"Update OrdersItem set Production=@ProductionId where CompanyId=@CompanyId and id=@SalesOrderItemId", prm);
                    }



                    var list = await _db.QueryAsync<SalesOrderItemDetail>($@"Select  OrdersItem.id,OrdersItem.ItemId,Items.Name as ItemName,OrdersItem.Quantity,
                 OrdersItem.PricePerUnit, OrdersItem.TotalAll, OrdersItem.TaxId, Tax.TaxName,OrdersItem.TaxValue as Rate,OrdersItem.SalesItem,OrdersItem.Ingredients,OrdersItem.Production from Orders 
              left join OrdersItem on OrdersItem.OrdersId = Orders.id left join Items on Items.id = OrdersItem.ItemId left 
              join Measure on Measure.id = OrdersItem.MeasureId left  join Tax on Tax.id = OrdersItem.TaxId
              where Orders.CompanyId =@CompanyId and Orders.id =@SalesOrderId  and OrdersItem.id =@SalesOrderItemId", prm);



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


        [Route("UpdateItems")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SalesOrderUpdateItems>> UpdateItems(SalesOrderUpdateItems T)
        {
            ValidationResult result = await _SalesOrderUpdateItems.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _salescontrol.UpdateItem(T, CompanyId);
                if (hata=="true")
                {
                    await _salesOrderItemRepository.UpdateItems(T, T.id, CompanyId);
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@CompanyId", CompanyId);
                    param2.Add("@OrderId", T.id);
                    param2.Add("@OrderItemId", T.OrderItemId);

                    if (T.Quotes == null)
                    {
                        var sales = await _db.QueryAsync<int>($@"select SalesItem from OrdersItem where CompanyId=@CompanyId and id=@OrderItemId", param2);
                        int salesId = sales.First();
                        if (salesId != 1)
                        {
                            if (salesId == 2)
                            {
                                param2.Add("@ProductionId", 1);
                                await _db.ExecuteAsync($"Update OrdersItem set Production=@ProductionId where CompanyId=@CompanyId and id=@OrderItemId", param2);
                            }
                            else if (salesId == 3)
                            {
                                param2.Add("@ProductionId", 4);
                                await _db.ExecuteAsync($"Update OrdersItem set Production=@ProductionId where CompanyId=@CompanyId and id=@OrderItemId", param2);
                            }
                        }
                        else if (salesId == 1)
                        {
                            param2.Add("@ProductionId", 0);
                            await _db.ExecuteAsync($"Update OrdersItem set Production=@ProductionId where CompanyId=@CompanyId and id=@OrderItemId", param2);
                        }
                    }


                    param2.Add("@LocationId", T.LocationId);
                    var list = await _db.QueryAsync<SalesOrderItemDetail>($@"Select OrdersItem.id as id,OrdersItem.ItemId,Items.Name as ItemName,OrdersItem.Quantity,OrdersItem.Tip,
                 OrdersItem.PricePerUnit, OrdersItem.TotalAll, OrdersItem.TaxId, Tax.TaxName,OrdersItem.TaxValue as Rate,OrdersItem.SalesItem,OrdersItem.Ingredients,OrdersItem.Production,
         		   (LocationStock.StockCount-SUM(rez.RezerveCount)- ISNULL(OrdersItem.Quantity,0)+(SUM(ISNULL(ManufacturingOrder.PlannedQuantity,0)))+ISNULL(Rezerve.RezerveCount,0))as missing
           from Orders 
              left join OrdersItem on OrdersItem.OrdersId = Orders.id 
        left join Items on Items.id = OrdersItem.ItemId
        left join Measure on Measure.id = OrdersItem.MeasureId
        left join Tax on Tax.id = OrdersItem.TaxId
        LEFT join ManufacturingOrder on ManufacturingOrder.SalesOrderItemId=OrdersItem.id 
		and ManufacturingOrder.SalesOrderId=Orders.id and ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1
        LEFT join Rezerve on Rezerve.SalesOrderItemId=OrdersItem.id and Rezerve.SalesOrderId=Orders.id and Rezerve.ItemId=Items.id
		left join Rezerve rez on rez.ItemId=Items.id and rez.Status=1  and rez.LocationId=@LocationId
           left join LocationStock on LocationStock.ItemId=Items.id and LocationStock.LocationId=@LocationId
              where Orders.CompanyId = @CompanyId and Orders.id = @OrderId  and OrdersItem.id =@OrderItemId   and Rezerve.ItemId=@CompanyId
        Group by OrdersItem.id,OrdersItem.ItemId,Items.Name,OrdersItem.Quantity,OrdersItem.Tip,
                 OrdersItem.PricePerUnit, OrdersItem.TotalAll, OrdersItem.TaxId, Tax.TaxName,OrdersItem.TaxValue,
                 OrdersItem.SalesItem,OrdersItem.Ingredients,OrdersItem.Production,LocationStock.StockCount,Rezerve.RezerveCount ", param2);

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
