﻿using BL.Extensions;
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


        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<CompanyUpdate>> Update(CompanyUpdate T)
        {
            ValidationResult result = await _CompanyUpdate.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int UserId = user[1];
                var hata =await _IDCONTROL.GetControl("Locations", T.id);
                if (hata.Count()==0)
                {
                    await _company.Update(T, UserId);
                    var list = new CompanyUpdate
                    {
                        Adres1 = T.Adres1,
                        Adres2 = T.Adres2,
                        Sehir = T.Sehir,
                        Ulke = T.Ulke,
                        Cadde = T.Cadde,
                        PostaKodu = T.PostaKodu
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

    }
}
