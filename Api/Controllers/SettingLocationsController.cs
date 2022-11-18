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
        private readonly IUserService _userService;
        private readonly ILocationsRepository _location;
        private readonly IDbConnection _db;
        private readonly IValidator<IdControl> _Idcontrol;
        private readonly IValidator<LocationsDTO> _LocationDTO;
        private readonly IValidator<LocationsInsert> _LocationInsert;
        private readonly IIDControl _idcontrol;




        public SettingLocationsController(ILocationsRepository location, IUserService userService, IDbConnection db, IValidator<IdControl> ıdcontrol, IValidator<LocationsInsert> locationInsert, IValidator<LocationsDTO> locationDTO, IIDControl idcontrol)
        {
            _location = location;
            _userService = userService;
            _db = db;
            _Idcontrol = ıdcontrol;
            _LocationInsert = locationInsert;
            _LocationDTO = locationDTO;
            _idcontrol = idcontrol;
        }
        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<LocationsDTO>> List()
        {
            List<int> user = _userService.CompanyId();
            int CompanyId = user[0];
            var list = await _location.List(CompanyId);
            return Ok(list);
        }

        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<LocationsDTO>> Insert(LocationsInsert T)
        {
            ValidationResult result = await _LocationInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _userService.CompanyId();
                int CompanyId = user[0];

                int id = await _location.Insert(T, CompanyId);
                DynamicParameters param = new DynamicParameters();
                param.Add("@CompanyId", CompanyId);
                param.Add("@id", id);
                var list = await _db.QueryAsync<LocationsDTO>($"Select * From Locations where CompanyId = @CompanyId and id = @id", param);
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
            ValidationResult result = await _LocationDTO.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _userService.CompanyId();
                int CompanyId = user[0];
                string hata = await _idcontrol.GetControl("Locations", T.id, CompanyId);
                if (hata == "true")
                {
                    await _location.Update(T, CompanyId);
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
            ValidationResult result = await _Idcontrol.ValidateAsync(T);
            if (result.IsValid)
            {

                List<int> user = _userService.CompanyId();
                int CompanyId = user[0];
                string hata = await _idcontrol.GetControl("Locations", T.id, CompanyId);
                if (hata == "true")
                {
                    await _location.Delete(T, CompanyId);
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
