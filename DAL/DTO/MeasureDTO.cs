using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class MeasureDTO
    {
        public int id { get; set; }
        public string Name { get; set; } = null!;
        public int? CompanyId { get; set; }
    }
    public class MeasureUpdate
    {
        public int id { get; set; }
        public string Name { get; set; } = null!;
    }
    public class MeasureInsert
    {
        public string Name { get; set; } = null!;
    }

}
