using BL.Extensions;
using BL.Services.IdControl;
using BL.Services.Orders;
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
using System.Security.AccessControl;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.ItemDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static Dapper.SqlMapper;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IDbConnection _db;
        private readonly IOrdersRepository _order;
        private readonly IPurchaseOrderControl _control;
        private readonly IValidator<PurchaseOrderInsert> _PurcOrderInsert;
        private readonly IValidator<PurchaseOrderInsertItem> _PurcOrderInsertItem;
        private readonly IValidator<PurchaseOrderUpdate> _PurcOrderUpdate;
        private readonly IValidator<PurchaseItem> _PurhaseItem;
        private readonly IValidator<DeleteItems> _DeleteItems;
        private readonly IValidator<Delete> _Delete;
        private readonly IIDControl _idcontrol;
        private readonly IPermissionControl _izinkontrol;

        public OrdersController(IOrdersRepository order, IDbConnection db, IUserService user, IPurchaseOrderControl control, IValidator<PurchaseOrderInsert> purcOrderInsert, IValidator<PurchaseOrderInsertItem> purcOrderInsertItem, IValidator<PurchaseOrderUpdate> purcOrderUpdate, IValidator<PurchaseItem> purhaseItem, IValidator<DeleteItems> deleteItems, IValidator<Delete> delete, IIDControl idcontrol, IPermissionControl izinkontrol)
        {
            _order = order;
            _db = db;
            _user = user;
            _control = control;
            _PurcOrderInsert = purcOrderInsert;
            _PurcOrderInsertItem = purcOrderInsertItem;
            _PurcOrderUpdate = purcOrderUpdate;
            _PurhaseItem = purhaseItem;
            _DeleteItems = deleteItems;
            _Delete = delete;
            _idcontrol = idcontrol;
            _izinkontrol = izinkontrol;
        }

        [Route("Details/{id}")]
        [HttpGet, Authorize]
        public async Task<ActionResult<PurchaseDetails>> Details(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatinAlmaGoruntule, Permison.SatinAlmaHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _order.Details(id);

            return Ok(list);
        }
        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<PurchaseOrder>> Insert(PurchaseOrderInsert T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatinAlmaEkleyebilirVeDuzenleyebilir, Permison.SatinAlmaHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _PurcOrderInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _control.Insert(T);
                if (hata.Count()==0)
                {
                    int id = await _order.Insert(T,UserId);
                    PurchaseOrderInsertItem B = new PurchaseOrderInsertItem();
                    B.VergiId = T.VergiId;
                    B.StokId = T.StokId;
                    B.Miktar = T.Miktar;
                    B.OlcuId = T.OlcuId;
                    int inserid = await _order.InsertPurchaseItem(B, id);
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@id", id);
                    var list=await _order.Details(id);
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
        [Route("InsertItems")]
        [HttpPost, Authorize]
        public async Task<ActionResult<PurchaseOrder>> InsertItems(PurchaseOrderInsertItem T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatinAlmaEkleyebilirVeDuzenleyebilir, Permison.SatinAlmaHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _PurcOrderInsertItem.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _control.InsertItem(T);
                if (hata.Count() == 0)
                {
                    int id = await _order.InsertPurchaseItem(T, T.SatinAlmaId);
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@OrderId", T.SatinAlmaId);
                    param2.Add("@id", id);
                    var list = await _db.QueryAsync<OrdersResponse>($"Select sad.id,SatinAlma.id as SatinAlmaId,sad.StokId,u.Isim as UrunIsmi, sad.Miktar, sad.OlcuId,Olcu.Isim as OlcuIsmi, sad.ToplmaTutar, sad.BirimFiyat, sad.TumTutar, sad.VergiId, Vergi.VergiIsmi,sad.VergiDegeri from SatinAlma left join SatinAlmaDetay sad on sad.SatinAlmaId = SatinAlma.id left  join Urunler u on u.id = sad.StokId left join Olcu on Olcu.id = sad.OlcuId left    join Vergi on Vergi.id = sad.VergiId where  SatinAlma.id = @OrderId and sad.id = @id ", param2);

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
        public async Task<ActionResult<PurchaseOrderUpdate>> Update(PurchaseOrderUpdate T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatinAlmaEkleyebilirVeDuzenleyebilir, Permison.SatinAlmaHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _PurcOrderUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                var control = await _control.Update(T);
                if (control.Count() == 0)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@id", T.id);
                    param1.Add("@ContactId", T.TedarikciId);
                    param1.Add("@LocationId", T.DepoId);
                    await _order.Update(T);
                    var list = await _order.Details(T.id);
                    return Ok(list);
                }
                else
                {
                    return BadRequest(control);
                }
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }



        }

        [Route("UpdatePurchaseItem")]
        [HttpPut, Authorize]
        public async Task<ActionResult<PurchaseItem>> UpdatePurchaseItem(PurchaseItem T)
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
            ValidationResult result = await _PurhaseItem.ValidateAsync(T);
            if (result.IsValid)
            {
                var control = await _control.UpdatePurchaseItem(T);
                if (control.Count() == 0)
                {

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("id", T.id);
                    param2.Add("@CompanyId", CompanyId);
                    param2.Add("@OrderId", T.SatinAlmaId);
                    await _order.UpdatePurchaseItem(T);
                    var list = await _db.QueryAsync<PurchaseOrderUpdateItemResponse>($"Select sad.id,SatinALma.id as SatinAlmaId,sad.StokId,u.Isim as UrunIsmi, sad.Miktar, sad.OlcuId,Olcu.Isim as OlcuIsmi,  sad.BirimFiyat, sad.TumToplam, sad.VergiId, Vergi.VergiIsim,sad.VergiDegeri from SatinAlma left join SatinAlmaDetay sad on sad.SatinAlmaId = SatinAlma.id left  join Urunler u on u.id = sad.StokId left   join Olcu on Olcu.id = sad.OlcuId left    join Vergi on Vergi.id = sad.VergiId where  SatinAlma.id = @OrderId and sad.id = @id ", param2);
                    return Ok(list);
                }
                else
                {
                    return BadRequest(control);
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
        public async Task<ActionResult<DeleteItems>> DeleteItems(DeleteItems T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatinAlmaSilebilir, Permison.SatinAlmaHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _DeleteItems.ValidateAsync(T);
            if (result.IsValid)
            {
                var control = await _control.DeleteItem(T);
                if (control.Count() == 0)
                {
                    await _order.DeleteItems(T);
                    return Ok("Silme İşlemi Başarıyla Gerçekleşti");
                }
                else
                {
                    return BadRequest(control);
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
        public async Task<ActionResult<Delete>> Delete(List<Delete> A)
        {

            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatinAlmaSilebilir, Permison.SatinAlmaHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var userId = user[1];
            await _order.Delete(A, userId);
            return Ok("Silme İşlemi Başarıyla Gerçekleşti");


        }





    }
}

