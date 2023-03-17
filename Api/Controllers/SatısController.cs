using BL.Extensions;
using BL.Services.SalesOrder;
using DAL.Contracts;
using DAL.DTO;
using Dapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockListDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SatısController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly ISatısRepository _satıs;
        private readonly IDbConnection _db;
        private readonly ISalesOrderControl _salescontrol;
        private readonly ISatısListRepository _list;
        private readonly IValidator<SatısDTO> _SalesOrderInsert;
        private readonly IValidator<SatısInsertItem> _SalesOrderInsertItem;
        private readonly IValidator<SalesOrderUpdate> _SalesOrderUpdate;
        private readonly IValidator<SatısDeleteItems> _SalesOrderDeleteItems;

        private readonly IValidator<SalesDone> _SalesOrderDone;
        private readonly IValidator<SalesOrderMake> _SalesOrderMake;
        private readonly IValidator<SatısUpdateItems> _SalesOrderUpdateItems;
        private readonly IPermissionControl _izinkontrol;


        public SatısController(IUserService user, ISatısRepository satıs, IDbConnection db, ISalesOrderControl salescontrol, ISatısListRepository list, IValidator<SatısDTO> salesOrderInsert, IValidator<SatısInsertItem> salesOrderInsertItem = null, IValidator<SalesOrderUpdate> salesOrderUpdate = null, IValidator<SalesOrderMake> salesOrderMake = null, IValidator<SatısUpdateItems> salesOrderUpdateItems = null, IPermissionControl izinkontrol = null, IValidator<SatısDeleteItems> salesOrderDeleteItems = null, IValidator<SalesDone> salesOrderDone = null)
        {
            _user = user;
            _satıs = satıs;
            _db = db;
            _salescontrol = salescontrol;
            _list = list;
            _SalesOrderInsert = salesOrderInsert;
            _SalesOrderInsertItem = salesOrderInsertItem;
            _SalesOrderUpdate = salesOrderUpdate;
            _SalesOrderMake = salesOrderMake;
            _SalesOrderUpdateItems = salesOrderUpdateItems;
            _izinkontrol = izinkontrol;
            _SalesOrderDeleteItems = salesOrderDeleteItems;
            _SalesOrderDone = salesOrderDone;
        }

        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SatısDTO>> Insert(SatısDTO T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisEkleyebilirVeDuzenleyebilir, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _SalesOrderInsert.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _salescontrol.Insert(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                int id = await _satıs.Insert(T, CompanyId);
                await _salescontrol.Adress(id, T.ContactId);
                var list = await _db.QueryAsync<SalesOrderUpdate>($"Select * from SalesOrder where id={id} ");
                return Ok(list);
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

        }

        [Route("InsertItem")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SatısDTO>> InsertItem(SatısInsertItem T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisEkleyebilirVeDuzenleyebilir, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _SalesOrderInsertItem.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _salescontrol.InsertItem(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                int id = await _satıs.InsertPurchaseItem(T, CompanyId);
                var list = await _db.QueryAsync<SatısInsertItem>($"Select * from SalesOrderItem where id={id} ");
                return Ok(list);
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }


        }

        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<SatısDTO>> Update(SalesOrderUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisEkleyebilirVeDuzenleyebilir, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _SalesOrderUpdate.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _salescontrol.Update(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                await _satıs.Update(T, CompanyId);
                var list = await _db.QueryAsync<SalesOrderUpdate>($"Select * from SalesOrder where id={T.id}");
                return Ok(list);
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }



        }
        [Route("UpdateItem")]
        [HttpPut, Authorize]
        public async Task<ActionResult<SatısDTO>> UpdateItem(SatısUpdateItems T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisEkleyebilirVeDuzenleyebilir, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _SalesOrderUpdateItems.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _salescontrol.UpdateItem(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                await _satıs.UpdateItems(T, CompanyId);
                var list = await _db.QueryAsync<SatısInsertItem>($"Select * from SalesOrderItem where id={T.id}");
                return Ok(list);
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

        }
        [Route("UpdateAddress")]
        [HttpPut, Authorize]
        public async Task<ActionResult<SatısDTO>> UpdateAddress(SalesOrderCloneAddress T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisEkleyebilirVeDuzenleyebilir, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            await _satıs.UpdateAddress(T, T.id, CompanyId);
            return Ok();

        }
        [Route("Delete")]
        [HttpDelete, Authorize]
        public async Task<ActionResult<SatısDTO>> Delete(List<SatısDelete> T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisSilebilir, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            int User = user[1];
            await _satıs.DeleteStockControl(T, CompanyId, User);
            return Ok();


        }
        [Route("DeleteItems")]
        [HttpDelete, Authorize]
        public async Task<ActionResult<SatısDTO>> DeleteItems(SatısDeleteItems T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisSilebilir, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _SalesOrderDeleteItems.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _salescontrol.DeleteItems(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                await _satıs.DeleteItems(T, CompanyId);
                return Ok();
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

        }
        [Route("Make")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SatısDTO>> Make(SalesOrderMake T)
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
            ValidationResult result = await _SalesOrderMake.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _salescontrol.Make(T);
                if (hata.Count() != 0)
                {
                    return BadRequest(hata);
                }
                await _satıs.Make(T, CompanyId);
                return Ok();
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

        }

        [Route("SatısTamamlama")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SatısDTO>> SatısTamamlama(SalesDone T, int DeliveryId)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisTamamlama, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _SalesOrderDone.ValidateAsync(T);
            if (result.IsValid)
            {
                await _satıs.DoneSellOrder(T, CompanyId, UserId);
                return Ok("Başarılı");
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }


        }

        [Route("Details")]
        [HttpGet, Authorize]
        public async Task<ActionResult<SatısDTO>> Details(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisGoruntule, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _list.Detail(CompanyId, id);
            return Ok(list);
        }
        [Route("SalesOrderList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SatısDTO>> SalesOrderList(SatısListFiltre T, int? KATSAYI, int? SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisGoruntule, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }

            var list = await _list.SalesOrderList(T, CompanyId, KATSAYI, SAYFA);
            return Ok(list);
        }
        [Route("SalesOrderDoneList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SatısDTO>> SalesOrderDoneList(SatısListFiltre T, int? KATSAYI, int? SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisGoruntule, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _list.SalesOrderDoneList(T, CompanyId, KATSAYI, SAYFA);
            return Ok(list);
        }
        [Route("IngredientsMissingList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<SatısDTO>> IngredientsMissingList(IngredientMis T)
        {

            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisGoruntule, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }

            var list = await _list.IngredientsMissingList(T, CompanyId);
            return Ok(list);
        }
        [Route("SalesManufacturingList")]
        [HttpGet, Authorize]
        public async Task<ActionResult<SatısDTO>> SalesManufacturingList(int SalesOrderId, int SalesOrderItemId)
        {

            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatisGoruntule, Permison.SatisHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _list.SalesManufacturingList(SalesOrderId, SalesOrderItemId, CompanyId);
            return Ok(list);
        }




    }
}

