using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using DAL.StockControl;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.BomDTO;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.ProductOperationsBomDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DAL.Repositories
{
    public class ManufacturingOrderItemRepository : IManufacturingOrderItemRepository
    {
        private readonly IDbConnection _db;
        private readonly ILocationStockRepository _locstok;
        private readonly IStockControl _control;


        public ManufacturingOrderItemRepository(ILocationStockRepository locstok, IDbConnection db, IStockControl control)
        {
            _locstok = locstok;
            _db = db;
            _control = control;
        }

        public async Task BuyStockControl(ManufacturingPurchaseOrder T, int? missing, int CompanyId)
        {
            int? LocationId = T.LocationId;
            DynamicParameters param = new DynamicParameters();
            param.Add("@Tip", "Ingredients");
            param.Add("@ManufacturingOrderItemId", T.ManufacturingOrderItemId);
            param.Add("@OrderId", T.ManufacturingOrderId);
            param.Add("@ItemId", T.ItemId);
            param.Add("@CompanyId", CompanyId);
            param.Add("@LocationId", LocationId);
            // materyalin DefaultPrice
            // Bul
            string sqlb = $@"select ISNULL(SUM(Quantity),0) from Orders 
                left join OrdersItem on OrdersItem.OrdersId = Orders.id and Orders.LocationId=@LocationId
                and OrdersItem.ItemId = @ItemId where Orders.CompanyId = @CompanyId
                and DeliveryId = 1 and Orders.ManufacturingOrderId={T.ManufacturingOrderId} and Orders.IsActive=1";
            var expectedsorgu = await _db.QueryAsync<float>(sqlb, param);
            float expected = expectedsorgu.First();


            List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select  (Select ISNULL(AllStockQuantity, 0) from Items where Items.id = @ItemId and Items.CompanyId = @CompanyId)as Quantity, (Select ISNULL(id, 0) from LocationStock where ItemId =@ItemId and LocationId = @LocationId and CompanyId = @CompanyId)   as LocationStockId, (select ISNULL(DefaultPrice, 0) From Items where CompanyId = @CompanyId and id = @ItemId)as  DefaultPrice,(Select ManufacturingOrderItems.[Availability] from ManufacturingOrderItems where CompanyId=@CompanyId and Tip='Ingredients' and id=@ManufacturingOrderItemId and OrderId=@OrderId and ItemId=@ItemId)as [Availability]", param)).ToList();
            float DefaultPrice = sorgu.First().DefaultPrice;
            int Availability = sorgu.First().Availability;
            float locationstock = sorgu.First().LocationStock;
            param.Add("@Cost", DefaultPrice * T.Quantity);
            param.Add("@LocationStockId", sorgu.First().LocationStockId);



            //Avaibility Hesapla Stoktaki miktar işlemi gerçekleştirmeye yetiyormu kontrol et

            string Tip = "Material";

            if (sorgu.First().LocationStockId == 0)
            {
                await _locstok.Insert(Tip, T.ItemId, CompanyId, LocationId);
                param.Add("@Availability", 0);
            }
            else
            {

                var Count = await _db.QueryFirstAsync<int>($"Select ISNULL(Rezerve.RezerveCount,0)as Count from Rezerve where CompanyId=@CompanyId and ItemId=@ItemId and ManufacturingOrderId=@OrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Rezerve.Status=1", param);

                var Counts = Count;

                if (Availability == 2)
                {
                    param.Add("@Availability", 2);
                    param.Add("@RezerveCount", Counts);
                    param.Add("@LocationStockCount", locationstock);

                }
                else if (missing * (-1) <= expected && expected > 0)
                {
                    param.Add("@Availability", 1);
                    param.Add("@RezerveCount", Counts);
                    param.Add("@LocationStockCount", locationstock);
                }

                else
                {


                    param.Add("@RezerveCount", Counts + locationstock);
                    param.Add("@LocationStockCount", 0);
                    await _db.ExecuteAsync($"Update LocationStock set StockCount=@LocationStockCount where CompanyId=@CompanyId and   id=@LocationStockId", param);
                    param.Add("@Availability", 0);
                }


                param.Add("@Status", 1);

                await _db.ExecuteAsync($"Update Rezerve set  Tip=@Tip,ItemId=@ItemId,RezerveCount=@RezerveCount,LocationId=@LocationId,Status=@Status,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId and ItemId=@ItemId and ManufacturingOrderItemId=@ManufacturingOrderItemId ", param);



            }
            string sql = $@"Update ManufacturingOrderItems Set Tip=@Tip,ItemId=@ItemId,Cost=@Cost,Availability=@Availability where CompanyId=@CompanyId and OrderId=@OrderId and ItemId=@ItemId and id=@ManufacturingOrderItemId  ";
            await _db.ExecuteAsync(sql, param);

        }

        public async Task DeleteItems(ManufacturingDeleteItems T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@OrdersId", T.OrdersId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@CompanyId", CompanyId);
            List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"select  (select id from Rezerve where ManufacturingOrderId = @OrdersId and CompanyId = @CompanyId and ItemId = @ItemId and Status=1 and ManufacturingOrderItemId=@id )as RezerveId", prm)).ToList();
            prm.Add("@SalesOrderRezerveId", sorgu.First().RezerveId);
            if (T.ItemId != 0)
            {
                await _db.ExecuteAsync($"Update Orders set ManufacturingOrderId=NULL,ManufacturingOrderItemId=NULL where ManufacturingOrderId=@OrdersId and CompanyId=@CompanyId  and ManufacturingOrderItemId=@id and Orders.IsActive=1 and Orders.DeliveryId=1", prm);
                await _db.ExecuteAsync($"Delete From ManufacturingOrderItems  where ItemId = @ItemId and CompanyId = @CompanyId and id=@id and OrderId=@OrdersId", prm);

                prm.Add("@Status", 4);
                await _db.ExecuteAsync($"Update Rezerve set Status=@Status where ManufacturingOrderId=@OrdersId and CompanyId=@CompanyId and ItemId=@ItemId and ManufacturingOrderItemId=@id   and id=@SalesOrderRezerveId", prm);
            }
            else
            {
                await _db.ExecuteAsync($"Delete From ManufacturingOrderItems  where  CompanyId = @CompanyId and id=@id and OrderId=@OrdersId", prm);

            }
        }

        public async Task DeleteStockControl(IdControl T, int CompanyId, int UserId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@id", T.id);
            param.Add("@DateTime", DateTime.Now);
            param.Add("@User", UserId);
            param.Add("@IsActive", false);
            List<bool> IsActived = (await _db.QueryAsync<bool>($"select IsActive from ManufacturingOrder where id=@id and CompanyId=@CompanyId ", param)).ToList();
            if (IsActived[0] == false)
            {

            }
            else
            {
                List<int> ItemsCount = (await _db.QueryAsync<int>($"select id from Rezerve where ManufacturingOrderId=@id and CompanyId=@CompanyId and Status=1", param)).ToList();
                await _db.QueryAsync($"Update ManufacturingOrder Set IsActive=@IsActive,DeletedUser=@User,DeleteDate=@DateTime where id = @id and CompanyId = @CompanyId ", param);
                foreach (var item in ItemsCount)
                {
                    param.Add("@RezerveId", item);
                    param.Add("@Status", 4);
                    await _db.ExecuteAsync($"Update Rezerve set Status=@Status where  CompanyId=@CompanyId and id=@RezerveId", param);

                }
                string sqlsorgu = $@"Select * from ManufacturingOrderItems where CompanyId=@CompanyId and ManufacturingOrderItems.OrderId=@id";
                var ManuItems = await _db.QueryAsync<ManufacturingOrderResponse>(sqlsorgu, param);
                foreach (var item in ManuItems)
                {
                    ManufacturingDeleteItems A = new ManufacturingDeleteItems();

                    A.OrdersId = T.id;
                    A.id = item.id;
                    if (item.Tip == "Ingredients")
                    {
                        A.ItemId = (int)item.ItemId;
                    }
                    await DeleteItems(A, CompanyId);
                }


            }
        }

        public async Task<IEnumerable<ManufacturingOrderItemsIngredientsDetail>> IngredientsDetail(int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", id);
            var Location = await _db.QueryAsync<int>($"Select LocationId From ManufacturingOrder where CompanyId = {CompanyId} and id = {id}");
            prm.Add("@LocationId", Location.First());
            string sql = $@"  select moi.id,moi.Tip,moi.ItemId,Items.Name,ISNULL(Notes,'')AS Note,moi.PlannedQuantity as Quantity,moi.Cost,moi.Availability,
            (ISNULL(LocationStock.StockCount,0)-ISNULL(SUM(DISTINCT(Rezerve.RezerveCount)),0))+(ISNULL(rez.RezerveCount,0))-(ISNULL(moi.PlannedQuantity,0))+ISNULL(SUM(DISTINCT(case when Orders.DeliveryId=1 then OrdersItem.Quantity else 0 end)),0)AS missing
            from ManufacturingOrderItems moi
            left join ManufacturingOrder mao on mao.id=moi.OrderId
            left join Items on Items.id=moi.ItemId
            left join LocationStock on LocationStock.ItemId=moi.ItemId and LocationStock.LocationId=@LocationId
            left join OrdersItem on OrdersItem.ItemId=moi.ItemId 
            right join Orders on Orders.id=OrdersItem.OrdersId and Orders.ManufacturingOrderId=mao.id 
            left join Rezerve on Rezerve.ItemId=Items.id  and Rezerve.Status=1  and Rezerve.LocationId=@LocationId
			 left join Rezerve rez on rez.ManufacturingOrderId=mao.id and rez.ManufacturingOrderItemId=moi.id  and rez.Status=1  and rez.LocationId=@LocationId
            where mao.id=@id and moi.Tip='Ingredients' and mao.LocationId=@LocationId  and mao.Status!=3 and mao.CompanyId=@CompanyId
            Group by moi.id,moi.Tip,moi.ItemId,Items.Name,moi.Notes,moi.PlannedQuantity ,moi.Cost,moi.Availability,
            moi.PlannedQuantity,LocationStock.StockCount,rez.RezerveCount,orders.DeliveryId,OrdersItem.Quantity   
            ";
            var IngredientsDetail = await _db.QueryAsync<ManufacturingOrderItemsIngredientsDetail>(sql, prm);

            return IngredientsDetail.ToList();
        }

        public async Task<int> IngredientsInsert(ManufacturingOrderItemsIngredientsInsert T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@Tip", "Ingredients");
            param.Add("@OrderId", T.OrderId);
            param.Add("@ItemId", T.ItemId);
            param.Add("@Notes", T.Note);
            param.Add("@CompanyId", CompanyId);
            param.Add("@LocationId", T.LocationId);
            param.Add("@PlannedQuantity", T.Quantity);
            int rezerveid = 0;
            //param.Add("@Cost", T.Cost);
            //param.Add("@Availability", T.Availability);
            List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select  (Select ISNULL(id, 0) from LocationStock where ItemId = @ItemId  and LocationId = (Select LocationId From ManufacturingOrder where CompanyId =@CompanyId  and id = @OrderId) and CompanyId = @CompanyId)   as LocationStockId, (select ISNULL(DefaultPrice, 0) From Items where CompanyId = @CompanyId and id = @ItemId)as  DefaultPrice", param)).ToList();

            param.Add("@LocationStockId", sorgu.First().LocationStockId);

            //yeni costu buluyoruz.
            float DefaultPrice = sorgu.First().DefaultPrice;
            var newCost = T.Quantity * DefaultPrice;
            param.Add("@Cost", newCost);

            float? rezerve = _control.Count(T.ItemId, CompanyId, T.LocationId);

            if (rezerve >= 0)
            {

                if (rezerve >= T.Quantity)//Stok sayısı istesnilenden büyük ise rezerve sayısıadetolur
                {
                    param.Add("@RezerveCount", T.Quantity);
                    var newStockCount = rezerve - (T.Quantity);
                    param.Add("@LocationStockCount", newStockCount);
                    param.Add("@Availability", 2);

                }
                else
                {

                    param.Add("@RezerveCount", rezerve);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                    param.Add("@LocationStockCount", 0);
                    param.Add("@Availability", 0);
                }
                param.Add("@Status", 1);

                rezerveid = await _db.QuerySingleAsync<int>($"Insert into Rezerve  (Tip,ManufacturingOrderId,ItemId,RezerveCount,LocationId,Status,LocationStockCount,CompanyId) OUTPUT INSERTED.[id] values (@Tip,@OrderId,@ItemId,@RezerveCount,@LocationId,@Status,@LocationStockCount,@CompanyId)", param);


            }
            else
            {
                param.Add("@Availability", 0);
                rezerveid = await _db.QuerySingleAsync<int>($"Insert into Rezerve  (Tip,ManufacturingOrderId,ItemId,RezerveCount,LocationId,Status,LocationStockCount,CompanyId) OUTPUT INSERTED.[id] values (@Tip,@OrderId,@ItemId,@RezerveCount,@LocationId,@Status,@LocationStockCount,@CompanyId)", param);
            }

            string sql = $@"Insert into ManufacturingOrderItems (Tip,OrderId,ItemId,Notes,PlannedQuantity,Cost,Availability,CompanyId) OUTPUT INSERTED.[id] values (@Tip,@OrderId,@ItemId,@Notes,@PlannedQuantity,@Cost,@Availability,@CompanyId)";
            int ManuId = await _db.QuerySingleAsync<int>(sql, param);
            param.Add("@ManufacturingOrderItemId", ManuId);

            param.Add("@SalesOrderId", T.SalesOrderId);
            param.Add("@SalesOrderItemId", T.SalesOrderItemId);
            if (T.SalesOrderId != 0)
            {
                await _db.ExecuteAsync($"Update Rezerve set ManufacturingOrderItemId=@ManufacturingOrderItemId where CompanyId=@CompanyId and id={rezerveid} ", param);
            }
            else
            {
                await _db.ExecuteAsync($"Update Rezerve set ManufacturingOrderItemId=@ManufacturingOrderItemId where CompanyId=@CompanyId and id={rezerveid}", param);

            }
            if (T.SalesOrderId != 0)
            {
                DynamicParameters prm = new DynamicParameters();
                prm.Add("@OrderId", T.OrderId);
                prm.Add("@CompanyId", CompanyId);
                prm.Add("@ItemId", T.ItemId);
                prm.Add("@SalesOrderId", T.SalesOrderId);
                prm.Add("@SalesOrderItemId", T.SalesOrderItemId);
                string sqlr = $@"select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.SalesOrderId=@SalesOrderId and ManufacturingOrder.SalesOrderItemId=@SalesOrderItemId and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrder.Status !=3 and ManufacturingOrder.IsActive=1";
                var availability = await _db.QueryAsync<int>(sqlr, prm);
                prm.Add("@Ingredients", availability.First());
                await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);

            }



            return ManuId;
        }

        public async Task IngredientsUpdate(ManufacturingOrderItemsIngredientsUpdate T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@id", T.id);
            param.Add("@OrderId", T.OrderId);
            param.Add("@ItemId", T.ItemId);
            param.Add("@Tip", " Ingredients");
            param.Add("@LocationId", T.LocationId);
            param.Add("@PlannedQuantity", T.Quantity);
            string sqlu = $@"Update ManufacturingOrderItems SET  PlannedQuantity = @PlannedQuantity where CompanyId = @CompanyId and id = @id and OrderId = @OrderId";
            await _db.ExecuteAsync(sqlu, param);

            List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select (Select ISNULL(id, 0) from LocationStock where ItemId = @ItemId  and LocationId = (Select LocationId From ManufacturingOrder where CompanyId =@CompanyId  and id = @OrderId) and CompanyId = @CompanyId)   as LocationStockId, (select ISNULL(DefaultPrice, 0) From Items where CompanyId = @CompanyId and id = @ItemId)as  DefaultPrice", param)).ToList();

            param.Add("@LocationStockId", sorgu.First().LocationStockId);

            float DefaultPrice = sorgu.First().DefaultPrice;
            var newCost = T.Quantity * DefaultPrice;

            float? rezerve = _control.Count(T.ItemId, CompanyId, T.LocationId);
            string sqld = $@"select ISNULL(RezerveCount,0) from Rezerve where ManufacturingOrderId=@OrderId and ItemId=@ItemId and LocationId=@LocationId and ManufacturingOrderItemId=@id and Status=1";
            var rezervestockCount = await _db.QueryAsync<float>(sqld, param);
            string sqlb = $@"select ISNULL(SUM(Quantity),0) from Orders 
                left join OrdersItem on OrdersItem.OrdersId = Orders.id and Orders.LocationId=@LocationId
                and OrdersItem.ItemId = @ItemId where Orders.CompanyId = @CompanyId
                and DeliveryId = 1 and Orders.ManufacturingOrderId={T.OrderId} and Orders.IsActive=1 ";
            var expectedsorgu = await _db.QueryAsync<float>(sqlb, param);
            float expected = expectedsorgu.First();

            float rezervecount = 0;
            if (rezervestockCount.Count() == 0)
            {
                rezervecount = 0;
            }
            else
            {
                rezervecount = rezervestockCount.First();
            }






            if (rezervecount >= T.Quantity)
            {
                param.Add("@Availability", 2);
                var newStock = rezervecount - T.Quantity;
                param.Add("@LocationStockCount", rezerve + newStock);
                param.Add("@RezerveCount", T.Quantity);
                param.Add("@Status", 1);
                await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId and LocationId=@LocationId and ManufacturingOrderItemId=@id and ItemId=@ItemId  and Status=1 ", param);

            }

            else if (rezervecount < T.Quantity)
            {
                float? newQuantity = T.Quantity - rezervecount;
                if (rezerve >= newQuantity)
                {

                    var newStockCount = rezerve - newQuantity;
                    param.Add("@LocationStockCount", newStockCount);
                    var newrezervecount = rezervecount + newQuantity;
                    param.Add("@RezerveCount", newrezervecount);
                    param.Add("@Availability", 2);
                }
                else if ((T.Quantity - rezervecount <= expected && expected > 0))
                {
                    param.Add("@Availability", 1);
                    param.Add("@RezerveCount", rezervecount);
                }
                else
                {
                    param.Add("@Availability", 0);
                    param.Add("@RezerveCount", rezerve + rezervecount);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                    param.Add("@LocationStockCount", 0);

                }
                param.Add("@Status", 1);
                await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId and LocationId=@LocationId and ManufacturingOrderItemId=@id and ItemId=@ItemId  and Status=1 ", param);

            }
            else if ((T.Quantity - rezervecount <= expected && expected > 0))
            {
                param.Add("@Availability", 1);
            }
            param.Add("@id", T.id);
            param.Add("@OrderId", T.OrderId);
            param.Add("@ItemId", T.ItemId);
            param.Add("@Notes", T.Note);
            param.Add("@PlannedQuantity", T.Quantity);
            param.Add("@Cost", newCost);
            param.Add("@CompanyId", CompanyId);
            string sql = $@"Update ManufacturingOrderItems SET ItemId = @ItemId , Notes = @Notes , PlannedQuantity = @PlannedQuantity, Cost = @Cost,Availability = @Availability
                            where CompanyId = @CompanyId and id = @id and OrderId = @OrderId";
            await _db.ExecuteAsync(sql, param);


            if (T.SalesOrderId != 0)
            {
                DynamicParameters prm = new DynamicParameters();
                prm.Add("@OrderId", T.OrderId);
                prm.Add("@CompanyId", CompanyId);
                prm.Add("@ItemId", T.ItemId);
                prm.Add("@SalesOrderId", T.SalesOrderId);
                prm.Add("@SalesOrderItemId", T.SalesOrderItemId);
                string sqlr = $@"select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.SalesOrderId=@SalesOrderId and ManufacturingOrder.SalesOrderItemId=@SalesOrderItemId and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrder.Status !=3 and ManufacturingOrder.IsActive=1";
                var availability = await _db.QueryAsync<int>(sqlr, prm);
                prm.Add("@Ingredients", availability.First());
                await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);

            }

        }

        public async Task InsertOrderItems(int id, int? SalesOrderId, int? SalesOrderItemId, int CompanyId)
        {
            //Eklenen Orderin Item ını buluyoruz
            //List int ile ayrı model açmadan tek değer parametleri alabiliyoruz
            var ItemIdAL = await _db.QueryFirstAsync<int>($"Select ItemId From ManufacturingOrder where CompanyId = {CompanyId} and id = {id}");
            int ProductId = ItemIdAL;
            var LocationIdAl = await _db.QueryFirstAsync<int>($"Select LocationId From ManufacturingOrder where CompanyId = {CompanyId} and id = {id}");
            int LocationId = LocationIdAl;
            var adetbul = await _db.QueryFirstAsync<int>($"Select PlannedQuantity From ManufacturingOrder where CompanyId = {CompanyId} and id = {id}");
            //Eklenen Ordera ait ıtemin  Bomlarını buluyoruz
            var BomList = await _db.QueryAsync<BOM>($"Select * From Bom where CompanyId = {CompanyId} and ProductId = {ProductId} and IsActive = 1");



            foreach (var item in BomList)
            {

                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Ingredients");
                param.Add("@OrderId", id);
                param.Add("@ItemId", item.MaterialId);
                param.Add("@Notes", item.Note);
                param.Add("@SalesOrderId", SalesOrderId);
                param.Add("@SalesOrderItemId", SalesOrderItemId);
                param.Add("@CompanyId", CompanyId);
                param.Add("@LocationId", LocationId);
                // materyalin DefaultPrice,stockıd,locatinstock,locationstockId,RezerveCount
                // Bul
                List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($@" select (select ISNULL(DefaultPrice, 0) From Items where CompanyId = @CompanyId and id = @ItemId)as  DefaultPrice,(Select ISNULL(id, 0) from LocationStock where ItemId=@ItemId and LocationId = @LocationId and CompanyId = @CompanyId)   as LocationStockId,(select Tip from Items where id=@ItemId)as Tip,(select ContactId from Orders where id=@SalesOrderId)as ContactId", param)).ToList();

                param.Add("@PlannedQuantity", item.Quantity * adetbul);
                float DefaultPrice = sorgu.First().DefaultPrice;
                param.Add("@Cost", DefaultPrice * item.Quantity * adetbul);
                //Avaibility Hesapla Stoktaki miktar işlemi gerçekleştirmeye yetiyormu kontrol et


                string Tip = "Material";
                string sqlb = $@"select ISNULL(SUM(Quantity),0) from Orders 
                left join OrdersItem on OrdersItem.OrdersId = Orders.id
                and OrdersItem.ItemId = @ItemId where Orders.CompanyId = @CompanyId
                and DeliveryId = 1 and Orders.SalesOrderId is null and Orders.ManufacturingOrderId is null and Orders.IsActive=1";
                var expected = await _db.QueryFirstAsync<int>(sqlb, param);
                param.Add("@LocationStockId", sorgu.First().LocationStockId);


                if (sorgu.First().LocationStockId == 0)
                {
                    await _locstok.Insert(Tip, item.MaterialId, CompanyId, LocationId);
                    param.Add("@Availability", 0);
                }
                else
                {
                    int? rezerve = _control.Count(item.MaterialId, CompanyId, LocationId);
                    if (SalesOrderId != 0)
                    {
                        string sqld = $@"select ISNULL(RezerveCount,0) from Rezerve where SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and ItemId=@ItemId and LocationId=@LocationId and Status=1";
                        var rezerv = await _db.QueryAsync<float>(sqld, param);

                        float rezervestockCount = 0;
                        if (rezerv.Count() == 0)
                        {

                            param.Add("@RezervTip", sorgu.First().Tip);
                            param.Add("@RezerveDeger", 0);
                            param.Add("@Status", 1);
                            param.Add("@LocationStockCount", rezerve);
                            param.Add("@ContactId", sorgu.First().ContactId);

                            rezervestockCount = 0;
                            await _db.QueryAsync($"Insert into Rezerve (SalesOrderId,SalesOrderItemId,Tip,ItemId,RezerveCount,CustomerId,LocationId,Status,LocationStockCount,CompanyId) values (@SalesOrderId,@SalesOrderItemId,@RezervTip,@ItemId,@RezerveDeger,@ContactId,@LocationId,@Status,@LocationStockCount,@CompanyId)", param);
                        }
                        else
                        {
                            rezervestockCount = rezerv.First();
                        }
                        if (rezervestockCount == item.Quantity * adetbul)
                        {
                            param.Add("@Availability", 2);

                            param.Add("@LocationStockCount", rezerve);
                            param.Add("@RezerveCount", rezervestockCount);
                        }

                        else if (rezervestockCount > item.Quantity * adetbul)
                        {
                            param.Add("@Availability", 2);

                            var newStock = rezervestockCount - (item.Quantity * adetbul);
                            param.Add("@RezerveCount", item.Quantity * adetbul);
                            param.Add("@LocationStockCount", rezerve + newStock);

                        }

                        else
                        {
                            float newQuantity = (item.Quantity * adetbul) - rezervestockCount;
                            if (rezerve >= newQuantity)
                            {

                                var newStockCount = rezerve - newQuantity;
                                param.Add("@LocationStockCount", newStockCount);
                                var newrezervecount = rezervestockCount + newQuantity;
                                param.Add("@RezerveCount", newrezervecount);
                                param.Add("@Availability", 2);

                            }
                            else
                            {

                                param.Add("@RezerveCount", rezerve + rezervestockCount);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                                param.Add("@LocationStockCount", 0);
                                param.Add("@Availability", 0);

                            }
                            param.Add("@id", id);
                            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount,ManufacturingOrderId=@OrderId where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and LocationId=@LocationId and ItemId=@ItemId ", param);


                        }

                    }
                    else
                    {
                        if (rezerve >= 0)
                        {



                            int adet;
                            if (adetbul == 0)
                            {
                                adet = 0;
                            }
                            else
                            {
                                adet = adetbul;
                            }


                            if (rezerve >= item.Quantity * adet)//Stok sayısı istesnilenden büyük ise rezerve sayısıadetolur
                            {
                                param.Add("@RezerveCount", item.Quantity * adet);
                                param.Add("@Availability", 2);
                                param.Add("@LocationStockCount", rezerve - item.Quantity * adet);

                            }
                            else
                            {

                                param.Add("@RezerveCount", rezerve);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                                param.Add("@Availability", 0);
                                param.Add("@LocationStockCount", 0);

                            }
                            param.Add("@Status", 1);
                            param.Add("@id", id);
                            await _db.ExecuteAsync($"Insert into Rezerve  (Tip,ManufacturingOrderId,ItemId,RezerveCount,LocationId,Status,LocationStockCount,CompanyId) values   (@Tip,@id,@ItemId,@RezerveCount,@LocationId,@Status,@LocationStockCount,@CompanyId)", param);

                        }



                        else if ((rezerve - (item.Quantity * adetbul)) * (-2) <= expected && expected != 0)
                        {
                            param.Add("@Availability", 1);
                        }
                        else
                        {

                            param.Add("@Availability", 0);
                        }
                    }

                }
                string sql = $@"Insert into ManufacturingOrderItems (Tip,OrderId,ItemId,Notes,PlannedQuantity,Cost,Availability,CompanyId) values (@Tip,@OrderId,@ItemId,@Notes,@PlannedQuantity,@Cost,@Availability,@CompanyId)";
                await _db.ExecuteAsync(sql, param);
                string sqlf = $@"(select ManufacturingOrderItems.id from ManufacturingOrderItems where CompanyId=@CompanyId and ItemId=@ItemId and OrderId=@OrderId)";
                var ManuItemId = await _db.QueryFirstAsync<int>(sqlf, param);
                param.Add("@ManufacturingOrderItemId", ManuItemId);
                if (SalesOrderId != 0)
                {
                    await _db.ExecuteAsync($"Update Rezerve set ManufacturingOrderItemId=@ManufacturingOrderItemId where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and LocationId=@LocationId and ManufacturingOrderId=@OrderId and ItemId=@ItemId ", param);
                }
                else
                {
                    await _db.ExecuteAsync($"Update Rezerve set ManufacturingOrderItemId=@ManufacturingOrderItemId where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId and LocationId=@LocationId and ItemId=@ItemId ", param);
                }





            }

            if (BomList.Count() != 0)
            {
                if (SalesOrderId != 0)
                {
                    DynamicParameters dynamic = new DynamicParameters();
                    dynamic.Add("@OrderId", id);
                    dynamic.Add("@CompanyId", CompanyId);
                    dynamic.Add("@ItemId", ProductId);
                    dynamic.Add("@SalesOrderId", SalesOrderId);
                    dynamic.Add("@SalesOrderItemId", SalesOrderItemId);
                    dynamic.Add("@LocationId", LocationId);

                    string sqlr = $@"select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.SalesOrderId=@SalesOrderId and ManufacturingOrder.SalesOrderItemId=@SalesOrderItemId and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrder.Status !=3 and ManufacturingOrder.IsActive=1";
                    var Ingredients = await _db.QuerySingleAsync<int>(sqlr, dynamic);
                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@Ingredients", Ingredients);
                    param3.Add("@CompanyId", CompanyId);
                    param3.Add("@ItemId", ProductId);
                    param3.Add("@SalesOrderId", SalesOrderId);
                    param3.Add("@SalesOrderItemId", SalesOrderItemId);

                    string sql = "Update OrdersItem set Ingredients = @Ingredients where CompanyId = @CompanyId and OrdersId = @SalesOrderId and id = @SalesOrderItemId and ItemId = @ItemId ";
                    await _db.ExecuteAsync(sql, new { Ingredients, CompanyId, SalesOrderId, SalesOrderItemId, ItemId = ProductId });


                }
            }




            //Eklenen Ordera ait ıtemin Operation Bomlarını buluyoruz
            var OperationList = await _db.QueryAsync<ProductOperationsBOM>($"Select * From ProductOperationsBom where CompanyId = {CompanyId} and ItemId = {ProductId}");

            foreach (var item in OperationList)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Operations");
                param.Add("@OrderId", id);
                param.Add("@OperationId", item.OperationId);
                param.Add("@ResourceId", item.ResourceId);
                param.Add("@PlannedTime ", item.OperationTime * adetbul);
                param.Add("@Status", 0);
                param.Add("@CostPerHour", item.CostHour);
                param.Add("@Cost", (item.CostHour / 60 / 60) * item.OperationTime * adetbul);
                param.Add("@CompanyId", CompanyId);

                string sql = $@"Insert into ManufacturingOrderItems (Tip,OrderId,OperationId,ResourceId,PlannedTime,Status,CostPerHour,Cost,CompanyId) values (@Tip,@OrderId,@OperationId,@ResourceId,@PlannedTime,@Status,@CostPerHour,@Cost,@CompanyId)";
                await _db.ExecuteAsync(sql, param);
            }
            if (SalesOrderId != 0)
            {
                DynamicParameters prm = new DynamicParameters();
                prm.Add("@CompanyId", CompanyId);
                prm.Add("@ItemId", ProductId);
                prm.Add("@SalesOrderId", SalesOrderId);
                prm.Add("@LocationId", LocationId);
                prm.Add("@SalesOrderItemId", SalesOrderItemId);
                var mis = await _db.QueryFirstAsync<float>($@"select (    (Select ISNULL(Quantity,0) from OrdersItem 
            left join Orders on Orders.id=OrdersItem.OrdersId
            where Orders.id=@SalesOrderId and OrdersItem.id=@SalesOrderItemId)+
            (ISNULL((select SUM(PlannedQuantity) from ManufacturingOrder where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId),0)-
            (ISNULL((select RezerveCount from Rezerve where SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and ItemId=@ItemId and Status=1 ),0))))as missing", prm);

                if (mis < 0)
                {
                    prm.Add("@SalesItem", 1);
                }
                else
                {
                    prm.Add("@SalesItem", 2);
                }


                await _db.ExecuteAsync($"Update OrdersItem set SalesItem=@SalesItem where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
            }
        }

        public async Task<IEnumerable<ManufacturingOrderItemsOperationDetail>> OperationDetail(int CompanyId, int id)
        {
            string sql = $@"Select moi.id,
                            moi.OperationId,Operations.[Name] as OperationName,
                            moi.ResourceId ,Resources.[Name] as ResourceName,
                            moi.PlannedTime,moi.CostPerHour,
                            Cast(ISNULL(moi.Cost,0)as decimal(15,2)) as Cost,
                            moi.[Status]
                            From ManufacturingOrderItems moi
                            left join Operations on Operations.id = moi.OperationId
                            left join Resources on moi.ResourceId = Resources.id
                            where Tip='Operations' and moi.OrderId={id} and moi.CompanyId={CompanyId}";
            var OperationDetail = await _db.QueryAsync<ManufacturingOrderItemsOperationDetail>(sql);
            return OperationDetail.ToList();
        }

        public async Task<int> OperationsInsert(ManufacturingOrderItemsOperationsInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "Operations");
            prm.Add("@ResourceId", T.ResourceId);
            prm.Add("@OperationId", T.OperationId);
            prm.Add("@PlannedTime", T.PlannedTime);
            prm.Add("@OrderId", T.OrderId);
            prm.Add("@CompanyId", CompanyId);
            return await _db.QuerySingleAsync<int>($"Insert into ManufacturingOrderItems (Tip,OrderId,ResourceId, OperationId,PlannedTime,CompanyId) OUTPUT INSERTED.[id] values (@Tip,@OrderId,@ResourceId, @OperationId,@PlannedTime, @CompanyId)", prm);
        }

        public async Task OperationsUpdate(ManufacturingOrderItemsOperationsUpdate T, int CompanyId)
        {
            var Costbul = await _db.QueryAsync<float>($"Select CAST(({T.CostPerHour}*{T.PlannedTime})/60/60 as decimal(15,4)) ");
            float cost = Costbul.First();
            var newcost = Convert.ToDecimal(cost);
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@OrderId", T.OrderId);
            param.Add("@OperationId", T.OperationId);
            param.Add("@ResourceId", T.ResourceId);
            param.Add("@PlannedTime", T.PlannedTime);
            param.Add("@Status", T.Status);
            param.Add("CostPerHour", T.CostPerHour);
            param.Add("@Cost", newcost);
            param.Add("@CompanyId", CompanyId);
            string sql = $@"Update ManufacturingOrderItems SET OrderId = @OrderId, OperationId = @OperationId, ResourceId = @ResourceId, PlannedTime = @PlannedTime, Status = @Status,Cost = @Cost,CostPerHour=@CostPerHour
                            where CompanyId = @CompanyId and id = @id and OrderId = @OrderId";
            await _db.ExecuteAsync(sql, param);
        }

        public async Task UpdateOrderItems(ManufacturingOrderUpdate T, float eski, int CompanyId)

        {
            int id = T.id;

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@LocationId", T.LocationId);
            prm.Add("@id", id);


            //Eklenen Ordera ait ıtemin  Bomlarını buluyoruz
            var BomList = await _db.QueryAsync<BOM>($"Select id,ISNULL(ItemId,0) as MaterialId,ISNULL(PlannedQuantity,0) as Quantity,ISNULL(Notes,'') as Note from ManufacturingOrderItems where CompanyId = {CompanyId} and ManufacturingOrderItems.OrderId={id} and Tip='Ingredients'");
            var adetbul = await _db.QueryFirstAsync<float>($"Select PlannedQuantity From ManufacturingOrder where CompanyId = {CompanyId} and id = {id}");

            foreach (var item in BomList)
            {

                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Ingredients");
                param.Add("@ManufacturingOrderItemId", item.id);
                param.Add("@OrderId", id);
                param.Add("@ItemId", item.MaterialId);
                param.Add("@Notes", item.Note);
                if (adetbul == eski)
                {

                    param.Add("@PlannedQuantity", item.Quantity);
                }
                else if (adetbul > eski)
                {
                    float anadeger = item.Quantity / eski;
                    float yenideger = adetbul - eski;
                    var artışdegeri = yenideger * anadeger;
                    item.Quantity = item.Quantity + artışdegeri;
                    param.Add("@PlannedQuantity", item.Quantity);
                }
                else
                {
                    var yenideger = item.Quantity / eski;
                    var degerler = eski - adetbul;
                    item.Quantity = item.Quantity - (yenideger * degerler);
                    param.Add("@PlannedQuantity", item.Quantity);
                }


                param.Add("@CompanyId", CompanyId);
                param.Add("@LocationId", T.LocationId);
                string sql = $@"Update ManufacturingOrderItems Set PlannedQuantity=@PlannedQuantity where CompanyId=@CompanyId and OrderId=@OrderId and ItemId=@ItemId and id=@ManufacturingOrderItemId  ";
                await _db.ExecuteAsync(sql, param);
                // materyalin DefaultPrice
                // Bul
                string sqlb = $@"select ISNULL(SUM(Quantity),0) from Orders 
                left join OrdersItem on OrdersItem.OrdersId = Orders.id and Orders.LocationId=@LocationId
                and OrdersItem.ItemId = @ItemId where Orders.CompanyId = @CompanyId
                and DeliveryId = 1 and Orders.ManufacturingOrderId={id} and Orders.IsActive=1";
                var expected = await _db.QueryFirstAsync<float>(sqlb, param);

                List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select   (Select ISNULL(id, 0) from LocationStock where ItemId = @ItemId and LocationId = @LocationId and CompanyId = @CompanyId)   as LocationStockId, (select ISNULL(DefaultPrice, 0) From Items where CompanyId = @CompanyId and id = @ItemId)as  DefaultPrice", param)).ToList();
                float DefaultPrice = sorgu.First().DefaultPrice;
                param.Add("@Cost", DefaultPrice * item.Quantity);
                param.Add("@LocationStockId", sorgu.First().LocationStockId);


                float? LocationStock = _control.Count(item.MaterialId, CompanyId, T.LocationId);
                var Count = await _db.QueryAsync<int>($"Select ISNULL(Rezerve.RezerveCount,0)as Count from Rezerve where CompanyId=@CompanyId and ItemId=@ItemId and ManufacturingOrderId=@OrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Rezerve.Status=1 and Rezerve.LocationId=@LocationId", param);
                float? RezerveCounts = 0;
                float? deger;
                if (Count.Count() == 0)
                {
                    if (LocationStock >= item.Quantity)
                    {
                        deger = item.Quantity;
                    }

                    else
                    {
                        deger = LocationStock;

                    }
                    DynamicParameters prm2 = new DynamicParameters();
                    prm2.Add("@Status", 1);
                    prm2.Add("@LocationStockCount", LocationStock);
                    prm2.Add("@Tip", "Ingredients");
                    prm2.Add("@OrderId", id);
                    prm2.Add("@ManufacturingOrderItemId", item.id);
                    prm2.Add("@RezerveCount", deger);
                    prm2.Add("@ItemId", item.MaterialId);
                    prm2.Add("@LocationId", T.LocationId);
                    prm2.Add("@CompanyId", CompanyId);

                    await _db.ExecuteAsync($"Insert into Rezerve  (Tip,ManufacturingOrderId,ManufacturingOrderItemId,ItemId,RezerveCount,LocationId,Status,LocationStockCount,CompanyId) values   (@Tip,@OrderId,@ManufacturingOrderItemId,@ItemId,@RezerveCount,@LocationId,@Status,@LocationStockCount,@CompanyId)", prm2);



                }
                else
                {
                    RezerveCounts = Count.First();
                }




                if (RezerveCounts == item.Quantity)
                {

                    param.Add("@RezerveCount", RezerveCounts);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId  and ManufacturingOrderItemId=@ManufacturingOrderItemId and ItemId=@ItemId and Rezerve.LocationId=@LocationId", param);
                    param.Add("@Availability", 2);
                }
                else if (item.Quantity < RezerveCounts)
                {
                    param.Add("@RezerveCount", item.Quantity);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId  and ManufacturingOrderItemId=@ManufacturingOrderItemId and ItemId=@ItemId and Rezerve.LocationId=@LocationId", param);
                    param.Add("@Availability", 2);
                }
                else if (LocationStock > item.Quantity - RezerveCounts)
                {
                    param.Add("@RezerveCount", item.Quantity);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId  and ManufacturingOrderItemId=@ManufacturingOrderItemId and ItemId=@ItemId and Rezerve.LocationId=@LocationId", param);
                    param.Add("@Availability", 2);
                }
                else if (LocationStock == item.Quantity - RezerveCounts)
                {
                    param.Add("@RezerveCount", item.Quantity);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId  and ManufacturingOrderItemId=@ManufacturingOrderItemId and ItemId=@ItemId and Rezerve.LocationId=@LocationId", param);
                    param.Add("@Availability", 2);
                }
                else if (item.Quantity - RezerveCounts <= expected && expected > 0)
                {
                    param.Add("@Availability", 1);
                }
                else
                {

                    param.Add("@RezerveCount", LocationStock + RezerveCounts);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId  and ManufacturingOrderItemId=@ManufacturingOrderItemId and ItemId=@ItemId and Rezerve.LocationId=@LocationId", param);
                    param.Add("@Availability", 0);
                }


                string sqlw = $@"Update ManufacturingOrderItems Set Tip=@Tip,ItemId=@ItemId,Notes=@Notes,PlannedQuantity=@PlannedQuantity,Cost=@Cost,Availability=@Availability where CompanyId=@CompanyId and OrderId=@OrderId and ItemId=@ItemId and id=@ManufacturingOrderItemId  ";
                await _db.ExecuteAsync(sqlw, param);

            }
            if (T.SalesOrderId != 0)
            {
                prm.Add("@OrderId", id);
                prm.Add("@CompanyId", CompanyId);
                prm.Add("@ItemId", T.ItemId);
                prm.Add("@SalesOrderId", T.SalesOrderId);
                prm.Add("@SalesOrderItemId", T.SalesOrderItemId);
               
                string sqlr = $@"select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.SalesOrderId=@SalesOrderId and ManufacturingOrder.SalesOrderItemId=@SalesOrderItemId and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrder.Status !=3 and ManufacturingOrder.IsActive=1";
                var availability = await _db.QueryFirstAsync<int>(sqlr, prm);
                prm.Add("@Ingredients", availability);
                await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
            }




            //Eklenen Ordera ait ıtemin Operation Bomlarını buluyoruz
            var OperationList = await _db.QueryAsync<ProductOperationsBOM>($"Select ISNULL(id,0)As id,ISNULL(OperationId,0) as OperationId,ISNULL(ResourceId,0)as ResourceId,ISNULL(CostPerHour,0)as CostHour,ISNULL(PlannedTime,0)as OperationTime  from ManufacturingOrderItems where CompanyId = {CompanyId} and ManufacturingOrderItems.OrderId = {id} and Tip = 'Operations'");

            foreach (var item in OperationList)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Operations");
                param.Add("@OrderId", id);
                param.Add("@OperationId", item.OperationId);
                param.Add("@ResourceId", item.ResourceId);
                if (adetbul == eski)
                {
                    param.Add("@PlannedQuantity", item.OperationTime);
                }
                else
                {
                    var saatlik = item.OperationTime / eski;
                    item.OperationTime = saatlik * adetbul;

                }

                param.Add("@PlannedTime ", item.OperationTime);
                param.Add("@Cost", (item.CostHour / 60 / 60) * item.OperationTime);
                param.Add("@CompanyId", CompanyId);

                string sql = $@"Update ManufacturingOrderItems Set Tip=@Tip,OrderId=@OrderId,OperationId=@OperationId,ResourceId=@ResourceId,PlannedTime=@PlannedTime,Cost=@Cost where CompanyId=@CompanyId and OrderId=@OrderId and OperationId=@OperationId and ResourceId=ResourceId  ";
                await _db.ExecuteAsync(sql, param);
            }
        }
    }
}
