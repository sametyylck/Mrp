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

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingOperationsController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IOperationsRepository _operationsRepository;
        private readonly IDbConnection _db;
        private readonly IValidator<OperationsInsert> _OperationsInsert;
        private readonly IValidator<OperationsUpdate> _OperationsUpdate;
        private readonly IValidator<IdControl> _delete;
        private readonly IIDControl _idcontrol;


        public SettingOperationsController(IUserService user, IOperationsRepository operationsRepository, IDbConnection db, IValidator<OperationsInsert> operationsInsert, IValidator<OperationsUpdate> operationsUpdate, IValidator<IdControl> delete, IIDControl idcontrol)
        {
            _user = user;
            _operationsRepository = operationsRepository;
            _db = db;
            _OperationsInsert = operationsInsert;
            _OperationsUpdate = operationsUpdate;
            _delete = delete;
            _idcontrol = idcontrol;
        }
        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<OperitaonsDTO>> List()
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _operationsRepository.List(CompanyId);

            return Ok(list);
        }

        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<OperitaonsDTO>> Insert(OperationsInsert T)
        {
            ValidationResult result = await _OperationsInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int id = await _operationsRepository.Insert(T, CompanyId);
                string sql = $"Select * From Operations where CompanyId = {CompanyId} and id = {id}";
                var eklenen = await _db.QueryAsync<OperitaonsDTO>(sql);
                if (eklenen.Count() == 0)
                {
                    return BadRequest("Operasyon Eklenirken Bir Hata Oluştu.");
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
        public async Task<ActionResult<OperitaonsDTO>> Update(OperationsUpdate T)
        {
            ValidationResult result = await _OperationsUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
             
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _idcontrol.GetControl("Operations", T.id, CompanyId);
                if (hata=="true")
                {
                    await _operationsRepository.Update(T, CompanyId);
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
        public async Task<ActionResult<OperitaonsDTO>> Delete(IdControl T)
        {
            ValidationResult result = await _delete.ValidateAsync(T);
            if (result.IsValid)
            {

                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string tablo = "Operations";
                string hata=await _idcontrol.GetControl(tablo, T.id, CompanyId);
                if (hata=="true")
                {
                    await _operationsRepository.Delete(T, CompanyId);
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
