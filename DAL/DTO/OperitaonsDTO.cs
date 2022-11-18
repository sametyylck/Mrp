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
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? CompanyId { get; set; }
    }
    public class OperationsInsert
    {
        public string Name { get; set; } = string.Empty;

    }
    public class OperationsUpdate
    {
        public int id { get; set; }
        public string Name { get; set; } = string.Empty;

    }
}
