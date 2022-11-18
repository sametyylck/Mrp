﻿using BL.Services.IdControl;
using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using DAL.Repositories;
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
    public class SettingResourceController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IDbConnection _db;
        private readonly IResourceRepository _resource;
        private readonly IValidator<ResourcesUpdate> _ResourceUpdate;
        private readonly IValidator<ResourcesInsert> _ResourceInsert;
        private readonly IValidator<IdControl> _delete;
        private readonly IIDControl _idcontrol;
        public SettingResourceController(IUserService user, IDbConnection db, IResourceRepository resource, IValidator<ResourcesUpdate> resourceUpdate, IValidator<ResourcesInsert> resourceInsert, IValidator<IdControl> delete, IIDControl idcontrol)
        {
            _user = user;
            _db = db;
            _resource = resource;
            _ResourceUpdate = resourceUpdate;
            _ResourceInsert = resourceInsert;
            _delete = delete;
            _idcontrol = idcontrol;
        }
        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<ResourcesDTO>> List()
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _resource.List(CompanyId);

            return Ok(list);
        }

        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<ResourcesDTO>> Insert(ResourcesInsert T)
        {
            ValidationResult result = await _ResourceInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int id = await _resource.Insert(T, CompanyId);

                DynamicParameters param = new DynamicParameters();
                param.Add("@CompanyId", CompanyId);
                param.Add("@id", id);
                var list = await _db.QueryAsync<ResourcesDTO>($"Select * From Resources where CompanyId = @CompanyId and id = @id", param);
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
        public async Task<ActionResult<ResourcesDTO>> Update(ResourcesUpdate T)
        {
            ValidationResult result = await _ResourceUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _idcontrol.GetControl("Resources", T.id, CompanyId);
                if (hata == "true")
                {
                    await _resource.Update(T, CompanyId);
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
        public async Task<ActionResult<ResourcesDTO>> Delete(IdControl T)
        {
            ValidationResult result = await _delete.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _idcontrol.GetControl("Resources", T.id, CompanyId);
                if (hata == "true")
                {
                    await _resource.Delete(T, CompanyId);
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