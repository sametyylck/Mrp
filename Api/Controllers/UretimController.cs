using BL.Extensions;
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
using System.ComponentModel.Design;
using System.Data;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.PurchaseOrderDTO;
using PurchaseBuy = DAL.DTO.PurchaseBuy;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UretimController : ControllerBase
    {
        private readonly IUretimRepository _uretim;
        private readonly IUserService _user;
        private readonly IDbConnection _db;
        private readonly IOrdersRepository _order;
        private readonly IManufacturingOrderControl _control;
        private readonly IValidator<UretimDTO> _Manufacturinginsert;
        private readonly IValidator<UretimUpdate> _Manufacturinguptade;
        private readonly IValidator<UretimTamamlama> _ManufacturingDoneStock;
        private readonly IValidator<ManufacturingTaskDone> _Manufacturingtaskdone;
        private readonly IValidator<UretimDeleteItems> _ManufacturingItemsDelete;
        private readonly IValidator<UretimIngredientsUpdate> _ManuItemsIngUpdate;
        private readonly IValidator<UretimIngredientsInsert> _ManuItemIngInsert;
        private readonly IValidator<UretimOperationsInsert> _ManuItemOperInsert;
        private readonly IValidator<UretimOperationsUpdate> _ManuItemOperUpdate;
        private readonly IValidator<PurchaseBuy> _ManuItemPurchaseOrder;
        private readonly IPermissionControl _izinkontrol;
        private readonly IUretimList _uretimlist;


        public UretimController(IUretimRepository uretim, IUserService user, IDbConnection db, IOrdersRepository order, IManufacturingOrderControl control, IValidator<UretimDTO> manufacturinginsert, IValidator<UretimUpdate> manufacturinguptade, IValidator<UretimTamamlama> manufacturingDoneStock, IValidator<UretimDeleteItems> manufacturingItemsDelete, IPermissionControl izinkontrol, IValidator<UretimIngredientsUpdate> manuItemsIngUpdate, IValidator<ManufacturingTaskDone> manufacturingtaskdone, IValidator<UretimIngredientsInsert> manuItemIngInsert, IValidator<UretimOperationsInsert> manuItemOperInsert, IValidator<UretimOperationsUpdate> manuItemOperUpdate, IValidator<PurchaseBuy> manuItemPurchaseOrder, IUretimList uretimlist)
        {
            _uretim = uretim;
            _user = user;
            _db = db;
            _order = order;
            _control = control;
            _Manufacturinginsert = manufacturinginsert;
            _Manufacturinguptade = manufacturinguptade;
            _ManufacturingDoneStock = manufacturingDoneStock;
            _ManufacturingItemsDelete = manufacturingItemsDelete;
            _izinkontrol = izinkontrol;
            _ManuItemsIngUpdate = manuItemsIngUpdate;
            _Manufacturingtaskdone = manufacturingtaskdone;
            _ManuItemIngInsert = manuItemIngInsert;
            _ManuItemOperInsert = manuItemOperInsert;
            _ManuItemOperUpdate = manuItemOperUpdate;
            _ManuItemPurchaseOrder = manuItemPurchaseOrder;
            _uretimlist = uretimlist;
        }

        [Route("Insert")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] UretimDTO T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimEkleyebilirVeDuzenleyebilir, Permison.UretimHepsi, UserId);
            if (!izin)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _Manufacturinginsert.ValidateAsync(T);
            if (result.IsValid)
            {
               
           
                var hata = await _control.Insert(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                int id = await _uretim.Insert(T, CompanyId);
                await _uretim.InsertOrderItems(id, T.StokId, T.DepoId, T.PlananlananMiktar, 0, 0);
                var list = await _uretimlist.Detail(CompanyId, id);
                return Ok(list);
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
        }

        [Route("IngredientsInsert")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> IngredientsInsert([FromBody] UretimIngredientsInsert T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimEkleyebilirVeDuzenleyebilir, Permison.UretimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ManuItemIngInsert.ValidateAsync(T);
            if (result.IsValid)
            {
            
                var hata = await _control.IngredientInsert(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                await _uretim.IngredientsInsert(T);
                return Ok();
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

        }
        [Route("OperationsInsert")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> OperationsInsert([FromBody] UretimOperationsInsert T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimEkleyebilirVeDuzenleyebilir, Permison.UretimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ManuItemOperInsert.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _control.OperationsInsert(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                await _uretim.OperationsInsert(T);
            return Ok();
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

        }

        [Route("Update")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UretimUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimEkleyebilirVeDuzenleyebilir, Permison.UretimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _Manufacturinguptade.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _control.Update(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                await _uretim.Update(T);
                await _uretim.UpdateOrderItems(T.id, T.DepoId, T.PlanlananMiktar, T.eskiPlanned);
                return Ok();
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
        }

        [Route("IngredientsUpdate")]
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> IngredientsUpdate([FromBody] UretimIngredientsUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimEkleyebilirVeDuzenleyebilir, Permison.UretimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ManuItemsIngUpdate.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _control.IngredientsUpdate(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                await _uretim.IngredientsUpdate(T);
                return Ok();
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
        }
        [Route("OperationsUpdate")]
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> OperationsUpdate([FromBody] UretimOperationsUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimEkleyebilirVeDuzenleyebilir, Permison.UretimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ManuItemOperUpdate.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _control.OperationUpdate(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                await _uretim.OperationsUpdate(T);
                return Ok();
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
        }
        [Route("Delete")]
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(List<UretimDeleteKontrol> T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimSilebilir, Permison.UretimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var hata = await _control.DeleteKontrol(T);
            if (hata.Count() != 0)
            {
                return BadRequest(hata);
            }
            await _uretim.Delete(T,UserId);
            return Ok();

        }

        [Route("DeleteItems")]
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteItems([FromBody] UretimDeleteItems T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimSilebilir, Permison.UretimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ManufacturingItemsDelete.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _control.DeleteItems(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                await _uretim.DeleteItems(T);
                return Ok();
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
        }


        [Route("UrunTamamla")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UrunTamamla(UretimTamamlama T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimTamamlama, Permison.UretimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ManufacturingDoneStock.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _control.DoneStock(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                int User = user[1];
                await _uretim.DoneStock(T, User);
                return Ok();
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
        }

        [Route("PurchaseBuy")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PurchaseBuy(PurchaseBuy T)
        {
            try
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int UserId = user[1];
                var izin = await _izinkontrol.Kontrol(Permison.SatinAlmaEkleyebilirVeDuzenleyebilir, Permison.SatinAlmaHepsi, UserId);
                if (izin == false)
                {
                    List<string> izinhatasi = new();
                    izinhatasi.Add("Yetkiniz yetersiz");
                    return BadRequest(izinhatasi);
                }

                ValidationResult result = await _ManuItemPurchaseOrder.ValidateAsync(T);
                if (result.IsValid)
                {
                 
                    var hata = await _control.PurchaseOrder(T);
                    if (hata.Count() != 0)
                    {
                        return BadRequest(hata);
                    }
                    DynamicParameters prm = new();
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@OrderId", T.UretimId);
                    prm.Add("@id", T.UretimDetayId);
                    prm.Add("@LocationId", T.DepoId);
                    prm.Add("@ItemId", T.StokId);



                    string sqlsorgu = $@"
				 			 select  
              (select
             -ISNULL(moi.PlannedQuantity,0)+ISNULL((rez.RezerveCount),0)as Missing
            
            from ManufacturingOrder mo
			    left join ManufacturingOrderItems moi on mo.id=moi.OrderId 
            left join Items on Items.id=moi.ItemId 
		   LEFT join Rezerve rez on rez.ManufacturingOrderId=mo.id and rez.ManufacturingOrderItemId=moi.id and rez.Status=1 AND rez.LocationId=@LocationId
            where  moi.CompanyId = @CompanyId and moi.id = @id and moi.Tip='Ingredients'  and mo.id=@OrderId and  mo.Status!=3
            Group by moi.id,moi.Tip,moi.ItemId,Items.Name,Notes,moi.Cost,moi.Availability
            ,moi.PlannedQuantity,rez.RezerveCount)+
			(select ISNULL(SUM(OrdersItem.Quantity),0) from OrdersItem 
			left join Orders on Orders.id=OrdersItem.OrdersId
			where ManufacturingOrderId=@OrderId and ManufacturingOrderItemId=@id and Orders.DeliveryId=1 and Orders.IsActive=1 and Orders.LocationId=@LocationId)as missing";
                    int? missingdeger = await _db.QueryFirstAsync<int>(sqlsorgu, prm);

                    string sqlquery = $@"select 
            (select g.DefaultTaxPurchaseOrderId from GeneralDefaultSettings g where CompanyId=@CompanyId)as TaxId,
            (select MeasureId from Items where id=@ItemId and CompanyId=@CompanyId) as MeasureId		 			 ";
                    var degerler = await _db.QueryAsync<BuyKontrol>(sqlquery, prm);


                    PurchaseOrderInsert insert = new();
                    insert.UretimId = T.UretimId;
                    insert.UretimDetayId = T.UretimDetayId;
                    insert.DepoId = T.DepoId;
                    insert.TedarikciId = T.TedarikciId;
                    insert.BeklenenTarih = T.BeklenenTarih;
                    insert.Tip = T.Tip;
                    insert.StokId = T.StokId;
                    insert.SatisDetayId = 0;
                    insert.SatisId = 0;

                    insert.Bilgi = "";

                    int id = await _order.Insert(insert,UserId);
                    PurchaseOrderInsertItem insertitem = new();
                    insertitem.OlcuId = degerler.First().MeasureId;
                    insertitem.StokId = T.StokId;
                    insertitem.Miktar = T.Miktar;
                    insertitem.SatinAlmaId = id;
                    insertitem.VergiId = degerler.First().TaxId;

                    int inserid = await _order.InsertPurchaseItem(insertitem, id);
                    await _uretim.BuyStockControl(insert, missingdeger);


                    return Ok("true");
                }
                else
                {
                    result.AddToModelState(this.ModelState);
                    return BadRequest(result.ToString());
                }
            }
            catch (Exception ex)
            {

                return Ok(ex.Message);
            }
        }

        [Route("Uret")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Uret(UretimSemiProduct T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];

            UretimDTO insert = new();
            insert.PlananlananMiktar = T.PlannedQuantity;
            insert.Ozel = true;
            insert.OlusturmTarihi=DateTime.Now;
            insert.StokId = T.ItemId;
            insert.ParentId = T.ParentId;
            insert.Isim = T.Name;
            insert.BeklenenTarih = T.ExpectedDate;
            insert.DepoId = T.LocationId;
            int id = await _uretim.Insert(insert,CompanyId);
            await _uretim.InsertOrderItems(id, insert.StokId, insert.DepoId, insert.PlananlananMiktar, 0, 0);
            var list = await _uretimlist.Detail(CompanyId, id);
            return Ok(list);
        }

    }
}
