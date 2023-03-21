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
                    int id = await _adjusment.Insert(T);
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@CompanyId", CompanyId);
                    param2.Add("@id", id);
                    var list = await _db.QueryAsync<StockAdjusmentInsertResponse>(@$"Select StokDuzenleme.id as StokDuzenlemeDetayId, StokDuzenleme.Isim, StokDuzenleme.Sebeb, StokDuzenleme.Tarih, StokDuzenleme.DepoId,DepoVeAdresler.Isim, StokDuzenleme.Bilgi,StokDuzenleme.Toplam from StokDuzenleme                               left   join DepoVeAdresler on DepoVeAdresler.id = StokDuzenleme.DepoId
                    where StokDuzenleme.Aktif = 1 and StokDuzenleme.id = @id  ", param2);

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
                    int id = await _adjusment.InsertItem(T, T.StokDuzenlemeId,UserId);

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@id", id);
                    var list = await _db.QueryAsync<StockAdjusmentItems>($"Select StokDuzenlemeDetay.id,StokId,Urunler.Isim as UrunIsmi,Miktar,BirimFiyat,StokDuzenlemeId,Toplam From StokDuzenlemeDetay left join Urunler on Urunler.id = StokDuzenlemeDetay.StokId where StokDuzenlemeDetay.id = @id ", param2);

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
                    param1.Add("@DepoId", T.DepoId);
                    param1.Add("@id", T.id);

                    await _adjusment.Update(T);
                    var list = await _db.QueryAsync<StockAdjusmentInsertResponse>($"Select StokDuzenleme.id, StokDuzenleme.Isim, StokDuzenleme.Sebeb, StokDuzenleme.Tarih,StokDuzenleme.DepoId, DepoVeAdresler.Isim, StokDuzenleme.Bilgi from StokDuzenleme left  join DepoVeAdresler on DepoVeAdresler.id = StokDuzenleme.DepoId where  StokDuzenleme.Aktif = 1  AND StokDuzenleme.id = @id  ", param1);

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
        public async Task<ActionResult<StockAdjusmentUpdateItems>> UpdateStockAdjusmentItem(StockAdjusmentUpdateItems T)
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
                    await _adjusment.UpdateStockAdjusmentItem(T, UserId);
                    var list = await _db.QueryAsync<StockAdjusmentUpdateItems>($"Select StokDuzenlemeDetay.id,StokId,Urunler.Isim as UrunIsmi,Miktar,BirimFiyat,StokDuzenlemeId,Toplam From StokDuzenlemeDetay left join Urunler on Urunler.id = StokDuzenlemeDetay.StokId where  StokDuzenlemeDetay.id = @id ", param2);

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
                    await _adjusment.DeleteItems(T,UserId);
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
               
                var hata = await _control.GetControl("StokDuzenleme", T.id);
                if (hata.Count() == 0)
                {
                    await _adjusment.Delete(T, UserId);
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
            var list = await _adjusment.Detail(id);
            return Ok(list);

        }

        [Route("List")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockAdjusmentList>> List(StockAdjusmentList T, int KAYITSAYISI, int SAYFA)
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
            var list = await _adjusment.List(T, KAYITSAYISI, SAYFA);
            var count = list.Count();
            return Ok(new { list, count });

        }
    }
}
