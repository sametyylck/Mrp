using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using static DAL.DTO.StockListDTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IDbConnection _db;
        private readonly IStockRepository _stock;
        public StockController(IUserService user, IStockRepository stock, IDbConnection db)
        {
            _user = user;
            _stock = stock;
            _db = db;
        }
        [Route("MaterialList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockList>> MaterialList(StockList T,int KAYITSAYISI,int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            DynamicParameters prm = new DynamicParameters();
            var list = await _stock.MaterialList(T, CompanyId,KAYITSAYISI,SAYFA);
            var count = await _stock.MaterialCount(T, CompanyId);
            return Ok(new { list, count });

        }

        [Route("ProductList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockList>> ProductList(StockList T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            DynamicParameters prm = new DynamicParameters();
            var list = await _stock.ProductList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = await _stock.ProductCount(T, CompanyId);
            return Ok(new { list, count });

        }

        [Route("AllItemsList")]
        [HttpPost, Authorize]
        public async Task<ActionResult<StockListAll>> AllItemsList(StockListAll T, int KAYITSAYISI, int SAYFA)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            DynamicParameters prm = new DynamicParameters();
            var list = await _stock.AllItemsList(T, CompanyId, KAYITSAYISI, SAYFA);
            var count = await _stock.AllItemsCount(T, CompanyId);
            return Ok(new { list, count });

        }

    }
}
