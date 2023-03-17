using BL.Extensions;
using BL.Services.IdControl;
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
        private readonly IPermissionControl _izinkontrol;
        public SettingResourceController(IUserService user, IDbConnection db, IResourceRepository resource, IValidator<ResourcesUpdate> resourceUpdate, IValidator<ResourcesInsert> resourceInsert, IValidator<IdControl> delete, IIDControl idcontrol, IPermissionControl izinkontrol)
        {
            _user = user;
            _db = db;
            _resource = resource;
            _ResourceUpdate = resourceUpdate;
            _ResourceInsert = resourceInsert;
            _delete = delete;
            _idcontrol = idcontrol;
            _izinkontrol = izinkontrol;
        }
        /// <summary>
        /// Kaynak Liste
        /// </summary>
        /// <returns></returns>
        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<ResourcesDTO>> List()
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarKaynaklar, Permison.AyarlarHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _resource.List(CompanyId);

            return Ok(list);
        }


        /// <summary>
        /// Kaynak ekleme saatlik ücret ve ismi ile
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<ResourcesDTO>> Insert(ResourcesInsert T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarKaynaklar, Permison.AyarlarHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ResourceInsert.ValidateAsync(T);
            if (result.IsValid)
            {
                
                int id = await _resource.Insert(T, CompanyId);

                DynamicParameters param = new DynamicParameters();
                param.Add("@CompanyId", CompanyId);
                param.Add("@id", id);
                var list = await _db.QueryAsync<ResourcesDTO>($"Select id,Isim ,VarsayilanSaatlikUcret From Kaynaklar where id = @id", param);
                return Ok(list);
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

      

        }
        /// <summary>
        /// kaynak update isim ve varsayilan ücret
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<ResourcesDTO>> Update(ResourcesUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarKaynaklar, Permison.AyarlarHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ResourceUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _idcontrol.GetControl("Kaynaklar", T.id);
                if (hata.Count() == 0)
                {
                    await _resource.Update(T);
                    var list = await _db.QueryAsync<ResourcesDTO>($"Select id,Isim ,VarsayilanSaatlikUcret From Kaynaklar where id = {T.id}");
                    return Ok(list);
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
        /// <summary>
        /// silme işlemi id ile
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        [Route("Delete")]
        [HttpDelete, Authorize]
        public async Task<ActionResult<ResourcesDTO>> Delete(IdControl T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.AyarlarKaynaklar, Permison.AyarlarHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _delete.ValidateAsync(T);
            if (result.IsValid)
            {
                
                var hata = await _idcontrol.GetControl("Kaynaklar", T.id);
                if (hata.Count() == 0)
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
