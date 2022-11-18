using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class StockTransferDTO
    {
        public class StockTransferAll
        {
            public int id { get; set; }
            public string StockTransferName { get; set; } = string.Empty;
            public DateTime TransferDate { get; set; }
            public int? OriginId { get; set; }

            public int? DestinationId { get; set; }
            public string Info { get; set; } = string.Empty;
            public int? ItemId { get; set; }
            public float? Quantity { get; set; }
        }
        public class StockTransferInsert
        {
            public string StockTransferName { get; set; } = string.Empty;
            public DateTime TransferDate { get; set; }
            public int OriginId { get; set; }
            public int DestinationId { get; set; }
            public string Info { get; set; } = string.Empty;
            public int? ItemId { get; set; }
            public float? Quantity { get; set; }

        }
        public class StockTransferInsertItem
        {
            public int? ItemId { get; set; }
            public float? Quantity { get; set; }
            public int? StockTransferId { get; set; }
        }
        public class StockUpdate
        {
            public int id { get; set; }
            public string StockTransferName { get; set; }
            public float Total { get; set; }
            public DateTime TransferDate { get; set; }
            public string Info { get; set; }
        }
        public class StockTransferList
        {
            public int id { get; set; }
            public string StockTransferName { get; set; } = string.Empty;
            public DateTime TransferDate { get; set; }

            public int DestinationId { get; set; }
            public string DestinationName { get; set; } = string.Empty;
            public int OriginId { get; set; }
            public string OriginName { get; set; } = string.Empty;
            public string Info { get; set; } = string.Empty;
            public string Total { get; set; } = string.Empty;
        }
        public class StockTransferDelete
        {
            public int id { get; set; }
        }
        public class StockTransferDeleteItems
        {
            public int id { get; set; }
            public int StockTransferId { get; set; }

            public int ItemId { get; set; }
        }

        public class StockMergeSql
        {
            public int DefaultPrice { get; set; }
            public string Tip { get; set; } = string.Empty;
            public int OriginId { get; set; }
            public float OriginStockCount { get; set; }
            public int DestinationId { get; set; }
            public float DestinationStockCount { get; set; }
            public float? Quantity { get; set; }
            public int StockId { get; set; }
            public int originvarmi { get; set; }
            public int stockCountOrigin { get; set; }
            public int destinationvarmı { get; set; }
            public int? DestinationStockCounts { get; set; }
            public float? RezerveCountOrigin { get; set; }
            public float? RezerveCountDestination { get; set; }
        }

        public class StockTransferDetailsItems
        {
            public int id { get; set; }
            public int ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public float Quantity { get; set; }
            public int OriginId { get; set; }
            public string OriginLocationName { get; set; } = string.Empty;
            public float OriginLocationStockCount { get; set; }
            public int DestinationId { get; set; }
            public float CostPerUnit { get; set; }
            public string DestinationLocationName { get; set; } = string.Empty;
            public float DestinationLocationStockCount { get; set; }

            public float TransferValue { get; set; }
        }

        public class StockTransferItems
        {
            public int? id { get; set; }
            public int? StockTransferId { get; set; }
            public int? ItemId { get; set; }
            public float Quantity { get; set; }
        }
    }
}
