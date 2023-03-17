using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.PurchaseOrderDTO;

namespace DAL.DTO
{
    public class BomDTO
    {
        public class BOMInsert
        {
          
            public int MamulId { get; set; }
           
            public int MalzemeId { get; set; }
      
            public float Miktar { get; set; }
         
            public string? Bilgi { get; set; }
            public string? Tip { get; set; }
        }
        public class BOMUpdate
        {

            public int id { get; set; }

            public int MamulId { get; set; }

            public int MalzemeId { get; set; }

            public float Miktar { get; set; }

            public string? Bilgi { get; set; }
            public string? Tip { get; set; }


        }
        public class BOM
        {

            public int id { get; set; }

            public int MamulId { get; set; }

            public int MalzemeId { get; set; }

            public float Miktar { get; set; }

            public string? Bilgi { get; set; }


        }

        public class BOMDelete
        {
            public int id { get; set; }
        }

        public class ListBomMaterial
        {

            public int? id { get; set; }
            public string Isim { get; set; } = string.Empty;
        }
        public class ListBOM
        {
            public int id { get; set; }
            public int? MamulId { get; set; }
            public string MamulIsmi { get; set; }
            public int? MalzemeId { get; set; }
            public string MalzemeIsmi { get; set; } = string.Empty;
            public int? Miktar { get; set; }
            public string Bilgi { get; set; } = string.Empty;
            public float? Tutar { get; set; }
            public bool Aktif { get; set; }
        }
        public class MissingCount
        {
            [Required]
            public int id { get; set; }
            public int DepoId { get; set; }
            public int SatisDetayId { get; set; }
            [Required]
            public int MamulId { get; set; }
            public int MalzemeId { get; set; }
            public string MalzemeIsim { get; set; } = string.Empty;
            public int Kayıp { get; set; }
            public List<PurchaseOrdersItemDetails> PurchaseOrderList { get; set; } 


        }
    }
}
