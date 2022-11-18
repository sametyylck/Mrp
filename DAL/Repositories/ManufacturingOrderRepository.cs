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
using static DAL.DTO.ItemDTO;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;

namespace DAL.Repositories
{
    public class ManufacturingOrderRepository : IManufacturingOrderRepository
    {
        IDbConnection _db;
        IManufacturingOrderItemRepository _manuitem;

        public ManufacturingOrderRepository(IDbConnection db, IManufacturingOrderItemRepository manuitem)
        {
            _db = db;
            _manuitem = manuitem;
        }

        public async Task<IEnumerable<ManufacturingOrderDetail>> Detail(int CompanyId, int id)
        {
            string sql = $@"
                                Select ManufacturingOrder.id , ManufacturingOrder.[Name],ManufacturingOrder.ItemId,Items.[Name] as ItemName,ManufacturingOrder.ExpectedDate,ManufacturingOrder.SalesOrderId,ManufacturingOrder.SalesOrderItemId,                                 ManufacturingOrder.ProductionDeadline,ManufacturingOrder.CreatedDate,ManufacturingOrder.PlannedQuantity,
                                ManufacturingOrder.LocationId,Locations.LocationName,ManufacturingOrder.[Status],ManufacturingOrder.Info
                                From ManufacturingOrder
                                inner join Items on Items.id = ManufacturingOrder.ItemId
                                inner join Locations on Locations.id = ManufacturingOrder.LocationId
                                where ManufacturingOrder.CompanyId = {CompanyId} and ManufacturingOrder.id = {id}";
            var Detail = await _db.QueryAsync<ManufacturingOrderDetail>(sql);
            return Detail.ToList();
        }

        public async Task DoneStock(ManufacturingStock T, int CompanyId,int UserId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@CompanyId", CompanyId);
            param.Add("@SalesOrderId", T.SalesOrderId);
            param.Add("@SalesOrderItemId", T.SalesOrderItemId);
            var BomList = await _db.QueryAsync<DoneStock>($@"Select moi.id,moi.ItemId,moi.PlannedQuantity,moi.Tip,Rezerve.id as RezerveId,ManufacturingOrder.Status,ManufacturingOrder.LocationId   from ManufacturingOrderItems moi 
            left join ManufacturingOrder on ManufacturingOrder.id=moi.OrderId 
            left join Rezerve on Rezerve.ManufacturingOrderItemId=moi.id 
            where moi.CompanyId = @CompanyId and moi.OrderId=@id and ManufacturingOrder.IsActive=1", param);
            param.Add("@Status", T.Status);
            int Status = BomList.First().Status;
            if (T.Status == 3 && Status != 3)
            {
                foreach (var item in BomList)
                {
                    if (item.Tip == "Ingredients")
                    {
                        param.Add("@ItemId", item.ItemId);
                        param.Add("@LocationId", item.LocationId);

                        string sqla = $@"select 
                          (Select ISNULL(AllStockQuantity,0) from Items where id =@ItemId and CompanyId = @CompanyId)as Quantity,
                         (Select ISNULL(StockCount,0) from LocationStock where ItemId = @ItemId
                         and LocationId = (Select ISNULL(LocationId,0) From ManufacturingOrder where CompanyId = @CompanyId and id = @id)
                         and CompanyId = @CompanyId)as LocationsStockCount,
                         (Select ISNULL(id,0) from LocationStock where ItemId=@ItemId
                         
                         and LocationId = (Select ISNULL(LocationId,0) From ManufacturingOrder where CompanyId = @CompanyId and id =@id)
                         and CompanyId = @CompanyId)   as    LocationStockId";
                        param.Add("@OrderItemId", item.id);
                        var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, param);//
                        float? stockQuantity = sorgu.First().Quantity;
                        float? NewQuantity = stockQuantity - item.PlannedQuantity; //Tablodaki değer ile itemdeki değeri toplayarak yeni bir stok(quanitity) elde ediyoruz.
                        param.Add("@NewQuantity", NewQuantity);
                        await _db.ExecuteAsync($"Update Items SET AllStockQuantity =@NewQuantity where id = @ItemId  and CompanyId = @CompanyId", param); //Stok tablosuna yeni     değeri güncelleiyoruz.

                        param.Add("@User", UserId);
                        param.Add("@StockMovementQuantity", item.PlannedQuantity);
                        param.Add("@PreviousValue", stockQuantity);
                        param.Add("@Process", "AllStock");
                        param.Add("@Date", DateTime.Now);
                        param.Add("@Operation", "-");

                        param.Add("@Where", "ManufacturingOrderDone");
                        await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewQuantity,@Date,@User,@CompanyId,@LocationId,@ItemId)", param);

                        float? stockCount = sorgu.First().LocationsStockCount;
                        float? NewStockCount = stockCount - item.PlannedQuantity;
                        var stocklocationId = sorgu.First().LocationStockId;
                        param.Add("@ManufacturingOrderItemsId", item.id);
                        param.Add("@stocklocationId", stocklocationId);
                        param.Add("@NewStockCount", NewStockCount);

                        param.Add("@PreviousValue", stockCount);
                        param.Add("@Process", "LocationStock");
                        await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewStockCount,@Date,@User,@CompanyId,@LocationId,@ItemId)", param);

                        //Yeni count değerini tabloya güncelleştiriyoruz.
                        await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewStockCount where id = @stocklocationId  and CompanyId = @CompanyId", param);
                        param.Add("@Availability", 3);
                        await _db.ExecuteAsync($"Update ManufacturingOrderItems Set Availability=@Availability where id=@OrderItemId and CompanyId=@CompanyId ", param);
                        param.Add("@Statu", 4);
                        param.Add("@RezerveId", item.RezerveId);
                        _db.Execute($"Update Rezerve Set Status=@Statu where id=@RezerveId ", param);

                    }
                    else
                    {
                        param.Add("@ManuId", item.id);
                        param.Add("@Status", 3);
                        await _db.ExecuteAsync($"Update ManufacturingOrderItems Set Status=@Status where id=@ManuId and CompanyId=@CompanyId ", param);

                    }

                }

                string sqlc = $@"select  
                 (Select ISNULL(StockCount,0) from LocationStock where ItemId = (select ManufacturingOrder.ItemId from ManufacturingOrder where CompanyId=@CompanyId  and id=@id)
                 and LocationId = (Select ISNULL(LocationId,0) From ManufacturingOrder where CompanyId =@CompanyId and id =  @id)
                 and CompanyId =@CompanyId)as LocationsStockCount ,(Select ManufacturingOrder.PlannedQuantity from ManufacturingOrder where CompanyId=@CompanyId and id=@id)as ManufacturingQuantity,
                 (Select ISNULL(id,0) from LocationStock where ItemId = (select ManufacturingOrder.ItemId from ManufacturingOrder
                 where CompanyId=@CompanyId and id=@id)
                 and LocationId = (Select ISNULL(LocationId,0) From ManufacturingOrder where CompanyId =@CompanyId and id =  @id)
                 and CompanyId =@CompanyId ) as LocationStockId,  
                (Select ISNULL(AllStockQuantity,0)from Items where id=(select ManufacturingOrder.ItemId from ManufacturingOrder where CompanyId=@CompanyId  and id=@id) and CompanyId=@CompanyId)as Quantity, 
                (Select ManufacturingOrder.ItemId from ManufacturingOrder where CompanyId=@CompanyId  and id=@id)as ItemId ";
                var sorgu4 = await _db.QueryAsync<StockAdjusmentSql>(sqlc, param);
                var ManufacturingQuantity= sorgu4.First().ManufacturingQuantity;
                var AllStock = sorgu4.First().Quantity;
                var newstock = AllStock + ManufacturingQuantity;
                var LocationStock = sorgu4.First().LocationsStockCount;
                var newlocationstock = ManufacturingQuantity + LocationStock;
                param.Add("@StockCount", newlocationstock);
                param.Add("@AllStockQuantity", newstock);
                param.Add("@ItemId", sorgu4.First().ItemId);
                param.Add("@locationId", sorgu4.First().LocationStockId);


