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
using static DAL.DTO.TaxDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingTaxController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IDbConnection _db;
        private readonly ITaxRepository _tax;
        private readonly IValidator<IdControl> _Delete;
        private readonly IValidator<TaxInsert> _TaxInsert;
        private readonly IValidator<TaxUpdate> _TaxUpdate;
        private readonly IIDControl _idcontrol;
        private readonly IPermissionControl _izinkontrol;
        public SettingTaxController(IUserService user, IDbConnection db, ITaxRepository tax, IValidator<IdControl> delete, IValidator<TaxInsert> taxInsert, IValidator<TaxUpdate> taxUpdate, IIDControl idcontrol, IPermissionControl izinkontrol)
        {
            _user = user;
            _db = db;
            _tax = tax;
            _Delete = delete;
            _TaxInsert = taxInsert;
            _TaxUpdate = taxUpdate;
            _idcontrol = idcontrol;
            _izinkontrol = izinkontrol;
        }
        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<TaxClas>> List()
        {

            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarVergi, Permison.AyarlarHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _tax.List(CompanyId);
            return Ok(list);

        }
        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<TaxClas>> Insert(TaxInsert T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarVergi, Permison.AyarlarHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _TaxInsert.ValidateAsync(T);
            if (result.IsValid)
            {
               
                int id = await _tax.Insert(T, CompanyId);


                DynamicParameters param = new DynamicParameters();
                param.Add("@CompanyId", CompanyId);
                param.Add("@id", id);
                var list = await _db.QueryAsync<TaxClas>($"Select * From Tax where CompanyId = @CompanyId and id = @id ", param);
                return Ok(list.First());
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }


        }

        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<TaxClas>> Update(TaxUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarVergi, Permison.AyarlarHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _TaxUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                
                var hata = await _idcontrol.GetControl("Tax", T.id, CompanyId);
                if (hata.Count() == 0)
                {
                    await _tax.Update(T, CompanyId);
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
        public async Task<ActionResult<TaxClas>> Delete(IdControl T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarVergi, Permison.AyarlarHepsi, CompanyId, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _Delete.ValidateAsync(T);
            if (result.IsValid)
            {
                

                var hata = await _idcontrol.GetControl("Tax", T.id, CompanyId);
                if (hata.Count() == 0)
                {
                    await _tax.Delete(T, CompanyId);
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
    }
}
