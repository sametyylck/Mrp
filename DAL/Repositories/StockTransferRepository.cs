using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.StockTransferDTO;

namespace DAL.Repositories
{
    public class StockTransferRepository : IStockTransferRepository
    {
        IDbConnection _db;
        ILocationStockRepository _loc;

        public StockTransferRepository(IDbConnection db, ILocationStockRepository loc)
        {
            _db = db;
            _loc = loc;
        }

        public async Task<int> Count(StockTransferList T, int CompanyId)
        {
            List<int> kayitsayisi = (await _db.QueryAsync<int>($"Select COUNT(*) as kayitsayisi  from StockTransfer inner join Locations de on de.id = StockTransfer.OriginId inner join Locations da on da.id = StockTransfer.DestinationId where StockTransfer.CompanyId = {CompanyId} and StockTransfer.IsActive=1  and ISNULL(StockTransfer.StockTransferName, 0) LIKE '%{T.StockTransferName}%' and ISNULL(StockTransfer.DestinationId, 0) LIKE '%%' and ISNULL(da.LocationName, 0) LIKE '%{T.DestinationName}%' and ISNULL(de.LocationName, 0) LIKE '%{T.OriginName}%' and ISNULL(StockTransfer.Total,'') LIKE '%{T.Total}%'")).ToList();
            return kayitsayisi[0];
        }

        public async Task Delete(IdControl T, int CompanyId,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@IsActive", false);
            string sqls = $@"select st.id,st.ItemId,st.Quantity  from StockTransferItems  st
            left join StockTransfer on StockTransfer.id=st.StockTransferId
            where st.CompanyId=@CompanyId and StockTransfer.id=@id";
            var list = await _db.QueryAsync<StockTransferDetailsItems>(sqls, prm);
            foreach (var item in list)
            {
                prm.Add("@ItemId", item.ItemId);
                string sqlf = $@"declare @@Origin int,@@Destination int 
            set @@Origin=(Select OriginId from StockTransfer where id = @id and CompanyId = @CompanyId)
            set @@Destination=(Select DestinationId from StockTransfer where id = @id and CompanyId =  @CompanyId)
            select (Select DefaultPrice from Items where id = @ItemId and CompanyId = @CompanyId) as DefaultPrice,
            (Select Tip from Items where id = @ItemId and CompanyId = @CompanyId) as Tip,(Select @@Origin) as OriginId,
            (Select @@Destination) as DestinationId,
            (select id from LocationStock where LocationId = @@Origin and CompanyId = @CompanyId and ItemId = @ItemId) as   originvarmi,
            (Select StockCount from LocationStock where ItemId = @ItemId and LocationId = @@Origin and CompanyId =       @CompanyId) as     stockCountOrigin,
          
            (select id from LocationStock where LocationId = @@Destination and CompanyId = @CompanyId and ItemId = @ItemId) as   destinationvarmı
            ,(Select StockCount from LocationStock where ItemId = @ItemId and LocationId = @@Destination and CompanyId =             @CompanyId) as DestinationStockCounts";
                var sorgu = _db.Query<StockMergeSql>(sqlf, prm);

                float? Quantity = item.Quantity;
                var CostPerUnit = sorgu.First().DefaultPrice;
                var Tip = sorgu.First().Tip;
                var value = Quantity * CostPerUnit; //transfer value hesaplama
                prm.Add("@Total", value);
                int Origin = sorgu.First().OriginId;
                int Destination = sorgu.First().DestinationId;
                prm.Add("@Destination", Destination);
                prm.Add("@Origin", Origin);
                prm.Add("@TransferValue", value);
                prm.Add("@CostPerUnit", CostPerUnit);


                //Verilen konumlarda bu iteme ait stock değeri var mı kontrol edilir.Yoksa oluşturulur.
                if (sorgu.First().originvarmi == 0)
                {
                  await  _loc.Insert(Tip, item.ItemId, CompanyId, Origin);
                }
                float? OriginStockCount = sorgu.First().stockCountOrigin;

                if (sorgu.First().destinationvarmı == 0)
                {
                   await _loc.Insert(Tip, item.ItemId, CompanyId,  Destination);
                }

                float? DestinationStockCount = sorgu.First().DestinationStockCounts;

                var NewOriginStock = OriginStockCount + Quantity;
                var NewDestinationStock = DestinationStockCount - Quantity;



                prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
                prm.Add("@NewDestinationStock", NewDestinationStock);
                prm.Add("@CompanyId", CompanyId);

                prm.Add("@User", UserId);
                prm.Add("@StockMovementQuantity", Quantity);
                prm.Add("@PreviousValue", DestinationStockCount);
                prm.Add("@Process", "LocationStock");
                prm.Add("@Operation", "-");
                prm.Add("@Date", DateTime.Now);
                prm.Add("@Where", "StockTransferDelete");
                await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewDestinationStock,@Date,@User,@CompanyId,@Destination,@ItemId)", prm);

                prm.Add("@Operation", "+");

                prm.Add("@PreviousValue", OriginStockCount);
                prm.Add("@Process", "LocationStock");
                await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewOriginStock,@Date,@User,@CompanyId,@Origin,@ItemId)", prm);



