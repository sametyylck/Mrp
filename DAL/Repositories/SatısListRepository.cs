using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockListDTO;

namespace DAL.Repositories
{
    public class SatısListRepository:ISatısListRepository
    {
        private readonly IDbConnection _db;

        public SatısListRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SatısList>> SalesOrderList(SatısListFiltre T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@LocationId", T.LocationId);
            string sql = string.Empty;

            if (T.BaslangıcTarih==null || T.BaslangıcTarih=="" || T.SonTarih==null || T.SonTarih=="" )
            {
                if (T.LocationId == null)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName as CustomerName,SUM(sa.TotalAll)AS TotalAll,
        sa.DeliveryDeadline,MIN(SalesOrderItem.SalesItem)as SalesItem,Min(SalesOrderItem.Ingredients)as Ingredients,Min(SalesOrderItem.Production)as Production,sa.LocationId,Locations.LocationName,sa.DeliveryId
		FROM SalesOrder sa
        left join Contacts on Contacts.id=sa.ContactId
        left join SalesOrderItem on SalesOrderItem.SalesOrderId=sa.id
		left join Locations on Locations.id=sa.LocationId
       where sa.CompanyId=@CompanyId and sa.Tip='SalesOrder'  and sa.IsActive=1 and sa.DeliveryId!=4 and 
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(sa.TotalAll,'') like '%{T.TotalAll}%' and
        ISNULL(SalesItem,'') like '%{T.SalesItem}%' and ISNULL(Ingredients,'') like '%{T.Ingredients}%' and ISNULL(Production,'') like '%{T.Production}%' and ISNULL(sa.DeliveryId,'') like '%{T.DeliveryId}%' 
        group by sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName,sa.DeliveryDeadline,sa.DeliveryId,sa.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";
                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName as CustomerName,SUM(sa.TotalAll)AS TotalAll,
        sa.DeliveryDeadline,MIN(SalesOrderItem.SalesItem)as SalesItem,Min(SalesOrderItem.Ingredients)as Ingredients,Min(SalesOrderItem.Production)as Production,sa.LocationId,Locations.LocationName,sa.DeliveryId
		FROM SalesOrder sa
        left join Contacts on Contacts.id=sa.ContactId
        left join SalesOrderItem on SalesOrderItem.SalesOrderId=sa.id
		left join Locations on Locations.id=sa.LocationId
       where sa.CompanyId=@CompanyId and sa.Tip='SalesOrder'  and sa.LocationId=@LocationId and sa.IsActive=1 and sa.DeliveryId!=4 and 
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(sa.TotalAll,'') like '%{T.TotalAll}%' and
        ISNULL(SalesItem,'') like '%{T.SalesItem}%' and ISNULL(Ingredients,'') like '%{T.Ingredients}%' and ISNULL(Production,'') like '%{T.Production}%' and ISNULL(sa.DeliveryId,'') like '%{T.DeliveryId}%' 
        group by sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName,sa.DeliveryDeadline,sa.DeliveryId,sa.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";
                }
            }
            else
            {

                if (T.LocationId == null)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName as CustomerName,SUM(sa.TotalAll)AS TotalAll,
        sa.DeliveryDeadline,MIN(SalesOrderItem.SalesItem)as SalesItem,Min(SalesOrderItem.Ingredients)as Ingredients,Min(SalesOrderItem.Production)as Production,sa.LocationId,Locations.LocationName,sa.DeliveryId
		FROM SalesOrder sa
        left join Contacts on Contacts.id=sa.ContactId
        left join SalesOrderItem on SalesOrderItem.SalesOrderId=sa.id
		left join Locations on Locations.id=sa.LocationId
       where sa.CompanyId=@CompanyId and sa.Tip='SalesOrder'  and sa.IsActive=1 and sa.DeliveryId!=4 and 
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(sa.TotalAll,'') like '%{T.TotalAll}%' and
        ISNULL(SalesItem,'') like '%{T.SalesItem}%' and ISNULL(Ingredients,'') like '%{T.Ingredients}%' and ISNULL(Production,'') like '%{T.Production}%' and ISNULL(sa.DeliveryId,'') like '%{T.DeliveryId}%'  and sa.DeliveryDeadline BETWEEN '{T.BaslangıcTarih}' and '{T.SonTarih}'
        group by sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName,sa.DeliveryDeadline,sa.DeliveryId,sa.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";

                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName as CustomerName,SUM(sa.TotalAll)AS TotalAll,
        sa.DeliveryDeadline,MIN(SalesOrderItem.SalesItem)as SalesItem,Min(SalesOrderItem.Ingredients)as Ingredients,Min(SalesOrderItem.Production)as Production,sa.LocationId,Locations.LocationName,sa.DeliveryId
		FROM SalesOrder sa
        left join Contacts on Contacts.id=sa.ContactId
        left join SalesOrderItem on SalesOrderItem.SalesOrderId=sa.id
		left join Locations on Locations.id=sa.LocationId
       where sa.CompanyId=@CompanyId and sa.Tip='SalesOrder'  and sa.LocationId=@LocationId and sa.IsActive=1 and sa.DeliveryId!=4 and 
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(sa.TotalAll,'') like '%{T.TotalAll}%' and
        ISNULL(SalesItem,'') like '%{T.SalesItem}%' and ISNULL(Ingredients,'') like '%{T.Ingredients}%' and ISNULL(Production,'') like '%{T.Production}%' and ISNULL(sa.DeliveryId,'') like '%{T.DeliveryId}%' and sa.DeliveryDeadline BETWEEN '{T.BaslangıcTarih}' and '{T.SonTarih}'
        group by sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName,sa.DeliveryDeadline,sa.DeliveryId,sa.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";

                }
            }

        

