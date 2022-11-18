using DAL.Contracts;
using DAL.DTO;
using DAL.StockControl;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.BomDTO;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;

namespace DAL.Repositories
{
    public class SalesOrderRepository : ISalesOrderRepository
    {
        private readonly IDbConnection _db;
        private readonly ILocationStockRepository _loc;
        private readonly IStockControl _control;
        private readonly ISalesOrderItemRepository _salesorderitem;

        public SalesOrderRepository(IDbConnection db, ILocationStockRepository loc, IStockControl control, ISalesOrderItemRepository salesOrderItemRepository)
        {
            _db = db;
            _loc = loc;
            _control = control;
            _salesorderitem = salesOrderItemRepository;
        }

        public async Task DeleteItems(SalesDeleteItems T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@OrdersId", T.OrdersId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@CompanyId", CompanyId);
            if (T.Quotes == 0)
            {
                await _db.ExecuteAsync($"Delete From OrdersItem  where  CompanyId = @CompanyId and id=@OrdersId and OrdersId=@id", prm);
            }

            else if (T.ItemId != 0)
            {

                prm.Add("@Status", 4);

               await _db.ExecuteAsync($"Update Rezerve set Status=@Status where SalesOrderId=@OrdersId and CompanyId=@CompanyId and SalesOrderItemId=@id and ItemId=@ItemId", prm);

                List<ManufacturingOrderA> MANU = (await _db.QueryAsync<ManufacturingOrderA>($@"select ManufacturingOrder.id from ManufacturingOrder 
                where  ManufacturingOrder.SalesOrderId=@OrdersId and ManufacturingOrder.SalesOrderItemId=@id and ManufacturingOrder.ItemId=@ItemId and ManufacturingOrder.IsActive=1 and ManufacturingOrder.CompanyId=@CompanyId ", prm)).ToList();
                int varmı = 1;
                if (MANU.Count() == 0)
                {
                    varmı = 0;
                }


                var status = await _db.QueryFirstAsync<int>($@"select OrdersItem.SalesItem from OrdersItem 
                    where OrdersItem.OrdersId=@OrdersId and OrdersItem.id=@id and CompanyId=@CompanyId", prm);
                await _db.ExecuteAsync($"Delete From OrdersItem  where ItemId = @ItemId and CompanyId = @CompanyId and id=@id and OrdersId=@OrdersId", prm);
                if (varmı == 0)
                {

                    if (status != 3)
                    {
                        var BomList = await _db.QueryAsync<BOM>($"Select * From Bom where CompanyId =@CompanyId and ProductId =@ItemId  and IsActive = 1", prm);
                        foreach (var item in BomList)
                        {
                            prm.Add("@MaterialId", item.MaterialId);
  
                            prm.Add("@Status", 4);        
                            await _db.ExecuteAsync($"Update Rezerve set Status=@Status where SalesOrderId=@OrdersId and SalesOrderItemId=@id and CompanyId=@CompanyId and ItemId=@MaterialId", prm);

                        }
                        await _db.ExecuteAsync($"Delete from OrdersItem where id=@id and OrdersId=@OrdersId and CompanyId=@CompanyId", prm);

                    }

                }
                else
                {
                    await _db.ExecuteAsync($"Delete from OrdersItem where id=@id and OrdersId=@OrdersId and CompanyId=@CompanyId", prm);
                }

            }
            else
            {
                await _db.ExecuteAsync($"Delete From OrdersItem  where  CompanyId = @CompanyId and id=@id and OrdersId=@OrdersId", prm);

            }
        }

        public async Task DeleteStockControl(SalesDelete T, int CompanyId,int User)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@id", T.id);
            param.Add("@IsActive", false);
            param.Add("@User", User);
            param.Add("@Date", DateTime.Now);
            var IsActived = await _db.QueryAsync<bool>($"select IsActive from Orders where id=@id and CompanyId=@CompanyId ", param);
            if (T.Quotes == 0)
            {
                await _db.ExecuteAsync($"Update Orders Set IsActive=@IsActive,DeleteDate=@Date,DeletedUser=@User where id = @id and CompanyId = @CompanyId ", param);
            }
            else if (IsActived.First() == false)
            {

            }
            else
            {
                List<OperationsUpdate> MANU = (await _db.QueryAsync<OperationsUpdate>($"select ManufacturingOrder.id from ManufacturingOrder where  SalesOrderId=@id and IsActive=1 and CompanyId=@CompanyId", param)).ToList();
                foreach (var item in MANU)
                {
                    param.Add("@manuid", item.id);
                    await _db.ExecuteAsync($"Update ManufacturingOrder Set SalesOrderId=NULL , SalesOrderItemId=NULL , CustomerId=NULL where id = @manuid and CompanyId = @CompanyId ", param);
                    await _db.ExecuteAsync($"Update Rezerve Set SalesOrderId=NULL , SalesOrderItemId=NULL , CustomerId=NULL where ManufacturingOrderId = @manuid and CompanyId = @CompanyId ", param);
                }

                List<Manufacturing> ItemsCount = (await _db.QueryAsync<Manufacturing>($"select ItemId,RezerveCount from Rezerve where SalesOrderId=@id and CompanyId=@CompanyId and Status=1", param)).ToList();
                await _db.ExecuteAsync($"Update Orders Set IsActive=@IsActive,DeleteDate=@Date,DeletedUser=@User where id = @id and CompanyId = @CompanyId ", param);
                foreach (var item in ItemsCount)
                {
                    param.Add("@ItemId", item.ItemId);
                    param.Add("@Status", 4);  
                    await _db.ExecuteAsync($"Update Rezerve set Status=@Status where SalesOrderId=@id and CompanyId=@CompanyId and ItemId=@ItemId", param);
                }
            }
        }

        public async Task<int> Insert(SalesOrder T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@DeliveryDeadLine", T.DeliveryDeadline);
            prm.Add("@CreateDate", T.CreateDate);
            prm.Add("@DeliveryId", 0);
            prm.Add("@OrderName", T.OrderName);
            prm.Add("@Info", T.Info);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@LocationId", T.LocationId);
            prm.Add("@IsActive", true);
            prm.Add("@Quotes", T.Quotes);

            return await _db.QuerySingleAsync<int>($"Insert into Orders (Tip,Quotes,ContactId,DeliveryDeadline,CreateDate,OrderName,LocationId,Info,CompanyId,IsActive,DeliveryId) OUTPUT INSERTED.[id] values (@Tip,@Quotes,@ContactId,@DeliveryDeadline,@CreateDate,@OrderName,@LocationId,@Info,@CompanyId,@IsActive,@DeliveryId)", prm);
        }

        public async Task<int> InsertAddress(SalesOrderCloneAddress A, int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", A.Tip);
            prm.Add("FirstName", A.FirstName);
            prm.Add("LastName", A.LastName);
            prm.Add("CompanyName", A.CompanyName);
            prm.Add("Phone", A.Phone);
            prm.Add("AddressLine1", A.AddressLine1);
            prm.Add("AddressLine2", A.AddressLine2);
            prm.Add("CityTown", A.CityTown);
            prm.Add("StateRegion", A.StateRegion);
            prm.Add("ZipPostal", A.ZipPostal);
            prm.Add("Country", A.Country);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@IsActive", true);
            int adressid = 0;
            if (A.Tip=="BillingAddress")
            {
                adressid = await _db.QuerySingleAsync<int>($"Insert into Locations (Tip,FirstName,LastName,CompanyName,Phone, AddressLine1,AddressLine2,CityTown,StateRegion,ZipPostalCode,Country,CompanyId,IsActive)  OUTPUT INSERTED.[id]  values (@Tip,@FirstName,@LastName,@CompanyName,@Phone,@AddressLine1,@AddressLine2,@CityTown,@StateRegion,@ZipPostal,@Country,@CompanyId,@IsActive)", prm);
            }
            else if (A.Tip=="ShippingAddress")
            {
                adressid =  await _db.QuerySingleAsync<int>($"Insert into Locations (Tip,FirstName,LastName,CompanyName,Phone, AddressLine1,AddressLine2,CityTown,StateRegion,ZipPostalCode,Country,CompanyId,IsActive)  OUTPUT INSERTED.[id]  values (@Tip,@FirstName,@LastName,@CompanyName,@Phone,@AddressLine1,@AddressLine2,@CityTown,@StateRegion,@ZipPostal,@Country,@CompanyId,@IsActive)", prm);
            }
            return adressid;
         
        }



        public async Task<int> QuotesCount(SalesOrderList T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@LocationId", T.LocationId);
            string sql=string.Empty;
            if (T.LocationId==null)
            {
                sql = $@"   select COUNT(*)as kayitsayisi from(
            select x.* from (
        select Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName as CustomerName,SUM(Orders.TotalAll)AS TotalAll,
        Orders.DeliveryDeadline,Orders.Quotes FROM Orders
        left join Contacts on Contacts.id=Orders.ContactId
        left join OrdersItem on OrdersItem.OrdersId=Orders.id
       where Orders.CompanyId=@CompanyId and Orders.Tip='SalesOrder' and Orders.IsActive=1 and Orders.DeliveryId!=4 and Orders.Quotes='False' and
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(Orders.TotalAll,'') like '%{T.TotalAll}%' and
         ISNULL(Orders.Quotes,'') like '%{T.Quotes}%' 
        group by Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName,Orders.DeliveryDeadline,Orders.Quotes)x)a";
            }
            else
            {
                sql = $@"   select COUNT(*)as kayitsayisi from(
            select x.* from (
        select Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName as CustomerName,SUM(Orders.TotalAll)AS TotalAll,
        Orders.DeliveryDeadline,Orders.Quotes FROM Orders
        left join Contacts on Contacts.id=Orders.ContactId
        left join OrdersItem on OrdersItem.OrdersId=Orders.id
       where Orders.CompanyId=@CompanyId and Orders.Tip='SalesOrder' AND Orders.LocationId=@LocationId and Orders.IsActive=1 and Orders.DeliveryId!=4 and Orders.Quotes='False' and
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(Orders.TotalAll,'') like '%{T.TotalAll}%' and
         ISNULL(Orders.Quotes,'') like '%{T.Quotes}%' 
        group by Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName,Orders.DeliveryDeadline,Orders.Quotes)x)a";
            }

