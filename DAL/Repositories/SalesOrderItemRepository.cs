using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using DAL.StockControl;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.BomDTO;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.ProductOperationsBomDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;
using MissingCount = DAL.DTO.BomDTO.MissingCount;

namespace DAL.Repositories
{
    public class SalesOrderItemRepository : ISalesOrderItemRepository
    {
        private readonly IDbConnection _db;
        private readonly IStockControl _control;
        private readonly ILocationStockRepository _loc;
        private readonly IManufacturingOrderItemRepository _manufacturingOrderItem;

        public SalesOrderItemRepository(IDbConnection db, IStockControl control, ILocationStockRepository loc, IManufacturingOrderItemRepository manufacturingOrderItem)
        {
            _db = db;
            _control = control;
            _loc = loc;
            _manufacturingOrderItem = manufacturingOrderItem;
        }

        public async Task<int> Control(SalesOrderItem T, int OrdersId, int CompanyId)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SalesOrderId", OrdersId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@location", T.LocationId);
            string sqla = $@"select
        (Select Tip from Items where id = @ItemId and CompanyId = @CompanyId) as Tip,
       (Select ISNULL(StockCount,0) from LocationStock where ItemId = @ItemId  and LocationId = @location  and CompanyId = @CompanyId)as LocationsStockCount,
       (Select ISNULL(id,0) from LocationStock where ItemId = @ItemId  and LocationId = @location and CompanyId = @CompanyId) as  LocationStockId ";
            var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, prm);
            var locationStockId = sorgu.First().LocationStockId;
            prm.Add("@LocationStockId", locationStockId);
            prm.Add("@Tip", sorgu.First().Tip);

            var rezervecount =await _control.Count(T.ItemId, CompanyId, T.LocationId);

            if (rezervecount >= T.Quantity)//Stok sayısı istesnilenden büyük ise rezerve sayısı adet olur
            {
                prm.Add("@RezerveCount", T.Quantity);
                prm.Add("@LocationStockCount", rezervecount);
                T.Status = 3;

            }
            else
            {
                prm.Add("@RezerveCount", rezervecount);//Stok sayısı adetten kücük ise rezer sayısı Stok adeti kadar olur.
                prm.Add("@LocationStockCount", rezervecount);
                T.Status = 1;
            }
            prm.Add("@Status", 1);

            return await _db.QuerySingleAsync<int>($"Insert into Rezerve (SalesOrderId,Tip,ItemId,RezerveCount,CustomerId,LocationId,Status,LocationStockCount,CompanyId) OUTPUT INSERTED.[id]  values (@SalesOrderId,@Tip,@ItemId,@RezerveCount,@ContactId,@location,@Status,@LocationStockCount,@CompanyId)", prm);
        }

        public async Task<IEnumerable<SalesOrderDetail>> Detail(int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", id);
            string sql = $@"select o.id,o.ContactId,Contacts.DisplayName,o.LocationId,Locations.LocationName,o.DeliveryDeadline,o.CreateDate,o.OrderName,o.BillingAddressId,o.ShippingAddressId,
                o.Info,o.DeliveryId
                from Orders o
                left join OrdersItem oi on oi.OrdersId=o.id
                left join Contacts on Contacts.id=o.ContactId
	            LEFT join Locations on Locations.id=o.LocationId
                where o.CompanyId=@CompanyId and o.id=@id
                group by o.id,o.ContactId,Contacts.DisplayName,o.DeliveryDeadline,o.CreateDate,o.OrderName,o.BillingAddressId,o.ShippingAddressId,o.Info,o.LocationId,Locations.LocationName,o.DeliveryId";
            var details = await _db.QueryAsync<SalesOrderDetail>(sql, prm);
            return details;
        }

        public async Task DoneSellOrder(SalesDone T, int CompanyId, int UserId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@CompanyId", CompanyId);
            param.Add("@DeliveryId", T.DeliveryId);
            string sql = $@"select DeliveryId from Orders where CompanyId=@CompanyId and id=@id ";
            var st = await _db.QueryAsync<int>(sql, param);
            int eskiStatus = st.First();

            if (eskiStatus == 0)
            {

                if (T.DeliveryId == 2)
                {
                    var List = await _db.QueryAsync<SalesOrderItem>($@"select OrdersItem.ItemId,OrdersItem.id,OrdersItem.OrdersId as SalesOrderId,OrdersItem.Quantity,LocationId from Orders 
                     left join OrdersItem on OrdersItem.OrdersId=Orders.id
                     where Orders.CompanyId=@CompanyId and Orders.id=@id and OrdersItem.Stance=0", param);
                    param.Add("@LocationId", List.First().LocationId);
                    foreach (var item in List)
                    {
                        param.Add("@SalesOrderItemId", item.id);
                        param.Add("@ItemId", item.ItemId);
                        var make = await _db.QueryAsync<SalesOrderRezerve>($"select mo.id,Status from ManufacturingOrder mo where SalesOrderId=@id and SalesOrderItemId=@SalesOrderItemId and ItemId=@ItemId and CompanyId=@CompanyId and Status!=3 and IsActive=1", param);


                        if (make.Count() != 0)
                        {
                            foreach (var items in make)
                            {
                                if (items.Status != 3)
                                {
                                    int Status = 3;
                                    await DoneStock(items.id, T.id, item.id, Status, CompanyId,UserId);
                                }

                            }
                            param.Add("@Stance", 1);
                            param.Add("@Ingredients", 3);
                            param.Add("@SalesItem", 4);
                            param.Add("@Production", 4);
                            await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients,Stance=@Stance,SalesItem=@SalesItem,Production=@Production where CompanyId=@CompanyId and OrdersId=@id and id=@SalesOrderItemId and ItemId=@ItemId ", param);
                            param.Add("@Status", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Status=@Status where CompanyId=@CompanyId and SalesOrderId=@id and SalesOrderItemId=@SalesOrderItemId", param);

                        }
                        else
                        {

                            param.Add("@Stance", 1);
                            param.Add("@Ingredients", 3);
                            param.Add("@SalesItem", 4);
                            param.Add("@Production", 4);
                            await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients,Stance=@Stance,SalesItem=@SalesItem,Production=@Production where CompanyId=@CompanyId and OrdersId=@id and id=@SalesOrderItemId and ItemId=@ItemId ", param);
                            param.Add("@Status", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Status=@Status where CompanyId=@CompanyId and SalesOrderId=@id and SalesOrderItemId=@SalesOrderItemId", param);



                        }
                        List<SalesOrderUpdateItems> aa = (await _db.QueryAsync<SalesOrderUpdateItems>($@"select o.id,oi.id as OrderItemId,oi.Quantity,oi.ItemId,oi.PricePerUnit,oi.TaxId,o.ContactId,o.LocationId,o.DeliveryDeadline from Orders o 
                left join OrdersItem oi on oi.OrdersId = o.id where o.CompanyId = @CompanyId and oi.Stance = 0 and o.IsActive = 1 and o.id=@id and oi.ItemId = @ItemId and o.DeliveryId = 0 Order by o.DeliveryDeadline ", param)).ToList();
                        SalesOrderUpdateItems A = new SalesOrderUpdateItems();
                        foreach (var liste in aa)
                        {
                            A.id = liste.id;
                            A.OrderItemId = liste.OrderItemId;
                            A.TaxId = liste.TaxId;
                            A.Quantity = liste.Quantity;
                            A.ItemId = liste.ItemId;
                            A.ContactId = liste.ContactId;
                            A.LocationId = liste.LocationId;
                            A.DeliveryDeadline = liste.DeliveryDeadline;
                            A.PricePerUnit = liste.PricePerUnit;
                            param.Add("@RezerveCount", 0);
                            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and SalesOrderId={liste.id} and SalesOrderItemId={liste.OrderItemId} and ItemId={liste.ItemId} ", param);
                            await UpdateItems(A, T.id, CompanyId);
                        }


                    }



                    param.Add("@DeliveryId", 2);
                    await _db.ExecuteAsync($"Update Orders set DeliveryId=@DeliveryId where CompanyId=@CompanyId and id=@id", param);
                }
                else if (T.DeliveryId == 4)
                {

                    var List = await _db.QueryAsync<SalesOrderItem>($@"select OrdersItem.ItemId,OrdersItem.id,OrdersItem.OrdersId as SalesOrderId,OrdersItem.Quantity,LocationId from Orders 
                left join OrdersItem on OrdersItem.OrdersId=Orders.id
                where Orders.CompanyId=@CompanyId and Orders.id=@id and OrdersItem.Stance=0", param);
                    param.Add("@LocationId", List.First().LocationId);
                    foreach (var item in List)
                    {
                        param.Add("@SalesOrderItemId", item.id);
                        param.Add("@ItemId", item.ItemId);
                        var make = await _db.QueryAsync<SalesOrderRezerve>($"select mo.id,Status from ManufacturingOrder mo where SalesOrderId=@id and SalesOrderItemId=@SalesOrderItemId and ItemId=@ItemId and CompanyId=@CompanyId and Status!=3 and IsActive=1", param);


                        if (make.Count() != 0)
                        {
                            foreach (var items in make)
                            {
                                if (items.Status != 3)
                                {
                                    int Status = 3;
                                    await DoneStock(items.id, T.id, item.id, Status, CompanyId,UserId);
                                }

                            }



                            param.Add("@Stance", 2);
                            param.Add("@Ingredients", 3);
                            param.Add("@SalesItem", 4);
                            param.Add("@Production", 4);
                            await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients,Stance=@Stance,SalesItem=@SalesItem,Production=@Production where CompanyId=@CompanyId and OrdersId=@id and id=@SalesOrderItemId and ItemId=@ItemId ", param);
                            param.Add("@Status", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Status=@Status where CompanyId=@CompanyId and SalesOrderId=@id and SalesOrderItemId=@SalesOrderItemId", param);

                            List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select (Select ISNULL(AllStockQuantity, 0) from Items where id = @ItemId  and CompanyId = @CompanyId)as Quantity, (Select ISNULL(StockCount, 0) from LocationStock where ItemId = @ItemId  and LocationId = @LocationId and CompanyId = @CompanyId) as LocationStock, (Select ISNULL(id, 0) from LocationStock where ItemId = @ItemId and LocationId = @LocationId and CompanyId = @CompanyId)   as LocationStockId,(select RezerveCount from Rezerve where CompanyId=@CompanyId and SalesOrderId=@id and ItemId=@ItemId and Status!=4) as RezerveCount ", param)).ToList();
                            var stockall = sorgu.First().Quantity;
                            float? stockcount = stockall - item.Quantity;
                            param.Add("@StockCount", stockcount);
                            await _db.ExecuteAsync($"Update Items set AllStockQuantity=@StockCount where CompanyId=@CompanyId and  id=@ItemId", param);
                            param.Add("@User", UserId);
                            param.Add("@StockMovementQuantity", item.Quantity);
                            param.Add("@PreviousValue", stockall);
                            param.Add("@Process", "AllStock");
                            param.Add("@Date", DateTime.Now);
                            param.Add("@Operation", "-");

                            param.Add("@Where", "SalesOrderDone");
                            await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@StockCount,@Date,@User,@CompanyId,@LocationId,@ItemId)", param);



                            float? newstock = sorgu.First().LocationStock - item.Quantity;
                            param.Add("@LocationStockCount", newstock);
                            param.Add("@LocationStockId", sorgu.First().LocationStockId);
                            await _db.ExecuteAsync($"Update LocationStock set StockCount=@LocationStockCount where CompanyId=@CompanyId and  id=@LocationStockId", param);

                            param.Add("@PreviousValue", sorgu.First().LocationStock);
                            param.Add("@Process", "LocationStock");

                            await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@LocationStockCount,@Date,@User,@CompanyId,@LocationId,@ItemId)", param);
                        }
                        else
                        {


                            List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select (Select ISNULL(AllStockQuantity, 0) from Items where id =  @ItemId and CompanyId = @CompanyId)as Quantity, (Select ISNULL(StockCount, 0) from LocationStock where ItemId = @ItemId  and LocationId = @LocationId and CompanyId = @CompanyId) as LocationStock, (Select ISNULL(id, 0) from LocationStock where ItemId = @ItemId and LocationId = @LocationId and CompanyId = @CompanyId)   as LocationStockId", param)).ToList();

                            param.Add("@Stance", 2);
                            param.Add("@Ingredients", 3);
                            param.Add("@SalesItem", 4);
                            param.Add("@Production", 4);
                            await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients,Stance=@Stance,SalesItem=@SalesItem,Production=@Production where CompanyId=@CompanyId and OrdersId=@id and id=@SalesOrderItemId and ItemId=@ItemId ", param);

                            param.Add("@Status", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Status=@Status where CompanyId=@CompanyId and SalesOrderId=@id and SalesOrderItemId=@SalesOrderItemId", param);



                        }
                        List<SalesOrderUpdateItems> aa = (await _db.QueryAsync<SalesOrderUpdateItems>($@"select o.id,oi.id as OrderItemId,oi.Quantity,oi.ItemId,oi.PricePerUnit,oi.TaxId,o.ContactId,o.LocationId,o.DeliveryDeadline from Orders o 
                left join OrdersItem oi on oi.OrdersId = o.id where o.CompanyId = @CompanyId and oi.Stance = 0 and o.IsActive = 1 and o.id=@id and oi.ItemId = @ItemId and o.DeliveryId = 0 Order by o.DeliveryDeadline ", param)).ToList();
                        SalesOrderUpdateItems A = new SalesOrderUpdateItems();
                        foreach (var liste in aa)
                        {
                            A.id = liste.id;
                            A.OrderItemId = liste.OrderItemId;
                            A.TaxId = liste.TaxId;
                            A.Quantity = liste.Quantity;
                            A.ItemId = liste.ItemId;
                            A.ContactId = liste.ContactId;
                            A.LocationId = liste.LocationId;
                            A.DeliveryDeadline = liste.DeliveryDeadline;
                            A.PricePerUnit = liste.PricePerUnit;
                            param.Add("@RezerveCount", 0);
                            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and SalesOrderId={liste.id} and SalesOrderItemId={liste.OrderItemId} and ItemId={liste.ItemId} ", param);
                            await UpdateItems(A, T.id, CompanyId);
                        }


                    }



                    param.Add("@DeliveryId", 4);
                    await _db.ExecuteAsync($"Update Orders set DeliveryId=@DeliveryId where CompanyId=@CompanyId and id=@id", param);




                }
            }

            else if (eskiStatus == 2)
            {
                if (T.DeliveryId == 4)
                {
                    param.Add("@DeliveryId", 4);
                    await _db.ExecuteAsync($"Update Orders set DeliveryId=@DeliveryId where CompanyId=@CompanyId and id=@id", param);
                }
            }
            else if (eskiStatus == 4)
            {

            }
        }

        public async Task DoneStock(int id, int SalesOrderId, int SalesOrderItemId, int Status, int CompanyId,int UserId)

        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", id);
            param.Add("@CompanyId", CompanyId);
            param.Add("@SalesOrderId", SalesOrderId);
            param.Add("@SalesOrderItemId", SalesOrderItemId);
            var BomList = await _db.QueryAsync<DoneStock>($@"Select * from ManufacturingOrderItems 
            left join ManufacturingOrder on ManfacturingOrder.id=ManufacturingOrderItems.OrderId
            where CompanyId = {CompanyId} and ManufacturingOrderItems.OrderId={id} and ManufacturingOrder.IsActive=1", param);
            param.Add("@Status", Status);

            if (Status == 3)
            {
                foreach (var item in BomList)
                {
                    if (item.Tip == "Ingredients")
                    {
                        param.Add("ItemId", item.ItemId);

                        string sqla = $@"select 
                          (Select ISNULL(AllStockQuantity,0) from Items where Items.id =@ItemId and Items.CompanyId = @CompanyId)as Quantity,
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
                var ManufacturingQuantity = sorgu4.First().ManufacturingQuantity;
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
                 (select ItemId from ManufacturingOrder where CompanyId=@CompanyId and id=@id and ManufacturingOrder.IsActive=1)as ItemId,
		         (select PlannedQuantity from ManufacturingOrder where CompanyId=@CompanyId and id=@id and ManufacturingOrder.IsActive=1)AS Quantity,
                 (Select RezerveCount from Rezerve where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and ItemId=(select ItemId from ManufacturingOrder where CompanyId=@CompanyId and id=@id and ManufacturingOrder.IsActive=1))as RezerveCount, (select Tip from OrdersItem where CompanyId=@CompanyId and OrdersItem.id=@SalesOrderItemId) as Tip ";
                var sorgu3 = await _db.QueryAsync<LocaVarmı>(sqlv, param);//
                param.Add("@Ingredients", 3);
                param.Add("@SalesItem", 3);
                param.Add("@Production", 4);
                if (SalesOrderId != 0)
                {
                    float quantity = sorgu3.First().Quantity;
                    param.Add("@Tip", sorgu3.First().Tip);
                    param.Add("@ItemId", sorgu3.First().ItemId);
                    param.Add("@Status", 1);

                    param.Add("@RezerveCount", quantity + sorgu3.First().RezerveCount);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount ,ManufacturingOrderId=@id,Tip=@Tip  where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and ItemId=@ItemId ", param);
                    await _db.ExecuteAsync($"Update OrdersItem Set Ingredients=@Ingredients,SalesItem=@SalesItem,Production=@Production where id=@SalesOrderItemId and OrdersId=@SalesOrderId ", param);

                }

            }
            else
            {
                await _db.ExecuteAsync($"Update ManufacturingOrder Set Status=@Status where id=@id and CompanyId=@CompanyId ", param);
            }
        }

        public async Task IngredientsControl(SalesOrderItem T, int OrdersId, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SalesOrderId", OrdersId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@location", T.LocationId);
            var BomList = await _db.QueryAsync<BOM>($"Select * From Bom where CompanyId = {CompanyId} and ProductId = {T.ItemId} and IsActive = 1");
            var b = 0;


            foreach (var item in BomList)
            {

                DynamicParameters prm2 = new DynamicParameters();
                prm2.Add("@ItemId", item.MaterialId);
                prm2.Add("@CompanyId", item.CompanyId);
                prm2.Add("@location", T.LocationId);
                string sqlb = $@"select
                (Select ISNULL(Tip,'') from Items where id = @ItemId and CompanyId = @CompanyId)as Tip,
                 (Select ISNULL(id,0) from LocationStock where ItemId = @ItemId  and LocationId = @location and CompanyId = @CompanyId) as     LocationStockId ";
                var sorgu1 = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqlb, prm2);
                prm2.Add("@LocationStockId", sorgu1.First().LocationStockId);
                prm2.Add("@stockId", sorgu1.First().StockId);
                if (sorgu1.First().LocationStockId == 0)
                {
                    await _loc.Insert(sorgu1.First().Tip, item.MaterialId, CompanyId, T.LocationId);

                }

                var RezerveCount = await _control.Count(item.MaterialId, CompanyId, T.LocationId);//stocktaki adet
                var stokcontrol = T.Quantity * item.Quantity; //bir materialin kaç tane gideceği hesaplanıyor
                if (RezerveCount >= stokcontrol) //yeterli stok var mı
                {
                    var yenistockdeğeri = RezerveCount - stokcontrol;
                    var Rezerve = stokcontrol;
                    prm2.Add("@RezerveCount", Rezerve);
                    prm2.Add("@LocationStockCount", yenistockdeğeri);


                }
                else if (RezerveCount == stokcontrol)
                {

                    prm2.Add("@RezerveCount", stokcontrol);
                    prm2.Add("@LocationStockCount", 0);

                }
                else

                {
                    var yenistockdeğeri = 0;
                    var Rezerve = RezerveCount;
                    prm2.Add("@RezerveCount", Rezerve);
                    prm2.Add("@LocationStockCount", yenistockdeğeri);
                    b += 1;
                    T.Conditions = 1;

                }
                if (b > 0)
                {
                    T.Conditions = 1;
                }
                else
                {
                    T.Conditions = 3;
                }
                prm2.Add("@Status", 1);
                prm2.Add("@OrdersId", OrdersId);
                prm2.Add("@Tip", sorgu1.First().Tip);
                prm2.Add("@ContactsId", T.ContactId);
                await _db.ExecuteAsync($"Insert into Rezerve(SalesOrderId,Tip,ItemId,RezerveCount,CustomerId,LocationId,Status,LocationStockCount,CompanyId) values (@OrdersId,@Tip,@ItemId,@RezerveCount,@ContactsId,@location,@Status,@LocationStockCount,@CompanyId)", prm2);



            }
        }

        public async Task<IEnumerable<SalesOrderDTO.MissingCount>> IngredientsMissingList(SalesOrderDTO.MissingCount T, int CompanyId)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ProductId", T.ProductId);
            prm.Add("@id", T.id);
            prm.Add("@OrderItemId", T.SalesOrderItemId);
            prm.Add("@LocationId", T.LocationId);
            var make = await _db.QueryAsync<SalesOrderRezerve>($"select mo.id from ManufacturingOrder mo where SalesOrderId=@id and SalesOrderItemId=@OrderItemId and CompanyId=@CompanyId and IsActive=1 and Status!=3", prm);
            IEnumerable<MissingCount> materialid;
            IEnumerable<MissingCount> list = new List<MissingCount>();
            if (make.Count() == 0)
            {
                string sql = $"select Bom.MaterialId from Bom where  Bom.ProductId = @ProductId";
                materialid = await _db.QueryAsync<MissingCount>(sql, prm);

                foreach (var item in materialid)
                {
                    prm.Add("@MaterialId", item.MaterialId);
                    string sqlb = $@"select Bom.MaterialId,Items.Name as MaterialName,
        (Select Rezerve.RezerveCount from Rezerve where Rezerve.SalesOrderId = @id and Rezerve.ItemId= @MaterialId and SalesOrderItemId=@OrderItemId) -
        ((Select OrdersItem.Quantity from Orders left join OrdersItem on OrdersItem.OrdersId = Orders.id where Orders.id = @id and OrdersItem.id=@OrderItemId) *
        (select Bom.Quantity from Bom where Bom.MaterialId = @MaterialId and Bom.ProductId = @ProductId))
         AS Missing
        FROM Bom left join Items on Items.id = Bom.MaterialId where Bom.CompanyId = @CompanyId and Bom.ProductId = @ProductId and Bom.MaterialId = @MaterialId";
                    var a = await _db.QueryAsync<MissingCount>(sqlb, prm);
                    list.Append(a.First());

                }
            }
            else
            {
                materialid = await _db.QueryAsync<MissingCount>($@"SELECT ManufacturingOrderItems.ItemId as MaterialId from ManufacturingOrderItems 
            left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
            where ManufacturingOrder.SalesOrderId=@id and ManufacturingOrder.SalesOrderItemId=@OrderItemId and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrder.IsActive=1 and ManufacturingOrder.Status!=3 and ManufacturingOrderItems.Availability=0
            Group By ManufacturingOrderItems.ItemId", prm);
                foreach (var liste in materialid)
                {
                    prm.Add("@MaterialId", liste.MaterialId);
                    string sqlb = $@"select Bom.MaterialId,Items.Name as MaterialName,
             (Select SUM(Rezerve.RezerveCount) from Rezerve where Rezerve.SalesOrderId = @id and Rezerve.ItemId= @MaterialId and SalesOrderItemId=@OrderItemId) -
             (Select SUM(ManufacturingOrderItems.PlannedQuantity) from ManufacturingOrderItems 
		        LEFT join ManufacturingOrder on ManufacturingOrderItems.OrderId=ManufacturingOrder.id
		    where ManufacturingOrder.CompanyId =@CompanyId and ManufacturingOrder.SalesOrderId=@id and ManufacturingOrder.SalesOrderItemId=@OrderItemId and Tip='Ingredients' and ManufacturingOrderItems.ItemId=@MaterialId and ManufacturingOrder.IsActive=1 and ManufacturingOrder.Status!=@MaterialId)+
				(select ISNULL(SUM(Quantity),0) from Orders 
                left join OrdersItem on OrdersItem.OrdersId = Orders.id and Orders.LocationId=@LocationId
                and OrdersItem.ItemId = @MaterialId where Orders.CompanyId = @CompanyId
                and DeliveryId = 1 and Orders.SalesOrderId=@id and Orders.SalesOrderItemId=@OrderItemId and Orders.IsActive=1)
                 AS Missing
                 FROM Bom left join Items on Items.id = Bom.MaterialId where Bom.CompanyId = @CompanyId and Bom.ProductId = @ProductId and Bom.MaterialId = @MaterialId";
                    var a = await _db.QueryAsync<MissingCount>(sqlb, prm);

                    list.Append(a.First());

                }

            }



            return (IEnumerable<SalesOrderDTO.MissingCount>)list;
        }

        public async Task<int> InsertPurchaseItem(SalesOrderItem T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            var liste = await _db.QueryAsync<LocaVarmı>($"select (select DefaultTaxPurchaseOrderId from GeneralDefaultSettings where CompanyId=@CompanyId)as TaxId,(select DefaultPrice from Items where id =@ItemId and CompanyId=@CompanyId)as DefaultPrice", prm);
            prm.Add("@TaxId", T.TaxId);
            float rate = await _db.QueryFirstAsync<int>($"select  Rate from Tax where id =@TaxId and CompanyId=@CompanyId", prm);


            var PriceUnit = liste.First().DefaultPrice;

            var TotalPrice = (T.Quantity * PriceUnit); //adet*fiyat
            float? PlusTax = (TotalPrice * rate) / 100; //tax fiyatı hesaplama
            var TotalAll = TotalPrice + PlusTax; //toplam fiyat hesaplama  
            prm.Add("@Quantity", T.Quantity);
            prm.Add("@PricePerUnit", PriceUnit);
            prm.Add("@TaxValue", rate);
            prm.Add("@OrdersId", T.id);
            prm.Add("@PlusTax", PlusTax);
            prm.Add("@TotalPrice", TotalPrice);
            prm.Add("@TotalAll", TotalAll);
            prm.Add("@location", T.LocationId);
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@Stance", 0);
            if (T.Quotes == 0)
            {
                await _db.ExecuteAsync($"Update Orders set TotalAll=@TotalAll where CompanyId=@CompanyId and id=@OrdersId", prm);

                int id = await _db.QuerySingleAsync<int>($"Insert into OrdersItem(ItemId,Quantity,PricePerUnit,TaxId,TaxValue,OrdersId,TotalPrice,PlusTax,TotalAll,Stance,CompanyId) OUTPUT INSERTED.[id] values (@ItemId,@Quantity,@PricePerUnit,@TaxId,@TaxValue,@OrdersId,@TotalPrice,@PlusTax,@TotalAll,@Stance,@CompanyId)", prm);
                prm.Add("@SalesOrderItemId", id);
                return id;
            }
            else
            {

                string sqla = $@"select
                (Select ISNULL(Tip,'') from Items where id = @ItemId and CompanyId = @CompanyId)as Tip,     
                (Select ISNULL(id,0) from LocationStock where ItemId =  @ItemId  and LocationId = @location and CompanyId = @CompanyId)   as    LocationStockId,
               (select ISNULL(SUM(ManufacturingOrder.PlannedQuantity),0) as Quantity from ManufacturingOrder where     ManufacturingOrder.ItemId=@ItemId and ManufacturingOrder.CompanyId=@CompanyId and   ManufacturingOrder.CustomerId=@ContactId )as ManufacturingQuantity";
                var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, prm);
                var Stock = _control.Count(T.ItemId, CompanyId, T.LocationId);
                var locationStockId = sorgu.First().LocationStockId;
                var tip = sorgu.First().Tip;
                prm.Add("@LocationStockId", locationStockId);
                int rezervid = 0;

                rezervid = await Control(T, T.id, CompanyId);
                if (T.Status == 3)
                {
                    prm.Add("@SalesItem", 3);
                }
                else
                {
                    prm.Add("@SalesItem", 1);
                }


                if (T.Status == 3)
                {
                    prm.Add("@SalesItem", 3);
                    prm.Add("@Production", 4);

                    await _db.ExecuteAsync($"Update Orders set TotalAll=@TotalAll where CompanyId=@CompanyId and id=@OrdersId", prm);
                    prm.Add("@Ingredient", 3);

                    int itemid = await _db.QuerySingleAsync<int>($"Insert into OrdersItem(ItemId,Quantity,PricePerUnit,TaxId,TaxValue,OrdersId,SalesItem,TotalPrice,PlusTax,TotalAll,Stance,Ingredients,CompanyId,Production) OUTPUT INSERTED.[id] values (@ItemId,@Quantity,@PricePerUnit,@TaxId,@TaxValue,@OrdersId,@SalesItem,@TotalPrice,@PlusTax,@TotalAll,@Stance,@Ingredient,@CompanyId,@Production)", prm);
                    prm.Add("@SalesOrderItemId", itemid);

                    prm.Add("@RezerveId", rezervid);

                    await _db.ExecuteAsync($"Update Rezerve set SalesOrderItemId=@SalesOrderItemId where  CompanyId=@CompanyId and CustomerId=@ContactId and SalesOrderId=@OrdersId and LocationId=@location and id=@RezerveId  ", prm);
                    return itemid;
                }
                string sqlquery = $@"select * from ManufacturingOrder where CustomerId is null and SalesOrderId=0 and SalesOrderItemId=0 and Status!=3 and Private='false' and IsActive=1 and ItemId=@ItemId and LocationId=@location Order by id DESC";
                var EmptyManufacturing = await _db.QueryAsync<SalesOrderUpdateMakeBatchItems>(sqlquery, prm);
        

                await IngredientsControl(T, T.id, CompanyId);
                if (T.Conditions == 3)
                {
                    prm.Add("@Ingredient", 2);
                }
                else
                {
                    prm.Add("@Ingredient", 0);
                }
                await _db.ExecuteAsync($"Update Orders set TotalAll=@TotalAll where CompanyId=@CompanyId and id=@OrdersId", prm);

                int id = await _db.QuerySingleAsync<int>($"Insert into OrdersItem(ItemId,Quantity,PricePerUnit,TaxId,TaxValue,OrdersId,TotalPrice,PlusTax,TotalAll,Stance,SalesItem,Ingredients,CompanyId) OUTPUT INSERTED.[id] values (@ItemId,@Quantity,@PricePerUnit,@TaxId,@TaxValue,@OrdersId,@TotalPrice,@PlusTax,@TotalAll,@Stance,@SalesItem,@Ingredient,@CompanyId)", prm);

                prm.Add("@SalesOrderItemId", id);
                prm.Add("@id", T.id);

                if (EmptyManufacturing.Count() != 0)
                {
                    var degerler = 0;
                    string sqlp = $" select ISNULL(RezerveCount,0) from Rezerve where SalesOrderId=@id and  LocationId=@location and ItemId=@ItemId and CompanyId=@CompanyId";
                    var deger = await _db.QueryAsync<int>(sqlp, prm);
                    if (deger.Count() == 0)
                    {
                        degerler = 0;
                    }
                    else
                    {
                        degerler = deger.First();
                    }
                    int varmi = 0;
                    float? aranandeger = T.Quantity - degerler;
                    foreach (var item in EmptyManufacturing)
                    {
                       float toplamuretimadeti = item.PlannedQuantity;
                        if (varmi == 0)
                        {


                            if (toplamuretimadeti >= aranandeger)
                            {
                                prm.Add("@SalesOrderId", T.id);
                                prm.Add("@CompanyId", CompanyId);
                                prm.Add("@SalesOrderItemId", id);
                                prm.Add("@ItemId", T.ItemId);
                                prm.Add("@ManufacturingOrderId", item.id);
                                prm.Add("@ContactId", T.ContactId);

                                prm.Add("@SalesItem", 2);
                                await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@SalesOrderItemId and OrdersId=@SalesOrderId", prm);
                                await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=@ContactId,SalesOrderId=@SalesOrderId,SalesOrderItemId=@SalesOrderItemId where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);

                                string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId  and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.CompanyId=@CompanyId)";
                                var availability = await _db.QueryAsync<int>(sqlr, prm);
                                prm.Add("@Ingredients", availability.First());
                                await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                                varmi++;

                            }
                            else if (toplamuretimadeti < aranandeger)
                            {
                          
                                prm.Add("@SalesOrderId", T.id);
                                prm.Add("@CompanyId", CompanyId);
                                prm.Add("@SalesOrderItemId", id);
                                prm.Add("@ItemId", T.ItemId);
                                prm.Add("@ManufacturingOrderId", item.id);
                                prm.Add("@ContactId", T.ContactId);
                                prm.Add("@SalesItem", 1);
                                await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@SalesOrderItemId and OrdersId=@SalesOrderId", prm);
                                await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=@ContactId,SalesOrderId=@SalesOrderId,SalesOrderItemId=@SalesOrderItemId where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);

                                string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId  and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.CompanyId=@CompanyId)";
                                var availability = await _db.QueryAsync<int>(sqlr, prm);
                                prm.Add("@Ingredients", availability.First());
                                await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                                aranandeger = aranandeger - toplamuretimadeti;


                            }
                        }





                    }
                }

                await _db.ExecuteAsync($"Update Rezerve set SalesOrderItemId=@SalesOrderItemId where  CompanyId=@CompanyId and CustomerId=@ContactId and SalesOrderId=@OrdersId and LocationId=@location and SalesOrderItemId is null ", prm);
                return id;

            }
        }

        public async Task<IEnumerable<SalesOrderItemDetail>> ItemDetail(int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", id);
            string sqla = $@"Select LocationId from Orders where CompanyId=@CompanyId and id=@id";
            var sorgu = await _db.QueryAsync<int>(sqla, prm);
            prm.Add("@LocationId", sorgu.First());

            string sql = $@"
            Select OrdersItem.id as id,OrdersItem.ItemId,Items.Name as ItemName,OrdersItem.Quantity,Items.Tip,
           OrdersItem.PricePerUnit, OrdersItem.TotalAll, OrdersItem.TaxId, Tax.TaxName,OrdersItem.TaxValue as Rate,OrdersItem.SalesItem,OrdersItem.Ingredients,OrdersItem.Production,
		      (LocationStock.StockCount-SUM(ISNULL(rez.RezerveCount,0)))- ISNULL(OrdersItem.Quantity,0)+(SUM(ISNULL(ManufacturingOrder.PlannedQuantity,0)))as missing
		   from Orders 
        inner join OrdersItem on OrdersItem.OrdersId = Orders.id 
		left join Items on Items.id = OrdersItem.ItemId
		left join Measure on Measure.id = OrdersItem.MeasureId
		left join Tax on Tax.id = OrdersItem.TaxId
		LEFT join ManufacturingOrder on ManufacturingOrder.SalesOrderItemId=OrdersItem.id and ManufacturingOrder.SalesOrderId=Orders.id and ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1
        LEFT join Rezerve on Rezerve.SalesOrderItemId=OrdersItem.id and Rezerve.SalesOrderId=Orders.id and Rezerve.ItemId=Items.id
        LEFT join Rezerve rez on rez.ItemId=Items.id and rez.Status=1
	    left join LocationStock on LocationStock.ItemId=Items.id and LocationStock.LocationId=@LocationId
        where Orders.CompanyId = @CompanyId and Orders.id = @id  
		Group by OrdersItem.id,OrdersItem.ItemId,Items.Name,OrdersItem.Quantity,Items.Tip,
           OrdersItem.PricePerUnit, OrdersItem.TotalAll, OrdersItem.TaxId, Tax.TaxName,OrdersItem.TaxValue,
		         OrdersItem.SalesItem,OrdersItem.Ingredients,OrdersItem.Production,LocationStock.StockCount,Rezerve.RezerveCount";
            var ItemsDetail = await _db.QueryAsync<SalesOrderItemDetail>(sql, prm);
            // List<Classes_ManufacturingOrderDetail> Manu = new List<Classes_ManufacturingOrderDetail>(); ; 

            foreach (var item in ItemsDetail)
            {
                prm.Add("@SalesId", item.id);
                string sqlv = $@"select * from ManufacturingOrder where CompanyId=@CompanyId and SalesOrderItemId=@SalesId and ManufacturingOrder.IsActive=1 and ManufacturingOrder.Status!=3";
                var Manu = await _db.QueryAsync<ManufacturingOrderDetail>(sqlv, prm);
                item.MOList = Manu;
            }




            return ItemsDetail;
        }

        public async Task<IEnumerable<SalesOrderSellSomeList>> SellSomeList(SalesOrderSellSomeList T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", T.SalesOrderId);
            prm.Add("@OrderItemId", T.SalesOrderItemId);
            prm.Add("@ItemId", T.ItemId);
            string sql = $@"select Orders.id as SalesOrderId,OrdersItem.id as SalesOrderItemId,OrdersItem.ItemId,ContactId,Contacts.DisplayName as Customer,ISNULL(Rezerve.RezerveCount,0)AS Rezerve from Orders
        left join OrdersItem on OrdersItem.OrdersId=Orders.id
            left join Contacts on Contacts.id=Orders.ContactId
        left join Rezerve on Rezerve.SalesOrderId=Orders.id and Rezerve.ItemId=@ItemId and Rezerve.SalesOrderItemId=OrdersItem.id
        where Orders.id=@id and OrdersItem.id=@OrderItemId
";
            var details = await _db.QueryAsync<SalesOrderSellSomeList>(sql, prm);

            foreach (var item in details)
            {

                string sql2 = $@"select ManufacturingOrder.id,ManufacturingOrder.Name as OrderName,CustomerId as ContactId,Contacts.DisplayName as Customer,PlannedQuantity as Quantity from ManufacturingOrder
        left join Contacts on Contacts.id=ManufacturingOrder.CustomerId
        where ManufacturingOrder.SalesOrderId=@id and ManufacturingOrder.SalesOrderItemId=@OrderItemId and ManufacturingOrder.IsActive=1 and ManufacturingOrder.Status!=3 and ManufacturingOrder.ItemId=@ItemId";
                var Manufacturing = await _db.QueryAsync<SellSomeList>(sql2, prm);

                string sql3 = $@"select Orders.id,Orders.OrderName,ContactId,Contacts.DisplayName as Customer,(OrdersItem.Quantity-ISNULL(Rezerve.RezerveCount,0)-ISNULL(SUM(ManufacturingOrder.PlannedQuantity),0))as Missing
                from Orders
                left join OrdersItem on OrdersItem.OrdersId=Orders.id and OrdersItem.ItemId=@ItemId
                left join Contacts on Contacts.id=Orders.ContactId
                left join Rezerve on Rezerve.SalesOrderId=Orders.id and Rezerve.SalesOrderItemId=OrdersItem.id and Rezerve.Status!=4 and Rezerve.ItemId=@ItemId
                left join ManufacturingOrder on ManufacturingOrder.SalesOrderId=Orders.id and ManufacturingOrder.SalesOrderItemId=OrdersItem.id and ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1 and ManufacturingOrder.ItemId=@ItemId
                where Orders.id=@id and OrdersItem.id=@OrderItemId and Orders.IsActive=1 and Orders.CompanyId=@CompanyId
                Group by OrdersItem.Quantity,Rezerve.RezerveCount,Orders.id,Contacts.DisplayName,Orders.OrderName,Orders.ContactId";
                var Missing = await _db.QueryAsync<SellSomeList>(sql3, prm);
                item.ManufacturingList = Manufacturing;
                item.Missing = Missing;

            }


            return details;
        }

        public async Task StockControl(ManufacturingOrderA T, float? rezervecount, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@SalesOrderId", T.SalesOrderId);
            prm.Add("@CustomerId", T.ContactId);
            prm.Add("@location", T.LocationId);

            string sql = $"   select (Select ISNULL(id, 0) from Stock where ItemId = @ItemId and CompanyId = @CompanyId) as StockId, (Select ISNULL(Quantity, 0) from Stock where Stock.id = (Select ISNULL(id, 0) from Stock where ItemId = @ItemId and CompanyId = @CompanyId  ) and Stock.CompanyId = @CompanyId)as Quantity, (Select ISNULL(RezerveStockCount, 0) from LocationStock where StockId = (Select ISNULL(id, 0) from Stock where ItemId = @ItemId and CompanyId = @CompanyId) and LocationId = @location and CompanyId = @CompanyId)as LocationsStock,(Select ISNULL(id, 0) from LocationStock where StockId = (Select ISNULL(id, 0) from Stock where ItemId = @ItemId and CompanyId = @CompanyId) and LocationId = @location and CompanyId = @CompanyId) as LocationStockId";
            var sorgu = await _db.QueryAsync<LocaVarmı>(sql, prm);
            var stockdeger = sorgu.First().LocationStock;
            var yenistock = stockdeger + rezervecount;
            prm.Add("@RezerveCount", yenistock);
            prm.Add("@id", sorgu.First().LocationStockId);
            await _db.ExecuteAsync($"Update LocationStock set RezerveStockCount=@RezerveCount where CompanyId=@CompanyId and id=@id", prm);
            prm.Add("@Status", 2);
            await _db.ExecuteAsync($"Update SalesOrderRezerve set Status=@Status where ItemId=@ItemId and CompanyId=@CompanyId and CustomerId=@CustomerId and SalesOrderId=@SalesOrderId ", prm);
        }

        public async Task UpdateIngredientsControl(SalesOrderUpdateItems T, int OrdersId, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SalesOrderId", OrdersId);
            prm.Add("@OrderItemId", T.OrderItemId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@location", T.LocationId);
            string sqla = $@"select
        (Select Tip from Items where id = @ItemId and CompanyId = @CompanyId) as Tip,
       (Select ISNULL(id,0) from LocationStock where ItemId = @ItemId  and LocationId = @location and CompanyId = @CompanyId) as  LocationStockId";
            var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, prm);
            var BomList = await _db.QueryAsync<BOM>($"Select * From Bom where CompanyId = {CompanyId} and ProductId = {T.ItemId} and IsActive = 1");
            var b = 0;
            foreach (var item in BomList)
            {

                DynamicParameters prm2 = new DynamicParameters();
                prm2.Add("@ItemId", item.MaterialId);
                prm2.Add("@SalesOrderId", OrdersId);
                prm2.Add("@OrderItemId", T.OrderItemId);
                prm2.Add("@CompanyId", item.CompanyId);
                prm2.Add("@location", T.LocationId);
                string sqlb = $@"select
                (Select ISNULL(Tip,'') from Items where id = @ItemId and CompanyId = @CompanyId)as Tip,
                 (Select ISNULL(id,0) from LocationStock where ItemId =  @ItemId and LocationId = @location and CompanyId = @CompanyId) as     LocationStockId";
                var sorgu1 = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqlb, prm2);
                int degerler;
                string sqlp = $" select ISNULL(RezerveCount,0) from Rezerve where SalesOrderId=@SalesOrderId and SalesOrderItemId=@OrderItemId and LocationId=@location and ItemId=@ItemId and CompanyId=@CompanyId";
                var deger = await _db.QueryAsync<int>(sqlp, prm2);
                if (deger.Count() == 0)
                {
                    degerler = 0;
                }
                else
                {
                    degerler = deger.First();
                }




                var RezerveCount =await _control.Count(item.MaterialId, CompanyId, T.LocationId);//stocktaki adet
                var stokcontrol = T.Quantity * item.Quantity; //bir materialin kaç tane gideceği hesaplanıyor
                if (deger.Count() == 0)
                {


                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@ItemId", item.MaterialId);
                    param2.Add("@CompanyId", item.CompanyId);
                    param2.Add("@location", T.LocationId);
                    if (RezerveCount >= stokcontrol) //yeterli stok var mı
                    {

                        prm2.Add("@RezerveCount", stokcontrol);
                        prm2.Add("@LocationStockCount", RezerveCount);



                    }
                    else if (RezerveCount == stokcontrol)
                    {

                        prm2.Add("@RezerveCount", stokcontrol);
                        prm2.Add("@LocationStockCount", 0);

                    }
                    else

                    {
                        var yenistockdeğeri = 0;
                        var Rezerve = RezerveCount;
                        prm2.Add("@RezerveCount", Rezerve);
                        prm2.Add("@LocationStockCount", yenistockdeğeri);

                        b += 1;
                        T.Conditions = 1;

                    }
                    if (b > 0)
                    {
                        T.Conditions = 1;
                    }
                    else
                    {
                        T.Conditions = 3;
                    }
                    prm2.Add("@Status", 1);
                    prm2.Add("@OrdersId", OrdersId);
                    prm2.Add("@Tip", sorgu1.First().Tip);
                    prm2.Add("@ContactsId", T.ContactId);
                    prm2.Add("@OrderItemId", T.OrderItemId);
                    await _db.ExecuteAsync($"Insert into Rezerve (SalesOrderId,SalesOrderItemId,Tip,ItemId,RezerveCount,CustomerId,LocationId,Status,LocationStockCount,CompanyId) values (@OrdersId,@OrderItemId,@Tip,@ItemId,@RezerveCount,@ContactsId,@location,@Status,@LocationStockCount,@CompanyId)", prm2);




                }
                else
                {
                    if (degerler > stokcontrol)
                    {
                        prm2.Add("@RezerveCount", stokcontrol);
                        prm2.Add("@LocationStockCount", RezerveCount);

                    }

                    else if (stokcontrol == degerler)
                    {
                        prm2.Add("@RezerveCount", stokcontrol);
                        prm2.Add("@LocationStockCount", RezerveCount);
                    }
                    else if (RezerveCount > stokcontrol - degerler) //yeterli stok var mı
                    {
                        prm2.Add("@RezerveCount", stokcontrol);
                        prm2.Add("@LocationStockCount", RezerveCount);



                    }
                    else if (RezerveCount == stokcontrol - degerler)
                    {
                        prm2.Add("@RezerveCount", stokcontrol);
                        prm2.Add("@LocationStockCount", 0);

                    }
                    else

                    {

                        prm2.Add("@RezerveCount", RezerveCount + degerler);
                        prm2.Add("@LocationStockCount", RezerveCount);

                        b += 1;
                        T.Conditions = 1;

                    }
                }


                if (b > 0)
                {
                    T.Conditions = 1;
                }
                else
                {
                    T.Conditions = 3;
                }
                prm2.Add("@Status", 1);
                prm2.Add("@OrdersId", OrdersId);
                prm2.Add("@Tip", sorgu1.First().Tip);
                prm2.Add("@ContactsId", T.ContactId);
                await _db.ExecuteAsync($"Update Rezerve set Tip=@Tip,RezerveCount=@RezerveCount,CustomerId=@ContactsId,LocationId=@location,Status=@Status,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and SalesOrderId=@OrdersId and ItemId=@ItemId", prm2);



            }
            if (T.id != 0)
            {
                prm.Add("@OrderId", OrdersId);
                prm.Add("@CompanyId", CompanyId);
                prm.Add("@ItemId", T.ItemId);
                prm.Add("@SalesOrderId", T.id);
                prm.Add("@SalesOrderItemId", T.OrderItemId);

                if (b > 0)
                {
                    prm.Add("@Ingredients", 0);
                }
                else
                {
                    prm.Add("@Ingredients", 2);
                }

                await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
            }

        }

        public async Task UpdateItems(SalesOrderUpdateItems T, int id, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@OrderItemId", T.OrderItemId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@Quantity", T.Quantity);
            prm.Add("@PricePerUnit", T.PricePerUnit);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@TaxId", T.TaxId);
            prm.Add("@CustomerId", T.ContactId);
            var make = await _db.QueryAsync<SalesOrderRezerve>($"select mo.id from ManufacturingOrder mo where SalesOrderId=@id and SalesOrderItemId=@OrderItemId and CompanyId=@CompanyId and IsActive=1 and Status!=3", prm);
            var adetbul = await _db.QueryFirstAsync<float>($"Select Quantity From OrdersItem where CompanyId = {CompanyId} and id =@OrderItemId and OrdersId=@id", prm);
            float eski = adetbul;
            string sqlv = $@"Select ItemId  from  OrdersItem where CompanyId=@CompanyId and id=@OrderItemId";
            var Item = await _db.QuerySingleAsync<int>(sqlv, prm);
            if (T.ItemId != Item)
            {
                List<Manufacturing> ItemsCount = (await _db.QueryAsync<Manufacturing>($"select ItemId,RezerveCount from Rezerve where SalesOrderId=@id and CompanyId=@CompanyId and SalesOrderItemId=@OrderItemId  and Status=1", prm)).ToList();
                foreach (var item in ItemsCount)
                {
                    prm.Add("@ItemId", item.ItemId);
                    prm.Add("@Status", 4);
                    await _db.ExecuteAsync($"Update Rezerve set Status=@Status where SalesOrderId=@id and SalesOrderItemId=@OrderItemId and CompanyId=@CompanyId and ItemId=@ItemId", prm);
                }

            }

            int makeorderId;
            if (make.Count() == 0)
            {
                makeorderId = 0;
            }
            else
            {
                makeorderId = make.First().id;
            }
            T.ManufacturingOrderId = makeorderId;



            prm.Add("@location", T.LocationId);

            if (T.Quotes == 0)
            {
                prm.Add("@CompanyId", CompanyId);
                prm.Add("@ItemId", T.ItemId);
                var Rate = await _db.QueryFirstAsync<float>($"(select Rate from Tax where id =@TaxId and CompanyId=@CompanyId)", prm);
                float TaxRate = Rate;
                var PriceUnit = T.PricePerUnit;
                float totalprice = (T.Quantity * PriceUnit); //adet*fiyat
                float? PlusTax = (totalprice * TaxRate) / 100; //tax fiyatı hesaplama
                float? total = totalprice + PlusTax; //toplam fiyat hesaplama  
                prm.Add("@Quantity", T.Quantity);
                prm.Add("@PricePerUnit", PriceUnit);
                prm.Add("@TaxId", T.TaxId);
                prm.Add("@TaxValue", TaxRate);
                prm.Add("@OrdersId", id);
                prm.Add("@PlusTax", PlusTax);
                prm.Add("@TotalPrice", totalprice);
                prm.Add("@TotalAll", total);
                prm.Add("@ContactsId", T.ContactId);
                await _db.ExecuteAsync($@"Update OrdersItem set Quantity=@Quantity,TotalAll=@TotalAll,PricePerUnit=@PricePerUnit,TaxId=@TaxId,TaxValue=@TaxValue where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);

            }

            else if (makeorderId == 0)
            {
                int degerler;
                string sqlp = $" select ISNULL(RezerveCount,0) from Rezerve where SalesOrderId=@id and SalesOrderItemId=@OrderItemId and LocationId=@location and ItemId=@ItemId and CompanyId=@CompanyId";
                var deger = await _db.QueryAsync<int>(sqlp, prm);
                if (deger.Count() == 0)
                {
                    degerler = 0;
                }
                else
                {
                    degerler = deger.First();
                }
                prm.Add("@CompanyId", CompanyId);
                prm.Add("@ItemId", T.ItemId);
                prm.Add("@Null", null);
                await _db.ExecuteAsync($"Update OrdersItem set Tip=@Null where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);

                var Rate = await _db.QuerySingleAsync<float>($"(select Rate from Tax where id =@TaxId and CompanyId=@CompanyId)", prm);
                float TaxRate = Rate;


                var PriceUnit = T.PricePerUnit;
                float totalprice = (T.Quantity * PriceUnit); //adet*fiyat
                float? PlusTax = (totalprice * TaxRate) / 100; //tax fiyatı hesaplama
                float? total = totalprice + PlusTax; //toplam fiyat hesaplama  
                prm.Add("@Quantity", T.Quantity);
                prm.Add("@PricePerUnit", PriceUnit);
                prm.Add("@TaxId", T.TaxId);
                prm.Add("@TaxValue", TaxRate);
                prm.Add("@OrdersId", id);
                prm.Add("@PlusTax", PlusTax);
                prm.Add("@TotalPrice", totalprice);
                prm.Add("@TotalAll", total);
                prm.Add("@ContactsId", T.ContactId);
                string sqla = $@"select
                  (Select ISNULL(Tip,'') from Items where id = @ItemId and CompanyId = @CompanyId)as Tip,
                  (Select ISNULL(id,0) from LocationStock where ItemId = @ItemId  and LocationId = @location and CompanyId = @CompanyId)   as    LocationStockId ";
                var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, prm);
                var RezerveCount = await _control.Count(T.ItemId, CompanyId, T.LocationId);
                var locationStockId = sorgu.First().LocationStockId;
                var tip = sorgu.First().Tip;
                prm.Add("@LocationStockId", locationStockId);
                if (locationStockId == 0)
                {
                    await _loc.Insert(tip, T.ItemId, CompanyId, T.LocationId);
                }

                var status = 0;


                if (degerler > T.Quantity)
                {
                    prm.Add("@RezerveCount", T.Quantity);
                    prm.Add("@LocationStockCount", RezerveCount);
                    prm.Add("@SalesItem", 3);
                    status = 3;
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and SalesOrderId=@id and ItemId=@ItemId and SalesOrderItemId=@OrderItemId", prm);
                }
                else if (degerler == T.Quantity)
                {
                    prm.Add("@RezerveCount", degerler);
                    prm.Add("@LocationStockCount", RezerveCount);
                    prm.Add("@SalesItem", 3);
                    status = 3;
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and SalesOrderId=@id and ItemId=@ItemId and SalesOrderItemId=@OrderItemId", prm);
                }


                else if (RezerveCount > T.Quantity - degerler)
                {
                    prm.Add("@RezerveCount", T.Quantity);
                    prm.Add("@LocationStockCount", RezerveCount);
                    prm.Add("@SalesItem", 3);
                    status = 3;
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and SalesOrderId=@id and ItemId=@ItemId and SalesOrderItemId=@OrderItemId", prm);
                }

                else if (RezerveCount == T.Quantity - degerler && RezerveCount != 0)
                {
                    prm.Add("@RezerveCount", RezerveCount + degerler);//Stok sayısı adetten kücük ise rezer sayısı Stok adeti kadar olur.
                    prm.Add("@LocationStockCount", RezerveCount);
                    status = 3;
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and SalesOrderId=@id and ItemId=@ItemId and SalesOrderItemId=@OrderItemId", prm);
                    prm.Add("@SalesItem", 3);
                }
                else
                {
                    prm.Add("@RezerveCount", degerler + RezerveCount);
                    prm.Add("@LocationStockCount", RezerveCount);
                    prm.Add("@SalesItem", 1);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and SalesOrderId=@id and ItemId=@ItemId and SalesOrderItemId=@OrderItemId", prm);
                }



                if (status == 3)
                {
                    prm.Add("@SalesItem", 3);
                    prm.Add("@Production", 4);

                    await _db.ExecuteAsync($"Update Orders set TotalAll=@TotalAll where CompanyId=@CompanyId and id=@OrdersId", prm);
                    prm.Add("@Ingredient", 3);
                    await _db.ExecuteAsync($"Update OrdersItem set ItemId=@ItemId,Quantity=@Quantity,PricePerUnit=@PricePerUnit,TaxId=@TaxId,TaxValue=@TaxValue,OrdersId=@id,TotalPrice=@TotalPrice,PlusTax=@PlusTax,TotalAll=@TotalAll,SalesItem=@SalesItem,Ingredients=@Ingredient,Production=@Production where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id ", prm);
                }
                else
                {
                    await UpdateIngredientsControl(T, id, CompanyId);
                    if (T.Conditions == 3)
                    {
                        prm.Add("@Ingredient", 2);
                    }
                    else
                    {
                        prm.Add("@Ingredient", 0);
                    }
                }

                await _db.ExecuteAsync($"Update Orders set TotalAll=@TotalAll where CompanyId=@CompanyId and id=@OrdersId", prm);

                await _db.ExecuteAsync($@"Update OrdersItem set ItemId=@ItemId,Quantity=@Quantity,PricePerUnit=@PricePerUnit,TaxId=@TaxId,TaxValue=@TaxValue,TotalPrice=@TotalPrice,PlusTax=@PlusTax,
                TotalAll=@TotalAll,SalesItem=@SalesItem,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);



            }
            else if (T.Tip == "MakeBatch")
            {
                prm.Add("@CompanyId", CompanyId);
                prm.Add("@ItemId", T.ItemId);
                var Rate = await _db.QueryFirstAsync<float>($"(select Rate from Tax where id =@TaxId and CompanyId=@CompanyId)", prm);
                float TaxRate = Rate;
                var PriceUnit = T.PricePerUnit;
                float totalprice = (T.Quantity * PriceUnit); //adet*fiyat
                float? PlusTax = (totalprice * TaxRate) / 100; //tax fiyatı hesaplama
                float? total = totalprice + PlusTax; //toplam fiyat hesaplama  
                prm.Add("@Quantity", T.Quantity);
                prm.Add("@PricePerUnit", PriceUnit);
                prm.Add("@TaxId", T.TaxId);
                prm.Add("@TaxValue", TaxRate);
                prm.Add("@OrdersId", id);
                prm.Add("@PlusTax", PlusTax);
                prm.Add("@TotalPrice", totalprice);
                prm.Add("@TotalAll", total);
                prm.Add("@ContactsId", T.ContactId);
                await _db.ExecuteAsync($@"Update OrdersItem set Quantity=@Quantity,TotalAll=@TotalAll,PricePerUnit=@PricePerUnit,TaxId=@TaxId,TaxValue=@TaxValue where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);
                await UpdateMakeBatchItems(T, CompanyId, eski);
            }
            else if (T.Tip == "MakeOrder")
            {
                prm.Add("@CompanyId", CompanyId);
                prm.Add("@ItemId", T.ItemId);
                var Rate = await _db.QueryFirstAsync<float>($"(select Rate from Tax where id =@TaxId and CompanyId=@CompanyId)", prm);
                float TaxRate = Rate;
                var PriceUnit = T.PricePerUnit;
                float totalprice = (T.Quantity * PriceUnit); //adet*fiyat
                float? PlusTax = (totalprice * TaxRate) / 100; //tax fiyatı hesaplama
                float? total = totalprice + PlusTax; //toplam fiyat hesaplama  
                prm.Add("@Quantity", T.Quantity);
                prm.Add("@PricePerUnit", PriceUnit);
                prm.Add("@TaxId", T.TaxId);
                prm.Add("@TaxValue", TaxRate);
                prm.Add("@OrdersId", id);
                prm.Add("@PlusTax", PlusTax);
                prm.Add("@TotalPrice", totalprice);
                prm.Add("@TotalAll", total);
                prm.Add("@ContactsId", T.ContactId);
                prm.Add("@SalesItem", 2);
                await _db.ExecuteAsync($@"Update OrdersItem set Quantity=@Quantity,TotalAll=@TotalAll,PricePerUnit=@PricePerUnit,SalesItem=@SalesItem,TaxId=@TaxId,TaxValue=@TaxValue where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);
                await UpdateMakeItems(T, eski, CompanyId);
            }
        }

        public async Task UpdateMakeItems(SalesOrderUpdateItems T, float eski, int CompanyId)
        {
            int? id = T.id;
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@ManufacturingOrderId", T.ManufacturingOrderId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@OrderItemId", T.OrderItemId);
            prm.Add("@ItemsId", T.ItemId);


            var LocationId = await _db.QueryFirstAsync<int>($"Select LocationId From Orders where CompanyId =@CompanyId and id =@id", prm);
            prm.Add("@LocationId", LocationId);

            prm.Add("@PlannedQuantity", T.Quantity);
            string sqlp = $@"Update ManufacturingOrder Set PlannedQuantity=@PlannedQuantity where CompanyId=@CompanyId and id=@ManufacturingOrderId and SalesOrderId=@id and SalesOrderItemId=@OrderItemId and ItemId=@ItemsId  ";
            await _db.ExecuteAsync(sqlp, prm);


            //Eklenen Ordera ait ıtemin  Bomlarını buluyoruz
            var BomList = await _db.QueryAsync<BOM>($"Select id,ISNULL(ItemId,0) as MaterialId,ISNULL(PlannedQuantity,0) as Quantity,ISNULL(Notes,'') as Note from ManufacturingOrderItems where CompanyId = {CompanyId} and ManufacturingOrderItems.OrderId=@ManufacturingOrderId  and Tip='Ingredients'", prm);
            float adet = T.Quantity;


            foreach (var item in BomList)
            {

                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Ingredients");
                param.Add("@ManufacturingOrderItemId", item.id);
                param.Add("@ManufacturingOrderId", T.ManufacturingOrderId);
                param.Add("@id", id);
                param.Add("@CompanyId", CompanyId);
                param.Add("@OrderItemId", T.OrderItemId);
                param.Add("@ItemId", item.MaterialId);
                param.Add("@Notes", item.Note);
                if (adet == eski)
                {
                    param.Add("@PlannedQuantity", item.Quantity);
                }
                else if (adet > eski)
                {
                    float anadeger = item.Quantity / eski;
                    float yenideger = adet - eski;
                    var artışdegeri = yenideger * anadeger;
                    item.Quantity = item.Quantity + artışdegeri;
                    param.Add("@PlannedQuantity", item.Quantity);
                }
                else
                {
                    var yenideger = item.Quantity / eski;
                    var deger = eski - adet;
                    item.Quantity = item.Quantity - (yenideger * deger);
                    param.Add("@PlannedQuantity", item.Quantity);
                }


                param.Add("@CompanyId", CompanyId);
                param.Add("@LocationId", LocationId);
                string sql = $@"Update ManufacturingOrderItems Set PlannedQuantity=@PlannedQuantity where CompanyId=@CompanyId and OrderId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and ItemId=@ItemId ";
                await _db.ExecuteAsync(sql, param);
                // materyalin DefaultPrice
                // Bul
                string sqlb = $@"select ISNULL(SUM(Quantity),0) from Orders 
                left join OrdersItem on OrdersItem.OrdersId = Orders.id and Orders.LocationId=@LocationId
                and OrdersItem.ItemId = @ItemId where Orders.CompanyId = @CompanyId
                and DeliveryId = 1 and Orders.ManufacturingOrderId=@ManufacturingOrderId and Orders.IsActive=1 and Orders.Tip='PurchaseOrder' ";
                var expectedsorgu = await _db.QueryAsync<float>(sqlb, param);
                float expected = expectedsorgu.First();



                List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select (Select ISNULL(id, 0) from LocationStock where ItemId=@ItemId and LocationId = @LocationId and CompanyId = @CompanyId)   as LocationStockId, (select ISNULL(DefaultPrice, 0) From Items where CompanyId = @CompanyId and id = @ItemId)as  DefaultPrice", param)).ToList();
                float DefaultPrice = sorgu.First().DefaultPrice;
                param.Add("@Cost", DefaultPrice * item.Quantity);
                param.Add("@LocationStockId", sorgu.First().LocationStockId);

                string sqlTY = $@"select
           (moi.PlannedQuantity-SUM(ISNULL(OrdersItem.Quantity,0))-ISNULL((Rezerve.RezerveCount),0))as Missing
            
            from ManufacturingOrderItems moi
            left join Items on Items.id=moi.ItemId
            left join ManufacturingOrder on ManufacturingOrder.id=moi.OrderId 
			LEFT join Rezerve on Rezerve.ManufacturingOrderId=ManufacturingOrder.id and Rezerve.ManufacturingOrderItemId=moi.id and Rezerve.Status=1
            left join LocationStock on LocationStock.ItemId=Items.id and LocationStock.LocationId=@LocationId
            left join Orders on Orders.ManufacturingOrderId=ManufacturingOrder.id and  Orders.SalesOrderId is null  and moi.id=Orders.ManufacturingOrderItemId and Orders.ManufacturingOrderItemId=moi.id
            left join OrdersItem on OrdersItem.OrdersId=Orders.id  and DeliveryId = 1 and OrdersItem.ItemId=moi.ItemId
            where  moi.CompanyId = @CompanyId and moi.OrderId = @ManufacturingOrderId and moi.Tip='Ingredients'  and ManufacturingOrder.id=@ManufacturingOrderId and moi.id=@ManufacturingOrderItemId and 
			 ManufacturingOrder.Status!=3
            Group by moi.id,moi.Tip,moi.ItemId,Items.Name,Notes,moi.Cost,moi.Availability
            ,LocationStock.RezerveStockCount,moi.PlannedQuantity,SalesOrderRezerve.RezerveCount";
                var missingdeger = await _db.QueryAsync<float>(sqlTY, param);
                float missingcount;
                if (missingdeger.Count() == 0)
                {
                    missingcount = 0;
                }
                else
                {
                    missingcount = missingdeger.First();
                }



                //Avaibility Hesapla Stoktaki miktar işlemi gerçekleştirmeye yetiyormu kontrol et

                string Tip = "Material";

                if (sorgu.First().LocationStockId == 0)
                {
                    await _loc.Insert(Tip, item.MaterialId, CompanyId, LocationId);
                    param.Add("@Availability", 0);
                }
                else
                {


                    float RezerveStockCount = await _control.Count(item.MaterialId, CompanyId, LocationId);
                    List<int> Count = (await _db.QueryAsync<int>($"Select ISNULL(Rezerve.RezerveCount,0)as Count from Rezerve where CompanyId=@CompanyId and ItemId=@ItemId and ManufacturingOrderId=@ManufacturingOrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and ItemId=@ItemId and SalesOrderId=@id and SalesOrderItemId=@OrderItemId and SalesOrderRezerve.Status=1", param)).ToList();
                    float Counts;
                    if (Count.Count() == 0)
                    {
                        Counts = 0;
                    }
                    else
                    {
                        Counts = Count[0];
                    }
                    float newQuantity;
                    if (Counts == item.Quantity)
                    {
                        newQuantity = Count[0];
                        param.Add("@RezerveCount", newQuantity);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                        param.Add("@LocationStockCount", RezerveStockCount);
                        param.Add("@Availability", 2);
                    }
                    else if (Counts > item.Quantity)
                    {
                        newQuantity = Count[0] - item.Quantity;
                        param.Add("@RezerveCount", newQuantity);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                        param.Add("@LocationStockCount", RezerveStockCount);
                        param.Add("@Availability", 2);
                    }

                    else if (RezerveStockCount == item.Quantity - Counts)
                    {

                        param.Add("@RezerveCount", item.Quantity);
                        param.Add("@LocationStockCount", RezerveStockCount);
                        param.Add("@Availability", 2);
                    }
                    else if (RezerveStockCount > item.Quantity - Counts)
                    {
                        newQuantity = RezerveStockCount - item.Quantity;
                        param.Add("@RezerveCount", item.Quantity);
                        param.Add("@LocationStockCount", RezerveStockCount);
                        param.Add("@Availability", 2);
                    }


                    else if (RezerveStockCount < item.Quantity - Counts)
                    {

                        param.Add("@RezerveCount", RezerveStockCount + Counts);
                        param.Add("@LocationStockCount", RezerveStockCount);
                        param.Add("@Availability", 0);
                    }

                    else
                    {
                        float degeryok = 0;
                        if (missingdeger.Count() == 0)
                        {
                            degeryok = 1;

                        }
                        if (missingcount * (-1) <= expected && degeryok != 1 && missingcount * (-1) > 0 && expected > 0)
                        {
                            param.Add("@Availability", 1);
                        }
                        else
                        {
                            param.Add("@Availability", 0);
                        }

                    }

                    param.Add("@Status", 1);

                    await _db.ExecuteAsync($"Update Rezerve set  Tip=@Tip,ItemId=@ItemId,RezerveCount=@RezerveCount,LocationId=@LocationId,Status=@Status,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ItemId=@ItemId and ManufacturingOrderItemId=@ManufacturingOrderItemId and SalesOrderId=@id and SalesOrderItemId=@OrderItemId ", param);




                }
                string sqlw = $@"Update ManufacturingOrderItems Set Tip=@Tip,ItemId=@ItemId,Notes=@Notes,PlannedQuantity=@PlannedQuantity,Cost=@Cost,Availability=@Availability where CompanyId=@CompanyId and OrderId=@ManufacturingOrderId and ItemId=@ItemId and id=@ManufacturingOrderItemId  ";
                await _db.ExecuteAsync(sqlw, param);

                if (id != 0 || id != null)
                {
                    prm.Add("@OrderId", id);
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@ItemId", T.ItemId);
                    prm.Add("@SalesOrderId", T.id);
                    prm.Add("@SalesOrderItemId", T.OrderItemId);
                    prm.Add("@ManufacturingOrderItemId", item.id);

                    string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId and ManufacturingOrderItems.id=@ManufacturingOrderItemId and ManufacturingOrderItems.Tip='Ingredients')";
                    var availability = await _db.QueryAsync<int>(sqlr, prm);
                    prm.Add("@Ingredients", availability.First());
                    await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                }

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
                if (adet == eski)
                {
                    param.Add("@PlannedQuantity", item.OperationTime);
                }
                else if (adet > eski)
                {
                    float yenideger = adet - eski;
                    float artışdegeri = yenideger * item.OperationTime;
                    item.OperationTime = item.OperationTime + artışdegeri;

                }
                else
                {
                    float yenideger = item.OperationTime / eski;
                    float deger = eski - adet;
                    item.OperationTime = item.OperationTime - (yenideger * deger);

                }

                param.Add("@PlannedTime ", item.OperationTime);
                param.Add("@Status", 0);
                param.Add("@Cost", (item.CostHour / 60 / 60) * item.OperationTime);
                param.Add("@CompanyId", CompanyId);

                string sql = $@"Update ManufacturingOrderItems Set Tip=@Tip,OrderId=@OrderId,OperationId=@OperationId,ResourceId=@ResourceId,PlannedTime=@PlannedTime,Status=@Status,Cost=@Cost where CompanyId=@CompanyId and OrderId=@OrderId and OperationId=@OperationId and ResourceId=ResourceId  ";
                await _db.ExecuteAsync(sql, param);
            }
        }

        public async Task UpdateMakeBatchItems(SalesOrderUpdateItems T, int CompanyId, float eskiQuantity)
        {
            int? id = T.id;
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@OrderItemId", T.OrderItemId);
            prm.Add("@ItemId", T.ItemId);
            var LocationIdAl = await _db.QueryFirstAsync<int>($"Select LocationId From Orders where CompanyId =@CompanyId and id =@id", prm);
            int LocationId = LocationIdAl;
            prm.Add("@LocationId", LocationId);

            var status = await _db.QueryFirstAsync<int>($@"select OrdersItem.SalesItem from OrdersItem 
                    where OrdersItem.OrdersId=@id and OrdersItem.id=@OrderItemId and CompanyId=@CompanyId", prm);
            int statusId = status;
            prm.Add("@ItemsId", T.ItemId);


            //Adet azaltılırken hangi üretimin olacağı belirlenecek
            string sqlsorgu = $@"select ManufacturingOrder.id,ManufacturingOrder.PlannedQuantity from ManufacturingOrder 
            where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.SalesOrderId=@id  
            and ManufacturingOrder.SalesOrderItemId=@OrderItemId and ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1 and ManufacturingOrder.Private='false' Order by id DESC ";
            var Manufacturing = await _db.QueryAsync<SalesOrderUpdateMakeBatchItems>(sqlsorgu, prm);


            string sqlquerys = $@"select SUM(ISNULL(ManufacturingOrder.PlannedQuantity,0)) from ManufacturingOrder 
            where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.SalesOrderId=@id  
            and ManufacturingOrder.SalesOrderItemId=@OrderItemId and ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1 and ManufacturingOrder.Private='false'";
            var expected = await _db.QueryFirstAsync<int>(sqlquerys, prm);

            //adet yükseltilirken stokta yok ise boşta bir üretim var ise onu alır.
            string sqlquery = $@"select * from ManufacturingOrder where CustomerId is null and SalesOrderId is null and SalesOrderItemId is null and Status!=3 and Private='false' and IsActive=1 and ItemId=@ItemsId Order by id DESC";
            var EmptyManufacturing = await _db.QueryAsync<SalesOrderUpdateMakeBatchItems>(sqlquery, prm);

            string sqlb = $@"Select ISNULL(id,0) from LocationStock where ItemId =@ItemId and LocationId = @LocationId and CompanyId = @CompanyId";
            var locationstockid = await _db.QueryFirstAsync<int>(sqlb, prm);
            prm.Add("@LocationStockId", locationstockid);
            int degerler;
            string sqlp = $" select ISNULL(RezerveCount,0) from Rezerve where SalesOrderId=@id and SalesOrderItemId=@OrderItemId and LocationId=@LocationId and ItemId=@ItemId and CompanyId=@CompanyId";
            var deger = await _db.QueryAsync<int>(sqlp, prm);
            if (deger.Count() == 0)
            {
                degerler = 0;
            }
            else
            {
                degerler = deger.First();
            }

            var RezerveCount = await _control.Count(T.ItemId, CompanyId, LocationId);//stocktaki adet

            if (degerler > T.Quantity)
            {
                prm.Add("@SalesItem", 3);
                prm.Add("@Ingredient", 3);
                prm.Add("@ProductionId", 4);
                prm.Add("@RezerveCount", T.Quantity);
                prm.Add("@LocationStockCount", RezerveCount);
                await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem,Production=@ProductionId,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);
                await _db.ExecuteAsync($"Update Rezerve set  RezerveCount=@RezerveCount,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ItemId=@ItemId and ManufacturingOrderItemId=@ManufacturingOrderItemId and SalesOrderId=@id and SalesOrderItemId=@OrderItemId ", prm);
                if (statusId == 2)
                {

                    if (T.Quantity < eskiQuantity)
                    {
                        foreach (var item in Manufacturing)
                        {
                            await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=NULL ,SalesOrderId=NULL,SalesOrderItemId=NULL where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);
                        }
                    }

                }
            }
            else if (degerler == T.Quantity)
            {
                prm.Add("@SalesItem", 3);
                prm.Add("@Ingredient", 3);
                prm.Add("@ProductionId", 4);
                prm.Add("@RezerveCount", T.Quantity);
                prm.Add("@LocationStockCount", RezerveCount);
                await _db.ExecuteAsync($"Update Rezerve set  RezerveCount=@RezerveCount,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and ItemId=@ItemId  and SalesOrderId=@id and SalesOrderItemId=@OrderItemId ", prm);
                if (statusId == 2)
                {

                    if (T.Quantity < eskiQuantity)
                    {
                        foreach (var item in Manufacturing)
                        {
                            prm.Add("@ManuFacturingId", item.id);
                            await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=NULL ,SalesOrderId=NULL,SalesOrderItemId=NULL where CompanyId=@CompanyId and id=@ManuFacturingId", prm);
                        }
                    }

                }
                await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem,Production=@ProductionId,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);
            }
            else if (RezerveCount > T.Quantity - degerler)
            {
                prm.Add("@SalesItem", 3);
                prm.Add("@Ingredient", 3);
                prm.Add("@ProductionId", 4);
                prm.Add("@RezerveCount", T.Quantity);
                prm.Add("@LocationStockCount", RezerveCount);
                await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem,Production=@ProductionId,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);
                await _db.ExecuteAsync($"Update Rezerve set  RezerveCount=@RezerveCount,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ItemId=@ItemId and ManufacturingOrderItemId=@ManufacturingOrderItemId and SalesOrderId=@id and SalesOrderItemId=@OrderItemId ", prm);
                if (statusId == 2)
                {

                    if (T.Quantity < eskiQuantity)
                    {
                        foreach (var item in Manufacturing)
                        {
                            await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=NULL ,SalesOrderId=NULL,SalesOrderItemId=NULL where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);
                        }
                    }

                }
            }
            else if (RezerveCount == T.Quantity - degerler && RezerveCount != 0)
            {
                prm.Add("@SalesItem", 3);
                prm.Add("@Ingredient", 3);
                prm.Add("@ProductionId", 4);
                prm.Add("@RezerveCount", T.Quantity);
                prm.Add("@LocationStockCount", RezerveCount);
                await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem,Production=@ProductionId,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);
                await _db.ExecuteAsync($"Update Rezerve set  RezerveCount=@RezerveCount,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ItemId=@ItemId and ManufacturingOrderItemId=@ManufacturingOrderItemId and SalesOrderId=@id and SalesOrderItemId=@OrderItemId ", prm);
                if (statusId == 2)
                {

                    if (T.Quantity < eskiQuantity)
                    {
                        foreach (var item in Manufacturing)
                        {
                            await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=NULL ,SalesOrderId=NULL,SalesOrderItemId=NULL where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);
                        }
                    }

                }
            }
            else if (eskiQuantity > T.Quantity && Manufacturing.Count() != 0)
            {

                int varmi = 0;
                float toplamuretimadeti = 0;
                var dususmıktarı = eskiQuantity - T.Quantity;
                var uretimfarki = eskiQuantity - expected-degerler;
                uretimfarki = Math.Abs(uretimfarki);

                if (eskiQuantity == expected)
                {
                    float dusuculecekdeger = eskiQuantity - T.Quantity;
                    foreach (var item in Manufacturing)
                    {
                        prm.Add("@ManufacturingOrderId", item.id);
                        toplamuretimadeti = item.PlannedQuantity;
                        if (varmi == 0)
                        {


                            if (toplamuretimadeti > dusuculecekdeger)
                            {
                                prm.Add("@SalesItem", 2);
                                await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);

                                prm.Add("@SalesOrderId", id);
                                prm.Add("@CompanyId", CompanyId);
                                prm.Add("@SalesOrderItemId", T.OrderItemId);
                                prm.Add("@ItemId", T.ItemId);

                                var plannedquanittiy = toplamuretimadeti - dusuculecekdeger;
                                prm.Add("@Planned", plannedquanittiy);
                                await _db.ExecuteAsync($@"Update ManufacturingOrder set PlannedQuantity=@Planned where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);
                                ManufacturingOrderUpdate M = new ManufacturingOrderUpdate();
                                M.id = item.id;
                                M.LocationId = T.LocationId;
                                M.SalesOrderId = T.id;
                                M.SalesOrderItemId = T.OrderItemId;
                                M.ItemId = T.ItemId;
                                await _manufacturingOrderItem.UpdateOrderItems(M, toplamuretimadeti, CompanyId);

                                string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId  and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.CompanyId=@CompanyId)";
                                var availability = await _db.QueryAsync<int>(sqlr, prm);
                                prm.Add("@Ingredients", availability.First());
                                await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                                varmi++;

                            }
                            else if (toplamuretimadeti == dusuculecekdeger)
                            {
                                await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=NULL ,SalesOrderId=NULL,SalesOrderItemId=NULL where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);

                            }
                            else if (toplamuretimadeti < T.Quantity - degerler)
                            {
                                prm.Add("@SalesItem", 1);
                                await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=NULL ,SalesOrderId=NULL,SalesOrderItemId=NULL where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);


                            }

                        }

                    }

                }
                else if (eskiQuantity > expected)
                {
                    var dusuculecekdeger = dususmıktarı - uretimfarki;
                    dusuculecekdeger = Math.Abs(dusuculecekdeger);
                    foreach (var item in Manufacturing)
                    {
                        toplamuretimadeti = item.PlannedQuantity;
                        if (varmi == 0)
                        {


                            if (toplamuretimadeti > dusuculecekdeger)
                            {
                                prm.Add("@SalesItem", 2);
                                await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);

                                prm.Add("@SalesOrderId", id);
                                prm.Add("@CompanyId", CompanyId);
                                prm.Add("@SalesOrderItemId", T.OrderItemId);
                                prm.Add("@ItemId", T.ItemId);
                                prm.Add("@ManufacturingOrderId", item.id);
                                var plannedquanittiy = toplamuretimadeti - dusuculecekdeger;
                                prm.Add("@Planned", plannedquanittiy);
                                await _db.ExecuteAsync($@"Update ManufacturingOrder set PlannedQuantity=@Planned where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);
                                ManufacturingOrderUpdate M = new ManufacturingOrderUpdate();
                                M.id = item.id;
                                M.LocationId = T.LocationId;
                                M.SalesOrderId = T.id;
                                M.SalesOrderItemId = T.OrderItemId;
                                M.ItemId = T.ItemId;
                                await _manufacturingOrderItem.UpdateOrderItems(M, toplamuretimadeti, CompanyId);

                                string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId  and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.CompanyId=@CompanyId)";
                                var availability = await _db.QueryAsync<int>(sqlr, prm);
                                prm.Add("@Ingredients", availability.First());
                                await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                                varmi++;

                            }
                            else if (toplamuretimadeti == dusuculecekdeger)
                            {
                                await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=NULL ,SalesOrderId=NULL,SalesOrderItemId=NULL where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);

                            }
                            else if (toplamuretimadeti < T.Quantity - degerler)
                            {
                                prm.Add("@SalesItem", 1);
                                await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=NULL ,SalesOrderId=NULL,SalesOrderItemId=NULL where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);


                            }

                        }

                    }
                }
                else if (eskiQuantity < expected)
                {
                    float dusuculecekdeger = eskiQuantity - T.Quantity;
                    foreach (var item in Manufacturing)
                    {
                        toplamuretimadeti = item.PlannedQuantity;
                        if (varmi == 0)
                        {


                            if (toplamuretimadeti > dusuculecekdeger)
                            {
                                prm.Add("@SalesItem", 2);
                                await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);

                                prm.Add("@SalesOrderId", id);
                                prm.Add("@CompanyId", CompanyId);
                                prm.Add("@SalesOrderItemId", T.OrderItemId);
                                prm.Add("@ItemId", T.ItemId);
                                prm.Add("@ManufacturingOrderId", item.id);
                                var plannedquanittiy = toplamuretimadeti - dusuculecekdeger;
                                prm.Add("@Planned", plannedquanittiy);
                                await _db.ExecuteAsync($@"Update ManufacturingOrder set PlannedQuantity=@Planned where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);
                                ManufacturingOrderUpdate M = new ManufacturingOrderUpdate();
                                M.id = item.id;
                                M.LocationId = T.LocationId;
                                M.SalesOrderId = T.id;
                                M.SalesOrderItemId = T.OrderItemId;
                                M.ItemId = T.ItemId;
                                await _manufacturingOrderItem.UpdateOrderItems(M, toplamuretimadeti, CompanyId);

                                string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId  and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.CompanyId=@CompanyId)";
                                var availability = await _db.QueryAsync<int>(sqlr, prm);
                                prm.Add("@Ingredients", availability.First());
                                await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                                varmi++;

                            }
                            else if (toplamuretimadeti == dusuculecekdeger)
                            {
                                await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=NULL ,SalesOrderId=NULL,SalesOrderItemId=NULL where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);

                            }
                            else if (toplamuretimadeti < T.Quantity - degerler)
                            {
                                prm.Add("@SalesItem", 1);
                                await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=NULL ,SalesOrderId=NULL,SalesOrderItemId=NULL where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);


                            }

                        }

                    }
                }

            }
            else if (eskiQuantity<T.Quantity && expected>0 && T.Quantity<=expected+degerler)
            {
                foreach (var item in Manufacturing)
                {
                    var eksik = T.Quantity - degerler;
                    if (item.PlannedQuantity>=eksik)
                    {
                        prm.Add("@SalesItem", 2);
                        prm.Add("@ManufacturingOrderId", item.id);
                        await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);
                        string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId  and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.CompanyId=@CompanyId)";
                        var availability = await _db.QueryAsync<int>(sqlr, prm);
                        prm.Add("@Ingredients", availability.First());
                        await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@id and id=@OrderItemId and ItemId=@ItemId ", prm);

                    }

                }
            }
            else if (eskiQuantity < T.Quantity && EmptyManufacturing.Count() != 0)
            {
                int varmi = 0;
                float toplamuretimadeti = 0;
                float aranandeger = T.Quantity - degerler;
                foreach (var item in EmptyManufacturing)
                {
                    toplamuretimadeti = item.PlannedQuantity;
                    if (varmi == 0)
                    {


                        if (toplamuretimadeti >= aranandeger)
                        {
                            prm.Add("@SalesItem", 2);
                            await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);

                            prm.Add("@SalesOrderId", id);
                            prm.Add("@CompanyId", CompanyId);
                            prm.Add("@SalesOrderItemId", T.OrderItemId);
                            prm.Add("@ItemId", T.ItemId);
                            prm.Add("@ManufacturingOrderId", item.id);
                            prm.Add("@ContactId", T.ContactId);
                            await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=@ContactId,SalesOrderId=@SalesOrderId,SalesOrderItemId=@SalesOrderItemId where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);

                            string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId  and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.CompanyId=@CompanyId)";
                            var availability = await _db.QueryAsync<int>(sqlr, prm);
                            prm.Add("@Ingredients", availability.First());
                            await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                            varmi++;

                        }
                        else if (toplamuretimadeti < aranandeger)
                        {
                            prm.Add("@SalesItem", 1);
                            await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);
                            prm.Add("@SalesOrderId", id);
                            prm.Add("@CompanyId", CompanyId);
                            prm.Add("@SalesOrderItemId", T.OrderItemId);
                            prm.Add("@ItemId", T.ItemId);
                            prm.Add("@ManufacturingOrderId", item.id);
                            prm.Add("@ContactId", T.ContactId);
                            await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=@ContactId,SalesOrderId=@SalesOrderId,SalesOrderItemId=@SalesOrderItemId where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);

                            string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId  and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.CompanyId=@CompanyId)";
                            var availability = await _db.QueryAsync<int>(sqlr, prm);
                            prm.Add("@Ingredients", availability.First());
                            await _db.ExecuteAsync($"Update OrdersItem set Ingredients=@Ingredients where CompanyId=@CompanyId and OrdersId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                            aranandeger = aranandeger - toplamuretimadeti;


                        }
                        else
                        {
                            prm.Add("@SalesItem", 1);
                            await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);
                            await UpdateIngredientsControl(T, T.id, CompanyId);
                        }
                    }





                }

            }
            else
            {
                prm.Add("@SalesItem", 1);
                await _db.ExecuteAsync($@"Update OrdersItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);
                await UpdateIngredientsControl(T, T.id, CompanyId);
            }





        }

    }
}
