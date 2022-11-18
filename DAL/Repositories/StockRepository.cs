using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockListDTO;

namespace DAL.Repositories
{
    public class StockRepository : IStockRepository
    {
        IDbConnection _db;

        public StockRepository(IDbConnection dbConnection)
        {
            _db = dbConnection;
        }

        public async Task<int> AllItemsCount(StockListAll T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@location", T.locationId);
           
            if (T.locationId==null)
            {
                var kayitsayisi = await _db.QueryAsync<int>($@"select COUNT(*) as kayitsayisi from(
            Select x.*FROM
            ( Select Items.id,
            ISNULL((select StockCount from LocationStock where ItemId=Items.id and LocationStock.LocationId = @location),0) as InStock,
            Items.Tip,Items.Name as ItemName,Items.CategoryId as CategoryId,ISNULL(Categories.Name, '') as CategoryName,
            Items.ContactId as ContactId ,ISNULL(Contacts.DisplayName, '') as SupplierName,ISNULL(Items.VariantCode, '') as VariantCode,

             ISNULL(((NULLIF(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId = Items.id and
              LocationStock.LocationId = @location)),0)/NULLIF(( (select StockCount from LocationStock
               where ItemId = Items.id and LocationStock.LocationId = @location)),0)
              )),0) AS AverageCost,
			
			
			ISNULL((Items.DefaultPrice * ((select StockCount from LocationStock
            where 
            ItemId = Items.id and LocationStock.LocationId = @location))),0) as ValueInStock,
			 ISNULL(( Select SUM(ISNULL(ManufacturingOrderItems.PlannedQuantity,0)) as [Committed]from ManufacturingOrder
                left join ManufacturingOrderItems on ManufacturingOrderItems.OrderId=ManufacturingOrder.id
				
				where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.LocationId=@location
                 and ManufacturingOrder.[Status]!=3 and ManufacturingOrderItems.Tip='Ingredients' 
				 and ManufacturingOrder.id=ManufacturingOrderItems.OrderId AND ManufacturingOrderItems.ItemId=Items.id and ManufacturingOrder.IsActive=1 
				   ),0) as MaterialCommitted,


                (select ISNULL(SUM(Quantity),0) from Orders
                left join  OrdersItem on OrdersItem.OrdersId = Orders.id and OrdersItem.ItemId = Items.id
                    where 
            Orders.CompanyId = @CompanyId and Orders.DeliveryId = 1 and Orders.LocationId=@location and Orders.IsActive=1)as MaterialExpected,
			
			       (select ISNULL(SUM( ManufacturingOrder.PlannedQuantity),0) as Expected from ManufacturingOrder
		   		left join Items on Items.id=ManufacturingOrder.ItemId and Items.CategoryId=Categories.id 
				left join Locations on Locations.id=ManufacturingOrder.LocationId
          where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.ItemId=Items.id 
           and ManufacturingOrder.LocationId=@location and
          ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1) as ProductExpected,Items.IsActive,
            Items.CompanyId as CompanyId From Items
            left join Categories on   Categories.id = Items.CategoryId and Items.Tip='Product' 
            left join Contacts on   Contacts.id = Items.ContactId)x
            where 
            x.CompanyId = @CompanyId  and x.IsActive=1 AND ISNULL(x.ItemName,'') LIKE '%{T.ItemName}%' AND ISNULL(x.VariantCode,'') LIKE '%{T.VariantCode}%' AND 
            ISNULL(x.CategoryName,'') LIKE '%{T.CategoryName}%' AND ISNULL(x.SupplierName,'') LIKE '%{T.SupplierName}%' AND ISNULL(x.AverageCost,0) LIKE '%{T.AverageCost}%'   AND
            ISNULL(x.ValueInStock,0) LIKE '%{T.ValueInStock}%' AND ISNULL(x.InStock,0) LIKE '%{T.InStock}%' AND ISNULL(x.MaterialExpected,0) LIKE '%{T.MaterialExpected}%' AND ISNULL(x.ProductExpected,0) LIKE '%{T.ProductExpected}%'
			AND ISNULL(x.MaterialCommitted,0) LIKE '%{T.MaterailCommitted}%'
            Group By
                      x.id,x.Tip,x.ItemName,x.CategoryId,x.ContactId ,x.SupplierName,x.VariantCode,x.IsActive,
                    x.AverageCost,x.CompanyId,x.InStock,x.CategoryName,x.ValueInStock,x.MaterialExpected
                        ,x.ProductExpected,x.MaterialCommitted)a", prm);
                return kayitsayisi.First();
            }
            else
            {
                var kayitsayisi = await _db.QueryAsync<int>($@"select COUNT(*) as kayitsayisi from(
            Select x.*FROM
            ( Select Items.id,
            ISNULL((select StockCount from LocationStock where ItemId=Items.id and LocationStock.LocationId = @location),0) as InStock,
            Items.Tip,Items.Name as ItemName,Items.CategoryId as CategoryId,ISNULL(Categories.Name, '') as CategoryName,
            Items.ContactId as ContactId ,ISNULL(Contacts.DisplayName, '') as SupplierName,ISNULL(Items.VariantCode, '') as VariantCode,

             ISNULL(((NULLIF(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId = Items.id and
              LocationStock.LocationId = @location)),0)/NULLIF(( (select StockCount from LocationStock
               where ItemId = Items.id and LocationStock.LocationId = @location)),0)
              )),0) AS AverageCost,
			
			
			ISNULL((Items.DefaultPrice * ((select StockCount from LocationStock
            where 
            ItemId = Items.id and LocationStock.LocationId = @location))),0) as ValueInStock,
			 ISNULL(( Select SUM(ISNULL(ManufacturingOrderItems.PlannedQuantity,0)) as [Committed]from ManufacturingOrder
                left join ManufacturingOrderItems on ManufacturingOrderItems.OrderId=ManufacturingOrder.id
				
				where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.LocationId=@location
                 and ManufacturingOrder.[Status]!=3 and ManufacturingOrderItems.Tip='Ingredients' 
				 and ManufacturingOrder.id=ManufacturingOrderItems.OrderId AND ManufacturingOrderItems.ItemId=Items.id and ManufacturingOrder.IsActive=1 
				   ),0) as MaterialCommitted,


                (select ISNULL(SUM(Quantity),0) from Orders
                left join  OrdersItem on OrdersItem.OrdersId = Orders.id and OrdersItem.ItemId = Items.id
                    where 
            Orders.CompanyId = @CompanyId and Orders.DeliveryId = 1 and Orders.LocationId=@location and Orders.IsActive=1)as MaterialExpected,
			
			       (select ISNULL(SUM( ManufacturingOrder.PlannedQuantity),0) as Expected from ManufacturingOrder
		   		left join Items on Items.id=ManufacturingOrder.ItemId and Items.CategoryId=Categories.id 
				left join Locations on Locations.id=ManufacturingOrder.LocationId
          where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.ItemId=Items.id 
           and ManufacturingOrder.LocationId=@location and
          ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1) as ProductExpected,Items.IsActive,
            Items.CompanyId as CompanyId From Items
            left join Categories on   Categories.id = Items.CategoryId and Items.Tip='Product' 
            left join Contacts on   Contacts.id = Items.ContactId)x
            where 
            x.CompanyId = @CompanyId  and x.IsActive=1 AND ISNULL(x.ItemName,'') LIKE '%{T.ItemName}%' AND ISNULL(x.VariantCode,'') LIKE '%{T.VariantCode}%' AND 
            ISNULL(x.CategoryName,'') LIKE '%{T.CategoryName}%' AND ISNULL(x.SupplierName,'') LIKE '%{T.SupplierName}%' AND ISNULL(x.AverageCost,0) LIKE '%{T.AverageCost}%'   AND
            ISNULL(x.ValueInStock,0) LIKE '%{T.ValueInStock}%' AND ISNULL(x.InStock,0) LIKE '%{T.InStock}%' AND ISNULL(x.MaterialExpected,0) LIKE '%{T.MaterialExpected}%' AND ISNULL(x.ProductExpected,0) LIKE '%{T.ProductExpected}%'
			AND ISNULL(x.MaterialCommitted,0) LIKE '%{T.MaterailCommitted}%'
            Group By
                      x.id,x.Tip,x.ItemName,x.CategoryId,x.ContactId ,x.SupplierName,x.VariantCode,x.IsActive,
                    x.AverageCost,x.CompanyId,x.InStock,x.CategoryName,x.ValueInStock,x.MaterialExpected
                        ,x.ProductExpected,x.MaterialCommitted)a", prm);
                return kayitsayisi.First();
            }
         
        }

        public async Task<IEnumerable<StockListAll>> AllItemsList(StockListAll T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@location", T.locationId);
            string sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI} SET @SAYFA = {SAYFA}
            Select x.*FROM
            ( Select Items.id,
            ISNULL((select StockCount from LocationStock where ItemId=Items.id and LocationStock.LocationId = @location),0) as InStock,
            Items.Tip,Items.Name as ItemName,Items.CategoryId as CategoryId,ISNULL(Categories.Name, '') as CategoryName,
            Items.ContactId as ContactId ,ISNULL(Contacts.DisplayName, '') as SupplierName,ISNULL(Items.VariantCode, '') as VariantCode,

             ISNULL(((NULLIF(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId = Items.id and
              LocationStock.LocationId = @location)),0)/NULLIF(( (select StockCount from LocationStock
               where ItemId = Items.id and LocationStock.LocationId = @location)),0)
              )),0) AS AverageCost,
			
			
			ISNULL((Items.DefaultPrice * ((select StockCount from LocationStock
            where 
            ItemId = Items.id and LocationStock.LocationId = @location))),0) as ValueInStock,
			 ISNULL(( Select SUM(ISNULL(ManufacturingOrderItems.PlannedQuantity,0)) as [Committed]from ManufacturingOrder
                left join ManufacturingOrderItems on ManufacturingOrderItems.OrderId=ManufacturingOrder.id
				
				where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.LocationId=@location
                 and ManufacturingOrder.[Status]!=3 and ManufacturingOrderItems.Tip='Ingredients' 
				 and ManufacturingOrder.id=ManufacturingOrderItems.OrderId AND ManufacturingOrderItems.ItemId=Items.id and ManufacturingOrder.IsActive=1 
				   ),0) as MaterialCommitted,


                (select ISNULL(SUM(Quantity),0) from Orders
                left join  OrdersItem on OrdersItem.OrdersId = Orders.id and OrdersItem.ItemId = Items.id
                    where 
            Orders.CompanyId = @CompanyId and Orders.DeliveryId = 1 and Orders.LocationId=@location and Orders.IsActive=1)as MaterialExpected,
			
			       (select ISNULL(SUM( ManufacturingOrder.PlannedQuantity),0) as Expected from ManufacturingOrder
		   		left join Items on Items.id=ManufacturingOrder.ItemId and Items.CategoryId=Categories.id 
				left join Locations on Locations.id=ManufacturingOrder.LocationId
          where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.ItemId=Items.id 
           and ManufacturingOrder.LocationId=@location and
          ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1) as ProductExpected,Items.IsActive,
            Items.CompanyId as CompanyId From Items
            left join Categories on   Categories.id = Items.CategoryId and Items.Tip='Product' 
            left join Contacts on   Contacts.id = Items.ContactId)x
            where 
            x.CompanyId = @CompanyId  and x.IsActive=1 AND ISNULL(x.ItemName,'') LIKE '%{T.ItemName}%' AND ISNULL(x.VariantCode,'') LIKE '%{T.VariantCode}%' AND 
            ISNULL(x.CategoryName,'') LIKE '%{T.CategoryName}%' AND ISNULL(x.SupplierName,'') LIKE '%{T.SupplierName}%' AND ISNULL(x.AverageCost,0) LIKE '%{T.AverageCost}%'   AND
            ISNULL(x.ValueInStock,0) LIKE '%{T.ValueInStock}%' AND ISNULL(x.InStock,0) LIKE '%{T.InStock}%' AND ISNULL(x.MaterialExpected,0) LIKE '%{T.MaterialExpected}%' AND ISNULL(x.ProductExpected,0) LIKE '%{T.ProductExpected}%'
			AND ISNULL(x.MaterialCommitted,0) LIKE '%{T.MaterailCommitted}%'
            Group By
                      x.id,x.Tip,x.ItemName,x.CategoryId,x.ContactId ,x.SupplierName,x.VariantCode,x.IsActive,
                    x.AverageCost,x.CompanyId,x.InStock,x.CategoryName,x.ValueInStock,x.MaterialExpected
                        ,x.ProductExpected,x.MaterialCommitted
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";
            var list =await _db.QueryAsync<StockListAll>(sql, prm);
            return list.ToList();
        }

        public async Task<int> MaterialCount(StockList T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@location", T.locationId);
           var kayitsayisi =await _db.QueryAsync<int>($@"select count(*)as kayitsayisi from(
                Select x.* FROM
                (Select Items.id,
               ISNULL((select StockCount from LocationStock 
                where ItemId = Items.id and LocationStock.LocationId = @location),0) as   InStock,
                Items.Tip,Items.Name as ItemName,Items.CategoryId as CategoryId,ISNULL(Categories.Name, '') as CategoryName,
                Items.ContactId as ContactId ,ISNULL(Contacts.DisplayName, '') as SupplierName,ISNULL(Items.VariantCode, '') as VariantCode,
                    
							 ISNULL(((NULLIF(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId =Items.id
             and
              LocationStock.LocationId = @location)),0)/NULLIF(( (select StockCount from LocationStock
               where ItemId = Items.id and   LocationStock.LocationId = @location)),0)
              )),0) AS AverageCost,
                ISNULL((Items.DefaultPrice * 
                ((select ISNULL(StockCount,0) from LocationStock where 
                ItemId = Items.id and LocationStock.LocationId = @location))),0) as  ValueInStock,
       
			 ISNULL(( Select SUM(ISNULL(ManufacturingOrderItems.PlannedQuantity,0)) as [Committed]from ManufacturingOrder
                left join ManufacturingOrderItems on ManufacturingOrderItems.OrderId=ManufacturingOrder.id
				
				where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.LocationId=@location
                 and ManufacturingOrder.[Status]!=3 and ManufacturingOrderItems.Tip='Ingredients' 
				 and ManufacturingOrder.id=ManufacturingOrderItems.OrderId AND ManufacturingOrderItems.ItemId=Items.id and ManufacturingOrder.IsActive=1 
				   ),0) as Committed,
            
                ISNULL((Select 
                (select ISNULL(StockCount,0) from LocationStock where ItemId = Items.id
                and LocationStock.LocationId = @location) - ISNULL(( Select SUM(ISNULL(ManufacturingOrderItems.PlannedQuantity,0)) as [Committed]from ManufacturingOrder
                left join ManufacturingOrderItems on ManufacturingOrderItems.OrderId=ManufacturingOrder.id
				
				where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.LocationId=@location
                 and ManufacturingOrder.[Status]!=3 and ManufacturingOrderItems.Tip='Ingredients' 
				 and ManufacturingOrder.id=ManufacturingOrderItems.OrderId AND ManufacturingOrderItems.ItemId=Items.id and ManufacturingOrder.IsActive=1
				   ),0)
                +(select ISNULL(SUM(Quantity),0) from Orders left join OrdersItem on OrdersItem.OrdersId = Orders.id and 
                OrdersItem.ItemId = Items.id  where Orders.CompanyId = @CompanyId and Orders.DeliveryId = 1 and Orders.IsActive=1)),0) as Missing,Items.IsActive,
                (select ISNULL(SUM(Quantity),0) from Orders 
                left join OrdersItem on OrdersItem.OrdersId = Orders.id
                and OrdersItem.ItemId = Items.id where Orders.CompanyId = @CompanyId and Orders.LocationId=@location
                and Orders.DeliveryId = 1 and Orders.IsActive=1)as Expected, 
				
				Items.CompanyId as CompanyId From Items 
				left join ManufacturingOrderItems on ManufacturingOrderItems.ItemId=Items.id
				left join ManufacturingOrder on  ManufacturingOrderItems.OrderId=ManufacturingOrder.id
                left join Categories on   Categories.id = Items.CategoryId
                left join Contacts on   Contacts.id = Items.ContactId)x where x.CompanyId = @CompanyId  AND x.Tip = 'Material' AND x.IsActive=1 and ISNULL(x.ItemName,'')  
                LIKE '%{T.ItemName}%' AND ISNULL(x.VariantCode,'') LIKE '%{T.VariantCode}%' AND ISNULL(x.CategoryName, '') LIKE
                '%{T.CategoryName}%' AND ISNULL(x.SupplierName,'') LIKE '%{T.SupplierName}%' AND ISNULL(x.AverageCost,0) LIKE 
                '{T.AverageCost}%%' AND ISNULL(x.ValueInStock, 0) LIKE '%{T.ValueInStock}%' AND ISNULL(x.InStock,0) LIKE
                '%{T.InStock}%' AND ISNULL(x.Expected,0) LIKE '%{T.Expected}%' AND ISNULL(x.Committed,0) LIKE '%{T.Committed}%'  
                AND ISNULL(x.Missing,0) LIKE '%{T.Missing}%' Group By x.id,x.Tip,x.ItemName,x.CategoryId,x.ContactId ,
                x.SupplierName,x.VariantCode, x.AverageCost,x.CompanyId,x.InStock,x.CategoryName,x.ValueInStock,x.Committed,
                x.Expected,x.Missing,x.IsActive)a", prm);
            return kayitsayisi.First();
        }

        public async Task<IEnumerable<StockList>> MaterialList(StockList T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@location", T.locationId);
            string sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI} SET @SAYFA = {SAYFA}
                Select x.* FROM
                (Select Items.id,
               ISNULL((select StockCount from LocationStock 
                where ItemId = Items.id and LocationStock.LocationId = @location),0) as   InStock,
                Items.Tip,Items.Name as ItemName,Items.CategoryId as CategoryId,ISNULL(Categories.Name, '') as CategoryName,
                Items.ContactId as ContactId ,ISNULL(Contacts.DisplayName, '') as SupplierName,ISNULL(Items.VariantCode, '') as VariantCode,
                    
							 ISNULL(((NULLIF(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId =Items.id
             and
              LocationStock.LocationId = @location)),0)/NULLIF(( (select StockCount from LocationStock
               where ItemId = Items.id and   LocationStock.LocationId = @location)),0)
              )),0) AS AverageCost,
                ISNULL((Items.DefaultPrice * 
                ((select ISNULL(StockCount,0) from LocationStock where 
                ItemId = Items.id and LocationStock.LocationId = @location))),0) as  ValueInStock,
       
			 ISNULL(( Select SUM(ISNULL(ManufacturingOrderItems.PlannedQuantity,0)) as [Committed]from ManufacturingOrder
                left join ManufacturingOrderItems on ManufacturingOrderItems.OrderId=ManufacturingOrder.id
				
				where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.LocationId=@location
                 and ManufacturingOrder.[Status]!=3 and ManufacturingOrderItems.Tip='Ingredients' 
				 and ManufacturingOrder.id=ManufacturingOrderItems.OrderId AND ManufacturingOrderItems.ItemId=Items.id and ManufacturingOrder.IsActive=1 
				   ),0) as Committed,
            
                ISNULL((Select 
                (select ISNULL(StockCount,0) from LocationStock where ItemId = Items.id
                and LocationStock.LocationId = @location) - ISNULL(( Select SUM(ISNULL(ManufacturingOrderItems.PlannedQuantity,0)) as [Committed]from ManufacturingOrder
                left join ManufacturingOrderItems on ManufacturingOrderItems.OrderId=ManufacturingOrder.id
				
				where ManufacturingOrderItems.CompanyId=@CompanyId and ManufacturingOrder.LocationId=@location
                 and ManufacturingOrder.[Status]!=3 and ManufacturingOrderItems.Tip='Ingredients' 
				 and ManufacturingOrder.id=ManufacturingOrderItems.OrderId AND ManufacturingOrderItems.ItemId=Items.id and ManufacturingOrder.IsActive=1
				   ),0)
                +(select ISNULL(SUM(Quantity),0) from Orders left join OrdersItem on OrdersItem.OrdersId = Orders.id and 
                OrdersItem.ItemId = Items.id  where Orders.CompanyId = @CompanyId and Orders.DeliveryId = 1 and Orders.IsActive=1)),0) as Missing,Items.IsActive,
                (select ISNULL(SUM(Quantity),0) from Orders 
                left join OrdersItem on OrdersItem.OrdersId = Orders.id
                and OrdersItem.ItemId = Items.id where Orders.CompanyId = @CompanyId and Orders.LocationId=@location
                and Orders.DeliveryId = 1 and Orders.IsActive=1)as Expected, 
				
				Items.CompanyId as CompanyId From Items 
				left join ManufacturingOrderItems on ManufacturingOrderItems.ItemId=Items.id
				left join ManufacturingOrder on  ManufacturingOrderItems.OrderId=ManufacturingOrder.id
                left join Categories on   Categories.id = Items.CategoryId
                left join Contacts on   Contacts.id = Items.ContactId)x where x.CompanyId = @CompanyId  AND x.Tip = 'Material' AND x.IsActive=1 and ISNULL(x.ItemName,'')  
                LIKE '%{T.ItemName}%' AND ISNULL(x.VariantCode,'') LIKE '%{T.VariantCode}%' AND ISNULL(x.CategoryName, '') LIKE
                '%{T.CategoryName}%' AND ISNULL(x.SupplierName,'') LIKE '%{T.SupplierName}%' AND ISNULL(x.AverageCost,0) LIKE 
                '{T.AverageCost}%%' AND ISNULL(x.ValueInStock, 0) LIKE '%{T.ValueInStock}%' AND ISNULL(x.InStock,0) LIKE
                '%{T.InStock}%' AND ISNULL(x.Expected,0) LIKE '%{T.Expected}%' AND ISNULL(x.Committed,0) LIKE '%{T.Committed}%'  
                AND ISNULL(x.Missing,0) LIKE '%{T.Missing}%' Group By x.id,x.Tip,x.ItemName,x.CategoryId,x.ContactId ,
                x.SupplierName,x.VariantCode, x.AverageCost,x.CompanyId,x.InStock,x.CategoryName,x.ValueInStock,x.Committed,
                x.Expected,x.Missing,x.IsActive ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY";
            var list =await _db.QueryAsync<StockList>(sql, prm);
            return list.ToList();
        }

        public async Task<int> ProductCount(StockList T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@location", T.locationId);
            var kayitsayisi =await _db.QueryAsync<int>($@"select COUNT(*) as kayitsayisi from(
			Select  x.*FROM(
            Select Items.id,Items.[Name] as ItemName,Items.Tip,ISNULL(Items.VariantCode,'') as      VariantCode,Items.CategoryId,ISNULL  (Categories.[Name],'') as CategoryName
            ,Items.ContactId,ISNULL(Contacts.DisplayName,'')as SupplierName,
               ISNULL(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId=Items.id and
              LocationStock.LocationId = @location)),0) as ValueInStock,
              ISNULL(((NULLIF(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId =Items.id and
              LocationStock.LocationId = @location)),0)/NULLIF(( (select StockCount from LocationStock
               where ItemId =Items.id  and LocationStock.LocationId = @location)),0)
              )),0) AS AverageCost,
            
              ISNULL((select StockCount from LocationStock
               where ItemId =Items.id  and LocationStock.LocationId = @location),0) as InStock,
                (select ISNULL(SUM( ManufacturingOrder.PlannedQuantity),0) as Expected from ManufacturingOrder
              left join Items on Items.id=ManufacturingOrder.ItemId and Items.CategoryId=Categories.id
              where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.ItemId=Items.id and  ManufacturingOrder.LocationId=@location and
              ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1)as Expected,Items.IsActive,
             Items.CompanyId

            from Items 
            left join Contacts on Contacts.id=Items.ContactId
            left join Categories on Categories.id=Items.CategoryId )x
            where x.CompanyId=@CompanyId and x.Tip='Product' and x.IsActive=1  and ISNULL(x.ItemName,'') LIKE '%{T.ItemName}%' AND  ISNULL    (x.VariantCode,'') LIKE '%%' AND 
            ISNULL(x.CategoryName,'') LIKE '%{T.CategoryName}%' AND ISNULL(x.SupplierName,'') LIKE '%{T.SupplierName}%' AND ISNULL(x.AverageCost,0) LIKE '%{T.AverageCost}%'   AND
            ISNULL(x.ValueInStock,0) LIKE '%{T.ValueInStock}%' AND ISNULL(x.InStock,0) LIKE '%{T.InStock}%' and ISNULL(x.Expected,0) LIKE '%{T.Expected}%'
            Group By  x.Tip,x.ItemName,x.CategoryId,x.ContactId ,x.id,X.InStock,x.CategoryName,x.ValueInStock,x.Expected,
            x.SupplierName,x.VariantCode,x.AverageCost,x.CompanyId,x.IsActive)a", prm);
            return kayitsayisi.First();
        }

        public async Task<IEnumerable<StockList>> ProductList(StockList T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@location", T.locationId);

            string sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI} SET @SAYFA = {SAYFA}
			Select  x.*FROM(
            Select Items.id,Items.[Name] as ItemName,Items.Tip,ISNULL(Items.VariantCode,'') as      VariantCode,Items.CategoryId,ISNULL  (Categories.[Name],'') as CategoryName
            ,Items.ContactId,ISNULL(Contacts.DisplayName,'')as SupplierName,
               ISNULL(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId=Items.id and
              LocationStock.LocationId = @location)),0) as ValueInStock,
              ISNULL(((NULLIF(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId =Items.id and
              LocationStock.LocationId = @location)),0)/NULLIF(( (select StockCount from LocationStock
               where ItemId =Items.id  and LocationStock.LocationId = @location)),0)
              )),0) AS AverageCost,
            
              ISNULL((select StockCount from LocationStock
               where ItemId =Items.id  and LocationStock.LocationId = @location),0) as InStock,
                (select ISNULL(SUM( ManufacturingOrder.PlannedQuantity),0) as Expected from ManufacturingOrder
              left join Items on Items.id=ManufacturingOrder.ItemId and Items.CategoryId=Categories.id
              where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.ItemId=Items.id and  ManufacturingOrder.LocationId=@location and
              ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1)as Expected,Items.IsActive,
             Items.CompanyId

            from Items 
            left join Contacts on Contacts.id=Items.ContactId
            left join Categories on Categories.id=Items.CategoryId )x
            where x.CompanyId=@CompanyId and x.Tip='Product' and x.IsActive=1  and ISNULL(x.ItemName,'') LIKE '%{T.ItemName}%' AND  ISNULL    (x.VariantCode,'') LIKE '%%' AND 
            ISNULL(x.CategoryName,'') LIKE '%{T.CategoryName}%' AND ISNULL(x.SupplierName,'') LIKE '%{T.SupplierName}%' AND ISNULL(x.AverageCost,0) LIKE '%{T.AverageCost}%'   AND
            ISNULL(x.ValueInStock,0) LIKE '%{T.ValueInStock}%' AND ISNULL(x.InStock,0) LIKE '%{T.InStock}%' and ISNULL(x.Expected,0) LIKE '%{T.Expected}%'
            Group By  x.Tip,x.ItemName,x.CategoryId,x.ContactId ,x.id,X.InStock,x.CategoryName,x.ValueInStock,x.Expected,
            x.SupplierName,x.VariantCode,x.AverageCost,x.CompanyId,x.IsActive
			ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ";
            var list =await _db.QueryAsync<StockList>(sql, prm);
      
            return list.ToList();
        }

        public async Task<int> SemiProductCount(StockList T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@location", T.locationId);
            var kayitsayisi = await _db.QueryAsync<int>($@"select COUNT(*) as kayitsayisi from(
			Select  x.*FROM(
            Select Items.id,Items.[Name] as ItemName,Items.Tip,ISNULL(Items.VariantCode,'') as      VariantCode,Items.CategoryId,ISNULL  (Categories.[Name],'') as CategoryName
            ,Items.ContactId,ISNULL(Contacts.DisplayName,'')as SupplierName,
               ISNULL(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId=Items.id and
              LocationStock.LocationId = @location)),0) as ValueInStock,
              ISNULL(((NULLIF(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId =Items.id and
              LocationStock.LocationId = @location)),0)/NULLIF(( (select StockCount from LocationStock
               where ItemId =Items.id  and LocationStock.LocationId = @location)),0)
              )),0) AS AverageCost,
            
              ISNULL((select StockCount from LocationStock
               where ItemId =Items.id  and LocationStock.LocationId = @location),0) as InStock,
                (select ISNULL(SUM( ManufacturingOrder.PlannedQuantity),0) as Expected from ManufacturingOrder
              left join Items on Items.id=ManufacturingOrder.ItemId and Items.CategoryId=Categories.id
              where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.ItemId=Items.id and  ManufacturingOrder.LocationId=@location and
              ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1)as Expected,Items.IsActive,
             Items.CompanyId

            from Items 
            left join Contacts on Contacts.id=Items.ContactId
            left join Categories on Categories.id=Items.CategoryId )x
            where x.CompanyId=@CompanyId and x.Tip='SemiProduct' and x.IsActive=1  and ISNULL(x.ItemName,'') LIKE '%{T.ItemName}%' AND  ISNULL    (x.VariantCode,'') LIKE '%%' AND 
            ISNULL(x.CategoryName,'') LIKE '%{T.CategoryName}%' AND ISNULL(x.SupplierName,'') LIKE '%{T.SupplierName}%' AND ISNULL(x.AverageCost,0) LIKE '%{T.AverageCost}%'   AND
            ISNULL(x.ValueInStock,0) LIKE '%{T.ValueInStock}%' AND ISNULL(x.InStock,0) LIKE '%{T.InStock}%' and ISNULL(x.Expected,0) LIKE '%{T.Expected}%'
            Group By  x.Tip,x.ItemName,x.CategoryId,x.ContactId ,x.id,X.InStock,x.CategoryName,x.ValueInStock,x.Expected,
            x.SupplierName,x.VariantCode,x.AverageCost,x.CompanyId,x.IsActive)a", prm);
            return kayitsayisi.First();
        }

        public async Task<IEnumerable<StockList>> SemiProductList(StockList T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@location", T.locationId);

            string sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI} SET @SAYFA = {SAYFA}
			Select  x.*FROM(
            Select Items.id,Items.[Name] as ItemName,Items.Tip,ISNULL(Items.VariantCode,'') as      VariantCode,Items.CategoryId,ISNULL  (Categories.[Name],'') as CategoryName
            ,Items.ContactId,ISNULL(Contacts.DisplayName,'')as SupplierName,
               ISNULL(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId=Items.id and
              LocationStock.LocationId = @location)),0) as ValueInStock,
              ISNULL(((NULLIF(Items.DefaultPrice * ((select StockCount from LocationStock where ItemId =Items.id and
              LocationStock.LocationId = @location)),0)/NULLIF(( (select StockCount from LocationStock
               where ItemId =Items.id  and LocationStock.LocationId = @location)),0)
              )),0) AS AverageCost,
            
              ISNULL((select StockCount from LocationStock
               where ItemId =Items.id  and LocationStock.LocationId = @location),0) as InStock,
                (select ISNULL(SUM( ManufacturingOrder.PlannedQuantity),0) as Expected from ManufacturingOrder
              left join Items on Items.id=ManufacturingOrder.ItemId and Items.CategoryId=Categories.id
              where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.ItemId=Items.id and  ManufacturingOrder.LocationId=@location and
              ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1)as Expected,Items.IsActive,
             Items.CompanyId

            from Items 
            left join Contacts on Contacts.id=Items.ContactId
            left join Categories on Categories.id=Items.CategoryId )x
            where x.CompanyId=@CompanyId and x.Tip='SemiProduct' and x.IsActive=1  and ISNULL(x.ItemName,'') LIKE '%{T.ItemName}%' AND  ISNULL    (x.VariantCode,'') LIKE '%%' AND 
            ISNULL(x.CategoryName,'') LIKE '%{T.CategoryName}%' AND ISNULL(x.SupplierName,'') LIKE '%{T.SupplierName}%' AND ISNULL(x.AverageCost,0) LIKE '%{T.AverageCost}%'   AND
            ISNULL(x.ValueInStock,0) LIKE '%{T.ValueInStock}%' AND ISNULL(x.InStock,0) LIKE '%{T.InStock}%' and ISNULL(x.Expected,0) LIKE '%{T.Expected}%'
            Group By  x.Tip,x.ItemName,x.CategoryId,x.ContactId ,x.id,X.InStock,x.CategoryName,x.ValueInStock,x.Expected,
            x.SupplierName,x.VariantCode,x.AverageCost,x.CompanyId,x.IsActive
			ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ";
            var list = await _db.QueryAsync<StockList>(sql, prm);

            return list.ToList();
        }
    }
}
