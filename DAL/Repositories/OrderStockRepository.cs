using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using DAL.StockControl;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;
using static DAL.DTO.StockDTO;

namespace DAL.Repositories
{
    public class OrderStockRepository : IOrderStockRepository
    {
        private readonly IDbConnection _db;
        private readonly ILocationStockRepository _locationStockRepository;
        private readonly IStockControl _stockcontrol;
        public OrderStockRepository(IDbConnection db, ILocationStockRepository locationStockRepository, IStockControl stockcontrol)
        {
            _db = db;
            _locationStockRepository = locationStockRepository;
            _stockcontrol = stockcontrol;
        }

        public async Task<int> Count(PurchaseOrderLogsList T, int CompanyId)
        {
            if (T.LocationId == null)
            {
                List<int> kayitsayisi = (await _db.QueryAsync<int>($"Select COUNT(*) as kayitsayisi from Orders inner join Contacts on Contacts.id = Orders.ContactId where ISNULL(Orders.CompanyId,0) = {CompanyId}  and Contacts.CompanyId = {CompanyId} and Orders.DeliveryId=1 and Orders.IsActive=1 and Orders.Tip = '{T.Tip}' and ISNULL(Contacts.DisplayName,0) LIKE '%{T.SupplierName}%' and ISNULL(Orders.OrderName,0) LIKE '%{T.OrderName}%' and ISNULL(Orders.TotalAll,0) LIKE '%{T.TotalAll}%'")).ToList();
                return kayitsayisi[0];
            }
            else
            {
                List<int> kayitsayisi = (await _db.QueryAsync<int>($"Select COUNT(*) as kayitsayisi from Orders inner join Contacts on Contacts.id = Orders.ContactId where ISNULL(Orders.CompanyId,0) = {CompanyId}  and Contacts.CompanyId = {CompanyId} and Orders.DeliveryId=1 and  Orders.LocationId={T.LocationId}  and Orders.IsActive=1 and Orders.Tip = '{T.Tip}' and ISNULL(Contacts.DisplayName,0) LIKE '%{T.SupplierName}%' and ISNULL(Orders.OrderName,0) LIKE '%{T.OrderName}%' and ISNULL(Orders.TotalAll,0) LIKE '%{T.TotalAll}%'")).ToList();
                return kayitsayisi[0];
            }

        }

        public async Task<int> DoneCount(PurchaseOrderLogsList T, int CompanyId)
        {
            if (T.LocationId == null)
            {
                List<int> kayitsayisi = (await _db.QueryAsync<int>($"Select COUNT(*) as kayitsayisi from Orders inner join Contacts on Contacts.id = Orders.ContactId where ISNULL(Orders.CompanyId,0) = {CompanyId}  and Contacts.CompanyId = {CompanyId} and Orders.DeliveryId=2 and Orders.IsActive=1  and Orders.Tip = '{T.Tip}' and ISNULL(Contacts.DisplayName,0) LIKE '%{T.SupplierName}%' and ISNULL(Orders.OrderName,0) LIKE '%{T.OrderName}%' and ISNULL(Orders.TotalAll,0) LIKE '%{T.TotalAll}%'")).ToList();
                return kayitsayisi[0];
            }
            else
            {
                List<int> kayitsayisi = (await _db.QueryAsync<int>($"Select COUNT(*) as kayitsayisi from Orders inner join Contacts on Contacts.id = Orders.ContactId where ISNULL(Orders.CompanyId,0) = {CompanyId}  and Contacts.CompanyId = {CompanyId} and Orders.DeliveryId=2 and Orders.IsActive=1 and Orders.LocationId={T.LocationId}  and Orders.Tip = '{T.Tip}' and ISNULL(Contacts.DisplayName,0) LIKE '%{T.SupplierName}%' and ISNULL(Orders.OrderName,0) LIKE '%{T.OrderName}%' and ISNULL(Orders.TotalAll,0) LIKE '%{T.TotalAll}%'")).ToList();
                return kayitsayisi[0];
            }

        }

