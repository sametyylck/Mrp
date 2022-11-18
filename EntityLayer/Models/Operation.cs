using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Operation
    {
        public Operation()
        {
            ProductOperationsBoms = new HashSet<ProductOperationsBom>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool? IsActive { get; set; }
        public int? CompanyId { get; set; }

        public virtual Company? Company { get; set; }
        public virtual ICollection<ProductOperationsBom> ProductOperationsBoms { get; set; }
    }
}
