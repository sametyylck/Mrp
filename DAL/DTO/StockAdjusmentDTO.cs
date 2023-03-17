﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class StockAdjusmentDTO
    {
        public class StockAdjusmentClas
        {
            public int id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public int LocationId { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public string Info { get; set; } = string.Empty;
            public IEnumerable<StockAdjusmentItems> detay { get; set; }
        }
        public class StockAdjusmentUpdate
        {
            public int? id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public int? LocationId { get; set; }
            public string Info { get; set; } = string.Empty;
        }
        public class StockAdjusmentItems
        {
            public int id { get; set; }
            public int ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public float Adjusment { get; set; }
            public float CostPerUnit { get; set; }
            public int StockAdjusmentId { get; set; }
            public float AdjusmentValue { get; set; }
            public int InStock { get; set; }
        }
        public class StockAdjusmentUpdateItems
        {
            public int? id { get; set; }
            public int ItemId { get; set; }
            public float Adjusment { get; set; }
            public float CostPerUnit { get; set; }
            public int? StockAdjusmentId { get; set; }
        }
        public class StockAdjusmentAll
        {
            public int id { get; set; }
            public int? StockTakesId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public int LocationId { get; set; }
            public bool IsActive { get; set; }
            public string Info { get; set; } = string.Empty;
            public float? Total { get; set; }
            public int ItemId { get; set; }
            public float Adjusment { get; set; }
            public float CostPerUnit { get; set; }
            public int StockAdjusmentId { get; set; }
            public float AdjusmentValue { get; set; }

        }
        public class StockAdjusmentInsert
        {
            public int? StockTakesId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public int? LocationId { get; set; }
            public string Info { get; set; } = string.Empty;

        }
        public class StockAdjusmentInsertItem
        {
            public int? LocationId { get; set; }
            public int ItemId { get; set; }
            public float? Adjusment { get; set; }
            public float? CostPerUnit { get; set; }
            public int StockAdjusmentId { get; set; }

        }
        public class StockAdjusmentList
        {
            public int id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public float? Total { get; set; }
            public string LocationName { get; set; } = string.Empty;

        }

        public class StockAdjusmentInsertResponse
        {
            public int StockAdjusmentId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public int LocationId { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public string Info { get; set; } = string.Empty;
       }
        public class StockAdjusmentItemDelete
        {
            public int id { get; set; }
            public int ItemId { get; set; }
            public int StockAdjusmentId { get; set; }

        }

        public class StockAdjusmentStockUpdate
        {
            public int id { get; set; }
            public float? Quantity { get; set; }
            public float ManufacturingQuantity { get; set; }
            public int LocationStockId { get; set; }
            public float? Adjusment { get; set; }
            public int LocationsStockCount { get; set; }
            public int StockId { get; set; }
            public string Tip { get; set; } = string.Empty;
            public float? RezerveCount { get; set; }
        }
        public class StockAdjusmentSql
        {
            public int id { get; set; }
            public float? Quantity { get; set; }
            public float ManufacturingQuantity { get; set; }
            public int LocationStockId { get; set; }
            public float? Adjusment { get; set; }
            public int LocationsStockCount { get; set; }
            public int StockId { get; set; }
            public string Tip { get; set; } = string.Empty;
            public float? RezerveCount { get; set; }
            public int ItemId { get; set; }
        }
        public class LocaVarmı
        {
            public int id { get; set; }
            public int RezerveId { get; set; }
            public int MalzemeDurum { get; set; }
            public float LocationStock { get; set; }
            public float DepoStokId { get; set; }
            public string Tip { get; set; } = string.Empty;
            public float VarsayilanFiyat { get; set; }
            public int? RezerveStockCount { get; set; }
            public float? RezerveDeger { get; set; }
            public float OrdersItemCount { get; set; }
            public int StokId { get; set; }
            public int DepoId { get; set; }
            public float Miktari { get; set; }
            public int CariKod { get; set; }
            public int StokMiktar { get; set; }
            public float PlanlananMiktar { get; set; }
            public int VergiId { get; set; }
            public int VergiDeger { get; set; }
            public int Durum { get; set; }
            public float Kayıp { get; set; }



        }
        public class DeleteClas
        {
            public int id { get; set; }
            public string Tip { get; set; } = string.Empty;
            public int? Quotes { get; set; }
        }
    }
}
