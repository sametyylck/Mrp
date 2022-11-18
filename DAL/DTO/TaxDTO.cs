using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class TaxDTO
    {
        public class TaxClas
        {
            public int id { get; set; }
            public float Rate { get; set; }
            public string TaxName { get; set; } = string.Empty;
            public int? CompanyId { get; set; }
        }
        public class TaxInsert
        {
            public float Rate { get; set; }
            public string TaxName { get; set; } = string.Empty;

        }
        public class TaxUpdate
        {
            public int id { get; set; }
            public float Rate { get; set; }
            public string TaxName { get; set; } = string.Empty;
        }
    }
}
