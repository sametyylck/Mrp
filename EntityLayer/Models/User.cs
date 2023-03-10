using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Mail { get; set; } = null!;
        public string Password { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string? Role { get; set; }
        public string? CompanyId { get; set; } = null!;
        public IEnumerable<string> Roller { get; set; }
        public virtual Company? Company { get; set; }
    }
}
