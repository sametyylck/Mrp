using BL.Extensions;
using BL.Services.IdControl;
using BL.Services.Items;
using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using DAL.Repositories;
using Dapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using System;
using System.ComponentModel.Design;
using System.Data;
using System.Security.Claims;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.ItemDTO;
using static DAL.DTO.StockDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {

        private readonly IDbConnection _db;
        private readonly IUserService _user;
        private readonly IItemsRepository _item;
        private readonly ILocationStockRepository _loc;
        private readonly IItemsControl _control;
        private readonly IValidator<ItemsInsert> _ItemsInsert;
        private readonly IValidator<ItemsUpdate> _ItemsUpdate;
        private readonly IValidator<ItemsDelete> _ItemsDelete;
        private readonly IValidator<ItemsListele> _ItemsListele;
        private readonly IItemsControl _itemcontrol;
        private readonly IIDControl _idcontrol;
        private readonly IPermissionControl _izinkontrol;


        public ItemsController(IItemsRepository item, IUserService user, IDbConnection db, ILocationStockRepository loc, IItemsControl control, IValidator<ItemsInsert> validator, IValidator<ItemsUpdate> ıtemsUpdate, IValidator<ItemsDelete> ıtemsDelete, IValidator<ItemsListele> ıtemsListele, IItemsControl itemcontrol, IIDControl idcontrol, IPermissionControl izinkontrol)
        {
            _item = item;
            _user = user;
            _db = db;
            _loc = loc;
            _control = control;
            _ItemsInsert = validator;
            _ItemsUpdate = ıtemsUpdate;
            _ItemsDelete = ıtemsDelete;
            _ItemsListele = ıtemsListele;
            _itemcontrol = itemcontrol;
            _idcontrol = idcontrol;
            _izinkontrol = izinkontrol;
        }
        [Route("List")]
        [HttpPost, Authorize]
        public async Task<ActionResult<ItemsListele>> List(ItemsListele T, int KAYITSAYISI, int SAYFA)
        {

            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.ItemGoruntule, Permison.ItemlerHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ItemsListele.ValidateAsync(T);
            if (result.IsValid)
            {
                
                if (T.Tip == "Product")
                {
                    var list = await _item.ListProduct(T, KAYITSAYISI, SAYFA);
                    var count = list.Count();
                    return Ok(new { list, count });
                }
                else if (T.Tip == "Material")
                {
                    var list = await _item.ListMaterial(T, KAYITSAYISI, SAYFA);
                    var count = list.Count();
                    return Ok(new { list, count });
                }
                else if (T.Tip == "SemiProduct")
                {
                    var list = await _item.ListSemiProduct(T, KAYITSAYISI, SAYFA);
                    var count = list.Count();
                    return Ok(new { list, count });
                }
                else
                {
                    return BadRequest("Geçersiz Tip");
                }
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }


        }
        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<Items>> Insert(ItemsInsert T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.ItemEkleyebilirVeGuncelleyebilir, Permison.ItemlerHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ItemsInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata=await _itemcontrol.Insert(T);
                if (hata.Count()==0)
                {
                    //item eklendiği zmana eklenen itemin ıd ve tipini alarak stok tablosuna ekliyor...
                    int ItemId = await _item.Insert(T, UserId);
                    //Girilen iteme ait company nin default locasyonunu buluyoruz.
                    var LocationIdBul = await _db.QueryAsync<LocationsDTO>($"Select id From DepoVeAdresler");
                    int LocationId = LocationIdBul.First().id;
                    int id = await _loc.Insert(T.Tip, ItemId, LocationId);
                    //eklenen itemi response olarak dönüyoruz
                    var eklenen = await _item.Detail(ItemId);
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
        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<Items>> Update(ItemsUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.ItemEkleyebilirVeGuncelleyebilir, Permison.ItemlerHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ItemsUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata= await _idcontrol.GetControl("Urunler", T.id);
                if (hata.Count()==0)
                {
                    ItemsInsert A = new ItemsInsert();
                    A.Tip = T.Tip;
                    A.TedarikciId = T.TedarikciId;
                    A.OlcuId = T.OlcuId;
                    A.KategoriId = T.KategoriId;
                   var hata2= await _itemcontrol.Insert(A);
                    if (hata2.Count() == 0)
                    {
                        await _item.Update(T);
                        var list = await _item.Detail(T.id);
                        return Ok(list);
                    }
                    else
                    {
                        return BadRequest(hata2);
                    }
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
        public async Task<ActionResult<ItemsDelete>> Delete(ItemsDelete T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.ItemSilebilir, Permison.ItemlerHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ItemsDelete.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _control.Delete(T);
                if (hata.Count() == 0)
                {
                    string sql = $"select id from DepoVeAdresler where Isim='Ana Konum'";
                    var LocationStockId = await _db.QueryFirstAsync<int>(sql);
                    await _loc.Delete(T.id,LocationStockId);
                    await _item.Delete(T);
                    return Ok("Silme İşlemi Başarıyla Gerçekleşti");
                }
                else
                {
                    return BadRequest("Başarısız");
                }
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

      

        }

        [Route("Detail")]
        [HttpGet, Authorize]
        public async Task<ActionResult<ItemsDelete>> Detail(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.ItemGoruntule, Permison.ItemlerHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var itemDetail = await _item.Detail(id);
            if (itemDetail.Count() == 0)
            {
                return Ok("Böyle Bir İtem Yok");
            }
            return Ok(itemDetail.First());
        }
    }
}
