using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using DAL.StockControl;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockAdjusmentDTO;

namespace DAL.Repositories
{
    public class StockAdjusmentRepository : IStockAdjusmentRepository
    {
        IDbConnection _db;
        ILocationStockRepository _locationStockRepository;
        IItemsRepository _itemsRepository;
        private readonly IStockControl _control;

        public StockAdjusmentRepository(IDbConnection db, ILocationStockRepository locationStockRepository, IItemsRepository itemsRepository, IStockControl control)
        {
            _db = db;
            _locationStockRepository = locationStockRepository;
            _itemsRepository = itemsRepository;
            _control = control;
        }

        public async Task<int> Count(StockAdjusmentList T, int CompanyId)
        {
            var kayitsayisi = await _db.QueryAsync<int>($"Select COUNT(*) as kayitsayisi from StockAdjusment left join Locations on Locations.id = StockAdjusment.LocationId where  ISNULL(StockAdjusment.CompanyId, 0) = {CompanyId} and StockAdjusment.IsActive = 1 and StockAdjusment.CompanyId = {CompanyId}   and ISNULL(StockAdjusment.Reason,0) LIKE '%{T.Reason}%'and ISNULL(StockAdjusment.Total,0) LIKE '%{T.Total}%' and ISNULL(StockAdjusment.Name,0) LIKE '%{T.Name}%' and ISNULL(Locations.LocationName,0) LIKE '%{T.LocationName}%'");
            return kayitsayisi.First();
        }

        public async Task Delete(IdControl T, int CompanyId,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@IsActive", false);

            string sqls = $@"select st.id,st.Adjusment as Quantity,st.ItemId  from StockAdjusmentItems st
            left join StockAdjusment on StockAdjusment.id = st.StockAdjusmentId
            where StockAdjusment.id = @id and st.CompanyId = @CompanyId";
            var sqlsorgu = await _db.QueryAsync<LocaVarmı>(sqls, prm);//
            foreach (var item in sqlsorgu)
            {
                prm.Add("@ItemId", item.StokId);
                prm.Add("@itemid", item.id);
                string sql = $@"declare @@locationId int 
            set @@locationId=(Select LocationId From StockAdjusment where CompanyId = @CompanyId and id = @id)
            select 
            (select @@locationId)as StockId,
            (Select AllStockQuantity from Items where id = @ItemId  and CompanyId = @CompanyId)as Quantity,
            (Select StockCount from LocationStock where ItemId = @ItemId   and LocationId = @@locationId and CompanyId = @CompanyId)as LocationsStockCount,
			(Select Tip from Items where id=@ItemId and CompanyId=@CompanyId) as Tip,
            (Select id from LocationStock where ItemId = @ItemId and LocationId = @@locationId and CompanyId =   @CompanyId)   as    LocationStockId
            ";
                var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sql, prm);//
                float? adjusment = item.Miktari;
                float? LocationId = sorgu.First().StokId;//locationid cekiyorz.
                float? stockQuantity = sorgu.First().Miktar;
                float? NewQuantity = stockQuantity - adjusment; //Tablodaki değer ile itemdeki değeri toplayarak yeni bir stok(quanitity) elde ediyoruz.
                prm.Add("@NewQuantity", NewQuantity);
               await _db.ExecuteAsync($"Update Items SET AllStockQuantity =@NewQuantity where id = @ItemId  and CompanyId = @CompanyId", prm); //Stok tablosuna yeni değeri güncelleiyoruz.

                prm.Add("@User", UserId);
                prm.Add("@StockMovementQuantity", adjusment);
                prm.Add("@PreviousValue", stockQuantity);
                prm.Add("@Process", "AllStock");
                prm.Add("@Operation", "-");
                prm.Add("@LocationId", LocationId);
                prm.Add("@Date", DateTime.Now);
                prm.Add("@Where", "StockAdjusmentDelete");
                await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewQuantity,@Date,@User,@CompanyId,@LocationId,@ItemId)", prm);



