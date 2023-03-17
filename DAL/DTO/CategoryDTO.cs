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

            public string Isim { get; set; } = string.Empty;

        }

        public class CategoryInsert
        {  
            public string Isim { get; set; } = string.Empty;

        }
        public class CategoryUpdate
        {
            public int id { get; set; }

            public string Isim { get; set; } = string.Empty;


        }
        public class CategoryDelete
        {
            public int id { get; set; }

        }




        public class CategoryItemFilter
        {
            public string? Isim { get; set; }
        }
    }
}
