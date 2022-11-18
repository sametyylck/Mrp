using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.StockTakesDTO;

namespace BL.Services.StockTakes
{
    public class StockTakesControl : IStockTakesControl
    {
        private readonly IDbConnection _db;

        public StockTakesControl(IDbConnection db)
        {
            _db = db;
        }

        public async Task<string> DeleteItem(StockTakeDelete T, int CompanyId)
        {
            var list = await _db.QueryAsync<StockTakeDelete>($@"select
             (Select id  From StockTakesItem where CompanyId = {CompanyId} and ItemId={T.ItemId} and id={T.id} )as ItemId,
            (Select Count(*) as varmi From StockTakes where CompanyId = {CompanyId} and id = {T.id} and IsActive=1)as id
            "); 
            if (list.First().id == null)
            {
                return ("Boyle bir Itemid yok");
            }
            if (list.First().ItemId == null)
            {
                return ("Boyle bir location yok");
            }
            else
            {
                return ("true");
            }
        }

        public async Task<string> Insert(StockTakesInsert T, int CompanyId)
        {
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId
            ");
            if (list.First().LocationId==null)
            {
                return ("Boyle bir location yok");
            }
            else
            {
                return ("true");
            }
        }

        public async Task<string> InsertItem(List<StockTakeInsertItems> T , int CompanyId)
        {
            foreach (var item in T)
            {
                var list = await _db.QueryAsync<StockTakeInsertItems>($@"select
             (Select id  From Items where CompanyId = {CompanyId} and id={item.ItemId})as ItemId,
            (Select id as varmi From StockTakes where CompanyId = {CompanyId} and id = {item.StockTakesId} and IsActive=1)as StockTakesId
            ");
                if (list.First().ItemId == null)
                {
                    return ("Boyle bir Itemid yok");
                }
                if (list.First().StockTakesId == null)
                {
                    return ("StockTakesId,Boyle bir id yok");
                }  
            }
            return ("true");
     
        }

        public async Task<string> UpdateItem(StockTakesUpdateItems T, int CompanyId)
        {
            var list = await _db.QueryAsync<StockTakesUpdateItems>($@"select
             (Select id  From StockTakesItem where CompanyId = {CompanyId} and id={T.StockTakesItemId} and StockTakesId={T.StockTakesId})as StockTakesItemId,
            (Select id as varmi From StockTakes where CompanyId = {CompanyId} and id = {T.StockTakesId} and IsActive=1)as StockTakesId");
            if (list.First().StockTakesId == null)
            {
                return ("StockTakesId,Boyle bir id yok");
            }
            if (list.First().StockTakesItemId==null)
            {
                return ("Boyle bir eslesme mevcut degi.StockTakesItemId ve StockTakesId");
            }
            else
            {
                return ("true");
            }
        }
    }
}
