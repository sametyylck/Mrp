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
            public int? OperasyonId { get; set; }
            [Required]
            public int? KaynakId { get; set; }
            [Required]
            public float SaatlikUcret { get; set; }
            [Required]
            public float OperasyonZamani { get; set; }
            [Required]
            public int? StokId { get; set; }
            public bool Aktif { get; set; }
        }
        public class ProductOperationsBOMList
        {
            public int id { get; set; }
            public int? OperasyonId { get; set; }
            public string OperasyonIsmi { get; set; } = string.Empty;
            public int? KaynakId { get; set; }
            public string KaynakIsmi { get; set; } = string.Empty;
            public float? SaatlikUcret { get; set; }
            public float? OperasyonZamani { get; set; }
            public float? Tutar { get; set; }
            public bool Aktif { get; set; }
            public int? StokId { get; set; }
        }
        public class ProductOperationsBomOperation
        {

            public int? id { get; set; }
            public string Isim { get; set; } = string.Empty;
            public bool Aktif { get; set; }
        }
        public class ProductOperationsBomControl
        {
            public int varmi { get; set; }
        }
        public class ProductOperationsBOMInsert
        {

            public int? OperasyonId { get; set; }
            public int? KaynakId { get; set; }
            public float? SaatlikUcret { get; set; }
            public float OperasyonZamani { get; set; }
            public int? StokId { get; set; }
        }
        public class ProductOperationsBOMUpdate
        {
            public int id { get; set; }
            public int? OperasyonId { get; set; }
            public int? KaynakId { get; set; }      
            public float? SaatlikUcret { get; set; }     
            public float OperasyonZamani { get; set; }  

        }
    }
}
