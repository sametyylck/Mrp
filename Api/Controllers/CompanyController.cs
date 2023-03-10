using BL.Extensions;
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
    public class CompanyController : ControllerBase
    {
        private readonly IDbConnection _db;
        private readonly ICompanyRepository _company;
        private readonly IUserService _user;
        private readonly IValidator<IdControl> _IdControl;
        private readonly IValidator<CompanyUpdate> _CompanyUpdate;
        private readonly IValidator<CompanyUpdateCompany> _CompanyUpdateCompany;
        private readonly IIDControl _IDCONTROL;
        public CompanyController(ICompanyRepository company, IDbConnection db, IUserService user, IValidator<IdControl> ıdControl, IValidator<CompanyUpdate> companyUpdate, IValidator<CompanyUpdateCompany> companyUpdateCompany, IIDControl ıDCONTROL)
        {
            _company = company;
            _db = db;
            _user = user;
            _IdControl = ıdControl;
            _CompanyUpdate = companyUpdate;
            _CompanyUpdateCompany = companyUpdateCompany;
            _IDCONTROL = ıDCONTROL;
        }

        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<CompanyClas>> List(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _company.List(CompanyId);
            return (list.First());
        }

        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<CompanyUpdate>> Update(CompanyUpdate T)
        {
            ValidationResult result = await _CompanyUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                var hata =await _IDCONTROL.GetControl("Locations", T.id,CompanyId);
                if (hata.Count()==0)
                {
                    await _company.Update(T, CompanyId);
                    var list = new CompanyUpdate
                    {
                        AddressLine1 = T.AddressLine1,
                        AddressLine2 = T.AddressLine2,
                        CityTown = T.CityTown,
                        Country = T.Country,
                        StateRegion = T.StateRegion,
                        ZipPostalCode = T.ZipPostalCode
                    };
                    return (list);
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

        [HttpPut, Authorize]
        [Route("UpdateCompany")]
        public async Task<ActionResult<CompanyClas>> UpdateCompany(CompanyUpdateCompany T)
        {
            ValidationResult result = await _CompanyUpdateCompany.ValidateAsync(T);
            if (result.IsValid)
            {
              

                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                var hata = await _IDCONTROL.GetControl("Company", CompanyId, CompanyId);
                if (hata.Count() == 0)
                {
                    DynamicParameters prm = new DynamicParameters();
                    prm.Add("@id", CompanyId);
                    List<CompanyClas> list = (await _db.QueryAsync<CompanyClas>($"Select LocationId from Company where id = @id", prm)).ToList();
                    int locationId = list.First().LocationId;
                    await _company.UpdateCompany(T,CompanyId);
                    var company = new CompanyClas
                    {

                        DisplayName = T.DisplayName,
                        LegalName = T.LegalName
                    };
                    return (company);
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
        public async Task<ActionResult<CompanyClas>> Delete(IdControl T)
        {
            ValidationResult result = await _IdControl.ValidateAsync(T);
            if (result.IsValid)
            {

                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int User = user[1];

                var hata = await _IDCONTROL.GetControl("Locations", T.id, CompanyId);
                if (hata.Count() == 0)
                {
                await _company.Delete(T, CompanyId,User);

                }
                else
                {
                    return BadRequest(hata);
                }
                return Ok("başarılı");
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

        }
    }
}
