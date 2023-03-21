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
             (Select id  From StokDuzenlemeDetay where id={T.id} and  StokDuzenlemeId={T.StokDuzenelemeId})as id,

            (Select id as varmi From StokDuzenleme where  id = {T.StokDuzenelemeId})as StokDuzenelemeId,
             (Select id  From StokDuzenlemeDetay where  StokId={T.StokId} and StokDuzenlemeId={T.StokDuzenelemeId})as StokId

            ");
            if (list.First().StokDuzenlemeId == null)
            {
                hatalar.Add("StockAdjusmentId,Boyle bir id yok");
            }
            if (list.First().id == null)
            {
                hatalar.Add("id ve StockAdjusmentId eslesmiyor");
            }
            if (list.First().StokId == 0)
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
             (Select id  From StokSayim where id={T.StokSayimId})as StokSayimId,
            (Select id as varmi From DepoVeAdresler where id = {T.DepoId})as DepoId
            ");
            if (T.StokSayimId!=0)
            {
                if (T.StokSayimId !=null)
                {
                    if (list.First().StokSayimId == null)
                    {
                        hatalar.Add("Boyle bir id bulunamadı");
                    }
                }
               
            }
     
            if (list.First().DepoId == null)
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

            var stokcount = await _control.Count(T.StokId, T.DepoId);
            var list = await _db.QueryAsync<StockAdjusmentInsertItem>($@"select
             (Select id  From Urunler where  id={T.StokId} and Aktif=1)as StokId,
             (Select id  From StokDuzenleme where  id={T.StokDuzenlemeId} and Aktif=1)as StokDuzenlemeId,

            (Select id as varmi From DepoVeAdresler where  id = {T.DepoId})as DepoId
            ");
            if (list.First().StokId == null)
            {
                hatalar.Add("ItemId,Boyle bir id bulunamadı");
            }
            if (list.First().StokDuzenlemeId == null)
            {
                hatalar.Add("StockAdjusmentId,Boyle bir id bulunamadı");
            }
            if (list.First().DepoId == null)
            {
                hatalar.Add("LocationId,Boyle bir Location bulunamadı");
            }
            if (T.Miktar <0 && stokcount > 0)
            {
                if (T.Miktar +stokcount<0)
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

        public async Task<List<string>> Update(StockAdjusmentUpdate T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<StockAdjusmentUpdate>($@"select
             (Select id  From StokDuzenleme where  id={T.id} and Aktif=1)as id,

            (Select id  From DepoVeAdresler where  id = {T.DepoId})as DepoId
            ");
            if (list.First().id==null)
            {
                hatalar.Add("id,boyle bir id yok");
            }
            if (list.First().DepoId == null)
            {
                hatalar.Add("DepoId,Boyle bir DepoId bulunamadı");
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
            var adjusmentcount = T.Miktar - adjusment;
            var liste = await _db.QueryAsync<StockAdjusmentUpdate>($@"
            Select DepoId  From StokDuzenleme where id = {T.StokDuzenlemeId}
            ");
            var stokcount = await _control.Count(T.StokId,liste.First().DepoId);
            var list = await _db.QueryAsync<StockAdjusmentUpdateItems>($@"select
             (Select id  From StokDuzenlemeDetay where id={T.id} and  StokDuzenlemeId={T.StokDuzenlemeId})as id,

            (Select id as varmi From StokDuzenleme where  id = {T.StokDuzenlemeId})as StokDuzenlemeId,
             (Select id  From StokDuzenlemeDetay where  id={T.StokId} and StokDuzenlemeId={T.StokDuzenlemeId} )as StokId

            ");
            if (list.First().StokDuzenlemeId==null)
            {
                hatalar.Add("StockAdjusmentId,Boyle bir id yok");
            }
            if (list.First().id==null)
            {
                hatalar.Add("id ve StockAdjusmentId eslesmiyor");
            }
            if (list.First().StokId==null)
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
