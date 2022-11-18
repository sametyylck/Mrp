using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;

namespace BL.Services.StockAdjusment
{
    public class StockAdjusmentControl : IStockAdjusmentControl
    {
        private readonly IDbConnection _db;

        public StockAdjusmentControl(IDbConnection db)
        {
            _db = db;
        }

        public async Task<string> DeleteItems(StockAdjusmentItemDelete T, int CompanyId)
        {
            var list = await _db.QueryAsync<StockAdjusmentUpdateItems>($@"select
             (Select id  From StockAdjusmentItems where CompanyId = {CompanyId} and id={T.id} and  StockAdjusmentId={T.StockAdjusmentId})as id,

            (Select id as varmi From StockAdjusment where CompanyId = {CompanyId} and id = {T.StockAdjusmentId})as StockAdjusmentId,
             (Select id  From StockAdjusmentItems where CompanyId = {CompanyId} and ItemId={T.ItemId} and StockAdjusmentId={T.StockAdjusmentId})as ItemId

            ");
            if (list.First().StockAdjusmentId == null)
            {
                return ("StockAdjusmentId,Boyle bir id yok");
            }
            if (list.First().id == null)
            {
                return ("id ve StockAdjusmentId eslesmiyor");
            }
            if (list.First().ItemId == 0)
            {
                return ("Boyle bir ItemId yok");
            }
            else
            {
                return ("true");
            }
        }

        public async Task<string> Insert(StockAdjusmentInsert T, int CompanyId)
        {
            var list = await _db.QueryAsync<StockAdjusmentInsert>($@"select
             (Select id  From StockTakes where CompanyId = {CompanyId} and id={T.StockTakesId})as StockTakesId,
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId
            ");
            if (T.StockTakesId!=0)
            {
                if (T.StockTakesId!=null)
                {
                    if (list.First().StockTakesId == null)
                    {
                        return ("Boyle bir id bulunamadı");
                    }
                }
               
            }
     
            if (list.First().LocationId == null)
            {
                return ("Boyle bir Location bulunamadı");
            }
            else
            {
                return ("true");
            }
        }

        public async Task<string> InsertItem(StockAdjusmentInsertItem T, int CompanyId)
        {
            var list = await _db.QueryAsync<StockAdjusmentInsertItem>($@"select
             (Select id  From Items where CompanyId = {CompanyId} and id={T.ItemId} and IsActive=1)as ItemId,
             (Select id  From StockAdjusment where CompanyId = {CompanyId} and id={T.StockAdjusmentId} and IsActive=1)as StockAdjusmentId,

            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId
            ");
            if (list.First().ItemId == null)
            {
                return ("ItemId,Boyle bir id bulunamadı");
            }
            if (list.First().StockAdjusmentId == null)
            {
                return ("StockAdjusmentId,Boyle bir id bulunamadı");
            }
            if (list.First().LocationId == null)
            {
                return ("LocationId,Boyle bir Location bulunamadı");
            }
            else
            {
                return ("true");
            }

        }

        public async Task<string> Update(StockAdjusmentUpdate T, int CompanyId)
        {
            var list = await _db.QueryAsync<StockAdjusmentUpdate>($@"select
             (Select id  From StockAdjusment where CompanyId = {CompanyId} and id={T.id} and IsActive=1)as id,

            (Select id  From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId
            ");
            if (list.First().id==null)
            {
                return ("id,boyle bir id yok");
            }
            if (list.First().LocationId == null)
            {
                return ("LocationId,Boyle bir Location bulunamadı");
            }
            else
            {
                return ("true");
            }

        }

        public async Task<string> UpdateStockAdjusment(StockAdjusmentUpdateItems T, int CompanyId)
        {
            var list = await _db.QueryAsync<StockAdjusmentUpdateItems>($@"select
             (Select id  From StockAdjusmentItems where CompanyId = {CompanyId} and id={T.id} and  StockAdjusmentId={T.StockAdjusmentId})as id,

            (Select id as varmi From StockAdjusment where CompanyId = {CompanyId} and id = {T.StockAdjusmentId})as StockAdjusmentId,
             (Select id  From StockAdjusmentItems where CompanyId = {CompanyId} and id={T.ItemId} and StockAdjusmentId={T.StockAdjusmentId} )as ItemId

            ");
            if (list.First().StockAdjusmentId==null)
            {
                return ("StockAdjusmentId,Boyle bir id yok");
            }
            if (list.First().id==null)
            {
                return ("id ve StockAdjusmentId eslesmiyor");
            }
            if (list.First().ItemId==null)
            {
                return ("Boyle bir ItemId yok");
            }
            else
            {
                return ("true");
            }
        }
    }
}
