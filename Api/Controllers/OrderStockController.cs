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
using static DAL.DTO.PurchaseOrderDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderStockController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IDbConnection _db;
        private readonly IOrderStockRepository _orderStockRepository;
        private readonly IValidator<PurchaseOrderId> _Purchase;
        private readonly IIDControl _idcontrol;
        private readonly IPermissionControl _izinkontrol;

        public OrderStockController(IUserService user, IDbConnection db, IOrderStockRepository orderStockRepository, IValidator<PurchaseOrderId> purchase, IIDControl idcontrol, IPermissionControl izinkontrol)
        {
            _user = user;
            _db = db;
            _orderStockRepository = orderStockRepository;
            _Purchase = purchase;
            _idcontrol = idcontrol;
            _izinkontrol = izinkontrol;
        }
        [Route("OrdersStock")]
        [HttpPost, Authorize]
        public async Task<ActionResult<PurchaseOrderId>> OrdersStock(PurchaseOrderId T)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatinAlmaTamamlama, Permison.SatinAlmaHepsi,UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            ValidationResult result = await _Purchase.ValidateAsync(T);
            if (result.IsValid)
            {
                var hata =await _idcontrol.GetControl("SatinAlma", T.id);
                if (hata.Count() == 0)
                {
                    await _orderStockRepository.StockUpdate(T,UserId);
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
        [Route("List")]
        [HttpPost, Authorize]
        public async Task<ActionResult<PurchaseOrderLogsList>> List(PurchaseOrderLogsList T,int KAYITSAYISI,int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatinAlmaGoruntule, Permison.SatinAlmaHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _orderStockRepository.List(T, KAYITSAYISI, SAYFA);
            var count = list.Count();
            return Ok(new { list, count });
        }

        [Route("DoneList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<PurchaseOrderLogsList>> DoneList(PurchaseOrderLogsList T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int UserId = user[1];
            var izin = await _izinkontrol.Kontrol(Permison.SatinAlmaGoruntule, Permison.SatinAlmaHepsi, UserId);
            if (izin == false)
            {
                List<string> izinhatasi = new();
                izinhatasi.Add("Yetkiniz yetersiz");
                return BadRequest(izinhatasi);
            }
            var list = await _orderStockRepository.DoneList(T,KAYITSAYISI, SAYFA);
            var count = list.Count();
            return Ok(new { list, count });
        }
    }
}
