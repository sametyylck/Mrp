using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Resource
    {
        public Resource()
        {
            ProductOperationsBoms = new HashSet<ProductOperationsBom>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public double? DefaultCostHour { get; set; }
        public int? CompanyId { get; set; }
        public bool? IsActive { get; set; }

        public virtual Company? Company { get; set; }
        public virtual ICollection<ProductOperationsBom> ProductOperationsBoms { get; set; }
    }
}
