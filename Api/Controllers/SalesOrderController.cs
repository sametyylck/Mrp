using BL.Services.IdControl;
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
using System.ComponentModel.Design;
using System.Data;
using static DAL.DTO.BomDTO;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.SalesOrderDTO.Quotess;
using Quotess = DAL.DTO.SalesOrderDTO.Quotess;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesOrderController : ControllerBase
    
    {
        private readonly ISalesOrderRepository _salesOrder;
        private readonly IUserService _user;
        private readonly IDbConnection _db;
        private readonly ISalesOrderItemRepository _salesOrderItem;
        private readonly IValidator<SalesOrder> _SalesOrderInsert;
        private readonly IValidator<SalesOrderItem> _SalesOrderInsertItem;

        private readonly IValidator<SalesOrderUpdate> _SalesOrderUpdate;

        private readonly IValidator<SalesDelete> _SalesOrderDelete;

        private readonly IValidator<SalesDeleteItems> _SalesOrderDeleteItems;

        private readonly IValidator<SalesDone> _SalesOrderDone;

        private readonly IValidator<SalesOrderDTO.Quotess> _SalesOrderQuotesDone;
        private readonly ISalesOrderControl _salescontrol;
        private readonly IIDControl _idcontrol;


        public SalesOrderController(ISalesOrderRepository salesOrder, IUserService user, IDbConnection db, ISalesOrderItemRepository salesOrderItem, IValidator<SalesOrder> salesOrderInsert, IValidator<SalesOrderItem> salesOrderInsertItem, IValidator<SalesOrderUpdate> salesOrderUpdate, IValidator<SalesDelete> salesOrderDelete, IValidator<SalesDeleteItems> salesOrderDeleteItems, IValidator<SalesDone> salesOrderDone, IValidator<Quotess> salesOrderQuotesDone, ISalesOrderControl salescontrol, IIDControl idcontrol)
        {
            _salesOrder = salesOrder;
            _user = user;
            _db = db;
            _salesOrderItem = salesOrderItem;
            _SalesOrderInsert = salesOrderInsert;
            _SalesOrderInsertItem = salesOrderInsertItem;
            _SalesOrderUpdate = salesOrderUpdate;
            _SalesOrderDelete = salesOrderDelete;
            _SalesOrderDeleteItems = salesOrderDeleteItems;
            _SalesOrderDone = salesOrderDone;
            _SalesOrderQuotesDone = salesOrderQuotesDone;
            _salescontrol = salescontrol;
            _idcontrol = idcontrol;
        }
        [Route("SalesOrderList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SalesOrderList>> SalesOrderList(SalesOrderList T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _salesOrder.SalesOrderList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = await _salesOrder.SalesOrderCount(T, CompanyId);
            return Ok(new { list, count });

        }

        [Route("QuotesList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SalesOrderList>> QuotesList(SalesOrderList T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _salesOrder.QuotesList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = await _salesOrder.QuotesCount(T, CompanyId);
            return Ok(new { list, count });

        }


        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SalesOrder>> Insert(SalesOrder T)
        {
            ValidationResult result = await _SalesOrderInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _salescontrol.Insert(T, CompanyId);
                if (hata=="true")
                {

                    int id = await _salesOrder.Insert(T, CompanyId);
                    await _salescontrol.Adress(id, T.ContactId, CompanyId);
                    var list = await _db.QueryAsync<SalesOrderResponse>($"Select Orders.id,Orders.Tip,Orders.ContactId,Contacts.DisplayName as SupplierName,Orders.DeliveryDeadline,Orders.CreateDate, Orders.OrderName, Orders.LocationId,Orders.Info, Orders.CompanyId From Orders left join Contacts on Contacts.id = Orders.ContactId  where Orders.CompanyId = {CompanyId} and Orders.id = {id} and Orders.Tip = 'SalesOrder'");
                    return Ok(list.First());
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

        [Route("InsertItems")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SalesOrderItem>> InsertItems(SalesOrderItem T)
        {
            ValidationResult result = await _SalesOrderInsertItem.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata=await _salescontrol.InsertItem(T,CompanyId);
                if (hata == "true")
                {
                    int id = await _salesOrderItem.InsertPurchaseItem(T, CompanyId);
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@CompanyId", CompanyId);
                    param2.Add("@OrderId", T.id);
                    param2.Add("@LocationId", T.LocationId);
                    param2.Add("@id", id);
                    if (T.Quotes == 0)
                    {

                    }
                    else
                    {
                        var sales = await _db.QueryAsync<int>($@"select SalesItem from OrdersItem where CompanyId=@CompanyId and id=@id", param2);
                        int salesId = sales.First();
                        if (salesId != 1)
                        {
                            if (salesId == 2)
                            {
                                param2.Add("@ProductionId", 1);
                                await _db.ExecuteAsync($"Update OrdersItem set Production=@ProductionId where CompanyId=@CompanyId and id=@id", param2);
                            }
                            else if (salesId == 3)
                            {
                                param2.Add("@ProductionId", 4);
                                await _db.ExecuteAsync($"Update OrdersItem set Production=@ProductionId where CompanyId=@CompanyId and id=@id", param2);
                            }
                        }
                        else if (salesId == 1)
                        {
                            param2.Add("@ProductionId", 0);
                            await _db.ExecuteAsync($"Update OrdersItem set Production=@ProductionId where CompanyId=@CompanyId and id=@id", param2);
                        }
                    }


                    var list = await _db.QueryAsync<SalesOrderItemResponse>($@"Select  OrdersItem.id,OrdersItem.ItemId,Items.Name as ItemName,OrdersItem.Quantity,
                 OrdersItem.PricePerUnit, OrdersItem.TotalAll, OrdersItem.TaxId, Tax.TaxName,OrdersItem.TaxValue as Rate,OrdersItem.SalesItem,OrdersItem.Ingredients,OrdersItem.Production,OrdersItem.TotalPrice,

           ((LocationStock.StockCount-ISNULL(SUM(rez.RezerveCount),0))- ISNULL(OrdersItem.Quantity,0)+(SUM(ISNULL(ManufacturingOrder.PlannedQuantity,0)))+ISNULL(Rezerve.RezerveCount,0))as Missing
           from Orders 
              left join OrdersItem on OrdersItem.OrdersId = Orders.id
        left join Items on Items.id = OrdersItem.ItemId 
        left join Measure on Measure.id = OrdersItem.MeasureId 
        left  join Tax on Tax.id = OrdersItem.TaxId
        LEFT join ManufacturingOrder on ManufacturingOrder.SalesOrderItemId=OrdersItem.id and ManufacturingOrder.SalesOrderId=Orders.id and ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1
        LEFT join Rezerve on Rezerve.SalesOrderItemId=OrdersItem.id and Rezerve.SalesOrderId=Orders.id and Rezerve.ItemId=Items.id
		LEFT join Rezerve rez on rez.ItemId=Items.id and rez.Status=1 
        left join LocationStock on LocationStock.ItemId=Items.id and LocationStock.LocationId=@LocationId
         where Orders.CompanyId =@CompanyId and Orders.id =@OrderId  and OrdersItem.id =@İD
        Group by OrdersItem.id,OrdersItem.ItemId,Items.Name,OrdersItem.Quantity,OrdersItem.PricePerUnit,OrdersItem.TotalAll,OrdersItem.TaxId,Tax.TaxName,OrdersItem.TaxValue,
           OrdersItem.SalesItem,OrdersItem.Ingredients,OrdersItem.Production,LocationStock.StockCount,Rezerve.RezerveCount,REZ.RezerveCount,OrdersItem.TotalPrice", param2);

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
        [HttpPut, Authorize]
        public async Task<ActionResult<SalesOrderUpdate>> Update(SalesOrderUpdate T)
        {
            ValidationResult result = await _SalesOrderUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _salescontrol.Update(T, CompanyId);
                if (hata == "true")
                {
                    await _salesOrder.Update(T, T.id, CompanyId);
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

        [Route("Delete")]
        [HttpDelete, Authorize]
        public async Task<ActionResult<SalesDelete>> Delete(SalesDelete T)
        {
            ValidationResult result = await _SalesOrderDelete.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int userid = user[1];
                string hata =await _idcontrol.GetControl("Orders", T.id, CompanyId);
                if (hata == "true")
                {

                    await _salesOrder.DeleteStockControl(T, CompanyId, userid);
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
        [HttpDelete, Authorize]
        public async Task<ActionResult<SalesDeleteItems>> DeleteItems(SalesDeleteItems T)
        {
            ValidationResult result = await _SalesOrderDeleteItems.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _salescontrol.DeleteItems(T, CompanyId);
                if (hata == "true")
                {

                    await _salesOrder.DeleteItems(T, CompanyId);
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


        [Route("QuotesDone")]
        [HttpPost, Authorize]
        public async Task<ActionResult<Quotess>> QuotesDone(Quotess T)
        {
            ValidationResult result = await _SalesOrderQuotesDone.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata=await _salescontrol.QuotesDone(T, CompanyId);
                if (hata == "true")
                {

                    await _salesOrder.QuotesDone(T, CompanyId);
                    return Ok("Başarılı");
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


        [Route("DoneSell")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SalesDone>> DoneSell(SalesDone T)
        {
            ValidationResult result = await _SalesOrderDone.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int UserId = user[1];
                string hata = await _idcontrol.GetControl("Orders", T.id, CompanyId);
                if (hata == "true")
                {
                    await _salesOrderItem.DoneSellOrder(T, CompanyId,UserId);
                    return Ok("Basarili");
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
