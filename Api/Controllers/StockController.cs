using BL.Extensions;
using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using static DAL.DTO.StockListDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IDbConnection _db;
        private readonly IStockRepository _stock;
        private readonly IPermissionControl _izinkontrol;
        public StockController(IUserService user, IStockRepository stock, IDbConnection db, IPermissionControl izinkontrol)
        {
            _user = user;
            _stock = stock;
            _db = db;
            _izinkontrol = izinkontrol;
        }
        [Route("MaterialList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StokList>> MaterialList(StokList T,int KAYITSAYISI,int SAYFA)
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
            DynamicParameters prm = new DynamicParameters();
            var list = await _stock.MaterialList(T,KAYITSAYISI,SAYFA);
            var count = list.Count();
            return Ok(new { list, count });

        }

        [Route("ProductList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StokList>> ProductList(StokList T, int KAYITSAYISI, int SAYFA)
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
            DynamicParameters prm = new DynamicParameters();
            var list = await _stock.ProductList(T, KAYITSAYISI, SAYFA);
            var count = list.Count();
            return Ok(new { list, count });

        }

        [Route("AllItemsList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StokList>> AllItemsList(StokList T, int KAYITSAYISI, int SAYFA)
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
            DynamicParameters prm = new DynamicParameters();
            var list = await _stock.AllItemsList(T, KAYITSAYISI, SAYFA);
            var count = list.Count();
            return Ok(new { list, count });

        }

    }
}
