using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class ResourcesDTO
    {
        public int id { get; set; }

        public string Name { get; set; } = string.Empty;

        public double? DefaultCostHour { get; set; }
    }
    public class ResourcesInsert
    {

        public string Name { get; set; } = string.Empty;

        public double? DefaultCostHour { get; set; }

    }
    public class ResourcesUpdate
    {
        public int id { get; set; }

        public string Name { get; set; } = string.Empty;

        public double? DefaultCostHour { get; set; }

    }
}
