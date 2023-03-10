using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class LocationStockDTO
    {
        public int id { get; set; }
        public int ItemId { get; set; }
        public int LocationId { get; set; }
        public string Tip { get; set; }
        public int StockCount { get; set; }
    }
}
