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


            public string? Isim { get; set; } = null!;

            public int? KategoriId { get; set; }
            public int? OlcuId { get; set; }

            public int? TedarikciId { get; set; }

            public string? StokKodu { get; set; } = null!;

            //[MaxLength(15)]
            //Hata Verdiriyor
            public float? VarsayilanFiyat { get; set; }

            public string? Bilgi { get; set; } = null!;

        }
        public class ItemsUpdate
        {


            public int id { get; set; }
            public string? Tip { get; set; } = null!;


            public string? Isim { get; set; } = null!;

            public int? KategoriId { get; set; }
            public int? OlcuId { get; set; }

            public int? TedarikciId { get; set; }

            public string? StokKodu { get; set; } = null!;

            //[MaxLength(15)]
            //Hata Verdiriyor
            public float? VarsayilanFiyat { get; set; }

            public string? Bilgi { get; set; } = null!;

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
            public string? Isim { get; set; } 
            public int? KategoriId { get; set; }
            public string? KategoriIsmi { get; set; }
            public int? OlcuId { get; set; }
            public string? OlcuIsmi { get; set; } 
            public int? TedarikciId { get; set; }
            public string? TedarikciIsmi { get; set; } 
            public string? StokKodu { get; set; }
            public float? VarsayilanFiyat { get; set; }
            public float? MalzemeTutarı { get; set; }
            public float? OperasyonTutarı { get; set; }
            public float? StokAdet { get; set; }
            public string? Bilgi { get; set; } 
            public bool? Aktif { get; set; }
            public virtual CategoryClass? Category { get; set; }
        }

        public class ItemsListele
        {
            public int id { get; set; }
            public string? Tip { get; set; } = null!;
            public string? Isim { get; set; } = null!;
            public string? StokKodu { get; set; } 
            public int? KategoriId { get; set; }
            public string? KategoriIsmi { get; set; } 
            public float? VarsayilanFiyat { get; set; }
            public float? Maliyet { get; set; }
            public float? Kar { get; set; }
            public float? Margin { get; set; }
            public float? UrunZamani { get; set; } 
            public string? GorunenIsim { get; set; }
            public bool Aktif { get; set; }
            public string TedarikciIsim { get; set; }

        }



        public class costbul
        {
            public int MalzemeId { get; set; }
            public float VarsayilanFiyat { get; set; }
            public int Miktar { get; set; }

        }
    }
}
