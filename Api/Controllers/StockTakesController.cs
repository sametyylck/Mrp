using BL.Services.IdControl;
using BL.Services.StockTakes;
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
using Microsoft.Data.SqlClient.Server;
using Microsoft.Extensions.Configuration.UserSecrets;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.Design;
using System.Data;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ItemDTO;
using static DAL.DTO.StockTakesDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockTakesController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IStockTakesRepository _takes;
        private readonly IDbConnection _db;
        private readonly IValidator<StockTakesInsert> _StockTakesInsert;
        private readonly IValidator<StockTakeDelete> _StockTakeDeleteItem;
        private readonly IValidator<StockTakeInsertItems> _StockTakeInsertItems;
        private readonly IValidator<StockTakesUpdate> _StockTakesUpdate;
        private readonly IValidator<StockTakesDone> _StockTakesDone;
        private readonly IValidator<IdControl> _Delete;
        private readonly IValidator<StockTakesUpdateItems> _updateitem;
        private readonly IStockTakesControl _stocktakes;
        private readonly IIDControl _idcontrol;


        public StockTakesController(IStockTakesRepository takes, IUserService user, IDbConnection db, IValidator<StockTakesInsert> stockTakesInsert, IValidator<StockTakeDelete> stockTakeDelete, IValidator<StockTakeInsertItems> stockTakeInsertItems, IValidator<StockTakesUpdate> stockTakesUpdate, IValidator<StockTakesDone> stockTakesDone, IValidator<IdControl> delete, IValidator<StockTakesUpdateItems> updateitem, IStockTakesControl stocktakes, IIDControl idcontrol)
        {
            _takes = takes;
            _user = user;
            _db = db;
            _StockTakesInsert = stockTakesInsert;
            _StockTakeDeleteItem = stockTakeDelete;
            _StockTakeInsertItems = stockTakeInsertItems;
            _StockTakesUpdate = stockTakesUpdate;
            _StockTakesDone = stockTakesDone;
            _Delete = delete;
            _updateitem = updateitem;
            _stocktakes = stocktakes;
            _idcontrol = idcontrol;
        }
        [Route("ItemDetail/{id}")]
        [HttpGet, Authorize]
        public async Task<ActionResult<StockTakeItems>> ItemDetail(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            DynamicParameters prm = new DynamicParameters();
            var list = await _takes.ItemDetail(CompanyId, id);
            return Ok(list);

        }
        [Route("Detail/{id}")]
        [HttpGet, Authorize]
        public async Task<ActionResult<StockTakes>> Detail(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            DynamicParameters prm = new DynamicParameters();
            var list = await _takes.Detail(CompanyId, id);
            return Ok(list);

        }
        [Route("List")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockTakeList>> List(StockTakeList T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            DynamicParameters prm = new DynamicParameters();
            var list = await _takes.StockTakesList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = list.Count();
            return Ok(new { list, count });

        }
        [Route("StockTakesDone")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockTakesDone>> StockTakesDone(StockTakesDone T)
        {
            ValidationResult result = await _StockTakesDone.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int UserId = user[1];
                var hata = await _stocktakes.StockTakesDone(T);
                if (hata.Count()==0)
                {
                    await _takes.StockTakesDone(T, CompanyId, UserId);
                    return Ok("başarılı.");
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

        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockTakeList>> Insert(StockTakesInsert T)
        {
            ValidationResult result = await _StockTakesInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int UserId = user[1];
                var hata = await _stocktakes.Insert(T);
                if (hata.Count() == 0)
                {
                    DynamicParameters param = new DynamicParameters();
                    int id = await _takes.Insert(T, UserId);
                    param.Add("@id", id);

                    string sql = $"Select * from StokSayim where id=@id";
                    var response = await _db.QueryAsync<Items>(sql, param);
                    return Ok(response);
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
        public async Task<ActionResult<StockTakeItems>> InsertItems(List<StockTakeInsertItems> T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            foreach (var item in T)
            {
                ValidationResult result = await _StockTakeInsertItems.ValidateAsync(item);
                if (result.IsValid)
                {

                    var hata = await _stocktakes.InsertItem(T);
                    if (hata.Count() == 0)
                    {
                        await _takes.InsertItem(T, CompanyId);
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
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.First().StokSayimId);
            param.Add("@CompanyId", CompanyId);
            string sql = @$"select sti.id,StokId,Kategoriler.Isim,Bilgi,InStock,StokSayimId from StokSayimDetay  sti
                        left join Urunler on Urunler.id=ItemId
                        left join Kategoriler on Kategoriler.id=Urunler.KategoriId
                        where sti.StokSayimId=@id ";
            var response = await _db.QueryAsync<StockTakeInsertItemsResponse>(sql, param);
            return Ok(response);





        }

        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<StockTakes>> Update(StockTakesUpdate T)
        {
            ValidationResult result = await _StockTakesUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                var hata = await _idcontrol.GetControl("StokSayim", T.id);
                if (hata.Count() == 0)
                {
                    DynamicParameters param = new DynamicParameters();
                    int CompanyId = user[1];

                    await _takes.Update(T, T.id,CompanyId);
                    param.Add("@id", T.id);
                    string sql = @$"select id,Isim,OlusturmaTarihi,BaslangıcTarihi,BitisTarihi,Sebeb,Bilgi from StokSayim where id=@id";
                    var response = await _db.QueryAsync<StockTakesUpdate>(sql, param);
                    return Ok(response);
                 
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
        [HttpPut, Authorize]
        public async Task<ActionResult<StockTakes>> UpdateItems(StockTakesUpdateItems T)
        {
            ValidationResult result = await _updateitem.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                var hata = await _stocktakes.UpdateItem(T);
                if (hata.Count() == 0)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@CompanyId", CompanyId);
                    await _takes.UpdateItems(T, CompanyId);
                    param.Add("@CompanyId", CompanyId);
                    param.Add("@StockTakesId", T.StokSayimId);
                    param.Add("@id", T.StokSayimDetayId);
                    string sql = @$"select id as StokSayimDetayId,StokSayimId,SayilanMiktar,Bilgi from StokSayimDetay where id=@id and StokSayimId=@StockTakesId";
                    var response = await _db.QueryAsync<StockTakesUpdateItems>(sql, param);
                    return Ok(response);
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
        public async Task<ActionResult<StockTakeDelete>> Delete(IdControl T)
        {
            ValidationResult result = await _Delete.ValidateAsync(T);
            if (result.IsValid)
            {
               
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int User = user[1];
                var hata = await _idcontrol.GetControl("StokSayim", T.id);
                if (hata.Count() == 0)
                {
                    DynamicParameters param = new DynamicParameters();
                    await _takes.Delete(T, CompanyId,User);
                    return Ok("Silme  başarılı.");
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
        public async Task<ActionResult<StockTakeDelete>> DeleteItems(StockTakeDelete T)
        {
            ValidationResult result = await _StockTakeDeleteItem.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                var hata = await _stocktakes.DeleteItem(T);
                if (hata.Count() == 0)
                {
                    DynamicParameters param = new DynamicParameters();
                    await _takes.DeleteItems(T, CompanyId);
                    return Ok("Silme  başarılı.");
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
