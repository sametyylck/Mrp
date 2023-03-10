using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingCurrencyController : ControllerBase
    {
        private readonly ICurrencyRepository _currren;
     private readonly   IUserService _user;

        public SettingCurrencyController(IUserService userService, ICurrencyRepository currren)
        {

            _user = userService;
            _currren = currren;
        }
        [Route("List")]
        [HttpGet, Authorize]
        public async Task<ActionResult<CurrencyDTO>> List(string? kelime)
        {
            List<int> user = _user.CompanyId();
            int CompanyId = user[0];
            var list = await _currren.List(kelime);
            return Ok(list);
        }
    }
}
