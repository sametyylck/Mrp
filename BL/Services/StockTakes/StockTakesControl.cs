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
        public async Task<List<string>> StockTakesDone(StockTakesDone T)
        {
            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Status", T.Status);

            string sqlquery = $@"select Count(id) from StokSayim where id={T.id}";
            var id = await _db.QueryFirstAsync<int>(sqlquery);
            if (id==0)
            {
                hatalar.Add("Id bulunamadı");
                return hatalar;

            }

            string sql = $@"select Durum from StokSayim where id={T.id}";
            var Status = await _db.QueryFirstAsync<int>(sql);

            if (Status==1)
            {
                if (T.Status==2 || T.Status==3)
                {
                    var degerler = await _db.QueryAsync<StockTakeItems>($@"select * from StokSayimDetay where  StokSayimId={T.id}");
                    foreach (var item in degerler)
                    {
                        if (item.CountedQuantity==null)
                        {
                            hatalar.Add("Sayılan Miktar girilmedi");
                        }
                    }
                }
            }
            return hatalar;
        }

        public async Task<List<string>> DeleteItem(StockTakeDelete T)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<StockTakeDelete>($@"select
             (Select id  From StokSayimDetay where  StokId={T.ItemId} and id={T.id} )as ItemId,
            (Select Count(*) as varmi From StokSayim where  and id = {T.id} and IsActive=1)as id
            "); 
            if (list.First().id == null)
            {
                hatalar.Add("Boyle bir Itemid yok");
            }
            if (list.First().ItemId == null)
            {
                hatalar.Add("Boyle bir location yok");
                return hatalar;

            }
            else
            {
                return hatalar;
            }
        }

        public async Task<List<string>> Insert(StockTakesInsert T)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id as varmi From DepoVeAdresler where  id = {T.LocationId})as LocationId
            ");
            if (list.First().DepoId ==null)
            {
                hatalar.Add("Boyle bir location yok");
                return hatalar;

            }
            else
            {
                return hatalar;

            }
        }

        public async Task<List<string>> InsertItem(List<StockTakeInsertItems> T)
        {
            List<string> hatalar = new();
            foreach (var item in T)
            {
                var list = await _db.QueryAsync<StockTakeInsertItems>($@"select
             (Select id  From Urunler where  id={item.ItemId})as ItemId,
            (Select id as varmi From StokSayim where id = {item.StockTakesId} and Aktif=1)as StockTakesId
            ");
                if (list.First().ItemId == null)
                {
                    hatalar.Add("Boyle bir Itemid yok");
                }
                if (list.First().StockTakesId == null)
                {
                    hatalar.Add("StockTakesId,Boyle bir id yok");
                }  
            }
           return hatalar;
     
        }

        public async Task<List<string>> UpdateItem(StockTakesUpdateItems T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<StockTakesUpdateItems>($@"select
             (Select id  From StokSayimDetay where  id={T.StockTakesItemId} and StokSayimId={T.StockTakesId})as StockTakesItemId,
            (Select id as varmi From StokSayim where id = {T.StockTakesId} and Aktif=1)as StockTakesId");
            if (list.First().StockTakesId == null)
            {
                hatalar.Add("StockTakesId,Boyle bir id yok");
            }
            if (list.First().StockTakesItemId==null)
            {
                hatalar.Add("Boyle bir eslesme mevcut degi.StockTakesItemId ve StockTakesId");
                return hatalar;

            }
            else
            {
                return hatalar;
            }
        }
    }
}
