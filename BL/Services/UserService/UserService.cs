using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using DAL.DTO;

namespace BL.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public  List<int> CompanyId()
        {
            List<int> result = new List<int>();
      
            if ( _httpContextAccessor != null)
            {
                result.Add( Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.GivenName)));
                result.Add( Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Gender)));
               
            }
            return result;
        }

       
    }
}
