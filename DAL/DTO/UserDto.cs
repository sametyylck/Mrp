using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class UserDto
    {
        public string Mail { get; set; }=string.Empty;
        public string Password { get; set; }= string.Empty;
        public int CompanyId { get; set; }
    }
    public partial class Users
    {
        public int Id { get; set; }
        public string Mail { get; set; } = null!;
        public string? Password { get; set; }
        public string? CompanyId { get; set; } = null!;
    }

}
