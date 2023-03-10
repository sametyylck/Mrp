using BL.Extensions;
using BL.Services.IdControl;
using BL.Services.OperationsBom;
using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.ProductOperationsBomDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperationsBomController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IProductOperationsBomRepository _bom;

        private readonly IValidator<ProductOperationsBOMInsert> _PBomInsert;
        private readonly IValidator<ProductOperationsBOMUpdate> _PBomUpdate;
        private readonly IValidator<IdControl> _PDelete;
        private readonly IOperationBomControl _operationBomControl;
        private readonly IIDControl _idcontrol;
        private readonly IOperationBomControl _operationbomcontrol;
        private readonly IPermissionControl _izinkontrol;


        public OperationsBomController(IUserService user, IProductOperationsBomRepository bom, IValidator<ProductOperationsBOMInsert> pBomInsert, IValidator<ProductOperationsBOMUpdate> pBomUpdate, IValidator<IdControl> pDelete, IOperationBomControl operationBomControl, IIDControl idcontrol, IOperationBomControl operationbomcontrol, IPermissionControl izinkontrol)
        {
            _user = user;
            _bom = bom;
            _PBomInsert = pBomInsert;
            _PBomUpdate = pBomUpdate;
            _PDelete = pDelete;
            _operationBomControl = operationBomControl;
            _idcontrol = idcontrol;
            _operationbomcontrol = operationbomcontrol;
            _izinkontrol = izinkontrol;
        }

        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<ProductOperationsBOMList>> List(int ItemId)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var list = await _bom.List(CompanyId,ItemId);

            return Ok(list);
        }
        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<ProductOperationsBOM>> Insert(ProductOperationsBOMInsert T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            
            ValidationResult result = await _PBomInsert.ValidateAsync(T);
            if (result.IsValid)
            {   
                var hata = await _operationbomcontrol.Insert(T, CompanyId);
                if (hata.Count() == 0)
                {
                    int id = await _bom.Insert(T, CompanyId);
                    return Ok("");
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
        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<ProductOperationsBOM>> Update(ProductOperationsBOMUpdate T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
           
            ValidationResult result = await _PBomUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata = await _operationBomControl.Update(T, CompanyId);
                if (hata.Count() == 0)
                {
                    await _bom.Update(T, CompanyId);
                    return Ok("Başarılı");
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
        public async Task<ActionResult<ProductOperationsBOM>> Delete(IdControl T)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            ValidationResult result = await _PDelete.ValidateAsync(T);
            if (result.IsValid)
            {
                          var hata = await _idcontrol.GetControl("ProductOperationsBom", T.id, CompanyId);
                if (hata.Count() == 0)
                {
                    await _bom.Delete(T, CompanyId);
                    return Ok("Başarılı");
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
