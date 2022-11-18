using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class ProductOperationsBomDTO
    {
        public class ProductOperationsBOM
        {
            public int id { get; set; }
            [Required]
            public int? OperationId { get; set; }
            [Required]
            public int? ResourceId { get; set; }
            [Required]
            public float? CostHour { get; set; }
            [Required]
            public float OperationTime { get; set; }
            [Required]
            public int? ItemId { get; set; }
            public bool IsActive { get; set; }
            public int? CompanyId { get; set; }
        }
        public class ProductOperationsBOMList
        {
            public int id { get; set; }
            public int? OperationId { get; set; }
            public string OperationName { get; set; } = string.Empty;
            public int? ResourceId { get; set; }
            public string ResourcesName { get; set; } = string.Empty;
            public float? CostHour { get; set; }
            public int? OperationTime { get; set; }
            public float? Cost { get; set; }
            public bool IsActive { get; set; }
            public int? ItemId { get; set; }
            public int? CompanyId { get; set; }
        }
        public class ProductOperationsBomOperation
        {

            public int? id { get; set; }
            public string Name { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }
        public class ProductOperationsBomControl
        {
            public int varmi { get; set; }
        }
        public class ProductOperationsBOMInsert
        {

            public int? OperationId { get; set; }
            public int? ResourceId { get; set; }
            public float? CostHour { get; set; }
            public float OperationTime { get; set; }
            public int? ItemId { get; set; }
        }
        public class ProductOperationsBOMUpdate
        {
            public int id { get; set; }
            public int? OperationId { get; set; }
            public int? ResourceId { get; set; }      
            public float? CostHour { get; set; }     
            public float OperationTime { get; set; }  

        }
    }
}
