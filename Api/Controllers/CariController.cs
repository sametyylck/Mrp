using BL.Extensions;
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
using System.Data.Common;
using System.Runtime.CompilerServices;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.ItemDTO;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CariController : ControllerBase
    {
        private readonly IContactsRepository _contactsRepository;
        private readonly IDbConnection _db;
        private readonly IUserService _user;
        private readonly IValidator<ContactsInsert> _ContactsList;
        private readonly IValidator<ContactsDelete> _ContactsDelete;
        private readonly IValidator<ContactsUpdateAddress> _Contacts;
        private readonly IValidator<CariUpdate> _contactupdate;
        private readonly IContactControl _contactcontrol;
        private readonly IPermissionControl _izinkontrol;

        private readonly IIDControl _Idcontrol;



        public CariController(IContactsRepository contactsRepository, IUserService user, IDbConnection db, IValidator<ContactsDelete> contactsDelete, IValidator<ContactsUpdateAddress> contacts, IValidator<ContactsInsert> contactsList, IValidator<CariUpdate> contactupdate, IContactControl contactcontrol, IIDControl ıdcontrol, IPermissionControl izinkontrol)
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
            _izinkontrol = izinkontrol;
        }
        [Route("List")]
        [HttpPost, Authorize]
        public async Task<ActionResult<User>> List(ContactsFilters T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.IletisimGoruntule, Permison.IletisimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }

            var list = await _contactsRepository.List(T, KAYITSAYISI, SAYFA);
            var count = list.Count();

            return Ok(new { list, count });
        }
        [Route("Details")]
        [HttpGet, Authorize]
        public async Task<ActionResult<ContactsAll>> Details(int id)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.IletisimGoruntule, Permison.IletisimHepsi, UserId);
            if (izin == false)
            {       
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }

            var list = await _contactsRepository.Details(id);
            return (list.First());
        }
        [Route("CariTip")]
        [HttpGet, Authorize]
        public async Task<ActionResult<ContactsAll>> CariTip(int id)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.IletisimGoruntule, Permison.IletisimHepsi, UserId);
            if (izin == true)
            {
                string sql = $"Select * from CariTip";
                var list = await _db.QueryAsync<CariTip>(sql);
                return Ok(list);
            }
            else
            {
                List<string> hatalar = new();
                hatalar.Add("Yetkiniz yetersiz.");
                return BadRequest(hatalar);
            }

        }

        /// <summary>
        /// Cari ekleme sayfasi Tipe göre
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        [Route("Insert")]
        [HttpPost, Authorize]
        public async Task<ActionResult<ContactsList>> Insert(ContactsInsert T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.IletisimEkleyebilirVeDuzenleyebilir, Permison.IletisimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ContactsList.ValidateAsync(T);
            if (result.IsValid)
            {


                int id = await _contactsRepository.Insert(T, UserId);
                int billing = await _contactsRepository.InsertAddress("BillingAddress");
                int shipping = await _contactsRepository.InsertAddress("ShippingAddress");
                await _db.ExecuteAsync($"Update  Cari set  FaturaAdresId={shipping},KargoAdresId={billing} where CariKod={id}");

                var response = new ContactsFilters
                {
                    CariKod = id,
                    CariTipId = T.CariTipId,
                    ParaBirimiId = T.ParaBirimiId,
                    AdSoyad = T.AdSoyad,
                    Mail = T.Mail,
                    Telefon = T.Telefon,
                    VergiDairesi = T.VergiDairesi,
                    VergiNumarası=T.VergiNumarası

                };
                return Ok(response);

            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }

        }
        [Route("Update")]
        [HttpPut, Authorize]
        public async Task<ActionResult<ContactsAll>> Update(CariUpdate T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.IletisimEkleyebilirVeDuzenleyebilir, Permison.IletisimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _contactupdate.ValidateAsync(T);
            if (result.IsValid)
            {

                var hata = await _contactcontrol.Update(T);
                if (hata.Count() == 0)
                {
                    DynamicParameters prm = new DynamicParameters();
                    prm.Add("@id", T.CariKod); ;

                    await _contactsRepository.Update(T);
                    var response = new ContactsList
                    {
                        CariKod = T.CariKod,
                        CariTipId = T.CariTipId,
                        AdSoyad = T.AdSoyad,
                        Mail = T.Mail,
                        Telefon = T.Telefon,

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
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.IletisimEkleyebilirVeDuzenleyebilir, Permison.IletisimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _Contacts.ValidateAsync(T);
            if (result.IsValid)
            {


                var hata = await _contactcontrol.UpdateAddress(T);
                if (hata.Count() == 0)
                {
                    await _contactsRepository.UpdateAddress(T, T.id);
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
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.IletisimSil, Permison.IletisimHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _ContactsDelete.ValidateAsync(T);
            if (result.IsValid)
            {

                    await _contactsRepository.Delete(T);
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
