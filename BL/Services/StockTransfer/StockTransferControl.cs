using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.StockTransferDTO;

namespace BL.Services.StockTransfer
{
    public class StockTransferControl : IStockTransferControl
    {
        private readonly IDbConnection _db;
       

        public StockTransferControl(IDbConnection db)
        {
            _db = db;
        }

        public async Task<string> Insert(StockTransferInsert T, int CompanyId)
        {
            var list = await _db.QueryAsync<StockTransferAll>($@"select
             (Select id  From Items where CompanyId = {CompanyId} and id={T.ItemId})as ItemId,
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.DestinationId})as DestinationId,
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.OriginId})as OriginId

            ");
            if (T.DestinationId!=T.OriginId)
            {
                if (list.First().DestinationId == null)
                {
                    return ("DestinationId,Boyle bir location yok");
                }
                if (list.First().OriginId == null)
                {
                    return ("OriginId,Boyle bir location yok");
                }
            }
            else
            {
                return ("Konumlar ayni olamaz");
            }
            if (list.First().ItemId == null)
            {
                return ("ItemId,Boyler bir item yok");

            }
            else
            {
                return ("true");
            }



        }

        public async Task<string> InsertItem(StockTransferInsertItem T, int CompanyId)
        {
            var list = await _db.QueryAsync<StockTransferInsertItem>($@"select
             (Select id  From Items where CompanyId = {CompanyId} and id={T.ItemId})as ItemId,
            (Select id as varmi From StockTransfer where CompanyId = {CompanyId} and id = {T.StockTransferId} )as StockTransferId
            ");
            if (list.First().ItemId == null)
            {
                return ("ItemId,Boyler bir item yok");

            }
            if (list.First().StockTransferId==null)
            {
                return ("StockTransferId,boyle bir id bulunamadı");
            }
            else
            {
                return ("true");
            }
        }

        public async Task<string> UpdateItems(int? ItemId,int? StockTransferId, int? id,int CompanyId)
        {
            var list = await _db.QueryAsync<StockTransferItems>($@"select
             (Select id  From StockTransferItems where CompanyId = {CompanyId} and ItemId={ItemId} and id={id})as ItemId,
            (Select id as varmi From StockTransfer where CompanyId = {CompanyId} and id = {StockTransferId} and IsActive=1)as StockTransferId,
            (Select id as varmi From StockTransferItems where CompanyId = {CompanyId} and id = {id} and StockTransferId={StockTransferId})as id ");
           
            if (list.First().StockTransferId == null)
            {
                return ("StockTransferId,boyle bir id bulunamadı");
            }
            if (list.First().id==null)
            {
                return ("Boyle bir eslesme yok.Id ve StockTransferId");
            }
            if (list.First().ItemId == null)
            {
                return ("ItemId,Boyler bir item yok");

            }
            else
            {
                return ("true");
            }
        }
    }
}
