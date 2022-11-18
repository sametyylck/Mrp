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
        private readonly IUserService _userService;
        private readonly IValidator<GeneralDefaultSettings> _GeneralDefaultSettings;
        private readonly IGeneralDefaultSettingsControl _control;

        public SettingGeneralDefaultController(IUserService userService, IGeneralDefaultRepository general, IValidator<GeneralDefaultSettings> generalDefaultSettings, IGeneralDefaultSettingsControl control)
        {
            _userService = userService;
            _general = general;
            _GeneralDefaultSettings = generalDefaultSettings;
            _control = control;
        }
        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<DefaultSettingList>> List()
        {
            List<int> user = _userService.CompanyId();
            int CompanyId = user[0];
            var list = await _general.List(CompanyId);
            return Ok(list);
        }
        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<GeneralDefaultSettings>> Update(GeneralDefaultSettings T)
        {
            ValidationResult result = await _GeneralDefaultSettings.ValidateAsync(T);
            if (result.IsValid)
            {


                List<int> user = _userService.CompanyId();
                int CompanyId = user[0];
                string hata = await _control.Update(T, CompanyId);
                if (hata == "true")
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
