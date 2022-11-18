using BL.Services.IdControl;
using DAL.Contracts;
using DAL.DTO;
using Dapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Data;
using static DAL.DTO.BomDTO;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ContactDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingCategoryController : ControllerBase
    {
        private readonly IDbConnection _db;
        private readonly IUserService _user;
        private readonly ICategoryRepository _categoty;
        private readonly IValidator<IdControl> _CategoryDelete;
        private readonly IValidator<CategoryInsert> _CategoryInsert;
        private readonly IValidator<CategoryUpdate> _CategoryUpdate;
        private readonly IIDControl _idcontrol;





        public SettingCategoryController(ICategoryRepository categoty, IUserService user, IDbConnection db, IValidator<IdControl> categoryDelete, IValidator<CategoryInsert> categoryInsert, IValidator<CategoryUpdate> categoryUpdate, IIDControl idcontrol)
        {
            _categoty = categoty;
            _user = user;
            _db = db;
            _CategoryDelete = categoryDelete;
            _CategoryInsert = categoryInsert;
            _CategoryUpdate = categoryUpdate;
            _idcontrol = idcontrol;
        }
        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<CategoryClass>> List()
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _categoty.List(CompanyId);
            return Ok(list);
        }
        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<CategoryClass>> Insert(CategoryInsert T)
        {
            ValidationResult result = await _CategoryInsert.ValidateAsync(T);

            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int id = await _categoty.Insert(T, CompanyId);
                string sql = $"Select * From Categories where CompanyId = {CompanyId} and id = {id}";
                var eklenen = await _db.QueryAsync<CategoryClass>(sql);
                if (eklenen.Count() == 0)
                {
                    return BadRequest("Category Eklenirken Bir Hata Oluştu.");
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
        public async Task<ActionResult<CategoryClass>> Update(CategoryUpdate T)
        {
            ValidationResult result= await _CategoryUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
              
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _idcontrol.GetControl("Categories", T.id, CompanyId);
                if (hata=="true")
                {
                    await _categoty.Update(T, CompanyId);
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
        public async Task<ActionResult<CategoryClass>> Delete(IdControl T)
        {
            ValidationResult result = await _CategoryDelete.ValidateAsync(T);
            if (result.IsValid)
            {

                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _idcontrol.GetControl("Categories", T.id, CompanyId);
                if (hata == "true")
                {
                    await _categoty.Delete(T, CompanyId);
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
