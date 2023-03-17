using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class OperitaonsDTO
    {
        public int id { get; set; }
        public string Isim { get; set; } = string.Empty;
    }
    public class OperationsInsert
    {
        public string Isim { get; set; } = string.Empty;

    }
    public class OperationsUpdate
    {
        public int id { get; set; }
        public string Isim { get; set; } = string.Empty;

    }
}
