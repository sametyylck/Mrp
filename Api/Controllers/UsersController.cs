﻿using BL.Services.IdControl;
using BL.Services.Kullanıcı;
using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using DAL.Repositories;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.StockAdjusmentDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDbConnection _db;
        private readonly IUserService _user;
        private readonly IKullanıcıRepository _kullanıcı;
        private readonly IKullanıcıKontrol _kullanıcıkontrol;
        private readonly IIDControl _idcontrol;

        public UsersController(IDbConnection db, IUserService user, IKullanıcıRepository kullanıcı, IKullanıcıKontrol kullanıcıkontrol, IIDControl idcontrol)
        {
            _db = db;
            _user = user;
            _kullanıcı = kullanıcı;
            _kullanıcıkontrol = kullanıcıkontrol;
            _idcontrol = idcontrol;
        }

        [Route("KullanıcıInsert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> KullanıcıInsert(UserInsert T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var hata = await _kullanıcıkontrol.KullanıcıInsertKontrol(T.Mail, T.RoleId);
            if (hata.Count()!=0)
            {
                return BadRequest(hata);
            }
            int id = await _kullanıcı.KullanıcıInsert(T, UserId);
            var list = await _kullanıcı.KullanıcıDetail(id);
            return Ok(list);
        }
        [Route("KullanıcıUpdate")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> KullanıcıUpdate(UserUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var hata = await _kullanıcıkontrol.KullanıcıUpdateKontrol(T.id, T.Mail,T.RoleId);
            if (hata.Count() !=0)
            {
                return BadRequest(hata);
            }
            await _kullanıcı.KullanıcıUpdate(T, UserId);
            var list = await _kullanıcı.KullanıcıDetail(T.id);
            return Ok(list);
        }
        [Route("KullanıcıDelete")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> KullanıcıDelete(int id)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var hata = await _kullanıcıkontrol.KullanıcıDelete(id);
            if (hata.Count() != 0)
            {
                return BadRequest(hata);
            }
            await _kullanıcı.KullanıcıDelete(id, UserId);
            return Ok("Başarılı");
        }
        [Route("KullanıcıDetay")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> KullanıcıDetay(int id)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var hata = await _idcontrol.GetControl("Kullanıcılar", id);
            if (hata.Count() != 0)
            {
                return BadRequest(hata);
            }
            var list= await _kullanıcı.KullanıcıDetail(id);
            return Ok(list);
        }
        [Route("KullanıcıList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> KullanıcıList(string? kelime, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var list=await _kullanıcı.KullanıcıList(kelime, UserId);
            return Ok(list);
        }
        [Route("RoleInsert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> RoleInsert(RoleInsert T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var hata = await _kullanıcıkontrol.RoleInsert(T);
            if (hata.Count()!=0)
            {
                return BadRequest(hata);
            }
            int id=await _kullanıcı.RoleInsert(T, UserId);
            var list = await _kullanıcı.RoleDetail(id, UserId); ;

            return Ok(list);
        }
        [Route("RoleUpdate")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> RoleUpdate(RoleUpdate T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var hata = await _idcontrol.GetControl("Role", T.id);
            if (hata.Count() != 0)
            {
                return BadRequest(hata);
            }
            await _kullanıcı.RoleUpdate(T, UserId);
            var list = await _kullanıcı.RoleDetail(T.id,UserId); ;

            return Ok(list);
        }
        [Route("RoleDelete")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> RoleDelete(int id)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var hata = await _kullanıcıkontrol.RoleDelete(id);
            if (hata.Count() != 0)
            {
                return BadRequest(hata);
            }

            await _kullanıcı.RoleDelete(id, UserId);

            return Ok("Basarili");
        }
        [Route("RoleList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> RoleList(string? kelime)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var list= await _kullanıcı.RoleList(kelime, UserId); ;

            return Ok(list);
        }
        [Route("RoleDetay")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> RoleDetay(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var hata = await _idcontrol.GetControl("Role", id);
            if (hata.Count() != 0)
            {
                return BadRequest(hata);
            }
            var list=await _kullanıcı.RoleDetail(id, UserId); ;

            return Ok(list);
        }
        [Route("PermisionInsert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> PermisionInsert(PermisionInsert T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var hata = await _kullanıcıkontrol.PermisionKontrol(T.PermisionId, T.RoleId);
            if (hata.Count() != 0)
            {
                return BadRequest(hata);
            }
            await _kullanıcı.PermisionInsert(T, UserId); ;

            return Ok();
        }
        [Route("PermisionDelete")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> PermisionDelete(PermisionInsert T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var hata = await _kullanıcıkontrol.PermisionKontrol(T.PermisionId, T.RoleId);
            if (hata.Count() != 0)
            {
                return BadRequest(hata);
            }
            await _kullanıcı.PermisionDelete(T, UserId); ;

            return Ok();
        }
    }
}
