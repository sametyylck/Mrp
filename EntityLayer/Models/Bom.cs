using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Bom
    {
        public int id { get; set; }
        public int? ProductId { get; set; }
        public int? MaterialId { get; set; }
        public int? Quantity { get; set; }
        public string? Note { get; set; }
        public bool? IsActive { get; set; }
        public int? CompanyId { get; set; }

        public virtual Company? Company { get; set; }
        public virtual Item? Product { get; set; }
    }
}