                float? stockCount = sorgu.First().StokAdeti;
                float? NewStockCount = stockCount - adjusment;
                var stocklocationId = sorgu.First().DepoStokId;


                prm.Add("@stocklocationId", stocklocationId);
                prm.Add("@NewStockCount", NewStockCount); //Yeni count değerini tabloya güncelleştiriyoruz.
               await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewStockCount where id = @stocklocationId  and CompanyId = @CompanyId", prm);

                prm.Add("@PreviousValue", stockCount);
                prm.Add("@Process", "LocationStock");
                prm.Add("@Operation", "-");

                prm.Add("@Date", DateTime.Now);
                prm.Add("@Where", "StockAdjusmentDelete");
                await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewStockCount,@Date,@User,@CompanyId,@LocationId,@ItemId)", prm);

                prm.Add("itemid", item.id);
               await _db.ExecuteAsync($"Delete From StockAdjusmentItems  where ItemId = @ItemId and CompanyId = @CompanyId and id=@itemid and StockAdjusmentId=@id", prm);
            }
            prm.Add("@DateTime", DateTime.Now);
            prm.Add("@User", UserId);
            await _db.ExecuteAsync($"Update StockAdjusment Set IsActive=@IsActive,DeleteDate=@DateTime,DeletedUser=@User where id = @id and CompanyId = @CompanyId ", prm);
        }

        public async Task DeleteItems(StockAdjusmentItemDelete T, int CompanyId,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@StockAdjusmentId", T.StockAdjusmentId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@CompanyId", CompanyId);
            string sql = $@"declare @@locationId int 
            set @@locationId=(Select LocationId From StockAdjusment where CompanyId = @CompanyId and id = @StockAdjusmentId)
            select 
            (select @@locationId) as StockId,
            (Select AllStockQuantity from Items where Items.id = @ItemId  and Items.CompanyId = @CompanyId)as Quantity,
            (Select StockCount from LocationStock where ItemId = @ItemId  and LocationId = @@locationId and CompanyId = @CompanyId)as LocationsStockCount,
			(Select Tip from Items where id=@ItemId and CompanyId=@CompanyId) as Tip,
            (Select id from LocationStock where ItemId = @ItemId and LocationId = @@locationId and CompanyId =   @CompanyId)   as    LocationStockId,
            (select s.Adjusment from StockAdjusmentItems s
            left join StockAdjusment on s.StockAdjusmentId=s.id
            where s.CompanyId=@CompanyId and s.StockAdjusmentId=@StockAdjusmentId and s.id=@id)as Adjusment";
            var sorgu =await _db.QueryAsync<StockAdjusmentStockUpdate>(sql, prm);//
            float? adjusment = sorgu.First().Adjusment;
            int LocationId = sorgu.First().StokId;
            float? stockQuantity = sorgu.First().Miktar;
            float? NewQuantity = stockQuantity - adjusment; //Tablodaki değer ile itemdeki değeri toplayarak yeni bir stok(quanitity) elde ediyoruz.
            prm.Add("@NewQuantity", NewQuantity);
           await _db.ExecuteAsync($"Update Items SET AllStockQuantity =@NewQuantity where id = @ItemId  and CompanyId = @CompanyId", prm); //Stok tablosuna yeni değeri güncelleiyoruz.
            prm.Add("@User", UserId);
            prm.Add("@LocationId", LocationId);

            prm.Add("@StockMovementQuantity", adjusment);
            prm.Add("@PreviousValue", stockQuantity);
            prm.Add("@Process", "AllStock");
            prm.Add("@Date", DateTime.Now);
            prm.Add("@Operation", "-");

            prm.Add("@Where", "StockAdjusmentDelete");
            await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewQuantity,@Date,@User,@CompanyId,@LocationId,@ItemId)", prm);

            float? stockCount = sorgu.First().StokAdeti;
            float? NewStockCount = stockCount - adjusment;
            var stocklocationId = sorgu.First().DepoStokId;


            prm.Add("@stocklocationId", stocklocationId);
            prm.Add("@NewStockCount", NewStockCount); //Yeni count değerini tabloya güncelleştiriyoruz.
           await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewStockCount where id = @stocklocationId  and CompanyId = @CompanyId", prm);

            prm.Add("@PreviousValue", stockCount);
            prm.Add("@Process", "LocationStoc");
            prm.Add("@Date", DateTime.Now);
            prm.Add("@Operation", "-");
            prm.Add("@Where", "StockAdjusmentDelete");
            await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewStockCount,@Date,@User,@CompanyId,@LocationId,@ItemId)", prm);

            await _db.ExecuteAsync($"Delete From StockAdjusmentItems  where ItemId = @ItemId and CompanyId = @CompanyId and id=@id and StockAdjusmentId=@StockAdjusmentId", prm);
        }

        public async Task<IEnumerable<StockAdjusmentClas>> Detail(int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@CompanyId", CompanyId);

            var list =await _db.QueryAsync<StockAdjusmentClas>($"Select x.* From (Select StockAdjusment.id, StockAdjusment.Name, StockAdjusment.Date, StockAdjusment.Reason, StockAdjusment.LocationId, loc.LocationName as LocationName,StockAdjusment.Info,StockAdjusment.CompanyId as CompanyId from StockAdjusment left join Locations loc on loc.id = StockAdjusment.LocationId) x where x.CompanyId = @CompanyId and x.id = @id", prm);

            foreach (var item in list)
            {

                string sqla = $@"Select StockAdjusmentItems.id,StockAdjusmentItems.ItemId,Items.Name as ItemName,StockAdjusmentItems.Adjusment,
            StockAdjusmentItems.CostPerUnit,StockAdjusmentItems.AdjusmentValue,
            StockAdjusmentItems.StockAdjusmentId,l.StockCount as InStock 
            from StockAdjusmentItems 
            left join Items on Items.id = StockAdjusmentItems.ItemId
            left join StockAdjusment on StockAdjusment.id = StockAdjusmentItems.StockAdjusmentId 
            left join Locations on Locations.id = StockAdjusment.LocationId 
            left join LocationStock l on l.ItemId = Items.id
            and l.LocationId = StockAdjusment.LocationId
            and l.CompanyId = StockAdjusment.CompanyId 
            where StockAdjusmentItems.CompanyId = @CompanyId
            and StockAdjusmentItems.StockAdjusmentId = @id 
            Group By StockAdjusmentItems.id,StockAdjusmentItems.ItemId,
            Items.Name,StockAdjusmentItems.Adjusment,StockAdjusmentItems.CostPerUnit,
            StockAdjusmentItems.AdjusmentValue, StockAdjusmentItems.StockAdjusmentId,StockCount";
                var list2 = await _db.QueryAsync<StockAdjusmentItems>(sqla, prm);
                item.detay = list2;
            }
            return list.ToList();
        }

        public async Task<int> Insert(StockAdjusmentInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Name", T.Name);
            prm.Add("@Reason", T.Reason);
            prm.Add("@Date", T.Date);
            prm.Add("@LocationId", T.LocationId);
            prm.Add("@Info", T.Info);
            prm.Add("@StockTakesId", T.StockTakesId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@IsActive", true);
            int id= await _db.QuerySingleAsync<int>($"Insert into StockAdjusment (Name,StockTakesId,Reason,Date,LocationId,Info,CompanyId,IsActive) OUTPUT INSERTED.[id] values (@Name,@StockTakesId,@Reason,@Date,@LocationId,@Info,@CompanyId,@IsActive)", prm);
            //var model = new StockAdjusmentAll
            //{
            //    id = id,
            //    Name = T.Name,
            //    Date = T.Date,
            //    Reason = T.Reason,
            //    LocationId = T.LocationId,
            //    Info = T.Info,
            //    StockTakesId = T.StockTakesId,
            //    ItemId = T.ItemId,

            //};
            return id;
        }

        public async Task<int> InsertItem(StockAdjusmentInsertItem T, int StockAdjusmentId, int CompanyId,int user)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@LocationId", T.LocationId);

            string sqlh = $@" 
            select 
			(Select StockCount from LocationStock where ItemId = @ItemId   and LocationId = @LocationId and CompanyId = @CompanyId) as LocationStock,
            (Select id from LocationStock where ItemId = @ItemId   and LocationId = @LocationId and CompanyId = @CompanyId) as LocationStockId,
            (Select Tip from Items where CompanyId=@CompanyId and id=@ItemId)as Tip";
            var locationStockVarmı =await _db.QueryAsync<LocaVarmı>(sqlh, prm);
            var tip1 = locationStockVarmı.First().Tip;
            if (locationStockVarmı.First().DepoStokId == 0)
            {
             await   _locationStockRepository.Insert(tip1, T.ItemId,T.LocationId);
            }


            string sqlv = $@"select Tip,DefaultPrice from Items where CompanyId=@CompanyId and id=@ItemId";
            var itembul =await _db.QueryAsync<LocaVarmı>(sqlv, prm);
            var tip = itembul.First().Tip;
            var defaultprice = itembul.First().VarsayilanFiyat;

            var items =await _itemsRepository.Detail(T.ItemId);
            var IngredientCost = items.First().MalzemeTutarı;
            float? operioncost = items.First().OperasyonTutarı;
            float? AdjusmentValue = 0;
            if (tip == "Material")
            {
                var CostPerUnit = defaultprice;
                AdjusmentValue = T.CostPerUnit * T.Adjusment;
            }
            else
            {
                float? CostPerUnit = IngredientCost + operioncost;
               
                AdjusmentValue = CostPerUnit * T.Adjusment;
            }

            var Total = AdjusmentValue;
            prm.Add("@Adjusment", T.Adjusment);
            prm.Add("@CostPerUnit", T.CostPerUnit);
            prm.Add("@StockAdjusmentId", StockAdjusmentId);
            prm.Add("@AdjusmentValue", AdjusmentValue);
            prm.Add("@Total", Total);



            string sql = $@"declare @@locationId int 
            set @@locationId=(Select LocationId From StockAdjusment where CompanyId = @CompanyId and id = @StockAdjusmentId)
            select 
            (Select AllStockQuantity from Items where id = @ItemId  and CompanyId = @CompanyId)as Quantity,
            (Select StockCount from LocationStock where ItemId = @ItemId   and LocationId = @@locationId and CompanyId = @CompanyId)as LocationsStockCount,
			(Select Tip from Items where id=@ItemId and CompanyId=@CompanyId) as Tip,
            (Select id from LocationStock where ItemId = @ItemId and LocationId = @@locationId and CompanyId =   @CompanyId)   as    LocationStockId";
            var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sql, prm);//
            await _db.ExecuteAsync($"Update StockAdjusment set Total=@Total where id=@StockAdjusmentId and CompanyId=@CompanyId ", prm);

            float? stockQuantity = sorgu.First().Miktar;
            float? NewQuantity = stockQuantity + T.Adjusment; //Tablodaki değer ile itemdeki değeri toplayarak yeni bir stok(quanitity) elde ediyoruz.
            prm.Add("@NewQuantity", NewQuantity);
           await _db.ExecuteAsync($"Update Items SET AllStockQuantity =@NewQuantity where id = @ItemId  and CompanyId = @CompanyId", prm); //Stok tablosuna yeni değeri güncelleiyoruz.

            prm.Add("@User", user);
            prm.Add("@StockMovementQuantity", T.Adjusment);
            prm.Add("@PreviousValue", stockQuantity);
            prm.Add("@Process", "AllStock");
            prm.Add("@Date", DateTime.Now);
            prm.Add("@Operation", "+");

            prm.Add("@Where", "StockAdjusmentInsertItem");
            await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewQuantity,@Date,@User,@CompanyId,@LocationId,@ItemId)", prm);

            float? stockCount = sorgu.First().StokAdeti;
            float? NewStockCount = stockCount + T.Adjusment;
            var stocklocationId = sorgu.First().DepoStokId;
            if (stocklocationId == 0)
            {
              await  _locationStockRepository.Insert(tip, T.ItemId,  T.LocationId);
            }

            prm.Add("@stocklocationId", stocklocationId);
           
            prm.Add("@NewStockCount", NewStockCount); //Yeni count değerini tabloya güncelleştiriyoruz.
            _db.Execute($"Update LocationStock SET StockCount =@NewStockCount where id = @stocklocationId  and CompanyId = @CompanyId", prm);

            prm.Add("@PreviousValue", stockCount);
            prm.Add("@Process", "LocationStock");
            prm.Add("@Date", DateTime.Now);
            prm.Add("@Operation", "+");

            prm.Add("@Where", "StockAdjusmentInsertItem");
            await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewStockCount,@Date,@User,@CompanyId,@LocationId,@ItemId)", prm);



            int id= await _db.QuerySingleAsync<int>($"Insert into StockAdjusmentItems (ItemId,Adjusment,CostPerUnit,StockAdjusmentId,AdjusmentValue,CompanyId) OUTPUT INSERTED.[id] values (@ItemId,@Adjusment,@CostPerUnit,@StockAdjusmentId,@AdjusmentValue,@CompanyId)", prm);
            return id;
        }

        public async Task<IEnumerable<StockAdjusmentList>> List(StockAdjusmentList T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);

            string sql = $"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA} Select StockAdjusment.id,StockAdjusment.Name,Locations.LocationName as LocationName ,StockAdjusment.Reason,StockAdjusment.Total,StockAdjusment.Date from StockAdjusment left join Locations on Locations.id = StockAdjusment.LocationId where ISNULL(StockAdjusment.CompanyId,0) = {CompanyId}  and StockAdjusment.IsActive = 1 and StockAdjusment.CompanyId = {CompanyId}  and ISNULL(StockAdjusment.Reason,0) LIKE '%{T.Reason}%'and ISNULL(StockAdjusment.Total,0) LIKE '%{T.Total}%' and ISNULL(StockAdjusment.Name,0) LIKE '%{T.Name}%' and ISNULL(Locations.LocationName,0) LIKE '%{T.LocationName}%' ORDER BY StockAdjusment.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";

            var list =await _db.QueryAsync<StockAdjusmentList>(sql);
            return list.ToList();
        }

        public async Task Update(StockAdjusmentUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Name", T.Name);
            prm.Add("@Reason", T.Reason);
            prm.Add("@Date", T.Date);
            prm.Add("@LocationId", T.LocationId);
            prm.Add("@Info", T.Info);
            prm.Add("@CompanyId", CompanyId);

           await _db.ExecuteAsync($"Update StockAdjusment SET Name = @Name,Reason=@Reason,Date=@Date,LocationId=@LocationId,Info=@Info where id=@id  and CompanyId = @CompanyId", prm);
        }

        public async Task UpdateStockAdjusmentItem(StockAdjusmentUpdateItems T, int CompanyId,int UserId)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@StockAdjusmentId", T.StockAdjusmentId);
            string sqlv = $@"select Tip,DefaultPrice from Items where CompanyId=@CompanyId and id=@ItemId";
            var itembul =await _db.QueryAsync<LocaVarmı>(sqlv, prm);
            var tip = itembul.First().Tip;
            var defaultprice = itembul.First().VarsayilanFiyat;


            prm.Add("@id", T.id);
            string sql1 = $@"Select Adjusment from StockAdjusmentItems where id=@id and CompanyId=@CompanyId";
            var sorgu2 =await _db.QueryAsync<float>(sql1, prm);
            float adjusment = sorgu2.First();

            var items =await _itemsRepository.Detail(T.ItemId);
            var IngredientCost = items.First().MalzemeTutarı;
            float? operioncost = items.First().OperasyonTutarı;
            float? CostPerUnit = IngredientCost + operioncost;
            float? AdjusmentValue = 0;
            if (tip == "Material")
            {
               
                    AdjusmentValue = T.CostPerUnit * T.Adjusment;
            }
            else
            {
                if (T.CostPerUnit == CostPerUnit)
                {
                    T.CostPerUnit = (float)CostPerUnit;
                    AdjusmentValue = (float)CostPerUnit * T.Adjusment;
                }
                else
                {
                    AdjusmentValue = T.CostPerUnit * T.Adjusment;
                }

            }


            prm.Add("@Adjusment", T.Adjusment);
            prm.Add("@CostPerUnit", T.CostPerUnit);
            prm.Add("@StockAdjusmentId", T.StockAdjusmentId);
            prm.Add("@AdjusmentValue", AdjusmentValue);

          await  _db.ExecuteAsync($"Update StockAdjusmentItems SET ItemId=@ItemId,CostPerUnit=@CostPerUnit,AdjusmentValue=@AdjusmentValue,Adjusment=@Adjusment where StockAdjusmentId = @StockAdjusmentId and CompanyId = @CompanyId and id=@id", prm);


            if (T.Adjusment == adjusment)
            {
                return;
            }
            else
            {
                T.Adjusment = T.Adjusment - adjusment;
                string sql = $@"declare @@locationId int 
            set @@locationId=(Select LocationId From StockAdjusment where CompanyId = @CompanyId and id = @StockAdjusmentId)
            select 
            (select @@locationId) as StockId,
            (Select AllStockQuantity from Items where Items.id = @ItemId  and Items.CompanyId = @CompanyId)as Quantity,
            (Select StockCount from LocationStock where ItemId = @ItemId   and LocationId = @@locationId and CompanyId = @CompanyId)as LocationsStockCount,
            (Select id from LocationStock where ItemId = @ItemId and LocationId = @@locationId and CompanyId =   @CompanyId)   as    LocationStockId";
                var sorgu =await _db.QueryAsync<StockAdjusmentStockUpdate>(sql, prm);//
                int LocationId = sorgu.First().StokId;

                float? stockQuantity = sorgu.First().Miktar;
                float? NewQuantity = stockQuantity + T.Adjusment; //Tablodaki değer ile itemdeki değeri toplayarak yeni bir stok(quanitity) elde ediyoruz.
                prm.Add("@NewQuantity", NewQuantity);
                prm.Add("@LocationId", LocationId);

                await _db.ExecuteAsync($"Update Items SET AllStockQuantity =@NewQuantity where id = @ItemId  and CompanyId = @CompanyId", prm); //Stok tablosuna yeni değeri güncelleiyoruz.


                prm.Add("@User", UserId);
                prm.Add("@StockMovementQuantity", adjusment);
                prm.Add("@PreviousValue", stockQuantity);
                prm.Add("@Process", "AllStock");
                prm.Add("@Operation", "+");

                prm.Add("@Date", DateTime.Now);
                prm.Add("@Where", "StockAdjusmentDelete");
                await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewQuantity,@Date,@User,@CompanyId,@LocationId,@ItemId)", prm);

                float? stockCount = sorgu.First().StokAdeti;
                float? NewStockCount = stockCount + T.Adjusment;
                var stocklocationId = sorgu.First().DepoStokId;

                prm.Add("@stocklocationId", stocklocationId);

                prm.Add("@NewStockCount", NewStockCount); //Yeni count değerini tabloya güncelleştiriyoruz.
               await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewStockCount where id = @stocklocationId  and CompanyId = @CompanyId", prm);

                prm.Add("@PreviousValue", stockCount);
                prm.Add("@Process", "LocationStock");
                prm.Add("@Operation", "+");
                prm.Add("@Date", DateTime.Now);
                prm.Add("@Where", "StockAdjusmentDelete");
                await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewStockCount,@Date,@User,@CompanyId,@LocationId,@ItemId)", prm);


            }
        }
    }
}
