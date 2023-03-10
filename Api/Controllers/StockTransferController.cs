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
        private readonly IValidator<StockTransferItems> _StockTransferUpdateItems;
        private readonly IValidator<StockUpdate> _StockTransferUpdate;
        private readonly IValidator<IdControl> _StockTransferDelete;
        private readonly IValidator<StockTransferDeleteItems> _StockTransferDeleteItem;
        private readonly IValidator<StockTransferInsertItem> _StockTransferInsertItem;
        private readonly IValidator<StockTransferInsert> _StockTransferInsert;
        private readonly IIDControl _idcontrol;
        private readonly ILocationStockControl _locstokkontrol;
        private readonly IPermissionControl _izinkontrol;



        public StockTransferController(IUserService user, IDbConnection db, IStockTransferRepository transfer, IStockTransferControl control, IValidator<StockTransferItems> stockTransferItems, IValidator<StockUpdate> stockTransferUpdate, IValidator<StockTransferDeleteItems> stockTransferDeleteItem, IValidator<StockTransferInsertItem> stockTransferInsertItem, IValidator<StockTransferInsert> stockTransferInsert, IValidator<IdControl> stockTransferDelete, IIDControl idcontrol, ILocationStockControl locstokkontrol, IPermissionControl izinkontrol)
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
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferGoruntule, Permison.StokTransferHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _transfer.List(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = await _transfer.Count(T, CompanyId);
            return Ok(new { list, count });

        }

        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockTransferAll>> Insert(StockTransferInsert T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferEkle, Permison.StokTransferHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockTransferInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _control.Insert(T, CompanyId);
                if (hata.Count()==0)
                {
                    string sql1 = $"Select Tip From Items where CompanyId = {CompanyId} and id = {T.ItemId}";
                    var Tip = await _db.QueryFirstAsync<string>(sql1);
                    await _locstokkontrol.Kontrol(T.ItemId, T.OriginId, Tip, CompanyId);
                    await _locstokkontrol.Kontrol(T.ItemId, T.DestinationId, Tip, CompanyId);

                    var kontrol = await _locstokkontrol.AdresStokKontrol(T.ItemId, T.OriginId, T.DestinationId,T.Quantity, CompanyId);

                    if (kontrol.Count()!=0)
                    {
                        return BadRequest(kontrol);
                    }
                    int id = await _transfer.Insert(T, CompanyId);
                    StockTransferInsertItem C = new StockTransferInsertItem();
                    C.ItemId = T.ItemId;
                    C.Quantity = T.Quantity;
                    C.StockTransferId = id;
                    await _transfer.InsertStockTransferItem(C, id, CompanyId, UserId);

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@CompanyId", CompanyId);
                    param2.Add("@id", id);
                    //response dönüş
                    var list = await _db.QueryAsync<StockTransfer>($"Select * From StockTransfer where CompanyId = @CompanyId and id = @id ", param2);

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
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferEkle, Permison.StokTransferHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockTransferInsertItem.ValidateAsync(T);
            if (result.IsValid)
            {
                
                var hata = await _control.InsertItem(T, CompanyId);
                if (hata.Count()==0)
                {
                    int id = await _transfer.InsertStockTransferItem(T, T.StockTransferId, CompanyId, UserId);
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@CompanyId", CompanyId);
                    param2.Add("@id", T.StockTransferId);
                    var list = await _db.QueryAsync<StockTransferDetailsItems>($@"Select StockTransferItems.id,StockTransferItems.ItemId,StockTransferItems.Quantity,
                Items.Name as ItemName,StockTransferItems.CostPerUnit,    
                StockTransfer.DestinationId, l.LocationName as DestinationLocationName,
                v.StockCount as DestinationLocationStockCount,    StockTransfer.OriginId,
                m.LocationName as OriginLocationName, c.StockCount as OriginLocationStockCount,
                StockTransferItems.TransferValue from StockTransferItems
                left join Items on Items.id = StockTransferItems.ItemId 
                left join StockTransfer on StockTransfer.id = StockTransferItems.StockTransferId 
                left  join Locations l on l.id = StockTransfer.OriginId 
                left  join Locations m on m.id = StockTransfer.DestinationId
                left  join LocationStock c on c.ItemId = Items.id 
                and c.LocationId = StockTransfer.OriginId and l.CompanyId = StockTransfer.CompanyId 
                left join LocationStock v on v.ItemId = Items.id and v.LocationId = StockTransfer.DestinationId
                and m.CompanyId = StockTransfer.CompanyId
                where StockTransferItems.CompanyId = @CompanyId and
                StockTransferItems.StockTransferId = @id 
                Group BY StockTransferItems.id, StockTransferItems.ItemId, StockTransferItems.Quantity,
                Items.Name,StockTransfer.DestinationId, l.LocationName, v.StockCount, StockTransfer.OriginId,
                m.LocationName, c.StockCount, StockTransferItems.TransferValue, StockTransferItems.CostPerUnit ", param2);
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
        public async Task<ActionResult<StockUpdate>> Update(StockUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferEkle, Permison.StokTransferHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockTransferUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _idcontrol.GetControl("StockTransfer", T.id, CompanyId);
                if (hata.Count() == 0)
                {

                    await _transfer.Update(T, CompanyId);

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@CompanyId", CompanyId);
                    param2.Add("@id", T.id);
                    var list = await _db.QueryAsync<StockTransfer>($"Select * From StockTransfer where CompanyId = @CompanyId and id = @id ", param2);

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
        public async Task<ActionResult<StockTransferItems>> UpdateStockTransferItem(StockTransferItems T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferEkle, Permison.StokTransferHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockTransferUpdateItems.ValidateAsync(T);
            if (result.IsValid)
            {
              
                var hata = await _control.UpdateItems(T.ItemId,T.StockTransferId,T.id, CompanyId);
                if (hata.Count() == 0)
                {
                    var kontrol = await _control.AdresStokKontrol(T.id,T.ItemId, T.StockTransferId, T.Quantity, CompanyId);
                    if (kontrol.Count()!=0)
                    {
                        return BadRequest(kontrol);
                    }
                    int id=await _transfer.UpdateStockTransferItem(T, CompanyId, UserId);
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@CompanyId", CompanyId);
                    param2.Add("@id", T.StockTransferId);
                    //istenilen değerler response olarak dönülüyor
                    var list = await _transfer.Details(id, CompanyId);
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
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferSilebilir, Permison.StokTransferHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockTransferDeleteItem.ValidateAsync(T);
            if (result.IsValid)
            {
               
                var hata = await _control.UpdateItems(T.ItemId,T.StockTransferId,T.id, CompanyId);
                if (hata.Count() == 0)
                {
                    await _transfer.DeleteItems(T, CompanyId, UserId);
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
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferSilebilir, Permison.StokTransferHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _StockTransferDelete.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _idcontrol.GetControl("StockTransfer", T.id, CompanyId);
                if (hata.Count() == 0)
                {
                    await _transfer.Delete(T, CompanyId, UserId);
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
            var izin = await _izinkontrol.Kontrol(Permison.StokTransferGoruntule, Permison.StokTransferHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _transfer.Details(id, CompanyId);
            return Ok(list);


        }

    }
}