                param.Add("@User", UserId);
                param.Add("@StockMovementQuantity", ManufacturingQuantity);
                param.Add("@PreviousValue", LocationStock);
                param.Add("@Operation", "+");
                param.Add("@Process", "LocationStock");
                param.Add("@Date", DateTime.Now);
                param.Add("@Where", "ManufacturingOrderDone");
                await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@StockCount,@Date,@User,@CompanyId,@LocationId,@ItemId)", param);

                param.Add("@PreviousValue", AllStock);
                param.Add("@Process", "AllStock");

                await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@AllStockQuantity,@Date,@User,@CompanyId,@LocationId,@ItemId)", param);

                await _db.ExecuteAsync($"Update LocationStock Set StockCount=@StockCount where id=@locationId and CompanyId=@CompanyId ", param);
                await _db.ExecuteAsync($"Update Items Set AllStockQuantity=@AllStockQuantity where id=@ItemId and CompanyId=@CompanyId ", param);
                await _db.ExecuteAsync($"Update ManufacturingOrder Set Status=@Status where id=@id and CompanyId=@CompanyId ", param);



                string sqlv = $@"    select 
		         (select PlannedQuantity from ManufacturingOrder where CompanyId=@CompanyId and id=@id and ManufacturingOrder.IsActive=1)AS Quantity,
                 (Select RezerveCount from Rezerve where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and ItemId=@ItemId)as RezerveCount,(select Tip from OrdersItem where CompanyId=@CompanyId and OrdersItem.id=@SalesOrderItemId) as Tip,
                (select Quantity from OrdersItem where id=@SalesOrderItemId and OrdersId=@SalesOrderId)as PlannedQuantity ";
                var sorgu3 = await _db.QueryAsync<LocaVarmı>(sqlv, param);//
                param.Add("@Ingredients", 3);
                param.Add("@SalesItem", 3);
                param.Add("@Production", 4);
                if (T.SalesOrderId != 0)
                {
                    float quantity = sorgu3.First().Quantity;
                    float plannedquantity = sorgu3.First().PlannedQuantity;

                    if (quantity > plannedquantity)
                    {
                        param.Add("@Tip", sorgu3.First().Tip);

                        param.Add("@Status", 1);

                        param.Add("@RezerveCount", plannedquantity + sorgu3.First().RezerveCount);
                        await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount ,ManufacturingOrderId=@id,Tip=@Tip  where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and ItemId=@ItemId ", param);
                        var kalan = quantity - plannedquantity;
                        param.Add("@Kalan", kalan);
                        await _db.ExecuteAsync($"Update LocationStock Set StockCount=@Kalan where id=@locationId and CompanyId=@CompanyId ", param);
                        await _db.ExecuteAsync($"Update OrdersItem Set Ingredients=@Ingredients,SalesItem=@SalesItem,Production=@Production where id=@SalesOrderItemId and OrdersId=@SalesOrderId ", param);
                    }
                    else
                    {

                        param.Add("@Tip", sorgu3.First().Tip);

                        param.Add("@Status", 1);

                        param.Add("@RezerveCount", quantity + sorgu3.First().RezerveCount);
                        await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount ,ManufacturingOrderId=@id,Tip=@Tip  where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and ItemId=@ItemId ", param);
                        await _db.ExecuteAsync($"Update OrdersItem Set Ingredients=@Ingredients,SalesItem=@SalesItem,Production=@Production where id=@SalesOrderItemId and OrdersId=@SalesOrderId ", param);
                    }


                }

            }
            else if (T.Status != 3 && Status == 3 && T.Status != Status)
            {
                foreach (var item in BomList)
                {
                    if (item.Tip == "Ingredients")
                    {
                        param.Add("@ItemId", item.ItemId);
                        param.Add("@LocationId", item.LocationId);

                        string sqla = $@"    select 
                      (Select ISNULL(AllStockQuantity,0) from Items where Items.id =@ItemId and CompanyId = @CompanyId)as Quantity,
                      (Select ISNULL(StockCount,0) from LocationStock where ItemId =@ItemId
                      and LocationId = @LocationId and CompanyId = @CompanyId)as LocationsStockCount,
                      (Select ISNULL(id,0) from LocationStock where ItemId =@ItemId
                      and LocationId = @LocationId and CompanyId=@CompanyId) as  LocationStockId";
                        param.Add("@OrderItemId", item.id);
                        var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, param);//
                        var stockId = sorgu.First().StockId;
                        param.Add("@stockId", stockId);

                        float? stockQuantity = sorgu.First().Quantity;
                        float? NewQuantity = stockQuantity + item.PlannedQuantity; //Tablodaki değer ile itemdeki değeri toplayarak yeni bir stok(quanitity) elde ediyoruz.
                        param.Add("@NewQuantity", NewQuantity);
                        await _db.ExecuteAsync($"Update Items SET AllStockQuantity =@NewQuantity where id = @ItemId  and CompanyId = @CompanyId", param); //Stok tablosuna yeni     değeri güncelleiyoruz.

                        param.Add("@User", UserId);
                        param.Add("@StockMovementQuantity", item.PlannedQuantity);
                        param.Add("@PreviousValue", stockQuantity);
                        param.Add("@Operation", "+");
                        param.Add("@Process", "AllStock");
                        param.Add("@Date", DateTime.Now);
                        param.Add("@Where", "ManufacturingOrderDoneRevert");
                        await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewQuantity,@Date,@User,@CompanyId,@LocationId,@ItemId)", param);


                        float? stockCount = sorgu.First().LocationsStockCount;
                        float? NewStockCount = stockCount + item.PlannedQuantity;
                        var stocklocationId = sorgu.First().LocationStockId;
                        param.Add("@ManufacturingOrderItemsId", item.id);
                        param.Add("@stocklocationId", stocklocationId);
                        param.Add("@NewStockCount", NewStockCount);
                        //Yeni count değerini tabloya güncelleştiriyoruz.
                        await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewStockCount where id = @stocklocationId  and CompanyId = @CompanyId", param);
                        param.Add("@Availability", 2);
                        await _db.ExecuteAsync($"Update ManufacturingOrderItems Set Availability=@Availability where id=@OrderItemId and CompanyId=@CompanyId ", param);
                        param.Add("@Statu", 1);
                        param.Add("@RezerveId", item.RezerveId);
                        await _db.ExecuteAsync($"Update Rezerve Set Status=@Statu where id=@RezerveId ", param);

                        param.Add("@StockMovementQuantity", item.PlannedQuantity);
                        param.Add("@PreviousValue", stockCount);
                        param.Add("@Operation", "+");
                        param.Add("@Process", "LocationStock");
                        param.Add("@Date", DateTime.Now);
                        param.Add("@Where", "ManufacturingOrderDoneRevert");
                        await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewStockCount,@Date,@User,@CompanyId,@LocationId,@ItemId)", param);

                    }
                    else
                    {

                        param.Add("@ManuId", item.id);
                        param.Add("@Status", T.Status);
                        await _db.ExecuteAsync($"Update ManufacturingOrderItems Set Status=@Status where id=@ManuId and CompanyId=@CompanyId ", param);

                    }

                }

            
                string sqlc = $@"select  
                 (Select ISNULL(StockCount,0) from LocationStock where ItemId = (select ManufacturingOrder.ItemId from ManufacturingOrder where CompanyId=@CompanyId  and id=@id)
                 and LocationId = (Select ISNULL(LocationId,0) From ManufacturingOrder where CompanyId =@CompanyId and id =  @id)
                 and CompanyId =@CompanyId)as LocationsStockCount ,(Select ManufacturingOrder.PlannedQuantity from ManufacturingOrder where CompanyId=@CompanyId and id=@id)as ManufacturingQuantity,
                 (Select ISNULL(id,0) from LocationStock where ItemId = (select ManufacturingOrder.ItemId from ManufacturingOrder
                 where CompanyId=@CompanyId and id=@id)
                 and LocationId = (Select ISNULL(LocationId,0) From ManufacturingOrder where CompanyId =@CompanyId and id =  @id)
                 and CompanyId =@CompanyId ) as LocationStockId,  
                (Select ISNULL(AllStockQuantity,0)from Items where id=(select ManufacturingOrder.ItemId from ManufacturingOrder where CompanyId=@CompanyId  and id=@id) and CompanyId=@CompanyId)as Quantity, 
                (Select ManufacturingOrder.ItemId from ManufacturingOrder where CompanyId=@CompanyId  and id=@id)as ItemId ";
                var sorgu4 = await _db.QueryAsync<StockAdjusmentSql>(sqlc, param);
                var ManufacturingQuantity = sorgu4.First().ManufacturingQuantity;
                var AllStock = sorgu4.First().Quantity;
                var newstock = AllStock - ManufacturingQuantity;
                var LocationStock = sorgu4.First().LocationsStockCount;
                var newlocationstock = ManufacturingQuantity - LocationStock;
                param.Add("@StockCount", newlocationstock);
                param.Add("@AllStockQuantity", newstock);
                param.Add("@ItemId", sorgu4.First().ItemId);
                param.Add("@locationId", sorgu4.First().LocationStockId);

                param.Add("@User", UserId);
                param.Add("@StockMovementQuantity", ManufacturingQuantity);
                param.Add("@PreviousValue", LocationStock);
                param.Add("@Operation", "-");
                param.Add("@Process", "LocationStock");
                param.Add("@Date", DateTime.Now);
                param.Add("@Where", "ManufacturingOrderDone");
                await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@StockCount,@Date,@User,@CompanyId,@LocationId,@ItemId)", param);

                param.Add("@PreviousValue", AllStock);
                param.Add("@Process", "AllStock");

                await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@AllStockQuantity,@Date,@User,@CompanyId,@LocationId,@ItemId)", param);

                await _db.ExecuteAsync($"Update LocationStock Set StockCount=@StockCount where id=@locationId and CompanyId=@CompanyId ", param);
                await _db.ExecuteAsync($"Update Items Set AllStockQuantity=@AllStockQuantity where id=@ItemId and CompanyId=@CompanyId ", param);
                await _db.ExecuteAsync($"Update ManufacturingOrder Set Status=@Status where id=@id and CompanyId=@CompanyId ", param);



                string sqlv = $@"    select 
                (select ItemId from ManufacturingOrder where CompanyId=@CompanyId and id=@id and ManufacturingOrder.IsActive=1)as ItemId,
		        (select PlannedQuantity from ManufacturingOrder where CompanyId=@CompanyId and id=@id and ManufacturingOrder.IsActive=1)AS Quantity,
                (Select RezerveCount from Rezerve where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and ItemId=(select ItemId from ManufacturingOrder where CompanyId=@CompanyId and id=@id and ManufacturingOrder.IsActive=1))as RezerveCount, (select Tip from OrdersItem where CompanyId=@CompanyId and OrdersItem.id=@SalesOrderItemId) as Tip ";
                var sorgu3 = await _db.QueryAsync<LocaVarmı>(sqlv, param);//
                param.Add("@Ingredients", 3);
                param.Add("@SalesItem", 3);
                param.Add("@Production", 4);
                if (T.SalesOrderId != 0)
                {
                    float quantity = sorgu3.First().Quantity;
                    param.Add("@Tip", sorgu3.First().Tip);
                    param.Add("@ItemId", sorgu3.First().ItemId);

                    param.Add("@Status", 1);
                    param.Add("@ContactId", sorgu3.First().ContactId);

                    param.Add("@RezerveCount", sorgu3.First().RezerveCount - quantity);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount ,ManufacturingOrderId=@id  where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and ItemId=@ItemId ", param);

                    await _db.ExecuteAsync($"Update OrdersItem Set Ingredients=@Ingredients,SalesItem=@SalesItem,Production=@Production,Tip=@Tip where id=@SalesOrderItemId and OrdersId=@SalesOrderId ", param);
                }

                await _db.ExecuteAsync($"Update ManufacturingOrder Set Status=@Status where id=@id and CompanyId=@CompanyId ", param);
            }
            else
            {
                await _db.ExecuteAsync($"Update ManufacturingOrder Set Status=@Status where id=@id and CompanyId=@CompanyId ", param);
            }
        }
        public async Task<int> Insert(ManufacturingOrderA T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@Name", T.Name);
            param.Add("@ItemId", T.ItemId);
            if (T.PlannedQuantity == 0)
            {
                param.Add("@PlannedQuantity", 1);//ilk insert edilirken 1 değerini veriyoruz daha update kısmından kullanıcı kendisi ayarlayabilecek.
            }
            else
            {
                param.Add("@PlannedQuantity", T.PlannedQuantity);
            }

            param.Add("@ProductionDeadline", DateTime.Now);
            param.Add("@CreatedDate", T.CreatedDate);
            param.Add("@LocationId", T.LocationId);
            param.Add("@SalesOrderId", T.SalesOrderId);
            param.Add("@SalesOrderItemId", T.SalesOrderItemId);
            param.Add("@Info", T.Info);
            param.Add("@Status", 0);
            param.Add("@IsActive", true);
            param.Add("@Private", T.Private);
            param.Add("@CustomerId", T.ContactId);
            param.Add("@ExpectedDate", T.ExpectedDate);
            param.Add("@Tip", T.Tip);
            if (T.SalesOrderId != 0)
            {
                await _db.ExecuteAsync($"Update OrdersItem set Tip=@Tip where CompanyId=@CompanyId and id=@SalesOrderItemId and OrdersId=@SalesOrderId ", param);
            }
            string sql = string.Empty;
            if (T.Private == true)
            {
                sql = $@"Insert into ManufacturingOrder (Private,Name,SalesOrderId,SalesOrderItemId,CustomerId,IsActive,ItemId,PlannedQuantity,ExpectedDate,ProductionDeadline,CreatedDate,LocationId,Info,Status,CompanyId)  OUTPUT INSERTED.[id] values (@Private,@Name,@SalesOrderId,@SalesOrderItemId,@CustomerId,@IsActive,@ItemId,@PlannedQuantity,@ExpectedDate,@ProductionDeadline,@CreatedDate,@LocationId,@Info,@Status,@CompanyId)";
            }
            else
            {
                param.Add("@Private", false);
                sql = $@"Insert into ManufacturingOrder (Private,Name,SalesOrderId,SalesOrderItemId,CustomerId,IsActive,ItemId,PlannedQuantity,ExpectedDate,ProductionDeadline,CreatedDate,LocationId,Info,Status,CompanyId)  OUTPUT INSERTED.[id] values (@Private,@Name,@SalesOrderId,@SalesOrderItemId,@CustomerId,@IsActive,@ItemId,@PlannedQuantity,@ExpectedDate,@ProductionDeadline,@CreatedDate,@LocationId,@Info,@Status,@CompanyId)";
            }


            int id = await _db.QuerySingleAsync<int>(sql, param);

            return id;
        }

        public async Task<IEnumerable<ManufacturingOrderDoneList>> ScheludeDoneList(ManufacturingOrderDoneList T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("location", T.LocationId);
            string sql = string.Empty;
            if (T.LocationId == null)
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            ManufacturingOrder.
            , ManufacturingOrder.[Name], ManufacturingOrder.ItemId,
			Items.[Name] AS ItemName,ManufacturingOrder.LocationId,Locations.LocationName ,
            Categories.[Name] as  CategoryName,ISNULL(ManufacturingOrder.CustomerId,0) as CustomerId,
			ISNULL(Contacts.DisplayName,'') AS Customer,ManufacturingOrder.[Status],
            ISNULL(ManufacturingOrder.PlannedQuantity,0)as PlannedQuantity,
            SUM(ISNULL(ManufacturingOrderItems.PlannedTime,0))as PlannedTime,
			ISNULL(ManufacturingOrder.MaterialCost,0)as MaterialCost,ISNULL(ManufacturingOrder.OperationCost,0) as OperationCost
			,ISNULL(ManufacturingOrder.TotalCost,0)as TotalCost,ManufacturingOrder.DoneDate
            from ManufacturingOrder
            left join ManufacturingOrderItems on ManufacturingOrder.id= ManufacturingOrderItems.OrderId 
            left join Locations on Locations.id=ManufacturingOrder.LocationId
            left join Contacts on Contacts.id=ManufacturingOrder.CustomerId
            inner join Items on Items.id=ManufacturingOrder.ItemId
            inner join Categories on Categories.id=Items.CategoryId 
            where ManufacturingOrder.CompanyId=@CompanyId and  ManufacturingOrder.Status=3 and  ManufacturingOrder.IsActive=1 and 
			(ManufacturingOrderItems.Tip = 'Operations' or  ManufacturingOrderItems.Tip is null or   ManufacturingOrderItems.Tip='Ingredients')
            Group By ManufacturingOrder.id, ManufacturingOrder.[Name],  ManufacturingOrder.ItemId,
			ManufacturingOrder.CustomerId,Contacts.DisplayName,ManufacturingOrder.LocationId,ManufacturingOrder.DoneDate,
			ManufacturingOrder.MaterialCost,ManufacturingOrder.OperationCost,ManufacturingOrder.TotalCost,
            Locations.LocationName , Items.[Name],Categories.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrder.[Status]) x
            where ISNULL(PlannedTime,0) like '%{T.PlannedTime}%' AND  ISNULL(Name,'') Like '%{T.Name}%' AND    ISNULL     (Customer,'') like '%{T.Customer}%' and
            ISNULL(ItemName,'') like '%{T.ItemName}%' and ISNULL(CategoryName,'') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity,'') like '%{T.PlannedQuantity}%' and
             ISNULL(Status,'') like '%{T.Status}%' and    ISNULL(MaterialCost,'') like '%{T.MaterialCost}%' and    ISNULL(OperationCost,'') like '%{T.OperationCost}%'
			 and    ISNULL(TotalCost,'') like '%{T.TotalCost}%'
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
            }
            else
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            ManufacturingOrder.
, ManufacturingOrder.[Name], ManufacturingOrder.ItemId,
			Items.[Name] AS ItemName,ManufacturingOrder.LocationId,Locations.LocationName ,
            Categories.[Name] as  CategoryName,ISNULL(ManufacturingOrder.CustomerId,0) as CustomerId,
			ISNULL(Contacts.DisplayName,'') AS Customer,ManufacturingOrder.[Status],
            ISNULL(ManufacturingOrder.PlannedQuantity,0)as PlannedQuantity,
            SUM(ISNULL(ManufacturingOrderItems.PlannedTime,0))as PlannedTime,
			ISNULL(ManufacturingOrder.MaterialCost,0)as MaterialCost,ISNULL(ManufacturingOrder.OperationCost,0) as OperationCost
			,ISNULL(ManufacturingOrder.TotalCost,0)as TotalCost,ManufacturingOrder.DoneDate
            from ManufacturingOrder
            left join ManufacturingOrderItems on ManufacturingOrder.id= ManufacturingOrderItems.OrderId 
            left join Locations on Locations.id=ManufacturingOrder.LocationId
            left join Contacts on Contacts.id=ManufacturingOrder.CustomerId
            inner join Items on Items.id=ManufacturingOrder.ItemId
            inner join Categories on Categories.id=Items.CategoryId 
            where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.LocationId=@location and  ManufacturingOrder.Status=3 and  ManufacturingOrder.IsActive=1 and 
			(ManufacturingOrderItems.Tip = 'Operations' or    ManufacturingOrderItems.Tip is null or   ManufacturingOrderItems.Tip='Ingredients')
            Group By ManufacturingOrder.id, ManufacturingOrder.[Name],  ManufacturingOrder.ItemId,
			ManufacturingOrder.CustomerId,Contacts.DisplayName,ManufacturingOrder.LocationId,ManufacturingOrder.DoneDate,
			ManufacturingOrder.MaterialCost,ManufacturingOrder.OperationCost,ManufacturingOrder.TotalCost,
            Locations.LocationName , Items.[Name],Categories.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrder.[Status]) x
            where ISNULL(PlannedTime,0) like '%{T.PlannedTime}%' AND  ISNULL(Name,'') Like '%{T.Name}%' AND    ISNULL     (Customer,'') like '%{T.Customer}%' and
            ISNULL(ItemName,'') like '%{T.ItemName}%' and ISNULL(CategoryName,'') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity,'') like '%{T.PlannedQuantity}%' and
             ISNULL(Status,'') like '%{T.Status}%' and    ISNULL(MaterialCost,'') like '%{T.MaterialCost}%' and    ISNULL(OperationCost,'') like '%{T.OperationCost}%'
			 and    ISNULL(TotalCost,'') like '%{T.TotalCost}%'
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
            }
            var ScheludeOpenDoneList = await _db.QueryAsync<ManufacturingOrderDoneList>(sql, param);


            return ScheludeOpenDoneList.ToList();
        }

        public async Task<int> ScheludeDoneListCount(ManufacturingOrderDoneList T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@location", T.LocationId);
            string sql = string.Empty;
            if (T.LocationId == null)
            {
                sql = $@"select COUNT(*) as kayitsayisi from(
            select * from (Select
            ManufacturingOrder.id, ManufacturingOrder.[Name], ManufacturingOrder.ItemId,
			Items.[Name] AS ItemName,ManufacturingOrder.LocationId,Locations.LocationName ,
            Categories.[Name] as  CategoryName,ISNULL(ManufacturingOrder.CustomerId,0) as CustomerId,
			ISNULL(Contacts.DisplayName,'') AS Customer,ManufacturingOrder.[Status],
            ISNULL(ManufacturingOrder.PlannedQuantity,0)as PlannedQuantity,
            SUM(ISNULL(ManufacturingOrderItems.PlannedTime,0))as PlannedTime,
			ISNULL(ManufacturingOrder.MaterialCost,0)as MaterialCost,ISNULL(ManufacturingOrder.OperationCost,0) as OperationCost
			,ISNULL(ManufacturingOrder.TotalCost,0)as TotalCost,ManufacturingOrder.DoneDate
            from ManufacturingOrder
            left join ManufacturingOrderItems on ManufacturingOrder.id= ManufacturingOrderItems.OrderId 
            left join Locations on Locations.id=ManufacturingOrder.LocationId
            left join Contacts on Contacts.id=ManufacturingOrder.CustomerId
            inner join Items on Items.id=ManufacturingOrder.ItemId
            inner join Categories on Categories.id=Items.CategoryId 
            where 
              ManufacturingOrder.Status=3 and ManufacturingOrder.CompanyId=@CompanyId and
			(ManufacturingOrderItems.Tip = 'Operations' or    ManufacturingOrderItems.Tip is null or   ManufacturingOrderItems.Tip='Ingredients') and ManufacturingOrder.IsActive=1
            Group By ManufacturingOrder.id, ManufacturingOrder.[Name],  ManufacturingOrder.ItemId,
			ManufacturingOrder.CustomerId,Contacts.DisplayName,ManufacturingOrder.LocationId,ManufacturingOrder.DoneDate,
			ManufacturingOrder.MaterialCost,ManufacturingOrder.OperationCost,ManufacturingOrder.TotalCost,
            Locations.LocationName , Items.[Name],Categories.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrder.[Status]) x
            where ISNULL(PlannedTime,0) like '%{T.PlannedTime}%' AND  ISNULL(Name,'') Like '%{T.Name}%' AND    ISNULL     (Customer,'') like '%{T.Customer}%' and
            ISNULL(ItemName,'') like '%{T.ItemName}%' and ISNULL(CategoryName,'') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity,'') like '%{T.PlannedQuantity}%' and
             ISNULL(Status,'') like '%{T.Status}%' and    ISNULL(MaterialCost,'') like '%{T.MaterialCost}%' and    ISNULL(OperationCost,'') like '%{T.OperationCost}%'
			 and    ISNULL(TotalCost,'') like '%{T.TotalCost}%')as kayisayisi";
            }
            else
            {
                sql = $@"select COUNT(*) as kayitsayisi from(
            select * from (Select
            ManufacturingOrder.id, ManufacturingOrder.[Name], ManufacturingOrder.ItemId,
			Items.[Name] AS ItemName,ManufacturingOrder.LocationId,Locations.LocationName ,
            Categories.[Name] as  CategoryName,ISNULL(ManufacturingOrder.CustomerId,0) as CustomerId,
			ISNULL(Contacts.DisplayName,'') AS Customer,ManufacturingOrder.[Status],
            ISNULL(ManufacturingOrder.PlannedQuantity,0)as PlannedQuantity,
            SUM(ISNULL(ManufacturingOrderItems.PlannedTime,0))as PlannedTime,
			ISNULL(ManufacturingOrder.MaterialCost,0)as MaterialCost,ISNULL(ManufacturingOrder.OperationCost,0) as OperationCost
			,ISNULL(ManufacturingOrder.TotalCost,0)as TotalCost,ManufacturingOrder.DoneDate
            from ManufacturingOrder
            left join ManufacturingOrderItems on ManufacturingOrder.id= ManufacturingOrderItems.OrderId 
            left join Locations on Locations.id=ManufacturingOrder.LocationId
            left join Contacts on Contacts.id=ManufacturingOrder.CustomerId
            inner join Items on Items.id=ManufacturingOrder.ItemId
            inner join Categories on Categories.id=Items.CategoryId 
            where 
            ManufacturingOrder.LocationId=@location and  ManufacturingOrder.Status=3 and ManufacturingOrder.CompanyId=@CompanyId and
			(ManufacturingOrderItems.Tip = 'Operations' or    ManufacturingOrderItems.Tip is null or   ManufacturingOrderItems.Tip='Ingredients') and ManufacturingOrder.IsActive=1
            Group By ManufacturingOrder.id, ManufacturingOrder.[Name],  ManufacturingOrder.ItemId,
			ManufacturingOrder.CustomerId,Contacts.DisplayName,ManufacturingOrder.LocationId,ManufacturingOrder.DoneDate,
			ManufacturingOrder.MaterialCost,ManufacturingOrder.OperationCost,ManufacturingOrder.TotalCost,
            Locations.LocationName , Items.[Name],Categories.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrder.[Status]) x
            where ISNULL(PlannedTime,0) like '%{T.PlannedTime}%' AND  ISNULL(Name,'') Like '%{T.Name}%' AND    ISNULL     (Customer,'') like '%{T.Customer}%' and
            ISNULL(ItemName,'') like '%{T.ItemName}%' and ISNULL(CategoryName,'') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity,'') like '%{T.PlannedQuantity}%' and
             ISNULL(Status,'') like '%{T.Status}%' and    ISNULL(MaterialCost,'') like '%{T.MaterialCost}%' and    ISNULL(OperationCost,'') like '%{T.OperationCost}%'
			 and    ISNULL(TotalCost,'') like '%{T.TotalCost}%')as kayisayisi";
            }
            var kayitsayisi = await _db.QueryAsync<int>(sql, prm);
            return kayitsayisi.First();
        }

        public async Task<IEnumerable<ManufacturingOrderList>> ScheludeOpenList(ManufacturingOrderList T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("location", T.LocationId);
            string sql = string.Empty;
            if (T.LocationId == null)
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            ManufacturingOrder.id,ManufacturingOrder.ExpectedDate, ManufacturingOrder.[Name], ManufacturingOrder.ItemId, Items.[Name] AS            ItemName,ManufacturingOrder.LocationId,Locations.LocationName ,
            ISNULL(Categories.[Name],'') as  CategoryName,ISNULL(ManufacturingOrder.CustomerId,0) as CustomerId,ISNULL     (Contacts.DisplayName,'')    AS Customer,ManufacturingOrder.[Status],
            ISNULL(ManufacturingOrder.PlannedQuantity,0)as PlannedQuantity,
            SUM(ISNULL(ManufacturingOrderItems.PlannedTime,0))as PlannedTime,
            ISNULL(ManufacturingOrder.ProductionDeadline,'') as ProductDeadline,
            ISNULL(min(ManufacturingOrderItems.Availability),0) as Availability
            from ManufacturingOrder
            left join ManufacturingOrderItems on ManufacturingOrder.id= ManufacturingOrderItems.OrderId 
            left join Locations on Locations.id=ManufacturingOrder.LocationId
            left join Contacts on Contacts.id=ManufacturingOrder.CustomerId
            left join Items on Items.id=ManufacturingOrder.ItemId
            left join Categories on Categories.id=Items.CategoryId 
            where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.IsActive=1 and   ManufacturingOrder.Status!=3 and (ManufacturingOrderItems.Tip = 'Operations' or    ManufacturingOrderItems.Tip is null or   ManufacturingOrderItems.Tip='Ingredients')
            Group By ManufacturingOrder.id, ManufacturingOrder.[Name], ManufacturingOrder.ItemId,ManufacturingOrder.CustomerId,Contacts.DisplayName,ManufacturingOrder.LocationId,
            Locations.LocationName ,ManufacturingOrder.ExpectedDate,
            Items.[Name],Categories.            [Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrder.ProductionDeadline,ManufacturingOrder.[Status]) x
            where ISNULL(PlannedTime,0) like '%{T.PlannedTime}%' AND ISNULL(Availability,0) like '%{T.Availability}%' and ISNULL(Name,'') Like '%{T.Name}%' AND    ISNULL     (Customer,'') like '%{T.Customer}%' and
            ISNULL(ItemName,'') like '%{T.ItemName}%' and ISNULL(CategoryName,'') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity,'') like '%{T.PlannedQuantity}%' and
            ISNULL(ProductDeadline,'') like '%{T.ProductDeadline}%' and ISNULL(Status,'') like '%{T.Status}%' 
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;
                        ";
            }
            else
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            ManufacturingOrder.id,ManufacturingOrder.ExpectedDate, ManufacturingOrder.[Name], ManufacturingOrder.ItemId, Items.[Name] AS            ItemName,ManufacturingOrder.LocationId,Locations.LocationName ,
            ISNULL(Categories.[Name],'') as  CategoryName,ISNULL(ManufacturingOrder.CustomerId,0) as CustomerId,ISNULL     (Contacts.DisplayName,'')    AS Customer,ManufacturingOrder.[Status],
            ISNULL(ManufacturingOrder.PlannedQuantity,0)as PlannedQuantity,
            SUM(ISNULL(ManufacturingOrderItems.PlannedTime,0))as PlannedTime,
            ISNULL(ManufacturingOrder.ProductionDeadline,'') as ProductDeadline,
            ISNULL(min(ManufacturingOrderItems.Availability),0) as Availability
            from ManufacturingOrder
            left join ManufacturingOrderItems on ManufacturingOrder.id= ManufacturingOrderItems.OrderId 
            left join Locations on Locations.id=ManufacturingOrder.LocationId
            left join Contacts on Contacts.id=ManufacturingOrder.CustomerId
            left join Items on Items.id=ManufacturingOrder.ItemId
            left join Categories on Categories.id=Items.CategoryId 
            where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.IsActive=1 and ManufacturingOrder.LocationId=@location and       ManufacturingOrder.Status!=3 and (ManufacturingOrderItems.Tip = 'Operations' or    ManufacturingOrderItems.Tip is null or   ManufacturingOrderItems.Tip='Ingredients')
            Group By ManufacturingOrder.id, ManufacturingOrder.[Name],          ManufacturingOrder.ItemId,ManufacturingOrder.CustomerId,Contacts.DisplayName,ManufacturingOrder.LocationId,
            Locations.LocationName ,ManufacturingOrder.ExpectedDate,
            Items.[Name],Categories.            [Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrder.ProductionDeadline,ManufacturingOrder.[Status]) x
            where ISNULL(PlannedTime,0) like '%{T.PlannedTime}%' AND ISNULL(Availability,0) like '%{T.Availability}%' and ISNULL(Name,'') Like '%{T.Name}%' AND    ISNULL     (Customer,'') like '%{T.Customer}%' and
            ISNULL(ItemName,'') like '%{T.ItemName}%' and ISNULL(CategoryName,'') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity,'') like '%{T.PlannedQuantity}%' and
            ISNULL(ProductDeadline,'') like '%{T.ProductDeadline}%' and ISNULL(Status,'') like '%{T.Status}%' 
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;
                        ";
            }


            var ScheludeOpenList = await _db.QueryAsync<ManufacturingOrderList>(sql, param);



            return ScheludeOpenList.ToList();
        }

        public async Task<int> ScheludeOpenListCount(ManufacturingOrderList T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@location", T.LocationId);
            string sql = string.Empty;
            if (T.LocationId == null)
            {
                sql = $@"select Count(*) as kayitsayisi from(
            select * from
            (Select
            ManufacturingOrder.id,ManufacturingOrder.ExpectedDate, ManufacturingOrder.[Name], ManufacturingOrder.ItemId, Items.[Name] AS            ItemName, ManufacturingOrder.LocationId, Locations.LocationName,
            Categories.[Name] as CategoryName, ISNULL(ManufacturingOrder.CustomerId, 0) as CustomerId, ISNULL(Contacts.DisplayName, '')    AS Customer, ManufacturingOrder.[Status],
            ISNULL(ManufacturingOrder.PlannedQuantity, 0) as PlannedQuantity,
            SUM(ISNULL(ManufacturingOrderItems.PlannedTime, 0)) as PlannedTime,
            ISNULL(ManufacturingOrder.ProductionDeadline, '') as ProductDeadline,
            min(ManufacturingOrderItems.[Availability]) as [Availability]
            from ManufacturingOrder
            left join ManufacturingOrderItems on ManufacturingOrder.id = ManufacturingOrderItems.OrderId
            left
            join Locations on Locations.id = ManufacturingOrder.LocationId
            left
            join Contacts on Contacts.id = ManufacturingOrder.CustomerId
            inner
            join Items on Items.id = ManufacturingOrder.ItemId
            inner
            join Categories on Categories.id = Items.CategoryId
            where ManufacturingOrder.CompanyId = @CompanyId  and  ManufacturingOrder.Status != 3 and(ManufacturingOrderItems.Tip = 'Operations' or    ManufacturingOrderItems.Tip is null or ManufacturingOrderItems.Tip = 'Ingredients') and ManufacturingOrder.IsActive=1
            Group By ManufacturingOrder.id, ManufacturingOrder.[Name], ManufacturingOrder.ItemId, ManufacturingOrder.CustomerId, Contacts.DisplayName, ManufacturingOrder.LocationId, Locations.LocationName,ManufacturingOrder.ExpectedDate,
            Items.[Name], Categories.[Name], ManufacturingOrder.PlannedQuantity, ManufacturingOrder.ProductionDeadline, ManufacturingOrder.[Status]) x
            where ISNULL(PlannedTime, 0) like '%{T.PlannedTime}%' AND ISNULL(Availability, 0) like '%{T.Availability}%' and ISNULL(Name, '') Like '%{T.Name}%' AND    ISNULL(Customer, '') like '%{T.Customer}%' and
            ISNULL(ItemName, '') like '%{T.ItemName}%' and ISNULL(CategoryName, '') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity, '') like '%{T.PlannedQuantity}%' and
            ISNULL(ProductDeadline, '') like '%{T.ProductDeadline}%' and ISNULL(Status, '') like '%{T.Status}%') as kayitsayisi";
            }
            else
            {
                sql = $@"select Count(*) as kayitsayisi from(
            select * from
            (Select
            ManufacturingOrder.id,ManufacturingOrder.ExpectedDate, ManufacturingOrder.[Name], ManufacturingOrder.ItemId, Items.[Name] AS            ItemName, ManufacturingOrder.LocationId, Locations.LocationName,
            Categories.[Name] as CategoryName, ISNULL(ManufacturingOrder.CustomerId, 0) as CustomerId, ISNULL(Contacts.DisplayName, '')    AS Customer, ManufacturingOrder.[Status],
            ISNULL(ManufacturingOrder.PlannedQuantity, 0) as PlannedQuantity,
            SUM(ISNULL(ManufacturingOrderItems.PlannedTime, 0)) as PlannedTime,
            ISNULL(ManufacturingOrder.ProductionDeadline, '') as ProductDeadline,
            min(ManufacturingOrderItems.[Availability]) as [Availability]
            from ManufacturingOrder
            left join ManufacturingOrderItems on ManufacturingOrder.id = ManufacturingOrderItems.OrderId
            left
            join Locations on Locations.id = ManufacturingOrder.LocationId
            left
            join Contacts on Contacts.id = ManufacturingOrder.CustomerId
            inner
            join Items on Items.id = ManufacturingOrder.ItemId
            inner
            join Categories on Categories.id = Items.CategoryId
            where ManufacturingOrder.CompanyId = @CompanyId and ManufacturingOrder.LocationId = @location and       ManufacturingOrder.Status != 3 and(ManufacturingOrderItems.Tip = 'Operations' or    ManufacturingOrderItems.Tip is null or ManufacturingOrderItems.Tip = 'Ingredients') and ManufacturingOrder.IsActive=1
            Group By ManufacturingOrder.id, ManufacturingOrder.[Name], ManufacturingOrder.ItemId, ManufacturingOrder.CustomerId, Contacts.DisplayName, ManufacturingOrder.LocationId, Locations.LocationName,ManufacturingOrder.ExpectedDate,
            Items.[Name], Categories.[Name], ManufacturingOrder.PlannedQuantity, ManufacturingOrder.ProductionDeadline, ManufacturingOrder.[Status]) x
            where ISNULL(PlannedTime, 0) like '%{T.PlannedTime}%' AND ISNULL(Availability, 0) like '%{T.Availability}%' and ISNULL(Name, '') Like '%{T.Name}%' AND    ISNULL(Customer, '') like '%{T.Customer}%' and
            ISNULL(ItemName, '') like '%{T.ItemName}%' and ISNULL(CategoryName, '') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity, '') like '%{T.PlannedQuantity}%' and
            ISNULL(ProductDeadline, '') like '%{T.ProductDeadline}%' and ISNULL(Status, '') like '%{T.Status}%') as kayitsayisi";
            }
            var kayitsayisi = await _db.QueryAsync<int>(sql, prm);
            return kayitsayisi.First();
        }

        public async Task TaskDone(ManufacturingTaskDone T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@OrderId", T.OrderId);
            param.Add("@CompanyId", CompanyId);
            param.Add("@CompletedDate", DateTime.Now);
            param.Add("@Status", T.Status);
            if (T.Status == 3)
            {
                await _db.ExecuteAsync($"Update ManufacturingOrderItems set CompletedDate=@CompletedDate , Status=@Status where CompanyId=@CompanyId and id=@id and OrderId=@OrderId ", param);
            }
            else
            {
                await _db.ExecuteAsync($"Update ManufacturingOrderItems set CompletedDate=@CompletedDate,Status=@Status where CompanyId=@CompanyId and id=@id and OrderId=@OrderId ", param);
            }

        }

        public async Task<IEnumerable<ManufacturingTask>> TaskDoneList(ManufacturingTask T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@LocationId", T.LocationId);
            string sql = string.Empty;
            if (T.LocationId == null)
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
             select x.* from(
        select ManufacturingOrderItems.id as id,ManufacturingOrder.id as ManufacturingOrderId ,ManufacturingOrderItems.ResourceId,Resources.[Name]as ResourcesName,ManufacturingOrder.[Name]as OrderName,ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,ManufacturingOrderItems.CompletedDate, 
        Items.[Name]as ItemName,ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name]as OperationName,ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status] from ManufacturingOrderItems
        left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
        left join Items on Items.id=ManufacturingOrder.ItemId
        left join Resources on Resources.id=ManufacturingOrderItems.ResourceId
        left join Operations on Operations.id=ManufacturingOrderItems.OperationId 
        where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.id=ManufacturingOrderItems.OrderId and ManufacturingOrderItems.Status=3  and 
        ISNULL(PlannedTime,0) like '%{T.PlannedTime}%'  and ISNULL(Resources.Name,'') Like '%{T.ResourcesName}%' AND    ISNULL(ManufacturingOrder.Name,'') like '%{T.OrderName}%' and
        ISNULL(Items.Name,'') like '%{T.ItemName}%' and ISNULL(ManufacturingOrder.PlannedQuantity,'') like '%{T.PlannedQuantity}%' and ISNULL(Operations.Name,'') like '%{T.OperationName}%' 
       
        Group By ManufacturingOrder.id,ManufacturingOrderItems.id,ManufacturingOrderItems.ResourceId,Resources.[Name],ManufacturingOrder.[Name],ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,ManufacturingOrderItems.CompletedDate,
        Items.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name],ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status])x
        ORDER BY x.ResourceId OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
            }
            else
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
             select x.* from(
        select ManufacturingOrderItems.id as id,ManufacturingOrder.id as ManufacturingOrderId ,ManufacturingOrderItems.ResourceId,Resources.[Name]as ResourcesName,ManufacturingOrder.[Name]as OrderName,ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,ManufacturingOrderItems.CompletedDate, 
        Items.[Name]as ItemName,ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name]as OperationName,ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status] from ManufacturingOrderItems
        left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
        left join Items on Items.id=ManufacturingOrder.ItemId
        left join Resources on Resources.id=ManufacturingOrderItems.ResourceId
        left join Operations on Operations.id=ManufacturingOrderItems.OperationId 
        where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.id=ManufacturingOrderItems.OrderId and ManufacturingOrderItems.Status=3  and ManufacturingOrder.LocationId=@LocationId AND
        ISNULL(PlannedTime,0) like '%{T.PlannedTime}%'  and ISNULL(Resources.Name,'') Like '%{T.ResourcesName}%' AND    ISNULL(ManufacturingOrder.Name,'') like '%{T.OrderName}%' and
        ISNULL(Items.Name,'') like '%{T.ItemName}%' and ISNULL(ManufacturingOrder.PlannedQuantity,'') like '%{T.PlannedQuantity}%' and ISNULL(Operations.Name,'') like '%{T.OperationName}%' 
       
        Group By ManufacturingOrder.id,ManufacturingOrderItems.id,ManufacturingOrderItems.ResourceId,Resources.[Name],ManufacturingOrder.[Name],ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,ManufacturingOrderItems.CompletedDate,
        Items.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name],ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status])x
        ORDER BY x.ResourceId OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
            }


            var TaskDoneList = await _db.QueryAsync<ManufacturingTask>(sql, param);



            return TaskDoneList;
        }

        public async Task<int> TaskDoneListCount(ManufacturingTask T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@LocationId", T.LocationId);
            string sql = string.Empty;
            if (T.LocationId == null)
            {
                sql = $@"select COUNT(*) as kayitsayisi from(
                    select x.* from(
                select ManufacturingOrder.id as ManufacturingOrderId,ManufacturingOrderItems.ResourceId,Resources.[Name]as ResourcesName,ManufacturingOrderItems.CompletedDate,ManufacturingOrder.[Name]as OrderName,ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,
            Items.[Name]as ItemName,ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name]as OperationName,ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status] from ManufacturingOrderItems
            left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
            left join Items on Items.id=ManufacturingOrder.ItemId
            left join Resources on Resources.id=ManufacturingOrderItems.ResourceId
            left join Operations on Operations.id=ManufacturingOrderItems.OperationId 
            where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.id=ManufacturingOrderItems.OrderId and ManufacturingOrderItems.Status=3  and 
          ISNULL(PlannedTime,0) like '%{T.PlannedTime}%'  and ISNULL(Resources.Name,'') Like '%{T.ResourcesName}%' AND    ISNULL(ManufacturingOrder.Name,'') like '%{T.OrderName}%' and
        ISNULL(Items.Name,'') like '%{T.ItemName}%' and ISNULL(ManufacturingOrder.PlannedQuantity,'') like '%{T.PlannedQuantity}%' and ISNULL(Operations.Name,'') like '%{T.OperationName}%' 
            Group By ManufacturingOrder.id,ManufacturingOrderItems.ResourceId,Resources.[Name],ManufacturingOrder.[Name],ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,
        Items.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,ManufacturingOrderItems.CompletedDate,Operations.[Name],ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status])x
            )kayitsayisi";
            }
            else
            {
                sql = $@"select COUNT(*) as kayitsayisi from(
                    select x.* from(
                select ManufacturingOrder.id as ManufacturingOrderId,ManufacturingOrderItems.ResourceId,Resources.[Name]as ResourcesName,ManufacturingOrderItems.CompletedDate,ManufacturingOrder.[Name]as OrderName,ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,
            Items.[Name]as ItemName,ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name]as OperationName,ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status] from ManufacturingOrderItems
            left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
            left join Items on Items.id=ManufacturingOrder.ItemId
            left join Resources on Resources.id=ManufacturingOrderItems.ResourceId
            left join Operations on Operations.id=ManufacturingOrderItems.OperationId 
            where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.id=ManufacturingOrderItems.OrderId and ManufacturingOrderItems.Status=3  and ManufacturingOrder.LocationId=@LocationId AND
          ISNULL(PlannedTime,0) like '%{T.PlannedTime}%'  and ISNULL(Resources.Name,'') Like '%{T.ResourcesName}%' AND    ISNULL(ManufacturingOrder.Name,'') like '%{T.OrderName}%' and
        ISNULL(Items.Name,'') like '%{T.ItemName}%' and ISNULL(ManufacturingOrder.PlannedQuantity,'') like '%{T.PlannedQuantity}%' and ISNULL(Operations.Name,'') like '%{T.OperationName}%' 
            Group By ManufacturingOrder.id,ManufacturingOrderItems.ResourceId,Resources.[Name],ManufacturingOrder.[Name],ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,
        Items.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,ManufacturingOrderItems.CompletedDate,Operations.[Name],ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status])x
            )kayitsayisi";
            }
            var kayitsayisi = await _db.QueryAsync<int>(sql, prm);
            return kayitsayisi.First();
        }

        public async Task<IEnumerable<ManufacturingTask>> TaskOpenList(ManufacturingTask T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@LocationId", T.LocationId);
            string sql = string.Empty;
            if (T.LocationId==null)
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
             select x.* from(
        select ManufacturingOrderItems.id as id,ManufacturingOrder.id as ManufacturingOrderId,ManufacturingOrderItems.ResourceId,Resources.[Name]as ResourcesName,ManufacturingOrder.[Name]as OrderName,ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,
        Items.[Name]as ItemName,ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name]as OperationName,ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status] from ManufacturingOrderItems
        left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
        left join Items on Items.id=ManufacturingOrder.ItemId
        left join Resources on Resources.id=ManufacturingOrderItems.ResourceId
        left join Operations on Operations.id=ManufacturingOrderItems.OperationId 
        where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.id=ManufacturingOrderItems.OrderId and ManufacturingOrderItems.Status!=3  and 
        ISNULL(PlannedTime,0) like '%{T.PlannedTime}%'  and ISNULL(Resources.Name,'') Like '%{T.ResourcesName}%' AND    ISNULL(ManufacturingOrder.Name,'') like '%{T.OrderName}%' and
        ISNULL(Items.Name,'') like '%{T.ItemName}%' and ISNULL(ManufacturingOrder.PlannedQuantity,'') like '%{T.PlannedQuantity}%' and ISNULL(Operations.Name,'') like '%{T.OperationName}%' 
        Group By ManufacturingOrder.id,ManufacturingOrderItems.ResourceId,Resources.[Name],ManufacturingOrder.[Name],ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,ManufacturingOrderItems.id,
        Items.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name],ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status])x
        ORDER BY x.ResourceId OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
            }
            else
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
             select x.* from(
        select ManufacturingOrderItems.id as id,ManufacturingOrder.id as ManufacturingOrderId,ManufacturingOrderItems.ResourceId,Resources.[Name]as ResourcesName,ManufacturingOrder.[Name]as OrderName,ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,
        Items.[Name]as ItemName,ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name]as OperationName,ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status] from ManufacturingOrderItems
        left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
        left join Items on Items.id=ManufacturingOrder.ItemId
        left join Resources on Resources.id=ManufacturingOrderItems.ResourceId
        left join Operations on Operations.id=ManufacturingOrderItems.OperationId 
        where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.id=ManufacturingOrderItems.OrderId and ManufacturingOrderItems.Status!=3  and ManufacturingOrder.LocationId=@LocationId AND
        ISNULL(PlannedTime,0) like '%{T.PlannedTime}%'  and ISNULL(Resources.Name,'') Like '%{T.ResourcesName}%' AND    ISNULL(ManufacturingOrder.Name,'') like '%{T.OrderName}%' and
        ISNULL(Items.Name,'') like '%{T.ItemName}%' and ISNULL(ManufacturingOrder.PlannedQuantity,'') like '%{T.PlannedQuantity}%' and ISNULL(Operations.Name,'') like '%{T.OperationName}%' 
        Group By ManufacturingOrder.id,ManufacturingOrderItems.ResourceId,Resources.[Name],ManufacturingOrder.[Name],ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,ManufacturingOrderItems.id,
        Items.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name],ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status])x
        ORDER BY x.ResourceId OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
            }
 
            var ScheludeOpenList = await _db.QueryAsync<ManufacturingTask>(sql, param);



            return ScheludeOpenList;
        }

        public async Task<int> TaskOpenListCount(ManufacturingTask T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@LocationId", T.LocationId);
            string sql = string.Empty;
            if (T.LocationId==null)
            {
                sql = $@"select COUNT(*) as kayitsayisi from(
                      select x.* from(
        select ManufacturingOrderItems.id as id,ManufacturingOrder.id as ManufacturingOrderId,ManufacturingOrderItems.ResourceId,Resources.[Name]as ResourcesName,ManufacturingOrder.[Name]as OrderName,ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,
        Items.[Name]as ItemName,ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name]as OperationName,ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status] from ManufacturingOrderItems
        left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
        left join Items on Items.id=ManufacturingOrder.ItemId
        left join Resources on Resources.id=ManufacturingOrderItems.ResourceId
        left join Operations on Operations.id=ManufacturingOrderItems.OperationId 
        where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.id=ManufacturingOrderItems.OrderId and ManufacturingOrderItems.Status!=3  and 
        ISNULL(PlannedTime,0) like '%{T.PlannedTime}%'  and ISNULL(Resources.Name,'') Like '%{T.ResourcesName}%' AND    ISNULL(ManufacturingOrder.Name,'') like '%{T.OrderName}%' and
        ISNULL(Items.Name,'') like '%{T.ItemName}%' and ISNULL(ManufacturingOrder.PlannedQuantity,'') like '%{T.PlannedQuantity}%' and ISNULL(Operations.Name,'') like '%{T.OperationName}%' 
        Group By ManufacturingOrder.id,ManufacturingOrderItems.ResourceId,Resources.[Name],ManufacturingOrder.[Name],ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,ManufacturingOrderItems.id,
        Items.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name],ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status])x
            )kayitsayisi";
            }
            else
            {
                sql = $@"select COUNT(*) as kayitsayisi from(
                      select x.* from(
        select ManufacturingOrderItems.id as id,ManufacturingOrder.id as ManufacturingOrderId,ManufacturingOrderItems.ResourceId,Resources.[Name]as ResourcesName,ManufacturingOrder.[Name]as OrderName,ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,
        Items.[Name]as ItemName,ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name]as OperationName,ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status] from ManufacturingOrderItems
        left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
        left join Items on Items.id=ManufacturingOrder.ItemId
        left join Resources on Resources.id=ManufacturingOrderItems.ResourceId
        left join Operations on Operations.id=ManufacturingOrderItems.OperationId 
        where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.id=ManufacturingOrderItems.OrderId and ManufacturingOrderItems.Status!=3  and ManufacturingOrder.LocationId=@LocationId AND
        ISNULL(PlannedTime,0) like '%{T.PlannedTime}%'  and ISNULL(Resources.Name,'') Like '%{T.ResourcesName}%' AND    ISNULL(ManufacturingOrder.Name,'') like '%{T.OrderName}%' and
        ISNULL(Items.Name,'') like '%{T.ItemName}%' and ISNULL(ManufacturingOrder.PlannedQuantity,'') like '%{T.PlannedQuantity}%' and ISNULL(Operations.Name,'') like '%{T.OperationName}%' 
        Group By ManufacturingOrder.id,ManufacturingOrderItems.ResourceId,Resources.[Name],ManufacturingOrder.[Name],ManufacturingOrder.ProductionDeadline,ManufacturingOrder.ItemId,ManufacturingOrderItems.id,
        Items.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrderItems.OperationId,Operations.[Name],ManufacturingOrderItems.PlannedTime,ManufacturingOrderItems.[Status])x
            )kayitsayisi";
            }
            var kayitsayisi = await _db.QueryAsync<int>(sql, prm);
            return kayitsayisi.First();
        }

        public async Task Update(ManufacturingOrderUpdate T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@CompanyId", CompanyId);
            param.Add("@Name", T.Name);
            param.Add("@ItemId", T.ItemId);
            param.Add("@ProductionDeadline", DateTime.Now);
            param.Add("@CreatedDate", T.CreatedDate);
            param.Add("@LocationId", T.LocationId);
            param.Add("@MaterialCost", T.MaterialCost);
            param.Add("@OperationCost", T.OperationCost);
            param.Add("@TotalCost", T.TotalCost);
            param.Add("@Info", T.Info);
            param.Add("@Status", 0);
            param.Add("@PlannedQuantity", T.PlannedQuantity);
            param.Add("@ExpectedDate", T.ExpectedDate);
            string sqlv = $@"Select ISNULL(PlannedQuantity,0)as PlannedQuantity ,ItemId,SalesOrderId,SalesOrderItemId,LocationId from  ManufacturingOrder where CompanyId=@CompanyId and id=@id and IsActive=1 and Status!=3";
            var deger = await _db.QueryAsync<ManufacturingOrderA>(sqlv, param);
            T.eskiPlanned = (float)deger.First().PlannedQuantity;
            T.eskiLocation = deger.First().LocationId;
            if (T.ItemId != deger.First().ItemId && deger.First().ItemId != null && T.ItemId != null)
            {
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
                    await _manuitem.DeleteItems(A, CompanyId);
                }
                param.Add("@ItemId", T.ItemId);
                string sql = $@"Update ManufacturingOrder Set Name=@Name,MaterialCost=@MaterialCost,OperationCost=@OperationCost,TotalCost=@TotalCost,ItemId=@ItemId,ProductionDeadline=@ProductionDeadline,ExpectedDate=@ExpectedDate,PlannedQuantity=@PlannedQuantity,CreatedDate=@CreatedDate,LocationId=@LocationId,Info=@Info,Status=@Status where CompanyId=@CompanyId and id=@id";
                await _db.ExecuteAsync(sql, param);
                int? SalesOrder = deger.First().SalesOrderId;
                int? SalesOrderItemId = deger.First().SalesOrderItemId;
                await _manuitem.InsertOrderItems(T.id, SalesOrder, SalesOrderItemId, CompanyId);





            }

            else
            {
                if (T.LocationId != T.eskiLocation)
                {
                    var rezervedegerler = await _db.QueryAsync<Manufacturing>($"select * from Rezerve where CompanyId={CompanyId} and ManufacturingOrderId={T.id} and Status=1");
                    foreach (var item in rezervedegerler)
                    {
                        param.Add("@ItemsId", item.ItemId);
                        param.Add("@Status", 4);
                        await _db.ExecuteAsync($"Update Rezerve set Status=@Status where ManufacturingOrderId=@id and CompanyId=@CompanyId and ItemId=@ItemsId", param);
                    }
                }

                string sql = $@"Update ManufacturingOrder Set Name=@Name,MaterialCost=@MaterialCost,OperationCost=@OperationCost,TotalCost=@TotalCost,ItemId=@ItemId,ProductionDeadline=@ProductionDeadline,ExpectedDate=@ExpectedDate,PlannedQuantity=@PlannedQuantity,CreatedDate=@CreatedDate,LocationId=@LocationId,Info=@Info,Status=@Status where CompanyId=@CompanyId and id=@id";
                await _db.ExecuteAsync(sql, param);
            }
        }
    }
}
