using BL.Extensions;
using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using static DAL.DTO.ManufacturingOrderDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UretimListController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IUretimList _list;
        private readonly IPermissionControl _izinkontrol;

        public UretimListController(IUserService user, IUretimList list, IPermissionControl izinkontrol)
        {
            _user = user;
            _list = list;
            _izinkontrol = izinkontrol;
        }

        [Route("Detail")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Detail(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimGoruntule, Permison.UretimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }

            var list=await _list.Detail(CompanyId, id);
            return Ok(list);
        }

        [Route("ScheludeOpenList")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ScheludeOpenList(ManufacturingOrderListArama T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimGoruntule, Permison.UretimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _list.ScheludeOpenList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = list.Count();
            return Ok(new { list, count });
        }
        [Route("ScheludeDoneList")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ScheludeDoneList(ManufacturingOrderDoneListArama T, int KAYITSAYISI, int SAYFA,DateTime? Tarih1,DateTime? Tarih2)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimGoruntule, Permison.UretimHepsi,UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _list.ScheludeDoneList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = list.Count();

            return Ok(new { list, count });
        }
        [Route("TaskOpenList")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> TaskOpenList(ManufacturingTaskArama T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimGoruntule, Permison.UretimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
   
           var list= await _list.TaskOpenList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = list.Count();
            return Ok(new { list, count });
        }
        [Route("TaskDoneList")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> TaskDoneList(ManufacturingTaskArama T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.UretimGoruntule, Permison.UretimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list=await _list.TaskDoneList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count=list.Count();

            return Ok(new {list,count});
        }
    }
}