            var ScheludeOpenList = await _db.QueryAsync<SatısList>(sql, param);


            foreach (var item in ScheludeOpenList)
            {
              
                param.Add("@Listid", item.id);
                param.Add("@SalesOrderId", item.id);
                string sqlsorgu = $@"SELECT ma.id,ma.Name,ma.ItemId,Items.Name as ItemName,ma.ExpectedDate,ma.PlannedQuantity,ma.TotalCost,ma.[Status] FROM ManufacturingOrder ma  
left join Items on Items.id=ma.ItemId
Where ma.SalesOrderId=@SalesOrderId and ma.CompanyId=@CompanyId and ma.IsActive=1 and ma.Status!=3";
                var Manufacturing = await _db.QueryAsync<ManufacturingOrderDetail>(sqlsorgu, param);
                item.MOList = Manufacturing;

            }

            return ScheludeOpenList;
        }
        public async Task<IEnumerable<SatısList>> SalesOrderDoneList(SatısListFiltre T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@LocationId", T.LocationId);
            string sql = string.Empty;

            if (T.BaslangıcTarih == null || T.BaslangıcTarih == "" || T.SonTarih == null || T.SonTarih == "")
            {
                if (T.LocationId == null)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName as CustomerName,SUM(sa.TotalAll)AS TotalAll,
        sa.DeliveryDeadline,MIN(SalesOrderItem.SalesItem)as SalesItem,Min(SalesOrderItem.Ingredients)as Ingredients,Min(SalesOrderItem.Production)as Production,sa.LocationId,Locations.LocationName,sa.DeliveryId
		FROM SalesOrder sa
        left join Contacts on Contacts.id=sa.ContactId
        left join SalesOrderItem on SalesOrderItem.SalesOrderId=sa.id
		left join Locations on Locations.id=sa.LocationId
       where sa.CompanyId=@CompanyId and sa.Tip='SalesOrder'  and sa.IsActive=1 and sa.DeliveryId=4 and 
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(sa.TotalAll,'') like '%{T.TotalAll}%' and
        ISNULL(SalesItem,'') like '%{T.SalesItem}%' and ISNULL(Ingredients,'') like '%{T.Ingredients}%' and ISNULL(Production,'') like '%{T.Production}%' and ISNULL(sa.DeliveryId,'') like '%{T.DeliveryId}%' 
        group by sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName,sa.DeliveryDeadline,sa.DeliveryId,sa.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";
                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName as CustomerName,SUM(sa.TotalAll)AS TotalAll,
        sa.DeliveryDeadline,MIN(SalesOrderItem.SalesItem)as SalesItem,Min(SalesOrderItem.Ingredients)as Ingredients,Min(SalesOrderItem.Production)as Production,sa.LocationId,Locations.LocationName,sa.DeliveryId
		FROM SalesOrder sa
        left join Contacts on Contacts.id=sa.ContactId
        left join SalesOrderItem on SalesOrderItem.SalesOrderId=sa.id
		left join Locations on Locations.id=sa.LocationId
       where sa.CompanyId=@CompanyId and sa.Tip='SalesOrder'  and sa.LocationId=@LocationId and sa.IsActive=1 and sa.DeliveryId=4 and 
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(sa.TotalAll,'') like '%{T.TotalAll}%' and
        ISNULL(SalesItem,'') like '%{T.SalesItem}%' and ISNULL(Ingredients,'') like '%{T.Ingredients}%' and ISNULL(Production,'') like '%{T.Production}%' and ISNULL(sa.DeliveryId,'') like '%{T.DeliveryId}%' 
        group by sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName,sa.DeliveryDeadline,sa.DeliveryId,sa.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";
                }
            }
            else
            {

                if (T.LocationId == null)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName as CustomerName,SUM(sa.TotalAll)AS TotalAll,
        sa.DeliveryDeadline,MIN(SalesOrderItem.SalesItem)as SalesItem,Min(SalesOrderItem.Ingredients)as Ingredients,Min(SalesOrderItem.Production)as Production,sa.LocationId,Locations.LocationName,sa.DeliveryId
		FROM SalesOrder sa
        left join Contacts on Contacts.id=sa.ContactId
        left join SalesOrderItem on SalesOrderItem.SalesOrderId=sa.id
		left join Locations on Locations.id=sa.LocationId
       where sa.CompanyId=@CompanyId and sa.Tip='SalesOrder'  and sa.IsActive=1 and sa.DeliveryId=4 and 
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(sa.TotalAll,'') like '%{T.TotalAll}%' and
        ISNULL(SalesItem,'') like '%{T.SalesItem}%' and ISNULL(Ingredients,'') like '%{T.Ingredients}%' and ISNULL(Production,'') like '%{T.Production}%' and ISNULL(sa.DeliveryId,'') like '%{T.DeliveryId}%'  and sa.DeliveryDeadline BETWEEN '{T.BaslangıcTarih}' and '{T.SonTarih}'
        group by sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName,sa.DeliveryDeadline,sa.DeliveryId,sa.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";

                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName as CustomerName,SUM(sa.TotalAll)AS TotalAll,
        sa.DeliveryDeadline,MIN(SalesOrderItem.SalesItem)as SalesItem,Min(SalesOrderItem.Ingredients)as Ingredients,Min(SalesOrderItem.Production)as Production,sa.LocationId,Locations.LocationName,sa.DeliveryId
		FROM SalesOrder sa
        left join Contacts on Contacts.id=sa.ContactId
        left join SalesOrderItem on SalesOrderItem.SalesOrderId=sa.id
		left join Locations on Locations.id=sa.LocationId
       where sa.CompanyId=@CompanyId and sa.Tip='SalesOrder'  and sa.LocationId=@LocationId and sa.IsActive=1 and sa.DeliveryId=4 and 
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(sa.TotalAll,'') like '%{T.TotalAll}%' and
        ISNULL(SalesItem,'') like '%{T.SalesItem}%' and ISNULL(Ingredients,'') like '%{T.Ingredients}%' and ISNULL(Production,'') like '%{T.Production}%' and ISNULL(sa.DeliveryId,'') like '%{T.DeliveryId}%' and sa.DeliveryDeadline BETWEEN '{T.BaslangıcTarih}' and '{T.SonTarih}'
        group by sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName,sa.DeliveryDeadline,sa.DeliveryId,sa.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";

                }
            }



            var ScheludeOpenList = await _db.QueryAsync<SatısList>(sql, param);


            foreach (var item in ScheludeOpenList)
            {

                param.Add("@Listid", item.id);
                param.Add("@SalesOrderId", item.id);
                string sqlsorgu = $@"SELECT ma.id,ma.Name,ma.ItemId,Items.Name as ItemName,ma.ExpectedDate,ma.PlannedQuantity,ma.TotalCost,ma.[Status] FROM ManufacturingOrder ma  
left join Items on Items.id=ma.ItemId
Where ma.SalesOrderId=@SalesOrderId and ma.CompanyId=@CompanyId and ma.IsActive=1 and ma.Status=3";
                var Manufacturing = await _db.QueryAsync<ManufacturingOrderDetail>(sqlsorgu, param);
                item.MOList = Manufacturing;

            }

            return ScheludeOpenList;
        }

        public async Task<IEnumerable<SalesOrderDetail>> Detail(int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", id);
            string sql = $@"select o.id,o.ContactId,Contacts.DisplayName,o.LocationId,Locations.LocationName,o.DeliveryDeadline,o.CreateDate,o.OrderName,o.BillingAddressId,o.ShippingAddressId,
                o.Info,o.DeliveryId
                from SalesOrder o
                left join OrdersItem oi on oi.OrdersId=o.id
                left join Contacts on Contacts.id=o.ContactId
	            LEFT join Locations on Locations.id=o.LocationId
                where o.CompanyId=@CompanyId and o.id=@id
                group by o.id,o.ContactId,Contacts.DisplayName,o.DeliveryDeadline,o.CreateDate,o.OrderName,o.BillingAddressId,o.ShippingAddressId,o.Info,o.LocationId,Locations.LocationName,o.DeliveryId";
            var details = await _db.QueryAsync<SalesOrderDetail>(sql, prm);
            foreach (var item in details)
            {
                DynamicParameters prm1 = new DynamicParameters();
                prm1.Add("@CompanyId", CompanyId);
                prm1.Add("@id", id);
                string sqla = $@"Select LocationId from SalesOrder where CompanyId=@CompanyId and id=@id";
                var sorgu = await _db.QueryAsync<int>(sqla, prm);
                prm1.Add("@LocationId", sorgu.First());

                string sql1 = $@"
           Select SalesOrderItem.id as id,SalesOrderItem.ItemId,Items.Name as ItemName,SalesOrderItem.Quantity,Items.Tip,
           SalesOrderItem.PricePerUnit, SalesOrderItem.TotalAll, SalesOrderItem.TaxId, Tax.TaxName,SalesOrderItem.TaxValue as Rate,SalesOrderItem.SalesItem,SalesOrderItem.Ingredients,SalesOrderItem.Production,
		     (SUM(ISNULL(rez.RezerveCount,0)))- ISNULL(SalesOrderItem.Quantity,0)+(SUM(ISNULL(ManufacturingOrder.PlannedQuantity,0)))as missing
		   from SalesOrder 
        inner join SalesOrderItem on SalesOrderItem.SalesOrderId = SalesOrder.id 
		left join Items on Items.id = SalesOrderItem.ItemId
		left join Tax on Tax.id = SalesOrderItem.TaxId
		LEFT join ManufacturingOrder on ManufacturingOrder.SalesOrderItemId=SalesOrderItem.id and ManufacturingOrder.SalesOrderId=SalesOrder.id and ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1
        LEFT join Rezerve on Rezerve.SalesOrderItemId=SalesOrderItem.id and Rezerve.SalesOrderId=SalesOrder.id and Rezerve.ItemId=Items.id
        LEFT join Rezerve rez on rez.ItemId=Items.id and rez.Status=1
        where SalesOrder.CompanyId = @CompanyId and SalesOrder.id = @id  
		Group by SalesOrderItem.id,SalesOrderItem.ItemId,Items.Name,SalesOrderItem.Quantity,Items.Tip,
           SalesOrderItem.PricePerUnit, SalesOrderItem.TotalAll, SalesOrderItem.TaxId, Tax.TaxName,SalesOrderItem.TaxValue,
		         SalesOrderItem.SalesItem,SalesOrderItem.Ingredients,SalesOrderItem.Production,Rezerve.RezerveCount";
                var ItemsDetail = await _db.QueryAsync<SatısDetail>(sql1, prm1);
                item.detay = ItemsDetail;
            }
            return details;
        }
        public async Task<IEnumerable<MissingCount>> IngredientsMissingList(IngredientMis T, int CompanyId)
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
        ((Select SalesOrderItem.Quantity from SalesOrder sa left join SalesOrderItem on SalesOrderItem.SalesOrderId = sa.id where sa.id = @id and SalesOrderItem.id=@OrderItemId) *
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
		    where ManufacturingOrder.CompanyId =@CompanyId and ManufacturingOrder.SalesOrderId=@id and ManufacturingOrder.SalesOrderItemId=@OrderItemId 
			and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.ItemId=@MaterialId and ManufacturingOrder.IsActive=1 
			and ManufacturingOrder.Status!=3)+
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
        public async Task<IEnumerable<SalesOrderSellSomeList>> SalesManufacturingList(int SalesOrderId,int SalesOrderItemId, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@SalesOrderId", SalesOrderId);
            prm.Add("@SalesOrderItemId", SalesOrderItemId);
            string sql = $@"select id,ItemId,PlannedQuantity,LocationId,[Status],[Name],ProductionDeadline from ManufacturingOrder ma where ma.SalesOrderId=@SalesOrderId and ma.SalesOrderItemId=@SalesOrderItemId and ma.IsActive=1 and ma.CompanyId=@CompanyId and ma.Status!=3";
            var details = await _db.QueryAsync<SalesOrderSellSomeList>(sql, prm);
            return details;
        }


    }
}