        public async Task<IEnumerable<PurchaseOrderLogsList>> DoneList(PurchaseOrderLogsList T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SupplierName", T.SupplierName);
            prm.Add("@CompanyId", CompanyId);
            string sql = string.Empty;
            if (T.LocationId == null)
            {
                sql = $"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}Select Orders.id,Orders.Tip,Orders.OrderName,Contacts.DisplayName as SupplierName,Orders.ExpectedDate, Orders.TotalAll,Orders.IsActive from Orders inner join Contacts on Contacts.id = Orders.ContactId and Orders.DeliveryId=2 where ISNULL(Orders.CompanyId,0) = {CompanyId}  and Orders.IsActive = 1 and Contacts.CompanyId ={CompanyId}   and Orders.Tip = '{T.Tip}' and ISNULL(Contacts.DisplayName,0) LIKE '%{T.SupplierName}%' and ISNULL(Orders.OrderName,0) LIKE '%{T.OrderName}%' and ISNULL(Orders.TotalAll,0) LIKE '%{T.TotalAll}%' ORDER BY Orders.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";
            }
            else
            {
                sql = $"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}Select Orders.id,Orders.Tip,Orders.OrderName,Contacts.DisplayName as SupplierName,Orders.ExpectedDate, Orders.TotalAll,Orders.IsActive from Orders inner join Contacts on Contacts.id = Orders.ContactId and Orders.DeliveryId=2 where ISNULL(Orders.CompanyId,0) = {CompanyId}  and Orders.IsActive = 1 and  Orders.LocationId={T.LocationId} and Contacts.CompanyId ={CompanyId}   and Orders.Tip = '{T.Tip}' and ISNULL(Contacts.DisplayName,0) LIKE '%{T.SupplierName}%' and ISNULL(Orders.OrderName,0) LIKE '%{T.OrderName}%' and ISNULL(Orders.TotalAll,0) LIKE '%{T.TotalAll}%' ORDER BY Orders.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";
            }
            var list = await _db.QueryAsync<PurchaseOrderLogsList>(sql);
            return list;
        }

        public async Task<IEnumerable<PurchaseOrderLogsList>> List(PurchaseOrderLogsList T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SupplierName", T.SupplierName);
            prm.Add("@CompanyId", CompanyId);
            string sql;
            if (T.LocationId == null)
            {
                sql = $"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}Select Orders.id,Orders.Tip,Orders.OrderName,Contacts.DisplayName as SupplierName,Orders.ExpectedDate, Orders.TotalAll,Orders.IsActive from Orders inner join Contacts on Contacts.id = Orders.ContactId and Orders.DeliveryId=1 where ISNULL(Orders.CompanyId,0) = {CompanyId}  and Orders.IsActive = 1 and Contacts.CompanyId ={CompanyId}   and Orders.Tip = '{T.Tip}' and ISNULL(Contacts.DisplayName,0) LIKE '%{T.SupplierName}%' and ISNULL(Orders.OrderName,0) LIKE '%{T.OrderName}%' and ISNULL(Orders.TotalAll,0) LIKE '%{T.TotalAll}%' ORDER BY Orders.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";
            }
            else
            {
                sql = $"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}Select Orders.id,Orders.Tip,Orders.OrderName,Contacts.DisplayName as SupplierName,Orders.ExpectedDate, Orders.TotalAll,Orders.IsActive from Orders inner join Contacts on Contacts.id = Orders.ContactId and Orders.DeliveryId=1 where ISNULL(Orders.CompanyId,0) = {CompanyId} and  Orders.LocationId={T.LocationId}  and Orders.IsActive = 1 and Contacts.CompanyId ={CompanyId}   and Orders.Tip = '{T.Tip}' and ISNULL(Contacts.DisplayName,0) LIKE '%{T.SupplierName}%' and ISNULL(Orders.OrderName,0) LIKE '%{T.OrderName}%' and ISNULL(Orders.TotalAll,0) LIKE '%{T.TotalAll}%' ORDER BY Orders.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";
            }






            var list = await _db.QueryAsync<PurchaseOrderLogsList>(sql);
            return list;
        }

