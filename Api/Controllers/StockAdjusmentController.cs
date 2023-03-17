using BL.Extensions;
using BL.Services.IdControl;
using BL.Services.StockAdjusment;
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
using System.Security.Claims;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;
using static Dapper.SqlMapper;
using Delete = DAL.DTO.StockAdjusmentDTO.DeleteClas;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockAdjusmentController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IDbConnection _db;
        private readonly IStockAdjusmentRepository _adjusment;
        private readonly IValidator<StockAdjusmentItemDelete> _StockAdjusmentItemDelete;
        private readonly IValidator<StockAdjusmentInsertItem> _StockAdjusmentInsertItem;
        private readonly IValidator<StockAdjusmentInsert> _StockAdjusmentInsert;
        private readonly IValidator<StockAdjusmentUpdate> _StockAdjusmentUpdate;
        private readonly IValidator<StockAdjusmentUpdateItems> _StockAdjusmentUpdateItem;
        private readonly IValidator<IdControl> _delete;
        private readonly IStockAdjusmentControl _stockadjusmentcontrol;
        private readonly IIDControl _control;
        private readonly IPermissionControl _izinkontrol;

        public StockAdjusmentController(IUserService user, IDbConnection db, IStockAdjusmentRepository adjusment, IValidator<StockAdjusmentItemDelete> stockAdjusmentItemDelete, IValidator<StockAdjusmentInsertItem> stockAdjusmentInsertItem, IValidator<StockAdjusmentInsert> stockAdjusmentInsert, IValidator<StockAdjusmentUpdate> stockAdjusmentUpdate, IValidator<StockAdjusmentUpdateItems> stockAdjusmentUpdateItem, IValidator<IdControl> delete, IStockAdjusmentControl stockadjusmentcontrol, IIDControl control, IPermissionControl izinkontrol)
        {
            _user = user;
            _db = db;
            _adjusment = adjusment;
            _StockAdjusmentItemDelete = stockAdjusmentItemDelete;
            _StockAdjusmentInsertItem = stockAdjusmentInsertItem;
            _StockAdjusmentInsert = stockAdjusmentInsert;
            _StockAdjusmentUpdate = stockAdjusmentUpdate;
            _StockAdjusmentUpdateItem = stockAdjusmentUpdateItem;
            _delete = delete;
            _stockadjusmentcontrol = stockadjusmentcontrol;
            _control = control;
            _izinkontrol = izinkontrol;
        }

        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockAdjusmentAll>> Insert(StockAdjusmentInsert T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokDuzenlemeEkleyebilirVeGuncelleyebilir, Permison.StokDuzenlemeHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockAdjusmentInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _stockadjusmentcontrol.Insert(T);
                if (hata.Count()==0)
                {
                    int id = await _adjusment.Insert(T, CompanyId);
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@CompanyId", CompanyId);
                    param2.Add("@id", id);
                    var list = await _db.QueryAsync<StockAdjusmentInsertResponse>(@$"Select StockAdjusment.id as StockAdjusmentId, StockAdjusment.Name, StockAdjusment.Reason, StockAdjusment.Date, StockAdjusment.LocationId,Locations.LocationName, StockAdjusment.Info,StockAdjusment.Total from StockAdjusment                               left   join Locations on Locations.id = StockAdjusment.LocationId
                    where StockAdjusment.CompanyId = @CompanyId and StockAdjusment.IsActive = 1 and StockAdjusment.id = @id  ", param2);

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

        [Route("InsertStockAdjusmentItems")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockAdjusmentAll>> InsertStockAdjusmentItems(StockAdjusmentInsertItem T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokDuzenlemeEkleyebilirVeGuncelleyebilir, Permison.StokDuzenlemeHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }

            ValidationResult result = await _StockAdjusmentInsertItem.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _stockadjusmentcontrol.InsertItem(T);
                if (hata.Count() == 0)
                {
                    int id = await _adjusment.InsertItem(T, T.StockAdjusmentId, CompanyId, UserId);

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@CompanyId", CompanyId);
                    param2.Add("@id", id);
                    var list = await _db.QueryAsync<StockAdjusmentItems>($"Select StockAdjusmentItems.id,ItemId,Items.Name as ItemName,Adjusment,CostPerUnit,StockAdjusmentId,AdjusmentValue From StockAdjusmentItems left join Items on Items.id = StockAdjusmentItems.ItemId where StockAdjusmentItems.CompanyId = @CompanyId and StockAdjusmentItems.id = @id ", param2);

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
        public async Task<ActionResult<StockAdjusmentClas>> Update(StockAdjusmentUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokDuzenlemeEkleyebilirVeGuncelleyebilir, Permison.StokDuzenlemeHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockAdjusmentUpdate.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _stockadjusmentcontrol.Update(T);
                if (hata.Count() == 0)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@CompanyId", CompanyId);
                    param1.Add("@LocationId", T.LocationId);
                    param1.Add("@id", T.id);

                    await _adjusment.Update(T, CompanyId);
                    var list = await _db.QueryAsync<StockAdjusmentInsertResponse>($"Select StockAdjusment.id as StockAdjusmentId, StockAdjusment.Name, StockAdjusment.Reason, StockAdjusment.Date,StockAdjusment.LocationId, Locations.LocationName, StockAdjusment.Info from StockAdjusment left  join Locations on Locations.id = StockAdjusment.LocationId where StockAdjusment.CompanyId = @CompanyId and StockAdjusment.IsActive = 1  AND StockAdjusment.id = @id  ", param1);

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

        [Route("UpdateStockAdjusmentItem")]
        [HttpPut, Authorize]
        public async Task<ActionResult<StockAdjusmentItems>> UpdateStockAdjusmentItem(StockAdjusmentUpdateItems T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokDuzenlemeEkleyebilirVeGuncelleyebilir, Permison.StokDuzenlemeHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockAdjusmentUpdateItem.ValidateAsync(T);
            if (result.IsValid)
            {
           
                var hata = await _stockadjusmentcontrol.UpdateStockAdjusment(T);
                if (hata.Count() == 0)
                {
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@id", T.id);
                    param2.Add("@CompanyId", CompanyId);
                    await _adjusment.UpdateStockAdjusmentItem(T, CompanyId, UserId);
                    var list = await _db.QueryAsync<StockAdjusmentItems>($"Select StockAdjusmentItems.id,ItemId,Items.Name as ItemName,Adjusment,CostPerUnit,StockAdjusmentId,AdjusmentValue From StockAdjusmentItems left join Items on Items.id = StockAdjusmentItems.ItemId where StockAdjusmentItems.CompanyId = @CompanyId and StockAdjusmentItems.id = @id ", param2);

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

        [Route("DeleteItems")]
        [HttpDelete, Authorize]
        public async Task<ActionResult<StockAdjusmentItemDelete>> DeleteItems(StockAdjusmentItemDelete T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokDuzenlemeSilme, Permison.StokDuzenlemeHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockAdjusmentItemDelete.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _stockadjusmentcontrol.DeleteItems(T);
                if (hata.Count() == 0)
                {
                    await _adjusment.DeleteItems(T, CompanyId, UserId);
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

        [Route("Delete")]
        [HttpDelete, Authorize]
        public async Task<ActionResult<Delete>> Delete(IdControl T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokDuzenlemeSilme, Permison.StokDuzenlemeHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _delete.ValidateAsync(T);
            if (result.IsValid)
            {
               
                var hata = await _control.GetControl("StockAdjusment", T.id);
                if (hata.Count() == 0)
                {
                    await _adjusment.Delete(T, CompanyId, UserId);
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

        [Route("Details/{id}")]
        [HttpGet, Authorize]
        public async Task<ActionResult<StockAdjusmentClas>> Details(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokDuzenlemeGoruntule, Permison.StokDuzenlemeHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            DynamicParameters prm = new DynamicParameters();
            var list = await _adjusment.Detail(CompanyId, id);
            return Ok(list);

        }

        [Route("List")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockAdjusmentItems>> List(StockAdjusmentList T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokDuzenlemeGoruntule, Permison.StokDuzenlemeHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            DynamicParameters prm = new DynamicParameters();
            var list = await _adjusment.List(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = await _adjusment.Count(T, CompanyId);
            return Ok(new { list, count });

        }
    }
}
