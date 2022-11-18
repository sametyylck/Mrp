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

        public OrderStockController(IUserService user, IDbConnection db, IOrderStockRepository orderStockRepository, IValidator<PurchaseOrderId> purchase, IIDControl idcontrol)
        {
            _user = user;
            _db = db;
            _orderStockRepository = orderStockRepository;
            _Purchase = purchase;
            _idcontrol = idcontrol;
        }
        [Route("OrdersStock")]
        [HttpPost, Authorize]
        public async Task<ActionResult<PurchaseOrderId>> OrdersStock(PurchaseOrderId T)
        {
            ValidationResult result = await _Purchase.ValidateAsync(T);
            if (result.IsValid)
            {
                List<int> user = _user.CompanyId();
                int CompanyId = user[0];
                int User = user[1];
                string hata =await _idcontrol.GetControl("Orders", T.id, CompanyId);
                if (hata=="true")
                {
                    await _orderStockRepository.StockUpdate(T, CompanyId, User);
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
            int CompanyId = user[0];
            var list = await _orderStockRepository.List(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = await _orderStockRepository.Count(T, CompanyId);
            return Ok(new { list, count });
        }

        [Route("DoneList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<PurchaseOrderLogsList>> DoneList(PurchaseOrderLogsList T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _orderStockRepository.DoneList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = await _orderStockRepository.DoneCount(T, CompanyId);
            return Ok(new { list, count });
        }
    }
}