        public async Task StockUpdate(PurchaseOrderId T, int CompanyId, int user)
        {
            DynamicParameters prm = new DynamicParameters();
            
            prm.Add("@id", T.id);
            prm.Add("@CompanyId", CompanyId);
            string sqla = $"Select LocationId From Orders where CompanyId = @CompanyId and id  = @id";
            var locationIdDeger = await _db.QueryAsync<PurchaseOrder>(sqla, prm); //gelen ordersId'nin locationId alıyoruz
            int? locaitonId = locationIdDeger.First().LocationId;
            prm.Add("@locationId", locaitonId);

            prm.Add("@DeliveryId", T.DeliveryId);
            string sqls = $"Select DeliveryId From Orders where CompanyId = @CompanyId and id= @id";
            var delivery = await _db.QueryFirstAsync<PurchaseOrderId>(sqls, prm);
            int olddeliveryId = delivery.DeliveryId; ;
            if (T.DeliveryId == 1 || T.DeliveryId == 2)
            {
                await _db.ExecuteAsync($"Update Orders SET DeliveryId = @DeliveryId where id = @id  and CompanyId = @CompanyId", prm);
            }


            string sql = $"Select id,ItemId,Quantity From OrdersItem where CompanyId = @CompanyId and OrdersId  = @id";
            var stokdegerler = await _db.QueryAsync<PurchaseItem>(sql, prm); //gelen ordersId'nin OrdersItem tablosundaki itemId ve Quantity e erişerek hangi itemin kaç tane artacağına bakılacak.

            if (olddeliveryId < T.DeliveryId && T.DeliveryId == 2)
            {
                foreach (var item in stokdegerler)
                {
                    prm.Add("@Quantity", item.Quantity);
                    prm.Add("@ItemId", item.ItemId);
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@OrdersItemid", item.id);
                    float? StockLocationRezerve = await _stockcontrol.Count(item.ItemId, CompanyId, locaitonId);


                    string sqlc = $"select * from Orders where id=@id and CompanyId=@CompanyId";
                    var sorgu3 = await _db.QueryAsync<PurchaseOrder>(sqlc, prm);//
                    int? salesorderId = sorgu3.First().SalesOrderId;
                    int? salesorderitemId = sorgu3.First().SalesOrderItemId;
                    int? manufacturingorderId = sorgu3.First().ManufacturingOrderId;
                    prm.Add("@SalesOrderId", salesorderId);
                    prm.Add("@SalesOrderItemId", salesorderitemId);
                    int? ProductId = 0;
                    if (salesorderId != 0)
                    {
                        var ItemIdAL = await _db.QueryAsync<int>($"Select ItemId From SalesOrderItem where CompanyId = @CompanyId and id =@SalesOrderItemId  and SalesOrderId=@SalesOrderId", prm);
                        ProductId = ItemIdAL.First();
                        prm.Add("@ProductId", ProductId);
                    }


                    float? adet = item.Quantity;


                    if (salesorderId != 0)
                    {


                        prm.Add("@SalesOrderId", sorgu3.First().SalesOrderId);
                        prm.Add("@SalesOrderItemId", sorgu3.First().SalesOrderItemId);
                        prm.Add("@RezerveCount", item.Quantity);
                        prm.Add("@LocationId", sorgu3.First().LocationId);
                        prm.Add("@Status", 1);
                        prm.Add("@Tip", sorgu3.First().Tip);
                        //prm.Add("@ManufacturingOrderItemId", sorgu3.First().ManufacturingOrderItemId);

                        string sqld = $@"select ISNULL(RezerveCount,0) as RezerveCount from Rezerve where SalesOrderId=@SalesOrderId and ItemId=@ItemId and LocationId=@LocationId and Status=1";
                        var rezervestockCount = await _db.QueryAsync<LocaVarmı>(sqld, prm);
                        float? rezervecount = rezervestockCount.First().RezerveCount;


                        string sqlf = $"select Tip from SalesOrderItem where id=@SalesOrderItemId and CompanyId=@CompanyId";
                        var sorgu5 = await _db.QueryAsync<SalesOrderUpdateItems>(sqlf, prm);//
                        var tip = sorgu5.First().Tip;
                        if (sorgu3.First().ManufacturingOrderId == 0 && tip == null)
                        {
                            float missing;
                            string missingsorgu = $@"
				    	 							Select 
                         ((select Quantity from SalesOrderItem where id=@SalesOrderItemId and SalesOrderId=@SalesOrderId )*
                        (select Quantity from Bom where Bom.ProductId=@ProductId and Bom.MaterialId=@ItemId) -ISNULL((Rezerve.RezerveCount),0))as Missing
                         
                         from Orders
                         left join OrdersItem on OrdersItem.OrdersId=Orders.id  and DeliveryId = 1
				    	 LEFT join Rezerve on Rezerve.SalesOrderId=@SalesOrderId and Rezerve.SalesOrderItemId=@SalesOrderItemId and Rezerve.ItemId=@ItemId
                         where Orders.IsActive=1 and Orders.CompanyId=@CompanyId
                         Group by Rezerve.RezerveCount";
                            var missingcount = await _db.QueryAsync<LocaVarmı>(missingsorgu, prm);
                            if (missingcount.Count() == 0)
                            {
                                missing = 0;
                            }
                            else
                            {
                                missing = missingcount.First().Missing;
                            }

                            if (missing == item.Quantity)
                            {

                                var counts = item.Quantity + rezervecount;
                                prm.Add("@RezerveCount", counts);
                                await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId  and Status=1 and ItemId=@ItemId", prm);
                            }
                            else if (missing < item.Quantity)
                            {

                                var newstocks = item.Quantity - missing;

                                prm.Add("@RezerveCount", rezervecount + missing);
                                await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId  and Status=1 and ItemId=@ItemId ", prm);
                                prm.Add("@Availability", 2);


                            }
                            else if (missing <= item.Quantity + StockLocationRezerve)
                            {
                                prm.Add("@RezerveCount", rezervecount + missing);
                                await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId  and Status=1 and ItemId=@ItemId ", prm);
                                prm.Add("@Availability", 2);
                            }
                            else
                            {

                                prm.Add("@RezerveCount", item.Quantity + StockLocationRezerve + rezervecount);
                                await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and Status=1 and ItemId=@ItemId", prm);

                            }

                        }
                        else if (tip != null)
                        {


                            float? kalan = item.Quantity;

                            string sqlquery = $@"select ma.id,me.id as ManufacturingOrderItemId,me.PlannedQuantity  from ManufacturingOrder ma
                            left join ManufacturingOrderItems me on me.OrderId=ma.id
                            where ma.CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and me.ItemId=@ItemId and me.Availability=0";
                            var sorgu6 = await _db.QueryAsync<UretimOzelClass>(sqlquery, prm);//
                            foreach (var list2 in sorgu6)
                            {
                                if (kalan > 0)
                                {


                                    prm.Add("@ManufacturingOrderId", sorgu6.First().id);
                                    prm.Add("@ManufacturingOrderItemId", sorgu6.First().ManufacturingOrderItemId);
                                    float missing;
                                    string missingsorgu = $@"
				    	 	Select 
                         ((select Quantity from SalesOrderItem where id=@SalesOrderItemId  and SalesOrderId=@SalesOrderId)*
                        (select ISNULL(PlannedQuantity,0) from ManufacturingOrderItems where id=@ManufacturingOrderItemId and ItemId=@ItemId) -ISNULL((Rezerve.RezerveCount),0))as Missing
                        
                         from Orders
                         left join OrdersItem on OrdersItem.OrdersId=Orders.id  and DeliveryId = 1
				    	 LEFT join Rezerve on Rezerve.SalesOrderId=@SalesOrderId and Rezerve.SalesOrderItemId=@SalesOrderItemId  and Rezerve.ItemId=@ItemId
                         where Orders.IsActive=1 and Orders.CompanyId=@CompanyId
                         Group by Rezerve.RezerveCount";
                                    var missingcount = await _db.QueryAsync<LocaVarmı>(missingsorgu, prm);
                                    if (missingcount.Count() == 0)
                                    {
                                        missing = 0;
                                    }
                                    else
                                    {
                                        missing = missingcount.First().Missing;
                                    }
                                    if (missing <= item.Quantity)
                                    {
                                        kalan = item.Quantity - missing;
                                        prm.Add("@RezerveCount", missing + rezervecount);
                                        await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Status=1", prm);
                                        prm.Add("@Availability", 2);
                                        await _db.ExecuteAsync($"Update ManufacturingOrderItems set Availability=@Availability where OrderId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and Itemıd=@ItemId and CompanyId=@CompanyId", prm);

                                    }

                                    else if (missing <= item.Quantity + StockLocationRezerve)
                                    {
                                        prm.Add("@RezerveCount", rezervecount + missing);
                                        await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Status=1 ", prm);
                                        prm.Add("@Availability", 2);

                                        await _db.ExecuteAsync($"Update ManufacturingOrderItems set Availability=@Availability where OrderId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and Itemıd=@ItemId and CompanyId=@CompanyId", prm);
                                    }
                                    else
                                    {
                                        prm.Add("@RezerveCount", item.Quantity + rezervecount);
                                        await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Status=1", prm);
                                        prm.Add("@Availability", 0);
                                        await _db.ExecuteAsync($"Update ManufacturingOrderItems set Availability=@Availability where OrderId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and Itemıd=@ItemId and CompanyId=@CompanyId", prm);
                                    }
                                    prm.Add("@CompanyId", CompanyId);
                                    prm.Add("@ItemId", ProductId);
                                    prm.Add("@SalesOrderId", sorgu3.First().SalesOrderId);
                                    prm.Add("@SalesOrderItemId", sorgu3.First().SalesOrderItemId);

                                    string sqlr = $@"select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder ma on ma.id=ManufacturingOrderItems.OrderId
                    where ma.SalesOrderId=SalesOrderId and ma.SalesOrderItemId=SalesOrderItemId and ma.CompanyId=CompanyId";
                                    var availability = await _db.QueryAsync(sqlr, prm);
                                    prm.Add("@Ingredients", availability.First());
                                    await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                                }

                            }
                        }









                    }
                    else if (salesorderId == 0 && manufacturingorderId != 0)
                    {
                        prm.Add("@ManufacturingOrderId", sorgu3.First().ManufacturingOrderId);
                        prm.Add("@ManufacturingOrderItemId", sorgu3.First().ManufacturingOrderItemId);

                        float missing;
                        string sqlsorgu = $@" 
		select
        (ISNULL(moi.PlannedQuantity,0)+(ISNULL(rez.RezerveCount,0)))AS missing,
		(ISNULL(moi.PlannedQuantity,0)),(ISNULL(rez.RezerveCount,0))
        from ManufacturingOrderItems moi
        left join ManufacturingOrder mao on mao.id=moi.OrderId
        left join Items on Items.id=moi.ItemId
        left join OrdersItem on OrdersItem.ItemId=moi.ItemId 
        left join Orders on Orders.id=OrdersItem.OrdersId and Orders.ManufacturingOrderId=mao.id and Orders.IsActive=1 and Orders.DeliveryId=1
        left join Rezerve rez on rez.ManufacturingOrderId=mao.id and rez.ManufacturingOrderItemId=moi.id and rez.ItemId=@ItemId
        where mao.id=@ManufacturingOrderId and moi.Tip='Ingredients' and mao.LocationId=@locationId  and mao.Status!=3 and mao.CompanyId=@CompanyId and moi.id=@ManufacturingOrderItemId
        Group by moi.id,moi.Tip,moi.ItemId,Items.Name,moi.Notes,moi.PlannedQuantity ,moi.Cost,moi.Availability,
        moi.PlannedQuantity,rez.RezerveCount";
                        List<int> missingdeger = (await _db.QueryAsync<int>(sqlsorgu, prm)).ToList();

                        if (missingdeger.Count() == 0)
                        {
                            missing = 0;
                        }
                        else
                        {
                            missing = missingdeger.First();
                        }
                        prm.Add("@LocationId", sorgu3.First().LocationId);
                        prm.Add("@Status", 1);
                        prm.Add("@Tip", sorgu3.First().Tip);

                        string sqld = $@"select ISNULL(RezerveCount,0) from Rezerve where ManufacturingOrderId=@ManufacturingOrderId  and ItemId=@ItemId and LocationId=@LocationId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Status=1";
                        var rezervestockCount = await _db.QueryAsync<float>(sqld, prm);
                        float rezervecount = 0;
                        rezervecount = rezervestockCount.First();

                        if (missing <= item.Quantity)
                        {

                            prm.Add("@RezerveCount", missing + rezervecount);
                            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Status=1", prm);
                            prm.Add("@Availability", 2);
                            await _db.ExecuteAsync($"Update ManufacturingOrderItems set Availability=@Availability where OrderId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and Itemıd=@ItemId and CompanyId=@CompanyId", prm);

                        }
                        else if (missing <= item.Quantity + StockLocationRezerve)
                        {
                            prm.Add("@RezerveCount", missing + rezervecount);
                            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Status=1", prm);
                            prm.Add("@Availability", 2);
                            await _db.ExecuteAsync($"Update ManufacturingOrderItems set Availability=@Availability where OrderId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and Itemıd=@ItemId and CompanyId=@CompanyId", prm);
                        }

                        else
                        {
                            prm.Add("@RezerveCount", item.Quantity + rezervecount + StockLocationRezerve);
                            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Status=1", prm);
                            prm.Add("@Availability", 0);
                            await _db.ExecuteAsync($"Update ManufacturingOrderItems set Availability=@Availability where OrderId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and Itemıd=@ItemId and CompanyId=@CompanyId", prm);
                        }


                    }
                    else
                    {
                        string sqlorder = $"select ManufacturingOrder.id as ManufacturingOrderId,ManufacturingOrderItems.id as ManufacturingOrderItemId,ManufacturingOrder.LocationId ,ManufacturingOrder.ExpectedDate from ManufacturingOrder left join Orders on Orders.ManufacturingOrderId = ManufacturingOrder.id and Orders.DeliveryId = 1 LEFT join ManufacturingOrderItems on ManufacturingOrderItems.OrderId = ManufacturingOrder.id left join OrdersItem on OrdersItem.OrdersId = Orders.id where ManufacturingOrder.CompanyId = @CompanyId and ManufacturingOrderItems.Availability = 0 and ManufacturingOrderItems.ItemId = @ItemId and  ManufacturingOrder.IsActive=1 and ManufacturingOrder.LocationId=@locationId Group by ManufacturingOrder.id,ManufacturingOrder.ExpectedDate,ManufacturingOrder.LocationId ,Orders.DeliveryId,ManufacturingOrderItems.id Order By ExpectedDate";
                        var ItemList = await _db.QueryAsync<PurchaseOrder>(sqlorder, prm);
                        string sqlf = $"select (Select AllStockQuantity from Items where Items.id = @ItemId  and Items.CompanyId = @CompanyId and Stock.Tip = 'Material')as Quantity,(Select StockCount from LocationStock where ItemId = @ItemId   and LocationId = @locationId and CompanyId = @CompanyId)as LocationsStockCount,(Select id from LocationStock where StockId = @@stockıd and LocationId = @locationId and CompanyId = @CompanyId) as LocationStockId";
                        var sorgu1 = _db.QueryAsync<StockDTO.Stock>(sqlf, prm);//

                        foreach (var list in ItemList)
                        {
                            float? kontrol = await _stockcontrol.Count(item.ItemId, CompanyId, locaitonId);

                            prm.Add("@ManufacturingOrderId", list.ManufacturingOrderId);
                            prm.Add("@LocationId", list.LocationId);
                            prm.Add("@Status", 1);
                            prm.Add("@Tip", sorgu3.First().Tip);
                            prm.Add("@ManufacturingOrderItemId", list.ManufacturingOrderItemId);
                            string sqld = $@"select ISNULL(RezerveCount,0) as RezerveCount from Rezerve where ManufacturingOrderId=@ManufacturingOrderId  and ItemId=@ItemId and LocationId=@LocationId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Status=1";
                            var rezervestockCount = await _db.QueryAsync<LocaVarmı>(sqld, prm);

                            float? rezervedeger = rezervestockCount.First().RezerveCount;


                            string sqlsorgu = $@"Select 
    (moi.PlannedQuantity-ISNULL(SUM(OrdersItem.Quantity),0)-ISNULL((Rezerve.RezerveCount),0))as Missing                            
 from ManufacturingOrderItems moi
 left join Items on Items.id=moi.ItemId
 left join ManufacturingOrder on ManufacturingOrder.id=moi.OrderId
 LEFT join Rezerve on Rezerve.ManufacturingOrderId=ManufacturingOrder.id and Rezerve.ManufacturingOrderItemId=moi.id and Rezerve.CompanyId=@CompanyId 
  LEFT join Rezerve a on a.ItemId=Items.id and a.ItemId=@ItemId and a.LocationId=@LocationId and a.Status=1 and a.CompanyId=@CompanyId 

left join LocationStock on LocationStock.ItemId=Items.id and LocationStock.LocationId=@LocationId and LocationStock.CompanyId=@CompanyId 
left join Orders on Orders.ManufacturingOrderId=ManufacturingOrder.id
 left join OrdersItem on OrdersItem.OrdersId=Orders.id  and DeliveryId = 1
 where  moi.CompanyId = @CompanyId   and moi.OrderId = @ManufacturingOrderId and moi.Tip='Ingredients'  
 and ManufacturingOrder.id=@ManufacturingOrderId and moi.ItemId=@ItemId  and  ManufacturingOrder.Status!=3

 Group by moi.id,moi.Tip,moi.ItemId,Items.Name,Notes,moi.Cost,moi.Availability
 ,LocationStock.StockCount,moi.PlannedQuantity,OrdersItem.Quantity,Rezerve.RezerveCount";
                            var missingdeger = await _db.QueryAsync<LocaVarmı>(sqlsorgu, prm);
                            float missing;
                            if (missingdeger.Count() == 0)
                            {
                                missing = 0;
                            }
                            else
                            {
                                missing = missingdeger.First().Missing;
                            }
                            if (item.Quantity > 0)
                            {

                                if (missing <= item.Quantity)
                                {


                                    var counts = item.Quantity - missing;
                                    item.Quantity = counts;
                                    prm.Add("@RezerveCount", rezervedeger + missing);
                                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Status=1", prm);
                                    prm.Add("@Availability", 2);
                                    await _db.ExecuteAsync($"Update ManufacturingOrderItems set Availability=@Availability where OrderId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and Itemıd=@ItemId and CompanyId=@CompanyId", prm);

                                }
                                else if (missing <= item.Quantity + StockLocationRezerve)
                                {

                                    var newstocks = item.Quantity - missing;
                                    item.Quantity = 0;
                                    prm.Add("@RezerveCount", missing);
                                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Status=1 ", prm);
                                    prm.Add("@Availability", 2);

                                    await _db.ExecuteAsync($"Update ManufacturingOrderItems set Availability=@Availability where OrderId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and Itemıd=@ItemId and CompanyId=@CompanyId", prm);
                                }
                                else
                                {
                                    var newstock = rezervedeger + item.Quantity + StockLocationRezerve;
                                    prm.Add("@RezerveCount", newstock);
                                    item.Quantity = 0;
                                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Status=1", prm);
                                    prm.Add("@Availability", 0);
                                    await _db.ExecuteAsync($"Update ManufacturingOrderItems set Availability=@Availability where OrderId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and Itemıd=@ItemId and CompanyId=@CompanyId", prm);
                                }

                            }




                        }

                    }


                    prm.Add("@Quantity", adet);
                    prm.Add("@ItemId", item.ItemId);
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@OrdersItemid", item.id);
                    string sqlb = $" select (Select AllStockQuantity from Items where Items.id = @ItemId  and Items.CompanyId = @CompanyId and Items.Tip = 'Material')as AllStockQuantity,(Select StockCount from LocationStock where ItemId = @ItemId   and LocationId = @locationId and CompanyId = @CompanyId)as LocationsStockCount,(Select id from LocationStock where ItemId = @ItemId and LocationId = @locationId and CompanyId = @CompanyId) as LocationStockId,(select Tip from Items where id=@ItemId and CompanyId=@CompanyId) as Tip";
                    var sorgu = await _db.QueryAsync<Stock>(sqlb, prm);//


                    float? stockQuantity = sorgu.First().AllStockQuantity;
                    float? NewQuantity = stockQuantity + adet; //Tablodaki değer ile itemdeki değeri toplayarak yeni bir stok(quanitity) elde ediyoruz.
                    prm.Add("@NewQuantity", NewQuantity);
                    await _db.ExecuteAsync($"Update Items SET AllStockQuantity =@NewQuantity where id = @ItemId  and CompanyId = @CompanyId", prm); //Stok tablosuna yeni değeri güncelleiyoruz.
                    prm.Add("@User", user);
                    prm.Add("@StockMovementQuantity", adet);
                    prm.Add("@PreviousValue", stockQuantity);
                    prm.Add("@Process", "AllStock");
                    prm.Add("@Operation", "+");

                    prm.Add("@Date", DateTime.Now);
                    prm.Add("@Where", "PurchaseOrder");

                    await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewQuantity,@Date,@User,@CompanyId,@locationId,@ItemId)", prm);
                    var stocklocationId = sorgu.First().LocationStockId;
                    prm.Add("@stocklocationId", stocklocationId);
                    float? stockCount = 0;
                    string Tip = sorgu.First().Tip;
                    if (stocklocationId == 0)
                    {
                        int locationid = await _locationStockRepository.Insert(Tip, item.ItemId, CompanyId, locaitonId); ;
                        prm.Add("@stocklocationId", locationid);
                        stockCount = 0;
                    }
                    else
                    {
                        stockCount = sorgu.First().LocationsStockCount;

                    }

                    float? NewStockCount = stockCount + adet;

                    prm.Add("@NewStockCount", NewStockCount); //Yeni count değerini tabloya güncelleştiriyoruz.
                    await _db.ExecuteAsync($"Update LocationStock SET StockCount =@NewStockCount where id = @stocklocationId  and CompanyId = @CompanyId", prm);

                    prm.Add("@PreviousValue", stockCount);
                    prm.Add("@Process", "LocationStock");

                    await _db.ExecuteAsync($"Insert into StockMovement ([Where],Operation,Process,Quantity,PreviousValue,NextValue,Date,[User],CompanyId,LocationId,ItemId) values(@Where,@Operation,@Process,@StockMovementQuantity,@PreviousValue,@NewStockCount,@Date,@User,@CompanyId,@locationId,@ItemId)", prm);


                }
            }
        }


    }
}
