using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string Ad { get; set; } = null!;
        public string? Soyisim { get; set; }
        public string Telefon { get; set; } = null!;
        public string Mail { get; set; } = null!;
        public string Sifre { get; set; }
    }
}
