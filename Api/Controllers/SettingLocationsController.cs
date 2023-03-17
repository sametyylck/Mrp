using BL.Extensions;
using BL.Services.IdControl;
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
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ItemDTO;
using static Dapper.SqlMapper;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingLocationsController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly ILocationsRepository _location;
        private readonly IDbConnection _db;
        private readonly IValidator<IdControl> _Idcontrol;
        private readonly IValidator<LocationsDTO> _LocationDTO;
        private readonly IValidator<LocationsInsert> _LocationInsert;
        private readonly IIDControl _idcontrol;
        private readonly IPermissionControl _izinkontrol;




        public SettingLocationsController(ILocationsRepository location, IUserService userService, IDbConnection db, IValidator<IdControl> ıdcontrol, IValidator<LocationsInsert> locationInsert, IValidator<LocationsDTO> locationDTO, IIDControl idcontrol, IPermissionControl izinkontrol)
        {
            _location = location;
            _user = userService;
            _db = db;
            _Idcontrol = ıdcontrol;
            _LocationInsert = locationInsert;
            _LocationDTO = locationDTO;
            _idcontrol = idcontrol;
            _izinkontrol = izinkontrol;
        }
        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<LocationsDTO>> List()
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarAdresler, Permison.AyarlarHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _location.List();
            return Ok(list);
        }

        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<LocationsDTO>> Insert(LocationsInsert T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarAdresler, Permison.AyarlarHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _LocationInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                int id = await _location.Insert(T, CompanyId);
                DynamicParameters param = new DynamicParameters();
                param.Add("@CompanyId", CompanyId);
                param.Add("@id", id);
                var list = await _db.QueryAsync<LocationsDTO>($"Select * From DepoVeAdresler where id = @id", param);
                return Ok(list.First());

            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

        }
        [Route("Update")]
        [HttpPost, Authorize]
        public async Task<ActionResult<LocationsDTO>> Update(LocationsDTO T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarAdresler, Permison.AyarlarHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _LocationDTO.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _idcontrol.GetControl("DepoVeAdresler", T.id);
                if (hata.Count() == 0)
                {
                    await _location.Update(T);
                    return Ok("Güncelleme İşlemi Başarıyla Gerçekleşti");
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
        public async Task<ActionResult<LocationsDTO>> Delete(IdControl T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarAdresler, Permison.AyarlarHepsi,UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _Idcontrol.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _idcontrol.GetControl("DepoVeAdresler", T.id);
                if (hata.Count() == 0)
                {
                    await _location.Delete(T);
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
    }
}
