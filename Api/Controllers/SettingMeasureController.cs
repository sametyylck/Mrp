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
using static DAL.DTO.ContactDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingMeasureController : ControllerBase
    {
       private readonly IMeasureRepository _measureRepository;
       private readonly IUserService _user;
        private readonly IDbConnection _db;
        private readonly IValidator<MeasureInsert> _MeasureInsert;
        private readonly IValidator<MeasureUpdate> _MeasureUpdate;
        private readonly IValidator<IdControl> _Delete;
        private readonly IIDControl _idcontrol;
        private readonly ILogger<SettingMeasureController> _logger;



        public SettingMeasureController(IMeasureRepository measureRepository, IUserService user, IDbConnection db, IValidator<MeasureInsert> measureInsert, IValidator<MeasureUpdate> measureUpdate, IValidator<IdControl> delete, IIDControl idcontrol, ILogger<SettingMeasureController> logger)
        {
            _measureRepository = measureRepository;
            _user = user;
            _db = db;
            _MeasureInsert = measureInsert;
            _MeasureUpdate = measureUpdate;
            _Delete = delete;
            _idcontrol = idcontrol;
            _logger = logger;
        }
        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<MeasureDTO>> List()
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _measureRepository.List(CompanyId); 

            return Ok( list);
        }
        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<MeasureDTO>> Insert(MeasureInsert T)
        {
            ValidationResult result = await _MeasureInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int id = await _measureRepository.Insert(T, CompanyId);

                string sql = $"Select * From Measure where CompanyId = {CompanyId} and id = {id}";
                var eklenen = await _db.QueryAsync<MeasureClas>(sql);
                if (eklenen.Count() == 0)
                {
                    return BadRequest("Measure Eklenirken Bir Hata Oluştu");
                }
                return Ok(eklenen.First());
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

       
        }

        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<MeasureDTO>> Update(MeasureUpdate T)
        {
            ValidationResult result = await _MeasureUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _idcontrol.GetControl("Measure", T.id, CompanyId);
                if (hata == "true")
                {
                    await _measureRepository.Update(T, CompanyId);
                    return Ok("Güncelleme İşlemi Başarıyla Gerçekleşti");
                }
                else
                {
                    _logger.LogWarning(hata);
                    return BadRequest(hata);
                }
              
            }
            else
            {
                
            
                result.AddToModelState(this.ModelState);
                string hata = result.ToString();
                _logger.LogWarning(hata);
                return BadRequest(hata);
            }
       
        }

        [Route("Delete")]
        [HttpDelete, Authorize]
        public async Task<ActionResult<MeasureDTO>> Delete(IdControl T)
        {
            ValidationResult result = await _Delete.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _idcontrol.GetControl("Measure", T.id, CompanyId);
                if (hata == "true")
                {
                    await _measureRepository.Delete(T, CompanyId);
                    return Ok("Silme İşlemi  Gerçekleşti");
                }
                else
                {
                    _logger.LogWarning(hata);
                    return BadRequest(hata);
                }
               
            }
            else
            {
                result.AddToModelState(this.ModelState);
                _logger.LogWarning(result.ToString());
                return BadRequest(result.ToString());
            }
        
        }
    }
}
