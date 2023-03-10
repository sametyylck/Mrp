using DAL.Contracts;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;

namespace DAL.Repositories
{
    public class UretimListRepository : IUretimList
    {
        private readonly IDbConnection _db;

        public UretimListRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ManufacturingOrderDetail>> Detail(int CompanyId, int id)
        {
            DynamicParameters prm = new();
            prm.Add("@CompanyId", CompanyId);

            string sql = $@"
                                Select ManufacturingOrder.id , ManufacturingOrder.[Name],ManufacturingOrder.ItemId,Items.[Name] as ItemName,ManufacturingOrder.ExpectedDate,ManufacturingOrder.SalesOrderId,ManufacturingOrder.SalesOrderItemId,                                 ManufacturingOrder.ProductionDeadline,ManufacturingOrder.CreatedDate,ManufacturingOrder.PlannedQuantity,
                                ManufacturingOrder.LocationId,Locations.LocationName,ManufacturingOrder.[Status],ManufacturingOrder.Info
                                From ManufacturingOrder
                                inner join Items on Items.id = ManufacturingOrder.ItemId
                                inner join Locations on Locations.id = ManufacturingOrder.LocationId
                                where ManufacturingOrder.CompanyId = {CompanyId} and ManufacturingOrder.id = {id}";
            var Detail = await _db.QueryAsync<ManufacturingOrderDetail>(sql);
            foreach (var item in Detail)
            {
                prm.Add("@LocationId", item.LocationId);
                prm.Add("@id", id);

                string sql1 = $@"  select moi.id,moi.Tip,moi.ItemId,Items.Name,ISNULL(Notes,'')AS Note,moi.PlannedQuantity as Quantity,moi.Cost,moi.Availability,
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
                var IngredientsDetail = await _db.QueryAsync<ManufacturingOrderItemsIngredientsDetail>(sql1, prm);
                string sql2 = $@"Select moi.id,
                            moi.OperationId,Operations.[Name] as OperationName,
                            moi.ResourceId ,Resources.[Name] as ResourceName,
                            moi.PlannedTime,moi.CostPerHour,
                            Cast(ISNULL(moi.Cost,0)as decimal(15,2)) as Cost,
                            moi.[Status]
                            From ManufacturingOrderItems moi
                            left join Operations on Operations.id = moi.OperationId
                            left join Resources on moi.ResourceId = Resources.id
                            where Tip='Operations' and moi.OrderId={id} and moi.CompanyId={CompanyId}";
                var OperationDetail = await _db.QueryAsync<ManufacturingOrderItemsOperationDetail>(sql2);
                item.IngredientDetail = IngredientsDetail;
                item.OperationDetail = OperationDetail;

            }

            return Detail.ToList();
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

        public async Task<IEnumerable<ManufacturingOrderDoneList>> ScheludeDoneList(ManufacturingOrderDoneList T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("location", T.LocationId);
            string sql = string.Empty;
            if (KAYITSAYISI == null)
            {
                KAYITSAYISI = 10;
            }
            if (SAYFA == null)
            {
                SAYFA = 1;
            }

            if (T.BaslangıcTarih == null || T.SonTarih == null)
            {

                if (T.LocationId == null || T.LocationId == 0)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            ManufacturingOrder.id
            , ManufacturingOrder.[Name], ManufacturingOrder.ItemId,
			Items.[Name] AS ItemName,ManufacturingOrder.LocationId,Locations.LocationName ,ManufacturingOrder.ExpectedDate,
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
            Locations.LocationName , Items.[Name],Categories.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrder.[Status],ManufacturingOrder.ExpectedDate) x
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
            ManufacturingOrder.id, ManufacturingOrder.[Name], ManufacturingOrder.ItemId,
			Items.[Name] AS ItemName,ManufacturingOrder.LocationId,Locations.LocationName ,ManufacturingOrder.ExpectedDate,
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
            Locations.LocationName , Items.[Name],Categories.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrder.[Status],ManufacturingOrder.ExpectedDate) x
			 where ISNULL(PlannedTime,0) like '%{T.PlannedTime}%' AND  ISNULL(Name,'') Like '%{T.Name}%' AND    ISNULL     (Customer,'') like '%{T.Customer}%' and
            ISNULL(ItemName,'') like '%{T.ItemName}%' and ISNULL(CategoryName,'') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity,'') like '%{T.PlannedQuantity}%' and
             ISNULL(Status,'') like '%{T.Status}%' and    ISNULL(MaterialCost,'') like '%{T.MaterialCost}%' and    ISNULL(OperationCost,'') like '%{T.OperationCost}%'
			 and    ISNULL(TotalCost,'') like '%{T.TotalCost}%'
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
                }
                var ScheludeOpenDoneList = await _db.QueryAsync<ManufacturingOrderDoneList>(sql, param);
                return ScheludeOpenDoneList.ToList();

            }
            else
            {
                var ilkgun = T.BaslangıcTarih.ToString();
                var songun = T.SonTarih.ToString();
                if (T.LocationId == null || T.LocationId == 0)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            ManufacturingOrder.id
            , ManufacturingOrder.[Name], ManufacturingOrder.ItemId,
			Items.[Name] AS ItemName,ManufacturingOrder.LocationId,Locations.LocationName ,ManufacturingOrder.ExpectedDate,
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
            Locations.LocationName , Items.[Name],Categories.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrder.[Status],ManufacturingOrder.ExpectedDate) x
            where ISNULL(PlannedTime,0) like '%{T.PlannedTime}%' AND  ISNULL(Name,'') Like '%{T.Name}%' AND    ISNULL     (Customer,'') like '%{T.Customer}%' and
            ISNULL(ItemName,'') like '%{T.ItemName}%' and ISNULL(CategoryName,'') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity,'') like '%{T.PlannedQuantity}%' and
             ISNULL(Status,'') like '%{T.Status}%' and    ISNULL(MaterialCost,'') like '%{T.MaterialCost}%' and    ISNULL(OperationCost,'') like '%{T.OperationCost}%'
			 and    ISNULL(TotalCost,'') like '%{T.TotalCost}%' and x.DoneDate BETWEEN '{ilkgun}' and '{songun}'
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            ManufacturingOrder.id, ManufacturingOrder.[Name], ManufacturingOrder.ItemId,
			Items.[Name] AS ItemName,ManufacturingOrder.LocationId,Locations.LocationName ,ManufacturingOrder.ExpectedDate,
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
            Locations.LocationName , Items.[Name],Categories.[Name],ManufacturingOrder.PlannedQuantity,ManufacturingOrder.[Status],ManufacturingOrder.ExpectedDate) x
			 where ISNULL(PlannedTime,0) like '%{T.PlannedTime}%' AND  ISNULL(Name,'') Like '%{T.Name}%' AND    ISNULL     (Customer,'') like '%{T.Customer}%' and
            ISNULL(ItemName,'') like '%{T.ItemName}%' and ISNULL(CategoryName,'') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity,'') like '%{T.PlannedQuantity}%' and
             ISNULL(Status,'') like '%{T.Status}%' and    ISNULL(MaterialCost,'') like '%{T.MaterialCost}%' and    ISNULL(OperationCost,'') like '%{T.OperationCost}%'
			 and    ISNULL(TotalCost,'') like '%{T.TotalCost}%' and x.DoneDate BETWEEN '{ilkgun}' and '{songun}'
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
                }
                var ScheludeOpenDoneList = await _db.QueryAsync<ManufacturingOrderDoneList>(sql, param);
                return ScheludeOpenDoneList.ToList();

            }



        }


        public async Task<IEnumerable<ManufacturingOrderList>> ScheludeOpenList(ManufacturingOrderList T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("location", T.LocationId);
            string sql = string.Empty;

            if (KAYITSAYISI == null)
            {
                KAYITSAYISI = 10;
            }
            if (SAYFA == null)
            {
                SAYFA = 1;
            }

            if (T.BaslangıcTarih == null || T.SonTarih == null)
            {
                if (T.LocationId == null || T.LocationId == 0)
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
            ISNULL(ItemName,'') like '%{T.ItemName}%' and ISNULL(CategoryName,'') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity,'') like '%{T.PlannedQuantity}%' 
            and ISNULL(Status,'') like '%{T.Status}%' 
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
            ISNULL(Status,'') like '%{T.Status}%' 
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;
                        ";
                }
            }
            else
            {
                var ilkgun = T.BaslangıcTarih.ToString();
                var songun = T.SonTarih.ToString();
                if (T.LocationId == null || T.LocationId == 0)
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
            ISNULL(ItemName,'') like '%{T.ItemName}%' and ISNULL(CategoryName,'') like '%{T.CategoryName}%' and ISNULL(PlannedQuantity,'') like '%{T.PlannedQuantity}%' 
            and ISNULL(Status,'') like '%{T.Status}%'  and x.ExpectedDate BETWEEN '{ilkgun}' and '{songun}'
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
            ISNULL(Status,'') like '%{T.Status}%'   and x.ExpectedDate BETWEEN '{ilkgun}' and '{songun}'
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;
                        ";
                }
            }


            var ScheludeOpenList = await _db.QueryAsync<ManufacturingOrderList>(sql, param);



            return ScheludeOpenList.ToList();
        }


        public async Task<IEnumerable<ManufacturingTask>> TaskDoneList(ManufacturingTask T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
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


        public async Task<IEnumerable<ManufacturingTask>> TaskOpenList(ManufacturingTask T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@LocationId", T.LocationId);
            string sql = string.Empty;
            if (T.LocationId == null)
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

    }
}