                await _db.ExecuteAsync($"Update StockTransfer set Total=@Total where id=@id and CompanyId=@CompanyId ", prm);

                await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewOriginStock where LocationId = @Origin and ItemId=@ItemId and CompanyId = @CompanyId", prm);
                await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewDestinationStock where LocationId = @Destination and ItemId=@ItemId  and CompanyId = @CompanyId", prm);


                prm.Add("itemid", item.id);
                await _db.ExecuteAsync($"Delete From StockTransferItems  where ItemId = @ItemId and CompanyId = @CompanyId and id=@itemid and id=@itemid", prm);
            }

            prm.Add("@DateTime", DateTime.Now);
            prm.Add("@User", UserId);
            await _db.ExecuteAsync($"Update StockTransfer Set IsActive=@IsActive,DeleteDate=@DateTime,DeletedUser=@User where id = @id and CompanyId = @CompanyId", prm);
        }

        public async Task DeleteItems(StockTransferDeleteItems T, int CompanyId,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@StockTransferId", T.StockTransferId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@CompanyId", CompanyId);
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


            //Verilen konumlarda bu iteme ait stock değeri var mı kontrol edilir.Yoksa oluşturulur.
            if (sorgu.First().originvarmi == 0)
            {
              await  _loc.Insert(Tip, T.ItemId, CompanyId, Origin);
            }
            float? OriginStockCount = sorgu.First().stockCountOrigin;

            if (sorgu.First().destinationvarmı == 0)
            {
              await  _loc.Insert(Tip, T.ItemId, CompanyId, Destination);
            }

            float? DestinationStockCount = sorgu.First().DestinationStockCounts;

            var NewOriginStock = OriginStockCount + Quantity;
            var NewDestinationStock = DestinationStockCount - Quantity;



            prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
            prm.Add("@NewDestinationStock", NewDestinationStock);
            prm.Add("@CompanyId", CompanyId);
            await _db.ExecuteAsync($"Update StockTransfer set Total=@Total where id=@StockTransferId and CompanyId=@CompanyId ", prm);

            prm.Add("@User", UserId);
            prm.Add("@StockMovementQuantity", Quantity);
            prm.Add("@PreviousValue", DestinationStockCount);
            prm.Add("@Process", "LocationStock");
            prm.Add("@Date", DateTime.Now);
            prm.Add("@Operation", "-");

            prm.Add("@Where", "StockTransferDeleteItem");
            await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewDestinationStock,@Date,@User,@CompanyId,@Destination,@ItemId)", prm);

            prm.Add("@Operation", "+");

            prm.Add("@PreviousValue", OriginStockCount);
            prm.Add("@Process", "LocationStock");
            await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewOriginStock,@Date,@User,@CompanyId,@Origin,@ItemId)", prm);



            await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewOriginStock where LocationId = @Origin and ItemId=@ItemId and CompanyId = @CompanyId", prm);
            await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewDestinationStock where LocationId = @Destination and ItemId=@ItemId  and CompanyId = @CompanyId", prm);

            await _db.ExecuteAsync($"Delete From StockTransferItems  where id = @id and CompanyId = @CompanyId and ItemId=@ItemId and StockTransferId=@StockTransferId", prm);
        }

        public async Task<IEnumerable<StockTransferList>> Details(int id, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@CompanyId", CompanyId);

            var list =await _db.QueryAsync<StockTransferList>($@" Select x.* From 
            (Select StockTransfer.id,StockTransfer.StockTransferName,StockTransfer.TransferDate,StockTransfer.DestinationId,da.LocationName as DestinationName,StockTransfer.Total,
                StockTransfer.OriginId,de.LocationName as OriginName,StockTransfer.Info,StockTransfer.CompanyId as CompanyId from StockTransfer
            left join Locations de on de.id = StockTransfer.OriginId left join Locations da on da.id=StockTransfer.DestinationId ) x  where x.CompanyId = @CompanyId and x.id=@id", prm);
            return list.ToList();
        }

        public async Task<int> Insert(StockTransferInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@StockTransferName", T.StockTransferName);
            prm.Add("@TransferDate", T.TransferDate);
            prm.Add("@OriginId", T.OriginId);
            prm.Add("@DestinationId", T.DestinationId);
            prm.Add("@Info", T.Info);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@OriginId", T.OriginId);
            prm.Add("IsActive", true);


            return await _db.QuerySingleAsync<int>($"Insert into StockTransfer (StockTransferName,TransferDate,OriginId,DestinationId,Info,CompanyId,IsActive) OUTPUT INSERTED.[id] values (@StockTransferName,@TransferDate,@OriginId,@DestinationId,@Info,@CompanyId,@IsActive)", prm);
        }

        public  async Task<int> InsertStockTransferItem(StockTransferInsertItem T, int? id, int CompanyId,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@Quantity", T.Quantity);
            prm.Add("@StockTransferId", id);
            //DefaultPrice,Tip,OriginId,DestinationId getiriliyor.
            string sqlf = $@"declare @@Origin int,@@Destination int 
            set @@Origin=(Select OriginId from StockTransfer where id = @StockTransferId and CompanyId = @CompanyId)
            set @@Destination=(Select DestinationId from StockTransfer where id = @StockTransferId and CompanyId =  @CompanyId)
            select (Select DefaultPrice from Items where id = @ItemId and CompanyId = @CompanyId) as DefaultPrice,
            (Select Tip from Items where id = @ItemId and CompanyId = @CompanyId) as Tip,(Select @@Origin) as OriginId,
            (Select @@Destination) as DestinationId,
            (select id from LocationStock where LocationId = @@Origin and CompanyId = @CompanyId and ItemId = @ItemId) as   originvarmi,
            (Select StockCount from LocationStock where ItemId = @ItemId and LocationId = @@Origin and CompanyId =       @CompanyId) as     stockCountOrigin,
            (select id from LocationStock where LocationId = @@Destination and CompanyId = @CompanyId and ItemId = @ItemId) as      destinationvarmı ,  (Select StockCount from LocationStock where ItemId = @ItemId and LocationId = @@Destination and CompanyId =   @CompanyId) as DestinationStockCounts            ";
            var sorgu = await _db.QueryAsync<StockMergeSql>(sqlf, prm);

            var CostPerUnit = sorgu.First().DefaultPrice;
            var Tip = sorgu.First().Tip;
            var value = T.Quantity * CostPerUnit; //transfer value hesaplama
            prm.Add("@Total", value);
            int Origin = sorgu.First().OriginId;
            int Destination = sorgu.First().DestinationId;
            prm.Add("@Destination", Destination);
            prm.Add("@Origin", Origin);
            prm.Add("@TransferValue", value);
            prm.Add("@CostPerUnit", CostPerUnit);


            //Verilen konumlarda bu iteme ait stock değeri var mı kontrol edilir.Yoksa oluşturulur.
            if (sorgu.First().originvarmi == 0)
            {
              await  _loc.Insert(Tip, T.ItemId, CompanyId, Origin);
            }
            float? OriginStockCount = sorgu.First().stockCountOrigin;
            float? DestinationStockCount = 0;
            if (sorgu.First().destinationvarmı == 0)
            {
               await _loc.Insert(Tip, T.ItemId, CompanyId,  Destination);
            }
            else
            {
                DestinationStockCount = sorgu.First().DestinationStockCounts;
            }

           

            var NewOriginStock = OriginStockCount - T.Quantity;
            var NewDestinationStock = DestinationStockCount + T.Quantity;



            prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
            prm.Add("@NewDestinationStock", NewDestinationStock);
            prm.Add("@CompanyId", CompanyId);
            await _db.ExecuteAsync($"Update StockTransfer set Total=@Total where id=@StockTransferId and CompanyId=@CompanyId ", prm);

            prm.Add("@User", UserId);
            prm.Add("@StockMovementQuantity", T.Quantity);
            prm.Add("@PreviousValue", DestinationStockCount);
            prm.Add("@Process", "LocationStock");
            prm.Add("@Date", DateTime.Now);
            prm.Add("@Operation", "+");

            prm.Add("@Where", "StockTransferDelete");
            await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewDestinationStock,@Date,@User,@CompanyId,@Destination,@ItemId)", prm);

            prm.Add("@Operation", "-");

            prm.Add("@PreviousValue", OriginStockCount);
            prm.Add("@Process", "LocationStock");
            await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewOriginStock,@Date,@User,@CompanyId,@Origin,@ItemId)", prm);


            await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewOriginStock where LocationId = @Origin and ItemId=@ItemId and CompanyId = @CompanyId", prm);
            await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewDestinationStock where LocationId = @Destination and ItemId=@ItemId  and CompanyId = @CompanyId", prm);
       
            return await _db.QuerySingleAsync<int>($"Insert into StockTransferItems (ItemId,CostPerUnit,Quantity,TransferValue,StockTransferId,CompanyId) OUTPUT INSERTED.[id]  values (@ItemId,@CostPerUnit,@Quantity,@TransferValue,@StockTransferId,@CompanyId)", prm);
        }

        public async Task<IEnumerable<StockTransferDetailsItems>> ItemDetails(int id, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@CompanyId", CompanyId);
            string sqla = $@"Select StockTransferItems.id,StockTransferItems.ItemId,StockTransferItems.Quantity,Items.Name as ItemName,
            StockTransferItems.CostPerUnit,StockTransfer.DestinationId,l.LocationName as DestinationLocationName,
            v.StockCount as DestinationLocationStockCount,StockTransfer.OriginId,m.LocationName as OriginLocationName,
            c.StockCount as OriginLocationStockCount,StockTransferItems.TransferValue from StockTransferItems 
            inner join Items on Items.id = StockTransferItems.ItemId 
            inner join StockTransfer on StockTransfer.id = StockTransferItems.StockTransferId 
            inner join Locations l on l.id = StockTransfer.OriginId inner join Locations m on m.id = StockTransfer.DestinationId 
            inner join LocationStock c on c.ItemId = Items.id and c.LocationId = StockTransfer.OriginId
            and l.CompanyId = StockTransfer.CompanyId inner join LocationStock v on v.ItemId = Items.id
            and v.LocationId = StockTransfer.DestinationId and m.CompanyId = StockTransfer.CompanyId 
            where StockTransferItems.CompanyId = @CompanyId and StockTransferItems.StockTransferId = @id
            Group BY StockTransferItems.id,StockTransferItems.ItemId,StockTransferItems.Quantity,Items.Name , StockTransfer.DestinationId,l.LocationName,v.StockCount, StockTransfer.OriginId,m.LocationName,c.StockCount, StockTransferItems.TransferValue,StockTransferItems.CostPerUnit";
            var list =await _db.QueryAsync<StockTransferDetailsItems>(sqla, prm);
            return list.ToList();
        }

        public async Task<IEnumerable<StockTransferList>> List(StockTransferList T, int CompanyId, int KAYITSAYISI, int sayfa)
        {
            string sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {sayfa}  Select x.* From (Select StockTransfer.id,StockTransfer.StockTransferName,StockTransfer.TransferDate,StockTransfer.IsActive,StockTransfer.DestinationId,da.LocationName as DestinationName,StockTransfer.OriginId,de.LocationName as OriginName,StockTransfer.Total,StockTransfer.CompanyId as CompanyId from StockTransfer inner join Locations de on de.id = StockTransfer.OriginId inner join Locations da on da.id=StockTransfer.DestinationId ) x  where x.CompanyId = {CompanyId} and x.IsActive=1  and ISNULL(x.StockTransferName,0) LIKE '%{T.StockTransferName}%'and ISNULL(x.DestinationId,0) LIKE '%%' and ISNULL(DestinationName,0) LIKE '%{T.DestinationName}%' and ISNULL(OriginName,0) LIKE '%{T.OriginName}%' and ISNULL(x.Total,0) LIKE '%{T.Total}%' ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;  ";

            var list = await _db.QueryAsync<StockTransferList>(sql);
            return list.ToList();
        }

        public async Task Update(StockUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@TransferDate", T.TransferDate);
            prm.Add("@StockTransferName", T.StockTransferName);
            prm.Add("@Info", T.Info);
            prm.Add("@Total", T.Total);
            prm.Add("@CompanyId", CompanyId);


            await _db.ExecuteAsync($"Update StockTransfer SET TransferDate = @TransferDate,StockTransferName=@StockTransferName,Info=@Info,Total=@Total where id=@id and CompanyId = @CompanyId", prm);
        }

        public async Task UpdateStockTransferItem(StockTransferItems T, int CompanyId,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            double? Total = 0;
            prm.Add("@id", T.id);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@Quantity", T.Quantity);
            prm.Add("@StockTransferId", T.StockTransferId);


            var deger = await _db.QueryAsync<int>($"Select  StockTransferItems.Quantity from StockTransferItems where id=@id and CompanyId=@CompanyId", prm);
            var quantity = T.Quantity - deger.First();

            //DefaultPrice,Tip,OriginId,DestinationId getiriliyor.Gelen id ile originvarmı,destinationvarmı kontrol edilir.Eğer var ise StockCount değerleri çekilir.
            string sqlf = $@"declare @@Origin int,@@Destination int 
            set @@Origin=(Select OriginId from StockTransfer where id = @StockTransferId and CompanyId = @CompanyId)
            set @@Destination=(Select DestinationId from StockTransfer where id = @StockTransferId and CompanyId =  @CompanyId)
            select (Select DefaultPrice from Items where id = @ItemId and CompanyId = @CompanyId) as DefaultPrice,
            (Select Tip from Items where id = @ItemId and CompanyId = @CompanyId) as Tip,(Select @@Origin) as OriginId,
            (Select @@Destination) as DestinationId,
            (select id from LocationStock where LocationId = @@Origin and CompanyId = @CompanyId and ItemId = @ItemId) as            originvarmi,
            (Select StockCount from LocationStock where ItemId = @ItemId and LocationId = @@Origin and CompanyId =       @CompanyId) as     stockCountOrigin,
            (select id from LocationStock where LocationId = @@Destination and CompanyId = @CompanyId and ItemId = @ItemId) as  destinationvarmı  , (Select StockCount from LocationStock where ItemId = @ItemId and LocationId = @@Destination and CompanyId = @CompanyId) as DestinationStockCounts    ";
            var sorgu = await _db.QueryAsync<StockMergeSql>(sqlf, prm);

            var CostPerUnit = sorgu.First().DefaultPrice;
            var Tip = sorgu.First().Tip;
            var value = T.Quantity * CostPerUnit; //transfer value hesaplama
            Total += value;
            int Origin = sorgu.First().OriginId;
            int Destination = sorgu.First().DestinationId;
            prm.Add("@Destination", Destination);
            prm.Add("@Origin", Origin);
            prm.Add("@Total", Total);
            prm.Add("@TransferValue", value);
            prm.Add("@CostPerUnit", CostPerUnit);

            if (sorgu.First().originvarmi == 0)
            {
                await _loc.Insert(Tip, T.ItemId, CompanyId, Origin);
            }
            float? OriginStockCount = sorgu.First().stockCountOrigin;

            if (sorgu.First().destinationvarmı == 0)
            {
                await _loc.Insert(Tip, T.ItemId, CompanyId, Destination);
            }

            float? DestinationStockCount = sorgu.First().DestinationStockCounts;

            var NewOriginStock = OriginStockCount - quantity;
            var NewDestinationStock = DestinationStockCount + quantity;
           await _db.ExecuteAsync($"Update StockTransferItems SET   ItemId=@ItemId,CostPerUnit=@CostPerUnit,Quantity=@Quantity,TransferValue=@TransferValue where StockTransferId=@StockTransferId and id=@id and CompanyId = @CompanyId", prm);
            prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
            prm.Add("@NewDestinationStock", NewDestinationStock);
            prm.Add("@CompanyId", CompanyId);


            prm.Add("@User", UserId);
            prm.Add("@StockMovementQuantity", quantity);
            prm.Add("@PreviousValue", DestinationStockCount);
            prm.Add("@Process", "LocationStock");
            prm.Add("@Date", DateTime.Now);
            prm.Add("@Operation", "+");

            prm.Add("@Where", "StockTransferDelete");
            await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewDestinationStock,@Date,@User,@CompanyId,@Destination,@ItemId)", prm);

            prm.Add("@Operation", "-");

            prm.Add("@PreviousValue", OriginStockCount);
            prm.Add("@Process", "LocationStock");
            await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewOriginStock,@Date,@User,@CompanyId,@Origin)", prm);



            await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewOriginStock where LocationId = @Origin and ItemId=@ItemId and CompanyId = @CompanyId", prm); //LocationStock tablosuna origin location değeri ile işleşen StockCount güncellenir.
            await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewDestinationStock where LocationId = @Destination and ItemId=@ItemId  and CompanyId = @CompanyId", prm); //LocationStock tablosuna origin destination değeri ile işleşen StockCount güncellenir.
          
        }
    }
}
