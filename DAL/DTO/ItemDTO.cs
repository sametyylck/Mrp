using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.CategoryDTO;
using static DAL.DTO.ContactDTO;

namespace DAL.DTO
{
    public class ItemDTO
    {
        public class Items
        {
            

            public int id { get; set; }
            public string? Tip { get; set; } = null!;


            public string? Name { get; set; } = null!;

            public int? CategoryId { get; set; }
            public int? MeasureId { get; set; }

            public int? ContactId { get; set; }

            public string? VariantCode { get; set; } = null!;

            //[MaxLength(15)]
            //Hata Verdiriyor
            public float? DefaultPrice { get; set; }

            public string? Info { get; set; } = null!;

            public int? CompanyId { get; set; }
        }
        public class ItemsInsert
        {
            public string? Tip { get; set; } = null!;


            public string? Name { get; set; } = null!;

            public int? CategoryId { get; set; }
            public int? MeasureId { get; set; }

            public int? ContactId { get; set; }

            public string? VariantCode { get; set; } = null!;

            //[MaxLength(15)]
            //Hata Verdiriyor
            public float? DefaultPrice { get; set; }

            public string? Info { get; set; } = null!;

        }
        public class ItemsUpdate
        {


            public int id { get; set; }
            public string? Tip { get; set; } = null!;


            public string? Name { get; set; } = null!;

            public int? CategoryId { get; set; }
            public int? MeasureId { get; set; }

            public int? ContactId { get; set; }

            public string? VariantCode { get; set; } = null!;

            //[MaxLength(15)]
            //Hata Verdiriyor
            public float? DefaultPrice { get; set; }

            public string? Info { get; set; } = null!;

            public int? CompanyId { get; set; }
        }


        public class ItemsDelete
        {

            public int id { get; set; }

            public string Tip { get; set; } = null!;
        }
        public class ListItems
        {
            public int id { get; set; }
            public string? Tip { get; set; }
            public string? Name { get; set; } 
            public int? CategoryId { get; set; }
            public string? CategoryName { get; set; }
            public int? MeasureId { get; set; }
            public string? MeasureName { get; set; } 
            public int? ContactId { get; set; }
            public string? ContactName { get; set; } 
            public string? VariantCode { get; set; }
            public float? DefaultPrice { get; set; }
            public float? IngredientsCost { get; set; }
            public float? OperationsCost { get; set; }
            public float? InStock { get; set; }
            public string? Info { get; set; } 
            public bool? IsActive { get; set; }

            public int? CompanyId { get; set; }
            public virtual CategoryClass? Category { get; set; }
        }

        public class ItemsListele
        {
            public int id { get; set; }
            public string? Tip { get; set; } = null!;
            public string? Name { get; set; } = null!;
            public string? VariantCode { get; set; } 
            public int? CategoryId { get; set; }
            public string? CategoryName { get; set; } 
            public float? DefaultPrice { get; set; }
            public float? Cost { get; set; }
            public float? Profit { get; set; }
            public float? Margin { get; set; }
            public string? ProductTime { get; set; } 
            public string? DisplayName { get; set; }
            public bool IsActive { get; set; }

            public virtual CategoryItemFilter Category { get; set; } = null!;
            public virtual ContactsItemFilter Contacts { get; set; } = null!;
        }



        public class costbul
        {
            public int MaterialId { get; set; }
            public float DefaultPrice { get; set; }
            public int Quantity { get; set; }

        }
    }
}
