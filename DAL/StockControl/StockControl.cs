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

        public async  Task<int> Count(int? ItemId,int? LocationId)
        {
            var prm = new DynamicParameters();
            prm.Add("@ItemId", ItemId);
            prm.Add("@LocationId", LocationId);
            var locationstock = _db.Query<LocaVarmı>($@"select (select id from DepoStoklar where StokId=@ItemId and DepoId=@LocationId)as DepoStokId,(Select Tip from Urunler where id=@ItemId)as Tip", prm);
            string Tip = locationstock.First().Tip;
            if (locationstock.First().DepoStokId == 0)
            {
             int id= await _locationstock.Insert(Tip, ItemId, LocationId);
            }
           
            var adetbul =  _db.QueryFirst<int>($@"select
            (Select StokAdeti from DepoStoklar where StokId = @ItemId and DepoId = @LocationId)
            -
            (select ISNULL(SUM(RezerveDeger),0) from Rezerve where StokId = @ItemId and DepoId = @LocationId and Durum = 1)",prm);
            return adetbul;
        }
    }
}
