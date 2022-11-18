using DAL.Contracts;
using DAL.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockAdjusmentDTO;

namespace DAL.StockControl
{
    public class StockControl : IStockControl
    {
        private readonly IDbConnection _db;
        private readonly ILocationStockRepository _locationstock;

        public StockControl(IDbConnection db, ILocationStockRepository locationstock)
        {
            _db = db;
            _locationstock = locationstock;
        }

        public int Count(int? ItemId, int CompanyId, int? LocationId)
        {
            var prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", ItemId);
            prm.Add("@LocationId", LocationId);
            var locationstock = _db.Query<LocaVarmı>($@"select (select id from LocationStock where ItemId=@ItemId and LocationId=@LocationId)as LocationStockId,(Select Tip from Items where id=@ItemId)as Tip", prm);
            string Tip = locationstock.First().Tip;
            if (locationstock.First().LocationStockId == 0)
            {
                _locationstock.Insert(Tip, ItemId, CompanyId, LocationId);
            }
           
            var adetbul =  _db.QueryFirst<int>($@"select
            (Select StockCount from LocationStock where ItemId = @ItemId and CompanyId = @CompanyId and LocationId = @LocationId and IsActive = 1)
            -
            (select ISNULL(SUM(RezerveCount),0) from Rezerve where ItemId = @ItemId and CompanyId = @CompanyId and LocationId = @LocationId and Status = 1)",prm);
            return adetbul;
        }
    }
}
