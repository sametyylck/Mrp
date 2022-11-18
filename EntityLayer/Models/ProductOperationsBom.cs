using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class ProductOperationsBom
    {
        public int Id { get; set; }
        public int? OperationId { get; set; }
        public int? ResourceId { get; set; }
        public double? CostHour { get; set; }
        public int? OperationTime { get; set; }
        public int? ItemId { get; set; }
        public bool? IsActive { get; set; }
        public int? CompanyId { get; set; }

        public virtual Company? Company { get; set; }
        public virtual Item? Item { get; set; }
        public virtual Operation? Operation { get; set; }
        public virtual Resource? Resource { get; set; }
    }
}
