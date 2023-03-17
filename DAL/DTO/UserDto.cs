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
        public string Mail { get; set; }
        public string Sifre { get; set; }
    }
    public partial class Users
    {
        public int Id { get; set; }
        public string Mail { get; set; } = null!;
        public string? Password { get; set; }
        public string? CompanyId { get; set; } = null!;
    }
    public class UserInsert
    {
        public int RoleId { get; set; }
        public string Mail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public string PhoneNumber { get; set; } = null!;
    }
    public class RoleInsert
    {
        public string RoleName { get; set; }
        public List<int> PermisionId { get; set; }
    }
    public class RoleUpdate
    {
        public int id { get; set; }
        public string RoleName { get; set; }

    }
    public class RoleList
    {
        public int id { get; set; }
        public string RoleName { get; set; }
        public List<string> PermisionName { get; set; }
    }
    public class PermisionInsert
    {
        public int RoleId { get; set; }
        public List<int> PermisionId { get; set; }
    }
    public class PermisionDelete
    {
        public int RoleId { get; set; }
        public List<int> PermisionId { get; set; }
    }
    public class PermisionDetay
    {
        public int id { get; set; }
        public int IzinId { get; set; }
        public string IzinIsmi { get; set; }
    }
    public class RoleDetay
    {
        public int id { get; set; }
        public string RoleName { get; set; }
        public IEnumerable<PermisionDetay> Permision { get; set; }
    }


    public class UserUpdate
    {
        public int id { get; set; }
        public int RoleId { get; set; }
        public string Mail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public string PhoneNumber { get; set; } = null!;

    }
    public class UserDetail
    {
        public int id { get; set; }
        public string Mail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public IEnumerable<PermisionDetay> Permision { get; set; }
    }


}