            var kayitsayisi = await _db.QueryFirstAsync<int>(sql, prm);
            return kayitsayisi;
        }

        public async Task QuotesDone(Quotess T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", T.id);
            prm.Add("@Quotes", T.Quotes);

            if (T.Quotes == 1)
            {

                List<ManufacturingOrderItemsIngredientsUpdate> Itemdegerler = (await _db.QueryAsync<ManufacturingOrderItemsIngredientsUpdate>($@"select ItemId,Quantity,OrdersItem.id from OrdersItem 
                inner join Orders on Orders.id=OrdersItem.OrdersId
                where OrdersItem.CompanyId=@CompanyId and OrdersItem.OrdersId=@id and Orders.IsActive=1 and Orders.DeliveryId!=4", prm)).ToList();
                foreach (var item in Itemdegerler)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@ItemId", item.ItemId);
                    param.Add("@CompanyId", CompanyId);
                    param.Add("@location", T.LocationId);
                    param.Add("@id", T.id);
                    param.Add("@OrderItemId", item.id);
                    param.Add("@ContactId", T.ContactId);

                    string sqla = $@"select
      (Select ISNULL(Tip,'') from Items where id = @ItemId and CompanyId = @CompanyId)as Tip,
     
       (Select ISNULL(id,0) from LocationStock where StockId =  (Select ISNULL(id,0) from Stock where ItemId = @ItemId and CompanyId = @CompanyId) and LocationId = @location and CompanyId = @CompanyId)   as    LocationStockId";
                    var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, param);
                    var RezerveCount = _control.Count(item.ItemId,CompanyId,T.LocationId);
                    var locationStockId = sorgu.First().LocationStockId;
                    var tip = sorgu.First().Tip;
                    param.Add("@LocationStockId", locationStockId);
                    if (locationStockId == 0)
                    {
                        await _loc.Insert(tip, item.ItemId, CompanyId, T.LocationId);
                       var LocaitonId = await _db.QuerySingleAsync<int>($@" Select ISNULL(id,0) from LocationStock where ItemId = @ItemId and CompanyId = @CompanyId) and LocationId = @location and CompanyId = @CompanyId", param);
                        locationStockId = LocaitonId;
                        prm.Add("@LocationStockId", locationStockId);
                    }
                 
                    SalesOrderItem A = new SalesOrderItem();
                    A.ItemId = item.ItemId;
                    A.LocationId = T.LocationId;
                    A.ContactId = T.ContactId;
                    A.Quantity = item.Quantity;
                    if (RezerveCount > 0)
                    {


                       await _salesorderitem.Control(A, T.id, CompanyId);
                        if (A.Status == 3)
                        {
                            param.Add("@SalesItem", 3);
                        }
                        else
                        {
                            param.Add("@SalesItem", 1);
                        }

                    }

                    else
                        param.Add("@SalesItem", 1);
                    if (A.Status == 3)
                    {
                        param.Add("@SalesItem", 3);
                        param.Add("@Production", 4);
                        param.Add("@Ingredient", 3);


                        await _db.ExecuteAsync($"Update OrdersItem set SalesItem=@SalesItem,Production=@Production,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId", param);


                        List<int> rezerveId =( await _db.QueryAsync<int>($"SELECT * FROM Rezerve where  CompanyId=@CompanyId and CustomerId=@ContactId and SalesOrderId=@id and LocationId=@location and Status=1 and SalesOrderItemId is null", param)).ToList();
                        param.Add("@RezerveId", rezerveId[0]);

                       await _db.QueryAsync($"Update Rezerve set SalesOrderItemId=@OrderItemId where  CompanyId=@CompanyId and CustomerId=@ContactId and SalesOrderId=@id and LocationId=@location and id=@RezerveId ", param);

                    }
                    else
                    {


                        await _salesorderitem.IngredientsControl(A, T.id, CompanyId);
                        if (A.Conditions == 3)
                        {
                            param.Add("@Ingredient", 2);
                        }
                        else
                        {
                            param.Add("@Ingredient", 0);
                        }
                        param.Add("@Production", 0);

                        await _db.ExecuteAsync($"Update Rezerve set SalesOrderItemId=@OrderItemId where  CompanyId=@CompanyId and CustomerId=@ContactId and SalesOrderId=@id and LocationId=@location and SalesOrderItemId is null ", param);

                        await _db.ExecuteAsync($"Update OrdersItem set SalesItem=@SalesItem,Production=@Production,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId", param);

                    }

                }

            }
            else
            {


            }
        }

