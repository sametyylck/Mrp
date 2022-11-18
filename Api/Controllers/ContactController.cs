using BL.Services.Contact;
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
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.ItemDTO;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactsRepository _contactsRepository;
        private readonly IDbConnection _db; 
        private readonly IUserService _user;
        private readonly IValidator<ContactsInsert> _ContactsList;
        private readonly IValidator<ContactsDelete> _ContactsDelete;
        private readonly IValidator<ContactsUpdateAddress> _Contacts;
        private readonly IValidator<ContactsList> _contactupdate;
        private readonly IContactControl _contactcontrol;

        private readonly IIDControl _Idcontrol;



        public ContactController(IContactsRepository contactsRepository, IUserService user, IDbConnection db, IValidator<ContactsDelete> contactsDelete, IValidator<ContactsUpdateAddress> contacts, IValidator<ContactsInsert> contactsList, IValidator<ContactsList> contactupdate, IContactControl contactcontrol, IIDControl ıdcontrol)
        {
            _contactsRepository = contactsRepository;
            _user = user;
            _db = db;
            _ContactsDelete = contactsDelete;
            _Contacts = contacts;
            _ContactsList = contactsList;
            _contactupdate = contactupdate;
            _contactcontrol = contactcontrol;
            _Idcontrol = ıdcontrol;
        }
        [Route("List")]
        [HttpPost,Authorize]
        public async Task<ActionResult<User>> List(ContactsFilters T,int KAYITSAYISI,int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list =await _contactsRepository.List(T, CompanyId, KAYITSAYISI, SAYFA);
            var count =await _contactsRepository.Count(T, CompanyId);

            return Ok(new {list,count});
        }
        [Route("Details")]
        [HttpGet, Authorize]
        public async Task<ActionResult<ContactsAll>> Details(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _contactsRepository.Details(id, CompanyId);
            return (list.First());
        }
        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<ContactsList>> Insert(ContactsInsert T)
        {

            ValidationResult result = await _ContactsList.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                if (T.Tip == "Customer")
                {
                    int id = await _contactsRepository.Insert(T, CompanyId);
                    int billing = await _contactsRepository.InsertAddress(CompanyId, "BillingAddress");
                    int shipping = await _contactsRepository.InsertAddress(CompanyId, "ShippingAddress");
                    await _db.ExecuteAsync($"Update  Contacts set  ShippingLocationId={shipping},BillingLocationId={billing} where id={id} and CompanyId={CompanyId}");

                    var response = new ContactsFilters
                    {
                        id = id,
                        Tip = T.Tip,
                        DisplayName = T.DisplayName,
                        Mail = T.Mail,
                        Phone = T.Phone,
                        Comment = T.Comment,
                        FirstName = T.FirstName,
                        LastName = T.LastName,

                    };
                    return Ok(response);
                }
                else if (T.Tip == "Supplier")
                {
                    int id = await _contactsRepository.Insert(T, CompanyId);
                    var response = new ContactsFilters
                    {
                        id = id,
                        Mail = T.Mail,
                        Comment = T.Comment,


                    };
                    return Ok(response);
                }

                return BadRequest("Hatalı tip değişkeni.");

            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
            
        }
        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<ContactsAll>> Update(ContactsList T)
        {
            ValidationResult result = await _contactupdate.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                var hata =await _contactcontrol.Update(T, CompanyId);
                if (hata=="true")
                {
                    DynamicParameters prm = new DynamicParameters();
                    prm.Add("@id", T.id); ;
                    prm.Add("@CompanyId", CompanyId);

                    await _contactsRepository.Update(T, CompanyId, T.id);
                    var response = new ContactsList
                    {
                        id = T.id,
                        FirstName = T.FirstName,
                        LastName = T.LastName,
                        CompanyName = T.CompanyName,
                        DisplayName = T.DisplayName,
                        Mail = T.Mail,
                        Phone = T.Phone,
                        Comment = T.Comment,

                    };
                    return Ok(response);
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
        /// adres bilgileri,billing ve shipping adress id
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        /// 
        [Route("UpdateAddress")]
        [HttpPut, Authorize]
        public async Task<ActionResult<Contacts>> UpdateAddress(ContactsUpdateAddress T)
        {

            ValidationResult result = await _Contacts.ValidateAsync(T);
            if (result.IsValid)
            {

                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                var hata = await _contactcontrol.UpdateAddress(T, CompanyId);
                if (hata=="true")
                {
                    await _contactsRepository.UpdateAddress(T, CompanyId, T.id);
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

        /// <summary>
        /// id,Tip
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        /// 
        [Route("Delete")]
        [HttpDelete, Authorize]
        public async Task<ActionResult<ContactsDelete>> Delete(ContactsDelete T)
        {
            ValidationResult result = await _ContactsDelete.ValidateAsync(T);
            if (result.IsValid)
            {
                string tabloadi= "Contacts";
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                var hata=await _Idcontrol.GetControl(tabloadi, T.id, CompanyId);
                if (hata=="true")
                {

                    await _contactsRepository.Delete(T, CompanyId);
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
