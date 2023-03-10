using BL.Services.Bom;
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
using Microsoft.IdentityModel.Logging;
using System.Data;
using System.Xml.Linq;
using static DAL.DTO.BomDTO;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.ItemDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BomController : ControllerBase
    {
        private readonly IDbConnection _db;
        private readonly IUserService _user;
        private readonly IBomRepository _bom;
        private IValidator<BOMInsert> _BOMInsert;
        private IValidator<BOMUpdate> _BOMUpdate;
        private IValidator<IdControl> _BOMDelete;
        private readonly IBomControl _bomcontrol;
        private readonly IIDControl _idcontrol;

        public BomController(IDbConnection db, IUserService user, IBomRepository bom, IValidator<BOMInsert> bOMInsert, IValidator<BOMUpdate> bOMUpdate, IValidator<IdControl> bOMDelete, IBomControl bomcontrol, IIDControl idcontrol)
        {
            _db = db;
            _user = user;
            _bom = bom;
            _BOMInsert = bOMInsert;
            _BOMUpdate = bOMUpdate;
            _BOMDelete = bOMDelete;
            _bomcontrol = bomcontrol;
            _idcontrol = idcontrol;
        }

        [Route("ListMaterial")]
        [HttpGet, Authorize]
        public async Task<ActionResult<BOM>> ListMaterial(string Name = "")
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _db.QueryAsync<ListBomMaterial>($"Select TOP 10 id,Name,IsActive From Items where Tip = 'Material' and CompanyId = {CompanyId} and IsActive = 1 and Name LIKE '%{Name}%'");
            return Ok(list);
        }
        [Route("List")]
        [HttpPost, Authorize]
        public async Task<ActionResult<BOM>> List(int ProductId)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _bom.List(ProductId, CompanyId);
            return Ok(list);
        }

        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<BOM>> Insert(BOMInsert T)
        {

            ValidationResult result = await _BOMInsert.ValidateAsync(T);
            if (result.IsValid)
            {

                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                var hata = await _bomcontrol.Insert(T, CompanyId);
                if (hata.Count()==0)
                {
                    int id = await _bom.Insert(T, CompanyId);
                    var eklenen = await _db.QueryAsync<ListBOM>($@"Select Bom.id,Bom.Quantity,
                                                                ISNULL(Bom.Note,'') as Note,Bom.IsActive,
                                                                CAST((Bom.Quantity * a.DefaultPrice)as decimal(15,2)) as StockCost,
                                                                a.id as MaterialId,a.Name as MaterialName,i.id as ProductId From Bom 
                                                                inner join Items a on a.id = Bom.MaterialId
                                                                inner join Items i on i.id = Bom.ProductId
                                                                where Bom.CompanyId = {CompanyId} and Bom.id = {id}");
                    return Ok(eklenen);
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
        public async Task<ActionResult<BOM>> Update(BOMUpdate T)
        {
            ValidationResult result = await _BOMUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
           
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
               var hata=await _bomcontrol.Update(T, CompanyId);
                if (hata.Count()==0)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@CompanyId", CompanyId);
                    param1.Add("@id", T.id);

                    await _bom.Update(T, CompanyId);
                    var list = await _db.QueryAsync<ListBOM>($@"select Bom.id,Bom.ProductId,a.[Name] as ProductName,Bom.MaterialId,b.[Name] as MaterialName,Bom.Quantity,ISNULL(Bom.Note,'')as Note  from Bom left join Items a on a.id=Bom.ProductId  left join Items b on b.id=Bom.MaterialId where Bom.CompanyId=@CompanyId and Bom.id=@id", param1);
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

        [Route("Delete")]
        [HttpDelete, Authorize]
        public async Task<ActionResult<BOM>> Delete(IdControl T)
        {
            ValidationResult result = await _BOMDelete.ValidateAsync(T);

            if (result.IsValid)
            {
      
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                var hata =await _idcontrol.GetControl("Bom", T.id, CompanyId);
                if (hata.Count()!=0)
                {
                    return BadRequest(hata);

                }
                await _bom.Delete(T, CompanyId);
                return Ok("Başarılı");
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

        }


    }



}

