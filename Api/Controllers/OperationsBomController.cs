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


        public OperationsBomController(IUserService user, IProductOperationsBomRepository bom, IValidator<ProductOperationsBOMInsert> pBomInsert, IValidator<ProductOperationsBOMUpdate> pBomUpdate, IValidator<IdControl> pDelete, IOperationBomControl operationBomControl, IIDControl idcontrol, IOperationBomControl operationbomcontrol)
        {
            _user = user;
            _bom = bom;
            _PBomInsert = pBomInsert;
            _PBomUpdate = pBomUpdate;
            _PDelete = pDelete;
            _operationBomControl = operationBomControl;
            _idcontrol = idcontrol;
            _operationbomcontrol = operationbomcontrol;
        }

        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<ProductOperationsBOMList>> List(int ItemId)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _bom.List(CompanyId,ItemId);

            return Ok(list);
        }
        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<ProductOperationsBOM>> Insert(ProductOperationsBOMInsert T)
        {
            ValidationResult result = await _PBomInsert.ValidateAsync(T);
            if (result.IsValid)
            {

                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _operationbomcontrol.Insert(T, CompanyId);
                if (hata == "true")
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
            ValidationResult result = await _PBomUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _operationBomControl.Update(T, CompanyId);
                if (hata=="true")
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
            ValidationResult result = await _PDelete.ValidateAsync(T);
            if (result.IsValid)
            {
          
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                string hata = await _idcontrol.GetControl("ProductOperationsBom", T.id, CompanyId);
                if (hata == "true")
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