        public async Task<IEnumerable<SalesOrderList>> QuotesList(SalesOrderList T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@LocationId", T.LocationId);
            string sql = string.Empty;
            if (T.LocationId==null)
            {

                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName as CustomerName,SUM(Orders.TotalAll)AS TotalAll,
        Orders.DeliveryDeadline,Orders.Quotes FROM Orders
        left join Contacts on Contacts.id=Orders.ContactId
        left join OrdersItem on OrdersItem.OrdersId=Orders.id
       where Orders.CompanyId=@CompanyId and Orders.Tip='SalesOrder'and Orders.IsActive=1 and Orders.DeliveryId!=4 and Orders.Quotes='False' and
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(Orders.TotalAll,'') like '%{T.TotalAll}%' and
         ISNULL(Orders.Quotes,'') like '%{T.Quotes}%' 
        group by Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName,Orders.DeliveryDeadline,Orders.Quotes)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";
            }
            else
            {

                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName as CustomerName,SUM(Orders.TotalAll)AS TotalAll,
        Orders.DeliveryDeadline,Orders.Quotes FROM Orders
        left join Contacts on Contacts.id=Orders.ContactId
        left join OrdersItem on OrdersItem.OrdersId=Orders.id
       where Orders.CompanyId=@CompanyId and Orders.Tip='SalesOrder' AND Orders.LocationId=@LocationId and Orders.IsActive=1 and Orders.DeliveryId!=4 and Orders.Quotes='False' and
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(Orders.TotalAll,'') like '%{T.TotalAll}%' and
         ISNULL(Orders.Quotes,'') like '%{T.Quotes}%' 
        group by Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName,Orders.DeliveryDeadline,Orders.Quotes)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";
            }

            var QuotesList = await _db.QueryAsync<SalesOrderList>(sql, param);

            return QuotesList;
        }

        public async Task<int> SalesOrderCount(SalesOrderList T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@LocationId", T.LocationId);
            string sql = string.Empty;
            if (T.LocationId==null)
            {
                sql = $@" select Count(*)as kayitsayisi from(
            select x.* from (
        select Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName as CustomerName,SUM(Orders.TotalAll)AS TotalAll,
        Orders.DeliveryDeadline,MIN(OrdersItem.SalesItem)as SalesItem,Min(OrdersItem.Ingredients)as Ingredients,Min(OrdersItem.Production)as Production,Orders.LocationId,Locations.LocationName,Orders.DeliveryId FROM Orders
        left join Contacts on Contacts.id=Orders.ContactId
        left join OrdersItem on OrdersItem.OrdersId=Orders.id
		left join Locations on Locations.id=Orders.LocationId
       where Orders.CompanyId=@CompanyId and Orders.Tip='SalesOrder' and Orders.IsActive=1 and Orders.DeliveryId!=4 and Orders.Quotes is null or Orders.Quotes=1 and
       ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(Orders.TotalAll,'') like '%{T.TotalAll}%' and
        ISNULL(SalesItem,'') like '%{T.SalesItem}%' and ISNULL(Ingredients,'') like '%{T.Ingredients}%' and ISNULL(Production,'') like '%{T.Production}%' and ISNULL(Orders.DeliveryId,'') like '%{T.DeliveryId}%' 
        group by Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName,Orders.DeliveryDeadline,Orders.DeliveryId,Orders.LocationId,Locations.LocationName)x)a ";
            }
            else
            {
                sql = $@" select Count(*)as kayitsayisi from(
            select x.* from (
        select Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName as CustomerName,SUM(Orders.TotalAll)AS TotalAll,
        Orders.DeliveryDeadline,MIN(OrdersItem.SalesItem)as SalesItem,Min(OrdersItem.Ingredients)as Ingredients,Min(OrdersItem.Production)as Production,Orders.LocationId,Locations.LocationName,Orders.DeliveryId FROM Orders
        left join Contacts on Contacts.id=Orders.ContactId
        left join OrdersItem on OrdersItem.OrdersId=Orders.id
		left join Locations on Locations.id=Orders.LocationId
       where Orders.CompanyId=@CompanyId and Orders.Tip='SalesOrder' AND Orders.LocationId=@LocationId and Orders.IsActive=1 and Orders.DeliveryId!=4 and Orders.Quotes is null or Orders.Quotes=1 and
       ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(Orders.TotalAll,'') like '%{T.TotalAll}%' and
        ISNULL(SalesItem,'') like '%{T.SalesItem}%' and ISNULL(Ingredients,'') like '%{T.Ingredients}%' and ISNULL(Production,'') like '%{T.Production}%' and ISNULL(Orders.DeliveryId,'') like '%{T.DeliveryId}%' 
        group by Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName,Orders.DeliveryDeadline,Orders.DeliveryId,Orders.LocationId,Locations.LocationName)x)a ";
            }

           var kayitsayisi =await _db.QueryFirstAsync<int>(sql, prm);
            return kayitsayisi;
        }

        public async Task<IEnumerable<SalesOrderList>> SalesOrderList(SalesOrderList T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@LocationId", T.LocationId);
            string sql = string.Empty;
            if (T.LocationId==null)
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName as CustomerName,SUM(Orders.TotalAll)AS TotalAll,
        Orders.DeliveryDeadline,MIN(OrdersItem.SalesItem)as SalesItem,Min(OrdersItem.Ingredients)as Ingredients,Min(OrdersItem.Production)as Production,Orders.LocationId,Locations.LocationName,Orders.DeliveryId FROM Orders
        left join Contacts on Contacts.id=Orders.ContactId
        left join OrdersItem on OrdersItem.OrdersId=Orders.id
		left join Locations on Locations.id=Orders.LocationId
       where Orders.CompanyId=@CompanyId and Orders.Tip='SalesOrder'  and Orders.IsActive=1 and Orders.DeliveryId!=4 and Orders.Quotes is null or Orders.Quotes=1 and
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(Orders.TotalAll,'') like '%{T.TotalAll}%' and
        ISNULL(SalesItem,'') like '%{T.SalesItem}%' and ISNULL(Ingredients,'') like '%{T.Ingredients}%' and ISNULL(Production,'') like '%{T.Production}%' and ISNULL(Orders.DeliveryId,'') like '%{T.DeliveryId}%' 
        group by Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName,Orders.DeliveryDeadline,Orders.DeliveryId,Orders.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";
            }
            else
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName as CustomerName,SUM(Orders.TotalAll)AS TotalAll,
        Orders.DeliveryDeadline,MIN(OrdersItem.SalesItem)as SalesItem,Min(OrdersItem.Ingredients)as Ingredients,Min(OrdersItem.Production)as Production,Orders.LocationId,Locations.LocationName,Orders.DeliveryId FROM Orders
        left join Contacts on Contacts.id=Orders.ContactId
        left join OrdersItem on OrdersItem.OrdersId=Orders.id
		left join Locations on Locations.id=Orders.LocationId
       where Orders.CompanyId=@CompanyId and Orders.Tip='SalesOrder' AND Orders.LocationId=@LocationId and Orders.IsActive=1 and Orders.DeliveryId!=4 and Orders.Quotes is null or Orders.Quotes=1 and
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(Orders.TotalAll,'') like '%{T.TotalAll}%' and
        ISNULL(SalesItem,'') like '%{T.SalesItem}%' and ISNULL(Ingredients,'') like '%{T.Ingredients}%' and ISNULL(Production,'') like '%{T.Production}%' and ISNULL(Orders.DeliveryId,'') like '%{T.DeliveryId}%' 
        group by Orders.id,Orders.OrderName,Orders.ContactId,Contacts.DisplayName,Orders.DeliveryDeadline,Orders.DeliveryId,Orders.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";
            }
   
            var ScheludeOpenList = await _db.QueryAsync<SalesOrderList>(sql, param);


            foreach (var item in ScheludeOpenList)
            {
                string missing = string.Empty;
                if (T.LocationId==null)
                {
                    missing = $@"Select  OrdersItem.id,OrdersItem.ItemId,Items.Name as ItemName,OrdersItem.Quantity,
                 OrdersItem.PricePerUnit, OrdersItem.TotalAll, OrdersItem.TaxId,OrdersItem.TotalPrice ,Tax.TaxName,OrdersItem.TaxValue as Rate,OrdersItem.SalesItem,OrdersItem.Ingredients,OrdersItem.Production,

           ((LocationStock.StockCount-ISNULL(SUM(rez.RezerveCount),0))- ISNULL(OrdersItem.Quantity,0)+(SUM(ISNULL(ManufacturingOrder.PlannedQuantity,0)))+ISNULL(Rezerve.RezerveCount,0))as Missing
           from Orders 
              left join OrdersItem on OrdersItem.OrdersId = Orders.id
        left join Items on Items.id = OrdersItem.ItemId 
        left join Measure on Measure.id = OrdersItem.MeasureId 
        left  join Tax on Tax.id = OrdersItem.TaxId
        LEFT join ManufacturingOrder on ManufacturingOrder.SalesOrderItemId=OrdersItem.id and ManufacturingOrder.SalesOrderId=Orders.id and ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1
        LEFT join Rezerve on Rezerve.SalesOrderItemId=OrdersItem.id and Rezerve.SalesOrderId=Orders.id and Rezerve.ItemId=Items.id
		LEFT join Rezerve rez on rez.ItemId=Items.id and rez.Status=1 
        left join LocationStock on LocationStock.ItemId=Items.id 
         where Orders.CompanyId =@CompanyId and Orders.id =@Listid
        Group by OrdersItem.id,OrdersItem.ItemId,Items.Name,OrdersItem.Quantity,OrdersItem.PricePerUnit,OrdersItem.TotalAll,OrdersItem.TaxId,Tax.TaxName,OrdersItem.TaxValue,
           OrdersItem.SalesItem,OrdersItem.Ingredients,OrdersItem.Production,LocationStock.StockCount,Rezerve.RezerveCount,REZ.RezerveCount,OrdersItem.TotalPrice ";
                }
                else
                {
                    missing = $@"Select  OrdersItem.id,OrdersItem.ItemId,Items.Name as ItemName,OrdersItem.Quantity,
                 OrdersItem.PricePerUnit, OrdersItem.TotalAll, OrdersItem.TaxId,OrdersItem.TotalPrice ,Tax.TaxName,OrdersItem.TaxValue as Rate,OrdersItem.SalesItem,OrdersItem.Ingredients,OrdersItem.Production,

           ((LocationStock.StockCount-ISNULL(SUM(rez.RezerveCount),0))- ISNULL(OrdersItem.Quantity,0)+(SUM(ISNULL(ManufacturingOrder.PlannedQuantity,0)))+ISNULL(Rezerve.RezerveCount,0))as Missing
           from Orders 
              left join OrdersItem on OrdersItem.OrdersId = Orders.id
        left join Items on Items.id = OrdersItem.ItemId 
        left join Measure on Measure.id = OrdersItem.MeasureId 
        left  join Tax on Tax.id = OrdersItem.TaxId
        LEFT join ManufacturingOrder on ManufacturingOrder.SalesOrderItemId=OrdersItem.id and ManufacturingOrder.SalesOrderId=Orders.id and ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1
        LEFT join Rezerve on Rezerve.SalesOrderItemId=OrdersItem.id and Rezerve.SalesOrderId=Orders.id and Rezerve.ItemId=Items.id
		LEFT join Rezerve rez on rez.ItemId=Items.id and rez.Status=1 
        left join LocationStock on LocationStock.ItemId=Items.id and LocationStock.LocationId=@LocationId
         where Orders.CompanyId =@CompanyId and Orders.id =@Listid
        Group by OrdersItem.id,OrdersItem.ItemId,Items.Name,OrdersItem.Quantity,OrdersItem.PricePerUnit,OrdersItem.TotalAll,OrdersItem.TaxId,Tax.TaxName,OrdersItem.TaxValue,
           OrdersItem.SalesItem,OrdersItem.Ingredients,OrdersItem.Production,LocationStock.StockCount,Rezerve.RezerveCount,REZ.RezerveCount,OrdersItem.TotalPrice ";
                }
                param.Add("@Listid", item.id);
                param.Add("@SalesOrderId", item.id);
                string sqlsorgu = $@"select * from ManufacturingOrder where CompanyId=1 and SalesOrderId=@SalesOrderId AND ManufacturingOrder.IsActive=1 and ManufacturingOrder.Status!=3";
                var Manufacturing = await _db.QueryAsync<ManufacturingOrderDetail>(sqlsorgu, param);
    
                var missinglist = await _db.QueryAsync<SalesOrderItemDetail>(missing, param);
                item.MOList = Manufacturing;
                item.MissingList = missinglist;

            }

            return ScheludeOpenList;
        }

        public async Task Update(SalesOrderUpdate T, int id, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@DeliveryDeadLine", T.DeliveryDeadline);
            prm.Add("@CreateDate", T.CreateDate);
            prm.Add("@OrderName", T.OrderName);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@LocationId", T.LocationId);
            prm.Add("@Total", T.Total);
            prm.Add("@Info", T.Info);
            var location = await _db.QueryAsync<int>($"Select LocationId from Orders where id=@id and CompanyId=@CompanyId ", prm);
            prm.Add("@eskilocationId", location.First());


            if (location.First() == T.LocationId)
            {
                await _db.ExecuteAsync($"Update Orders set ContactId=@ContactId,DeliveryDeadLine=@DeliveryDeadLine,CreateDate=@CreateDate,OrderName=@OrderName,Info=@Info,LocationId=@LocationId,TotalAll=@Total where CompanyId=@CompanyId and id=@id", prm);
            }
            else
            {
                await _db.ExecuteAsync($"Update Orders set ContactId=@ContactId,DeliveryDeadLine=@DeliveryDeadLine,CreateDate=@CreateDate,OrderName=@OrderName,Info=@Info,LocationId=@LocationId,TotalAll=@Total where CompanyId=@CompanyId and id=@id", prm);

                List<Manufacturing> ItemsCount = (await _db.QueryAsync<Manufacturing>($"select ItemId,RezerveCount from Rezerve where SalesOrderId=@id and CompanyId=@CompanyId and Status=1", prm)).ToList();
                foreach (var item in ItemsCount)
                {
                    prm.Add("@ItemId", item.ItemId);
                    prm.Add("@Status", 4);      
                    await _db.ExecuteAsync($"Update Rezerve set Status=@Status where SalesOrderId=@id and CompanyId=@CompanyId and ItemId=@ItemId", prm);

                }
                List<ManufacturingOrderItemsIngredientsUpdate> Itemdegerler = (await _db.QueryAsync<ManufacturingOrderItemsIngredientsUpdate>($@"select ItemId,Quantity,OrdersItem.id from OrdersItem 
                inner join Orders on Orders.id=OrdersItem.OrdersId
                where OrdersItem.CompanyId=@CompanyId and OrdersItem.OrdersId=@id and Orders.IsActive=1 and Orders.DeliveryId!=4", prm)).ToList();
                foreach (var item in Itemdegerler)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@ItemId", item.ItemId);
                    param.Add("@CompanyId", CompanyId);
                    param.Add("@location", T.LocationId);
                    param.Add("@id", id);
                    param.Add("@OrderItemId", item.id);
                    param.Add("@ContactId", T.ContactId);

                    string sqla = $@"select
                    (Select ISNULL(Tip,'') from Items where id = @ItemId and CompanyId = @CompanyId)as Tip,
                    (Select ISNULL(id,0) from LocationStock where ItemId=@ItemId and LocationId = @location and CompanyId = @CompanyId)   as    LocationStockId,
                     (select ISNULL(SUM(ManufacturingOrder.PlannedQuantity),0) as Quantity from ManufacturingOrder where  ManufacturingOrder.ItemId=@ItemId and ManufacturingOrder.CompanyId=@CompanyId and   ManufacturingOrder.CustomerId=@ContactId )as ManufacturingQuantity";
                    var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, param);
                    var RezerveCount = _control.Count(item.ItemId, CompanyId, T.LocationId);
                    var locationStockId = sorgu.First().LocationStockId;
                    var tip = sorgu.First().Tip;
                    param.Add("@LocationStockId", locationStockId);
                    if (locationStockId == 0)
                    {
                       int locaitonstid= await _loc.Insert(tip, item.ItemId, CompanyId, T.LocationId);
                        prm.Add("@LocationStockId", locaitonstid);
                    }
                    int rezervid = 0;
                    SalesOrderItem A = new SalesOrderItem();
                    A.ItemId = item.ItemId;
                    A.LocationId = T.LocationId;
                    A.ContactId = T.ContactId;
                    A.Quantity = item.Quantity;
                    if (RezerveCount > 0)
                    {


                        rezervid= await _salesorderitem.Control(A, T.id, CompanyId);
                        if (A.Status == 3)
                        {
                            param.Add("@SalesItem", 3);
                        }
                        else
                        {
                            param.Add("@SalesItem", 1);
                        }

                    }

                    else
                        param.Add("@SalesItem", 1);
                    if (A.Status == 3)
                    {
                        param.Add("@SalesItem", 3);
                        param.Add("@Production", 4);
                        param.Add("@Ingredient", 3);


                        await _db.ExecuteAsync($"Update OrdersItem set SalesItem=@SalesItem,Production=@Production,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId", param);

                        param.Add("@RezerveId", rezervid);

                        await _db.ExecuteAsync($"Update Rezerve set SalesOrderItemId=@OrderItemId where  CompanyId=@CompanyId and CustomerId=@ContactId and SalesOrderId=@id and LocationId=@location and id=@RezerveId ", param);

                    }
                    else
                    {


                        await _salesorderitem.IngredientsControl(A, T.id, CompanyId);
                        if (A.Conditions == 3)
                        {
                            param.Add("@Ingredient", 2);
                        }
                        else
                        {
                            param.Add("@Ingredient", 0);
                        }
                        param.Add("@Production", 0);

                        await _db.ExecuteAsync($"Update Rezerve set SalesOrderItemId=@OrderItemId where  CompanyId=@CompanyId and CustomerId=@ContactId and SalesOrderId=@id and LocationId=@location and SalesOrderItemId is null ", param);

                        await _db.ExecuteAsync($"Update OrdersItem set SalesItem=@SalesItem,Production=@Production,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId", param);

                    }

                }


            }
        }

        public async Task UpdateAddress(SalesOrderCloneAddress A, int id, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("FirstName", A.FirstName);
            prm.Add("LastName", A.LastName);
            prm.Add("CompanyName", A.CompanyName);
            prm.Add("Phone", A.Phone);
            prm.Add("AddressLine1", A.AddressLine1);
            prm.Add("AddressLine2", A.AddressLine2);
            prm.Add("CityTown", A.CityTown);
            prm.Add("StateRegion", A.StateRegion);
            prm.Add("ZipPostal", A.ZipPostal);
            prm.Add("Country", A.Country);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@IsActive", true);
            if (A.Tip == "ShippingAddress")
            {
                prm.Add("@Tip", "ShippingAddress");

               await _db.ExecuteAsync($"Update Locations set Tip=@Tip,FirstName=@FirstName,LastName=@LastName,CompanyName=@CompanyName,Phone=@Phone, AddressLine1=@AddressLine1,AddressLine2=@AddressLine2,CityTown=@CityTown,StateRegion=@StateRegion,ZipPostalCode=@ZipPostal,Country=@Country where CompanyId=@CompanyId and id=@id", prm);
            }
            else if (A.Tip == "BillingAddress")
            {
                prm.Add("@Tip", "BillingAddress");
               await _db.ExecuteAsync($"Update Locations set Tip=@Tip,FirstName=@FirstName,LastName=@LastName,CompanyName=@CompanyName,Phone=@Phone, AddressLine1=@AddressLine1,AddressLine2=@AddressLine2,CityTown=@CityTown,StateRegion=@StateRegion,ZipPostalCode=@ZipPostal,Country=@Country where CompanyId=@CompanyId and id=@id", prm);
            }
        }

        
    }
}
