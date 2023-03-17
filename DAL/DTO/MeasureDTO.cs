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
        public string Isim { get; set; } = null!;
    }
    public class MeasureUpdate
    {
        public int id { get; set; }
        public string Isim { get; set; } = null!;
    }
    public class MeasureInsert
    {
        public string Isim { get; set; } = null!;
    }

}
