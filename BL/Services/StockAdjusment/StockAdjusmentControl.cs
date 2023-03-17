using DAL.DTO;
using DAL.StockControl;
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
        private readonly IStockControl _control;

        public StockAdjusmentControl(IDbConnection db, IStockControl control)
        {
            _db = db;
            _control = control;
        }

        public async Task<List<string>> DeleteItems(StockAdjusmentItemDelete T)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<StockAdjusmentUpdateItems>($@"select
             (Select id  From StokDuzenlemeDetay where id={T.id} and  StokDuzenlemeId={T.StockAdjusmentId})as id,

            (Select id as varmi From StokDuzenleme where  id = {T.StockAdjusmentId})as StockAdjusmentId,
             (Select id  From StokDuzenlemeDetay where  StokId={T.ItemId} and StokDuzenlemeId={T.StockAdjusmentId})as ItemId

            ");
            if (list.First().StockAdjusmentId == null)
            {
                hatalar.Add("StockAdjusmentId,Boyle bir id yok");
            }
            if (list.First().id == null)
            {
                hatalar.Add("id ve StockAdjusmentId eslesmiyor");
            }
            if (list.First().ItemId == 0)
            {
                hatalar.Add("Boyle bir ItemId yok");
                return hatalar;

            }
            else
            {
                return hatalar;
            }
        }

        public async Task<List<string>> Insert(StockAdjusmentInsert T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<StockAdjusmentInsert>($@"select
             (Select id  From StokSayim where id={T.StockTakesId})as StockTakesId,
            (Select id as varmi From DepoVeAdresler where id = {T.LocationId})as LocationId
            ");
            if (T.StockTakesId!=0)
            {
                if (T.StockTakesId!=null)
                {
                    if (list.First().StockTakesId == null)
                    {
                        hatalar.Add("Boyle bir id bulunamadı");
                    }
                }
               
            }
     
            if (list.First().LocationId == null)
            {
                hatalar.Add("Boyle bir Location bulunamadı");
                return hatalar;

            }
            else
            {
                return hatalar;

            }
        }

        public async Task<List<string>> InsertItem(StockAdjusmentInsertItem T)
        {
            List<string> hatalar = new();

            var stokcount = await _control.Count(T.ItemId, T.LocationId);
            var list = await _db.QueryAsync<StockAdjusmentInsertItem>($@"select
             (Select id  From Urunle where  id={T.ItemId} and IsActive=1)as ItemId,
             (Select id  From StokDuzenleme where  id={T.StockAdjusmentId} and Aktif=1)as StockAdjusmentId,

            (Select id as varmi From DepoVeAdresler where  id = {T.LocationId})as LocationId
            ");
            if (list.First().ItemId == null)
            {
                hatalar.Add("ItemId,Boyle bir id bulunamadı");
            }
            if (list.First().StockAdjusmentId == null)
            {
                hatalar.Add("StockAdjusmentId,Boyle bir id bulunamadı");
            }
            if (list.First().LocationId == null)
            {
                hatalar.Add("LocationId,Boyle bir Location bulunamadı");
            }
            if (T.Adjusment<0 && stokcount > 0)
            {
                if (T.Adjusment+stokcount<0)
                {
                    hatalar.Add("Yeterli stok bulunamamştır.");

                }
                hatalar.Add("true");
                return hatalar;

            }

            else
            {
                hatalar.Add("true");
                return hatalar;

            }

        }

        public async Task<List<string>> Update(StockAdjusmentUpdate T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<StockAdjusmentUpdate>($@"select
             (Select id  From StokDuzenleme where  id={T.id} and Aktif=1)as id,

            (Select id  From DepoVeAdresler where  id = {T.LocationId})as LocationId
            ");
            if (list.First().id==null)
            {
                hatalar.Add("id,boyle bir id yok");
            }
            if (list.First().LocationId == null)
            {
                hatalar.Add("LocationId,Boyle bir Location bulunamadı");
                return hatalar;

            }
            else
            {
                return hatalar;

            }

        }

        public async Task<List<string>> UpdateStockAdjusment(StockAdjusmentUpdateItems T)
        {
            List<string> hatalar = new();

            string sql1 = $@"Select Miktar from StokDuzenlemeDetay where id={T.id}";
            var sorgu2 = await _db.QueryAsync<float>(sql1);
            float adjusment = sorgu2.First();
            var adjusmentcount = T.Adjusment - adjusment;
            var liste = await _db.QueryAsync<StockAdjusmentUpdate>($@"
            Select LocationId  From StokDuzenleme where id = {T.StockAdjusmentId}
            ");
            var stokcount = await _control.Count(T.ItemId,liste.First().LocationId);
            var list = await _db.QueryAsync<StockAdjusmentUpdateItems>($@"select
             (Select id  From StokDuzenlemeDetay where id={T.id} and  StokDuzenlemeId={T.StockAdjusmentId})as id,

            (Select id as varmi From StokDuzenleme where  id = {T.StockAdjusmentId})as StockAdjusmentId,
             (Select id  From StokDuzenlemeDetay where  id={T.ItemId} and StokDuzenlemeId={T.StockAdjusmentId} )as ItemId

            ");
            if (list.First().StockAdjusmentId==null)
            {
                hatalar.Add("StockAdjusmentId,Boyle bir id yok");
            }
            if (list.First().id==null)
            {
                hatalar.Add("id ve StockAdjusmentId eslesmiyor");
            }
            if (list.First().ItemId==null)
            {
                hatalar.Add("Boyle bir ItemId yok");
            }
            if (adjusmentcount < 0 && stokcount > 0)
            {
                if (adjusmentcount + stokcount < 0)
                {
                    hatalar.Add("Yeterli stok bulunamamştır.");

                }
                return hatalar;

            }
            else
            {
                return hatalar;

            }
        }
    }
}
