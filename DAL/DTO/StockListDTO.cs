using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.SalesOrderDTO;

namespace DAL.DTO
{
    public class StockListDTO
    {
        public class StockList
        {
            public int id { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public int? InStock { get; set; }
            public string Tip { get; set; } = string.Empty;
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public int ContactId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public int locationId { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public string VariantCode { get; set; } = string.Empty;
            public float? AverageCost { get; set; }
            public float? Expected { get; set; }
            public float? ValueInStock { get; set; }
            public float? Committed { get; set; }
            public float? Missing { get; set; }


        }
        public class StockListAll
        {
            public int id { get; set; }
            public string? ItemName { get; set; }
            public int? InStock { get; set; }
            public string? Tip { get; set; }
            public int CategoryId { get; set; }
            public string? CategoryName { get; set; }
            public int ContactId { get; set; }
            public string? SupplierName { get; set; }
            public int? locationId { get; set; }
            public string? LocationName { get; set; }
            public string? VariantCode { get; set; }
            public float? AverageCost { get; set; }
            public float? MaterialExpected { get; set; }
            public float? ProductExpected { get; set; }
            public float? ValueInStock { get; set; }
            public float? MaterailCommitted { get; set; }
            public float? Missing { get; set; }


        }

        public class SatısList
        {
            public int? id { get; set; }
            public int? DepoId { get; set; }
            public string? SatisIsmi  { get; set; }
            public int? CariId { get; set; }
            public string? CariAdSoyad { get; set; }
            public float? TumToplam { get; set; }
            public DateTime TeslimSuresi { get; set; }
            public int? SatisOgesi { get; set; }
            public int? Malzemeler { get; set; }
            public int? Uretme { get; set; }
            public int? DurumBelirteci { get; set; }
            public string? DepoIsmi { get; set; }
            public DateTime? BaslangıcTarih { get; set; }
            public DateTime? SonTarih { get; set; }

            public IEnumerable<ManufacturingOrderDetail>? MOList { get; set; }
        }
        public class SatısListFiltre
        {
            public int? DepoId { get; set; }
            public string? SatisIsmi { get; set; }
            public string? CariAdSoyad { get; set; }
            public float? TumToplam { get; set; }
            public int? SatisOgesi { get; set; }
            public int? Malzemeler { get; set; }
            public int? Uretme { get; set; }
            public int? DurumBelirteci { get; set; }
            public string? BaslangıcTarih { get; set; }
            public string? SonTarih { get; set; }
        }

        public class SatısDetail
        {
            public int id { get; set; }
            public string? Tip { get; set; }

            public int StokId { get; set; }
            public string? UrunIsmi { get; set; }
            public float Miktar { get; set; }
            public float BirimFiyat { get; set; }
            public int VergiId { get; set; }
            public string? VergiIsim { get; set; }
            public float VergiDegeri { get; set; }
            public float? TumToplam { get; set; }
            public int? SatisOgesi { get; set; }
            public int? Malzemeler { get; set; }
            public int? Uret { get; set; }
            public int Kayıp { get; set; }

        }

        public class IngredientMis
        {
            [Required]
            public int id { get; set; }
            public int DepoId { get; set; }
            public int SatisDetayId { get; set; }
            [Required]
            public int MamulId { get; set; }


        }
        public class QuotesDone
        {
            public int id { get; set; }
            public int? Quotes { get; set; }
            public int DepoId { get; set; }
            public int CariId { get; set; }

        }


    }
}
