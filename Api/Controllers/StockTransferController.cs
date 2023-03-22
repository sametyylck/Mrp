using BL.Extensions;
using BL.Services.IdControl;
using BL.Services.LocationStock;
using BL.Services.StockTransfer;
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
using static DAL.DTO.ContactDTO;
using static DAL.DTO.ItemDTO;
using static DAL.DTO.StockTransferDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockTransferController : ControllerBase
    {
        private readonly IDbConnection _db;
        private readonly IUserService _user;
        private readonly IStockTransferRepository _transfer;
        private readonly IStockTransferControl _control;
        private readonly IValidator<StokAktarimDetay> _StockTransferUpdateItems;
        private readonly IValidator<StockUpdate> _StockTransferUpdate;
        private readonly IValidator<IdControl> _StockTransferDelete;
        private readonly IValidator<StockTransferDeleteItems> _StockTransferDeleteItem;
        private readonly IValidator<StockTransferInsertItem> _StockTransferInsertItem;
        private readonly IValidator<StockTransferInsert> _StockTransferInsert;
        private readonly IIDControl _idcontrol;
        private readonly ILocationStockControl _locstokkontrol;
        private readonly IPermissionControl _izinkontrol;



        public StockTransferController(IUserService user, IDbConnection db, IStockTransferRepository transfer, IStockTransferControl control, IValidator<StokAktarimDetay> stockTransferItems, IValidator<StockUpdate> stockTransferUpdate, IValidator<StockTransferDeleteItems> stockTransferDeleteItem, IValidator<StockTransferInsertItem> stockTransferInsertItem, IValidator<StockTransferInsert> stockTransferInsert, IValidator<IdControl> stockTransferDelete, IIDControl idcontrol, ILocationStockControl locstokkontrol, IPermissionControl izinkontrol)
        {

            _user = user;
            _db = db;
            _transfer = transfer;
            _control = control;
            _StockTransferUpdateItems = stockTransferItems;
            _StockTransferUpdate = stockTransferUpdate;

            _StockTransferDeleteItem = stockTransferDeleteItem;
            _StockTransferInsertItem = stockTransferInsertItem;
            _StockTransferInsert = stockTransferInsert;
            _StockTransferDelete = stockTransferDelete;
            _idcontrol = idcontrol;
            _locstokkontrol = locstokkontrol;
            _izinkontrol = izinkontrol;
        }
        [Route("List")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockTransferList>> List(StockTransferList T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferGoruntule, Permison.StokTransferHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _transfer.List(T, KAYITSAYISI, SAYFA);
            var count = list.Count();
            return Ok(new { list, count });

        }

        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockTransferAll>> Insert(StockTransferInsert T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferEkle, Permison.StokTransferHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockTransferInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _control.Insert(T);
                if (hata.Count()==0)
                {
                    string sql1 = $"Select Tip From Urunler where id = {T.StokId}";
                    var Tip = await _db.QueryFirstAsync<string>(sql1);
                    await _locstokkontrol.Kontrol(T.StokId, T.BaslangicDepo, Tip);
                    await _locstokkontrol.Kontrol(T.StokId, T.HedefDepo, Tip);

                    var kontrol = await _locstokkontrol.AdresStokKontrol(T.StokId, T.BaslangicDepo, T.HedefDepo,T.Miktar);

                    if (kontrol.Count()!=0)
                    {
                        return BadRequest(kontrol);
                    }
                    int id = await _transfer.Insert(T);
                    StockTransferInsertItem C = new StockTransferInsertItem();
                    C.StokId = T.StokId;
                    C.Miktar = T.Miktar;
                    C.StokAktarimId = id;
                    await _transfer.InsertStockTransferItem(C, id, UserId);

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@CompanyId", CompanyId);
                    param2.Add("@id", id);
                    //response dönüş
                    var list = await _db.QueryAsync<StockTransferAll>($"Select * From StokAktarim where id = @id ", param2);

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
        [Route("InsertStockTransferItems")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockTransferAll>> InsertStockTransferItems(StockTransferInsertItem T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferEkle, Permison.StokTransferHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockTransferInsertItem.ValidateAsync(T);
            if (result.IsValid)
            {
                
                var hata = await _control.InsertItem(T);
                if (hata.Count()==0)
                {
                    string sqlf1 = $@"select sa.BaslangicDepo,sa.HedefDepo,Urunler.Tip from StokAktarim  sa
                    left join Urunler on Urunler.id={T.StokId}
                     where sa.id={T.StokAktarimId} ";
                    var sorgu1 = await _db.QueryAsync<StockTransferDetailsResponse>(sqlf1);
                    var baslangicdepo = sorgu1.First().BaslangicDepo;
                    var hedefdepo = sorgu1.First().HedefDepo;
                    var Tip = sorgu1.First().Tip;
                    await _locstokkontrol.Kontrol(T.StokId, baslangicdepo, Tip);
                    await _locstokkontrol.Kontrol(T.StokId, hedefdepo, Tip);
                    int id = await _transfer.InsertStockTransferItem(T,T.StokAktarimId,UserId);
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@id", T.StokAktarimId);
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
        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<StockUpdate>> Update(StockUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferEkle, Permison.StokTransferHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockTransferUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _idcontrol.GetControl("StokAktarim", T.id);
                if (hata.Count() == 0)
                {

                    await _transfer.Update(T);

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@CompanyId", CompanyId);
                    param2.Add("@id", T.id);
                    var list = await _db.QueryAsync<StockTransferAll>($"Select * From StokAktarim where id = @id ", param2);

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


        [Route("UpdateStockTransferItem")]
        [HttpPut, Authorize]
        public async Task<ActionResult<StockTransferItems>> UpdateStockTransferItem(StokAktarimDetay T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferEkle, Permison.StokTransferHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockTransferUpdateItems.ValidateAsync(T);
            if (result.IsValid)     
            {
              
                var hata = await _control.UpdateItems(T.StokId,T.StokAktarimId,T.id);
                if (hata.Count() == 0)
                {
                    var kontrol = await _control.AdresStokKontrol(T.id,T.StokId, T.StokAktarimId, T.Miktar);
                    if (kontrol.Count()!=0)
                    {
                        return BadRequest(kontrol);
                    }
                    int id=await _transfer.UpdateStockTransferItem(T, UserId);
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@id", T.StokAktarimId);
                    //istenilen değerler response olarak dönülüyor
                    var list = await _transfer.Details(id);
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
        public async Task<ActionResult<StockTransferDeleteItems>> DeleteItems(StockTransferDeleteItems T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferSilebilir, Permison.StokTransferHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockTransferDeleteItem.ValidateAsync(T);
            if (result.IsValid)
            {
               
                var hata = await _control.UpdateItems(T.StokId,T.StokAktarimId,T.id);
                if (hata.Count() == 0)
                {
                    await _transfer.DeleteItems(T, UserId);
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
        public async Task<ActionResult<StockTransferDelete>> Delete(IdControl T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferSilebilir, Permison.StokTransferHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockTransferDelete.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _idcontrol.GetControl("StokAktarim", T.id);
                if (hata.Count() == 0)
                {
                    await _transfer.Delete(T, UserId);
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
        public async Task<ActionResult<StockTransferList>> Details(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferGoruntule, Permison.StokTransferHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _transfer.Details(id);
            return Ok(list);


        }

    }
}
