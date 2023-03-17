using BL.Services.LocationStock;
using DAL.Models;
using DAL.StockControl;
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
        private readonly ILocationStockControl _loccontrol;
        private readonly IStockControl _control;


        public StockTransferControl(IDbConnection db, ILocationStockControl loccontrol, IStockControl control)
        {
            _db = db;
            _loccontrol = loccontrol;
            _control = control;
        }

        public async Task<List<string>> Insert(StockTransferInsert T)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<StockTransferAll>($@"select
             (Select id  From Urunler where id={T.ItemId})as ItemId,
            (Select id as varmi From DepoVeAdresler where id = {T.DestinationId})as DestinationId,
            (Select id as varmi From DepoVeAdresler where id = {T.OriginId})as OriginId

            ");
            if (T.DestinationId!=T.OriginId)
            {
                if (list.First().DestinationId == null)
                {
                    hatalar.Add("DestinationId,Boyle bir location yok");
                }
                if (list.First().OriginId == null)
                {
                    hatalar.Add("OriginId,Boyle bir location yok");

                }
            }
            else
            {
                hatalar.Add("Konumlar ayni olamaz");

            }
            if (list.First().ItemId == null)
            {
                hatalar.Add("ItemId,Boyler bir item yok");
                return hatalar;


            }
            else
            {
                return hatalar;
            }



        }

        public async Task<List<string>> InsertItem(StockTransferInsertItem T)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<StockTransferInsertItem>($@"select
             (Select id  From Urunler where id={T.ItemId})as ItemId,
            (Select id as varmi From StokAktarim where id = {T.StockTransferId} )as StockTransferId
            ");
            if (list.First().ItemId == null)
            {
                hatalar.Add("ItemId,Boyler bir item yok");

            }
            if (list.First().StockTransferId==null)
            {
                hatalar.Add("StockTransferId,boyle bir id bulunamadı");
                return hatalar;

            }
            else
            {
                return hatalar;
            }
        }

        public async Task Kontrol(int? Id, int? ItemId,int? StockTransferId)
        {
            DynamicParameters prm1 = new();
            prm1.Add("@ItemId", ItemId);
            prm1.Add("@id", Id);
            var ItemIdeski = await _db.QueryFirstAsync<int>($"Select  StokAktarimDetay.StokId from StokAktarimDetay where id=@id ", prm1);
            if (ItemId!=ItemIdeski)
            {
                DynamicParameters prm = new DynamicParameters();
                prm.Add("@id", Id);
                prm.Add("@StockTransferId", StockTransferId);
                prm.Add("@ItemId", ItemIdeski);
                string sqlf = $@"declare @@Origin int,@@Destination int
            set @@Origin=(Select OriginId from StockTransfer where id = @StockTransferId and CompanyId = @CompanyId)
            set @@Destination=(Select DestinationId from StockTransfer where id = @StockTransferId and CompanyId =  @CompanyId)
            select (Select DefaultPrice from Items where id = @ItemId and CompanyId = @CompanyId) as DefaultPrice,
            (Select Tip from Items where id = @ItemId and CompanyId = @CompanyId) as Tip,(Select @@Origin) as OriginId,
            (Select @@Destination) as DestinationId,
            (select id from LocationStock where LocationId = @@Origin and CompanyId = @CompanyId and ItemId = @ItemId) as   originvarmi,
            (Select StockCount from LocationStock where ItemId = @ItemId and LocationId = @@Origin and CompanyId =       @CompanyId) as     stockCountOrigin,
            (select id from LocationStock where LocationId = @@Destination and CompanyId = @CompanyId and ItemId = @ItemId) as  destinationvarmı
            ,(Select StockCount from LocationStock where ItemId = @ItemId and LocationId = @@Destination and CompanyId =             @CompanyId) as DestinationStockCounts,
            (select st.Quantity  from StockTransferItems st  where st.id=@id and st.ItemId=@ItemId and st.CompanyId=@CompanyId)as Quantity";
                var sorgu = await _db.QueryAsync<StockMergeSql>(sqlf, prm);
                float? Quantity = sorgu.First().Quantity;
                var CostPerUnit = sorgu.First().DefaultPrice;
                var Tip = sorgu.First().Tip;
                var value = Quantity * CostPerUnit; //transfer value hesaplama
                prm.Add("@Total", value);
                int Origin = sorgu.First().OriginId;
                int Destination = sorgu.First().DestinationId;
                int stockId = sorgu.First().StockId;
                prm.Add("@stockId", stockId);
                prm.Add("@Destination", Destination);
                prm.Add("@Origin", Origin);
                prm.Add("@TransferValue", value);
                prm.Add("@CostPerUnit", CostPerUnit);

                float? OriginStockCount = sorgu.First().stockCountOrigin;
                float? DestinationStockCount = sorgu.First().DestinationStockCounts;

                var NewOriginStock = OriginStockCount + Quantity;
                var NewDestinationStock = DestinationStockCount - Quantity;

                prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
                prm.Add("@NewDestinationStock", NewDestinationStock);

                await _db.ExecuteAsync($"Update DepoStoklar SET StockCount =@NewOriginStock where LocationId = @Origin and ItemId=@ItemId and CompanyId = @CompanyId", prm);
                await _db.ExecuteAsync($"Update DepoStoklar SET StockCount =@NewDestinationStock where LocationId = @Destination and ItemId=@ItemId  and CompanyId = @CompanyId", prm);

                string sql1 = $"Select Tip From Urunler where  id = {ItemId}";
                var yeniTip = await _db.QueryFirstAsync<string>(sql1);
                await _loccontrol.Kontrol(ItemId, Origin, yeniTip);
                await _loccontrol.Kontrol(ItemId, Destination, yeniTip);

            }

        }

        public async Task<List<string>> UpdateItems(int? ItemId,int? StockTransferId, int? id)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<StockTransferItems>($@"select
             (Select id  From StokAktarimDetay where ItemId={ItemId} and id={id})as ItemId,
            (Select id as varmi From StokAktarim where  id = {StockTransferId} and IsActive=1)as StockTransferId,
            (Select id as varmi From StokAktarimDetay where  id = {id} and StockTransferId={StockTransferId})as id ");
           
            if (list.First().StockTransferId == null)
            {
                hatalar.Add("StockTransferId,boyle bir id bulunamadı");
            }
            if (list.First().id==null)
            {
                hatalar.Add("Boyle bir eslesme yok.Id ve StockTransferId");
            }
            if (list.First().ItemId == null)
            {
                hatalar.Add("ItemId,Boyler bir item yok");
                return hatalar;
            }
            else
            {
                return hatalar;
            }
        }

        public async Task<List<string>> AdresStokKontrol(int? Id,int? ItemId,int? StockTransferId, float? Quantity)
        {
            DynamicParameters prm = new();
            prm.Add("@StockTransferId", StockTransferId);
            prm.Add("@id", Id);

            string sqlf = $@"select
            (Select OriginId from StokAktarim where id = @StockTransferId ) as OriginId,
            (Select DestinationId from StokAktarim where id = @StockTransferId ) as DestinationId";
            var sorgu = await _db.QueryAsync<StockMergeSql>(sqlf, prm);
            int OriginId = sorgu.First().OriginId;
            int DestinationId = sorgu.First().DestinationId;
            var deger = await _db.QueryAsync<int>($"Select  StokAktarimDetay.Miktar from StokAktarimDetay where id=@id ", prm);
            List<string> hatalar = new();
            var origincount = await _control.Count(ItemId, OriginId);
            var DesCount = await _control.Count(ItemId, DestinationId);
            if (deger.First()<Quantity)
            {
                if (origincount > 0)
                {
                    if (origincount - Quantity < 0)
                    {
                        string hata = "Girdiğiniz miktar stok miktarini aşıyor";
                        hatalar.Add(hata);
                    }
                }
                else
                {
                    string hata = "Adreste kullanılabilir stok bulunmamaktadir.";
                    hatalar.Add(hata);
                }
            }
            else
            {
                if (DesCount > 0)
                {
                    if (DesCount - Quantity < 0)
                    {
                        string hata = "Girdiğiniz miktar stok miktarini aşıyor";
                        hatalar.Add(hata);
                    }
                }
                else
                {
                    string hata = "Adreste kullanılabilir stok bulunmamaktadir.";
                    hatalar.Add(hata);
                }
            }

         
         

            return hatalar;
        }


    }
}
