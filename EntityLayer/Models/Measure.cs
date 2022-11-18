using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class MeasureClas
    {
        public MeasureClas()
        {
            Items = new HashSet<Item>();
        }

        public int id { get; set; }
        public string Name { get; set; } = null!;
        public int? CompanyId { get; set; }

        public virtual Company? Company { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }
}
