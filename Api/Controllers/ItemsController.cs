﻿using BL.Services.IdControl;
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


        public ItemsController(IItemsRepository item, IUserService user, IDbConnection db, ILocationStockRepository loc, IItemsControl control, IValidator<ItemsInsert> validator, IValidator<ItemsUpdate> ıtemsUpdate, IValidator<ItemsDelete> ıtemsDelete, IValidator<ItemsListele> ıtemsListele, IItemsControl itemcontrol, IIDControl idcontrol)
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
        }
        [Route("List")]
        [HttpPost, Authorize]
        public async Task<ActionResult<ItemsListele>> List(ItemsListele T, int KAYITSAYISI, int SAYFA)
        {
            ValidationResult result = await _ItemsListele.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                if (T.Tip == "Product")
                {
                    var list = await _item.ListProduct(CompanyId, T, KAYITSAYISI, SAYFA);
                    var count = await _item.Count(T, CompanyId);
                    return Ok(new { list, count });
                }
                else if (T.Tip == "Material")
                {
                    var list = await _item.ListMaterial(CompanyId, T, KAYITSAYISI, SAYFA);
                    var count = await _item.Count(T, CompanyId);
                    return Ok(new { list, count });
                }
                else if (T.Tip == "SemiProduct")
                {
                    var list = await _item.ListSemiProduct(CompanyId, T, KAYITSAYISI, SAYFA);
                    var count = await _item.Count(T, CompanyId);
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
            int CompanyId = user[0];

            ValidationResult result = await _ItemsInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                string hata=await _itemcontrol.Insert(T, CompanyId);
                if (hata=="true")
                {
                    //item eklendiği zmana eklenen itemin ıd ve tipini alarak stok tablosuna ekliyor...
                    int ItemId = await _item.Insert(T, CompanyId);
                    //Girilen iteme ait company nin default locasyonunu buluyoruz.
                    var LocationIdBul = await _db.QueryAsync<LocationsDTO>($"Select id From Locations where CompanyId = {CompanyId}");
                    int LocationId = LocationIdBul.First().id;
                    int id = await _loc.Insert(T.Tip, ItemId, CompanyId, LocationId);
                    //eklenen itemi response olarak dönüyoruz
                    var eklenen = await _db.QueryAsync<ListItems>($"Select * From Items where CompanyId = {CompanyId} and id = {ItemId}");
                    return Ok(eklenen.First());
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
            ValidationResult result = await _ItemsUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
       
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
              string hata= await _idcontrol.GetControl("Items", T.id, CompanyId);
                if (hata=="true")
                {
                    ItemsInsert A = new ItemsInsert();
                    A.Tip = T.Tip;
                    A.ContactId = T.ContactId;
                    A.MeasureId = T.MeasureId;
                    A.CategoryId = T.CategoryId;
                   string hata2= await _itemcontrol.Insert(A, CompanyId);
                    if (hata2=="true")
                    {
                        await _item.Update(T, CompanyId);
                        return Ok("Güncelleme İşlemi Başarıyla Gerçekleşti");
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
            ValidationResult result = await _ItemsDelete.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                var hata = await _control.Delete(T, CompanyId);
                if (hata =="true")
                {
                    string sql = $"select id from Locations where CompanyId={CompanyId} and LocationName='Ana Konum'";
                    var LocationStockId = await _db.QueryFirstAsync<int>(sql);
                    await _loc.Delete(LocationStockId, CompanyId);
                    await _item.Delete(T, CompanyId);
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
            var itemDetail = await _item.Detail(id, CompanyId);
            if (itemDetail.Count() == 0)
            {
                return Ok("Böyle Bir İtem Yok");
            }
            return Ok(itemDetail.First());
        }
    }
}
