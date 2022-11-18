using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class CategoryDTO
    {
        public class CategoryClass
        {
            public int id { get; set; }

            public string Name { get; set; } = string.Empty;

            public int? CompanyId { get; set; }
        }

        public class CategoryInsert
        {  
            public string Name { get; set; } = string.Empty;

            public int? CompanyId { get; set; }
        }
        public class CategoryUpdate
        {
            public int id { get; set; }

            public string Name { get; set; } = string.Empty;

            public int? CompanyId { get; set; }

        }
        public class CategoryDelete
        {
            public int id { get; set; }

        }




        public class CategoryItemFilter
        {
            public string? Name { get; set; }
        }
    }
}
