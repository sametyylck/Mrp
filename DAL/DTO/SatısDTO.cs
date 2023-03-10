using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class SatısDTO
    {


        public string? Tip { get; set; }
        public int? ContactId { get; set; }
        public string OrderName { get; set; } = string.Empty;
        public DateTime? DeliveryDeadline { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? LocationId { get; set; }
        public string Info { get; set; } = string.Empty;

    }
    public class SatısInsertItem
    {
        public int SalesOrderId { get; set; }
        public int? ItemId { get; set; }
        public int? LocationId { get; set; }
        public int? ContactId { get; set; }
        public int? TaxId { get; set; }
        public float? Quantity { get; set; }
        public int Status { get; set; }
        public int Conditions { get; set; }

    }
    public class TeklifInsertItem
    {
        public int SalesOrderId { get; set; }
        public int? ItemId { get; set; }
        public int? LocationId { get; set; }
        public int? ContactId { get; set; }
        public int? TaxId { get; set; }
        public float? Quantity { get; set; }

    }
    public class TeklifUpdateItems
    {
        public int id { get; set; }
        public int SalesOrderId { get; set; }
        public int ContactId { get; set; }
        public int LocationId { get; set; }
        public int ItemId { get; set; }
        public float Quantity { get; set; }
        public float PricePerUnit { get; set; }
        public int TaxId { get; set; }
        public DateTime DeliveryDeadline { get; set; }
        public string? Note { get; set; }
    }


    public class SatısDelete
    {
        public int id { get; set; }
    }
    public class SatısDeleteItems
    {
        public int id { get; set; }
        public int ItemId { get; set; }
        public int OrdersId { get; set; }
    }
    public class SatısUpdateItems
    {
        public int id { get; set; }
        public string? Tip { get; set; }
        public int SalesOrderId { get; set; }
        public int ManufacturingOrderId { get; set; }
        public int ContactId { get; set; }
        public int LocationId { get; set; }
        public int ItemId { get; set; }
        public float Quantity { get; set; }
        public float PricePerUnit { get; set; }
        public int TaxId { get; set; }
        public int Conditions { get; set; }
        public DateTime DeliveryDeadline { get; set; }
        public string? Note { get; set; }
    }




}
