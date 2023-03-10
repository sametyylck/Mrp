using BL.Extensions;
using BL.Services.SalesOrder;
using DAL.Contracts;
using DAL.DTO;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockListDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeklifController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IDbConnection _db;
        private readonly ITeklifRepository _teklif;
        private readonly ISalesOrderControl _salescontrol;
        private readonly IPermissionControl _izinkontrol;

        public TeklifController(IUserService user, IDbConnection db, ITeklifRepository teklif, ISalesOrderControl salescontrol, IPermissionControl izincontrol)
        {
            _user = user;
            _db = db;
            _teklif = teklif;
            _salescontrol = salescontrol;
            _izinkontrol = izincontrol;
        }

        [Route("Insert")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Insert(SatısDTO T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.TeklifEkleyebilirVeDuzenleyebilir, Permison.TeklifHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            int id = await _teklif.Insert(T, CompanyId);
            await _salescontrol.Adress(id, T.ContactId, CompanyId);
            var list = await _db.QueryAsync<SalesOrderUpdate>($"Select * from SalesOrder where Tip = 'Quotes' and id={id}");
            return Ok(list);
        }
        [Route("InsertItem")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> InsertPurchaseItem(TeklifInsertItem T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.TeklifEkleyebilirVeDuzenleyebilir, Permison.TeklifHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            int id = await _teklif.InsertPurchaseItem(T, CompanyId);
            var list = await _db.QueryAsync<TeklifUpdateItems>($"Select * from SalesOrderItem where id={id}");

            return Ok(list);
        }
        [Route("Update")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Update(SalesOrderUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.TeklifEkleyebilirVeDuzenleyebilir, Permison.TeklifHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            await _teklif.Update(T, CompanyId);
            var list = await _db.QueryAsync<SalesOrderUpdate>($"Select * from SalesOrder where Tip = 'Quotes' and id={T.id}");

            return Ok(list);
        }
        [Route("UpdateItems")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateItems(TeklifUpdateItems T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.TeklifEkleyebilirVeDuzenleyebilir, Permison.TeklifHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            await _teklif.UpdateItems(T, CompanyId);
            var list = await _db.QueryAsync<TeklifUpdateItems>($"Select * from SalesOrderItem where id={T.id}");

            return Ok(list);
        }
        [Route("Detail")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Detail(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.TeklifGoruntule, Permison.TeklifHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _teklif.Detail(CompanyId,id);
            return Ok(list);
        }
        [Route("SalesOrderList")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SalesOrderList(SatısListFiltre T, int? KAYITSAYISI, int? SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.TeklifGoruntule, Permison.TeklifHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _teklif.SalesOrderList(T, CompanyId,KAYITSAYISI,SAYFA);
            return Ok(list);
        }
        [Route("DeleteItems")]
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteItems(SatısDeleteItems T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.TeklifSilebilir, Permison.TeklifHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            await _teklif.DeleteItems(T, CompanyId);
            return Ok();
        }
        [Route("Delete")]
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(List<SatısDelete> A)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.TeklifSilebilir, Permison.TeklifHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
      

            await _teklif.DeleteStockControl(A, CompanyId, UserId);
            return Ok();
        }
        [Route("TeklifTamamla")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> TeklifTamamla(QuotesDone A)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.TeklifOnaylama, Permison.TeklifHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            await _teklif.QuotesDone(A, CompanyId);
            return Ok();
        }

    }
}
