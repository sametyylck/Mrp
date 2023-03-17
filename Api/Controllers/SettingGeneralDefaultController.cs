using BL.Extensions;
using BL.Services.GeneralDefaultSettings;
using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.GeneralSettingsDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingGeneralDefaultController : ControllerBase
    {
        private readonly IGeneralDefaultRepository _general;
        private readonly IUserService _user;
        private readonly IValidator<GeneralDefaultSettings> _GeneralDefaultSettings;
        private readonly IGeneralDefaultSettingsControl _control;
        private readonly IPermissionControl _izinkontrol;

        public SettingGeneralDefaultController(IUserService userService, IGeneralDefaultRepository general, IValidator<GeneralDefaultSettings> generalDefaultSettings, IGeneralDefaultSettingsControl control, IPermissionControl izinkontrol)
        {
            _user = userService;
            _general = general;
            _GeneralDefaultSettings = generalDefaultSettings;
            _control = control;
            _izinkontrol = izinkontrol;
        }
        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<DefaultSettingList>> List()
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarGenel, Permison.AyarlarGenel, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }

            var list = await _general.List(CompanyId);
            return Ok(list);
        }
        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<GeneralDefaultSettings>> Update(GeneralDefaultSettings T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarGenel, Permison.AyarlarGenel, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _GeneralDefaultSettings.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _control.Update(T);
                if (hata.Count()==0)
                {
                    await _general.Update(T, CompanyId);
                    return Ok("Güncelleme işlemi başarıyla gerçekleşti");
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
