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
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;
using static DAL.DTO.StockListDTO;

namespace DAL.Repositories
{

    public class TeklifRepository : ITeklifRepository
    {
        private readonly IDbConnection _db;
        private readonly ISatısRepository _satis;
        private readonly IStockControl _control;

        public TeklifRepository(IDbConnection db, ISatısRepository satis, IStockControl control)
        {
            _db = db;
            _satis = satis;
            _control = control;
        }
        public async Task<int> Insert(SatısDTO T, int CompanyId)
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

            return await _db.QuerySingleAsync<int>($"Insert into SalesOrder (Tip,ContactId,DeliveryDeadline,CreateDate,OrderName,LocationId,Info,CompanyId,IsActive,DeliveryId) OUTPUT INSERTED.[id] values (@Tip,@ContactId,@DeliveryDeadline,@CreateDate,@OrderName,@LocationId,@Info,@CompanyId,@IsActive,@DeliveryId)", prm);
        }
        public async Task<int> InsertPurchaseItem(TeklifInsertItem T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            var liste = await _db.QueryAsync<LocaVarmı>($@"select 
            (select Rate from Tax where id=(select DefaultTaxPurchaseOrderId from GeneralDefaultSettings where CompanyId=@CompanyId))as Rate,
            (select DefaultPrice from Items where id =@ItemId and CompanyId=@CompanyId)as DefaultPrice", prm);
            prm.Add("@TaxId", T.TaxId);
            float rate = await _db.QueryFirstAsync<int>($"select  Rate from Tax where id =@TaxId and CompanyId=@CompanyId", prm);


            var PriceUnit = liste.First().DefaultPrice;

            var TotalPrice = (T.Quantity * PriceUnit); //adet*fiyat
            float? PlusTax = (TotalPrice * rate) / 100; //tax fiyatı hesaplama
            var TotalAll = TotalPrice + PlusTax; //toplam fiyat hesaplama  
            prm.Add("@Quantity", T.Quantity);
            prm.Add("@PricePerUnit", PriceUnit);
            prm.Add("@TaxValue", rate);
            prm.Add("@OrdersId", T.SalesOrderId);
            prm.Add("@PlusTax", PlusTax);
            prm.Add("@TotalPrice", TotalPrice);
            prm.Add("@TotalAll", TotalAll);
            prm.Add("@location", T.LocationId);
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@Stance", 0);
            prm.Add("@SalesItem", 0);
            prm.Add("@Ingredients", 0);
            prm.Add("@Production", 0);

            int id = await _db.QuerySingleAsync<int>($"Insert into SalesOrderItem(ItemId,Quantity,PricePerUnit,TaxId,TaxValue,SalesOrderId,TotalPrice,PlusTax,TotalAll,Stance,SalesItem,Ingredients,CompanyId,Production) OUTPUT INSERTED.[id] values (@ItemId,@Quantity,@PricePerUnit,@TaxId,@TaxValue,@OrdersId,@TotalPrice,@PlusTax,@TotalAll,@Stance,@SalesItem,@Ingredients,@CompanyId,@Production)", prm);

            prm.Add("@SalesOrderItemId", id);
            prm.Add("@id", T.SalesOrderId);

            return id;


        }
        public async Task Update(SalesOrderUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@DeliveryDeadLine", T.DeliveryDeadline);
            prm.Add("@CreateDate", T.CreateDate);
            prm.Add("@OrderName", T.OrderName);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@LocationId", T.LocationId);
            prm.Add("@Total", T.Total);
            prm.Add("@Info", T.Info);
            var location = await _db.QueryAsync<int>($"Select LocationId from SalesOrder where id=@id and CompanyId=@CompanyId ", prm);
            prm.Add("@eskilocationId", location.First());

            await _db.ExecuteAsync($"Update SalesOrder set ContactId=@ContactId,DeliveryDeadLine=@DeliveryDeadLine,CreateDate=@CreateDate,OrderName=@OrderName,Info=@Info,LocationId=@LocationId,TotalAll=@Total where CompanyId=@CompanyId and id=@id", prm);

        }
        public async Task UpdateItems(TeklifUpdateItems T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.SalesOrderId);
            prm.Add("@OrderItemId", T.id);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@Quantity", T.Quantity);
            prm.Add("@PricePerUnit", T.PricePerUnit);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@TaxId", T.TaxId);
            prm.Add("@CustomerId", T.ContactId);
            prm.Add("@location", T.LocationId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);

            string sqlv = $@"Select ItemId  from  SalesOrderItem where CompanyId=@CompanyId and id=@OrderItemId";
            var Item = await _db.QuerySingleAsync<int>(sqlv, prm);
            if (T.ItemId != Item)
            {
                var liste = await _db.QueryAsync<LocaVarmı>($@"select 
            (select Rate from Tax where id=(select DefaultTaxPurchaseOrderId from GeneralDefaultSettings where CompanyId=@CompanyId))as Rate,
            (select DefaultPrice from Items where id =@ItemId and CompanyId=@CompanyId)as DefaultPrice", prm);
                prm.Add("@TaxId", T.TaxId);
                var Birimfiyat = liste.First().DefaultPrice;
                T.PricePerUnit = Birimfiyat;

            }

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
            prm.Add("@OrdersId", T.SalesOrderId);
            prm.Add("@PlusTax", PlusTax);
            prm.Add("@TotalPrice", totalprice);
            prm.Add("@TotalAll", total);
            prm.Add("@ContactsId", T.ContactId);

            await _db.ExecuteAsync($@"Update SalesOrderItem set ItemId=@ItemId,Quantity=@Quantity,TotalAll=@TotalAll,PricePerUnit=@PricePerUnit,TaxId=@TaxId,TaxValue=@TaxValue where CompanyId=@CompanyId and id=@OrderItemId and SalesOrderId=@id", prm);


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
           SalesOrderItem.PricePerUnit, SalesOrderItem.TotalAll, SalesOrderItem.TaxId, Tax.TaxName,SalesOrderItem.TaxValue as Rate
		   from SalesOrder 
        inner join SalesOrderItem on SalesOrderItem.SalesOrderId = SalesOrder.id 
		left join Items on Items.id = SalesOrderItem.ItemId
		left join Tax on Tax.id = SalesOrderItem.TaxId
        where SalesOrder.CompanyId = @CompanyId and SalesOrder.id = @id  
		Group by SalesOrderItem.id,SalesOrderItem.ItemId,Items.Name,SalesOrderItem.Quantity,Items.Tip,
           SalesOrderItem.PricePerUnit, SalesOrderItem.TotalAll, SalesOrderItem.TaxId, Tax.TaxName,SalesOrderItem.TaxValue";
                var ItemsDetail = await _db.QueryAsync<SatısDetail>(sql1, prm1);
                item.detay = ItemsDetail;
            }
            return details;
        }

        public async Task<IEnumerable<SatısList>> SalesOrderList(SatısListFiltre T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
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
        sa.DeliveryDeadline,sa.LocationId,Locations.LocationName,sa.DeliveryId
		FROM SalesOrder sa
        left join Contacts on Contacts.id=sa.ContactId
        left join SalesOrderItem on SalesOrderItem.SalesOrderId=sa.id
		left join Locations on Locations.id=sa.LocationId
       where sa.CompanyId=@CompanyId and sa.Tip='Quotes'  and sa.IsActive=1 and sa.DeliveryId!=4 and 
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(sa.TotalAll,'') like '%{T.TotalAll}%' and ISNULL(sa.DeliveryId,'') like '%{T.DeliveryId}%' 
        group by sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName,sa.DeliveryDeadline,sa.DeliveryId,sa.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";
                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName as CustomerName,SUM(sa.TotalAll)AS TotalAll,
        sa.DeliveryDeadline,sa.LocationId,Locations.LocationName,sa.DeliveryId
		FROM SalesOrder sa
        left join Contacts on Contacts.id=sa.ContactId
        left join SalesOrderItem on SalesOrderItem.SalesOrderId=sa.id
		left join Locations on Locations.id=sa.LocationId
       where sa.CompanyId=@CompanyId and sa.Tip='Quotes'  and sa.LocationId=@LocationId and sa.IsActive=1 and sa.DeliveryId!=4 and 
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(sa.TotalAll,'') like '%{T.TotalAll}%' and ISNULL(sa.DeliveryId,'') like '%{T.DeliveryId}%' 
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
        sa.DeliveryDeadline,sa.LocationId,Locations.LocationName,sa.DeliveryId
		FROM SalesOrder sa
        left join Contacts on Contacts.id=sa.ContactId
        left join SalesOrderItem on SalesOrderItem.SalesOrderId=sa.id
		left join Locations on Locations.id=sa.LocationId
       where sa.CompanyId=@CompanyId and sa.Tip='Quotes'  and sa.IsActive=1 and sa.DeliveryId!=4 and 
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(sa.TotalAll,'') like '%{T.TotalAll}%'  and ISNULL(sa.DeliveryId,'') like '%{T.DeliveryId}%'  and sa.DeliveryDeadline BETWEEN '{T.BaslangıcTarih}' and '{T.SonTarih}'
        group by sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName,sa.DeliveryDeadline,sa.DeliveryId,sa.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";

                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName as CustomerName,SUM(sa.TotalAll)AS TotalAll,
        sa.DeliveryDeadline,sa.LocationId,Locations.LocationName,sa.DeliveryId
		FROM SalesOrder sa
        left join Contacts on Contacts.id=sa.ContactId
        left join SalesOrderItem on SalesOrderItem.SalesOrderId=sa.id
		left join Locations on Locations.id=sa.LocationId
       where sa.CompanyId=@CompanyId and sa.Tip='Quotes'  and sa.LocationId=@LocationId and sa.IsActive=1 and sa.DeliveryId!=4 and 
        ISNULL(OrderName,0) like '%{T.OrderName}%'  and ISNULL(Contacts.DisplayName,'') Like '%{T.CustomerName}%' AND    ISNULL(sa.TotalAll,'') like '%{T.TotalAll}%' and ISNULL(sa.DeliveryId,'') like '%{T.DeliveryId}%' and sa.DeliveryDeadline BETWEEN '{T.BaslangıcTarih}' and '{T.SonTarih}'
        group by sa.id,sa.OrderName,sa.ContactId,Contacts.DisplayName,sa.DeliveryDeadline,sa.DeliveryId,sa.LocationId,Locations.LocationName)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";

                }
            }

            var ScheludeOpenList = await _db.QueryAsync<SatısList>(sql, param);
            return ScheludeOpenList;
        }

        public async Task DeleteItems(SatısDeleteItems T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@OrdersId", T.OrdersId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@CompanyId", CompanyId);
            if (T.ItemId != 0)
            {
                await _db.ExecuteAsync($"Delete from SalesOrderItem where id=@id and SalesOrderId=@OrdersId and CompanyId=@CompanyId", prm);
            }

        }
        public async Task DeleteStockControl(List<SatısDelete> A, int CompanyId, int User)
        {
            foreach (var T in A)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@CompanyId", CompanyId);
                param.Add("@id", T.id);
                param.Add("@IsActive", false);
                param.Add("@User", User);
                param.Add("@Date", DateTime.Now);
                var IsActived = await _db.QueryAsync<bool>($"select IsActive from SalesOrder where id=@id and CompanyId=@CompanyId ", param);
                if (IsActived.First() == false)
                {

                }
                else
                {
                    var detay = await _db.QueryAsync<TeklifUpdateItems>($"select * from SalesOrderItem where SalesOrderId=@id and CompanyId=@CompanyId ", param);
                    foreach (var item in detay)
                    {
                        param.Add("@itemid", item.id);

                        await _db.ExecuteAsync($"Delete from  SalesOrderItem where id = @itemid and CompanyId = @CompanyId ", param);

                    }

                    await _db.ExecuteAsync($"Delete from  SalesOrder where id = @id and CompanyId = @CompanyId ", param);
                }
            }

        }

        public async Task QuotesDone(QuotesDone T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", T.id);
            prm.Add("@Quotes", T.Quotes);

            if (T.Quotes == 1)
            {
                await _db.ExecuteAsync($"Update SalesOrder set Tip='SalesOrder' where CompanyId=@CompanyId and id=@id", prm);

                List<ManufacturingOrderItemsIngredientsUpdate> Itemdegerler = (await _db.QueryAsync<ManufacturingOrderItemsIngredientsUpdate>($@"select ItemId,Quantity,SalesOrderItem.id from SalesOrderItem 
                  inner join SalesOrder on SalesOrder.id=SalesOrderItem.SalesOrderId
                  where SalesOrderItem.CompanyId=@CompanyId and SalesOrderItem.SalesOrderId=@id and SalesOrder.IsActive=1 and SalesOrder.DeliveryId!=4", prm)).ToList();
                foreach (var item in Itemdegerler)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@ItemId", item.ItemId);
                    param.Add("@CompanyId", CompanyId);
                    param.Add("@location", T.LocationId);
                    param.Add("@id", T.id);
                    param.Add("@OrderItemId", item.id);
                    param.Add("@ContactId", T.ContactId);

                    var RezerveCount = await _control.Count(item.ItemId, CompanyId, T.LocationId);
                    string sqla = $@"select
                     (Select ISNULL(Tip,'') from Items where id = @ItemId and CompanyId = @CompanyId)as Tip";
                    var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, param);
                    var Tip = sorgu.First().Tip;
                    SatısInsertItem A = new SatısInsertItem();
                    A.ItemId = item.ItemId;
                    A.LocationId = T.LocationId;
                    A.ContactId = T.ContactId;
                    A.Quantity = item.Quantity;



                    await _satis.Control(A, T.id, Tip, CompanyId);
                    if (A.Status == 3)
                    {
                        param.Add("@SalesItem", 3);
                    }
                    else
                    {
                        param.Add("@SalesItem", 1);
                    }

                    if (A.Status == 3)
                    {
                        param.Add("@SalesItem", 3);
                        param.Add("@Production", 4);
                        param.Add("@Ingredient", 3);


                        await _db.ExecuteAsync($"Update SalesOrderItem set SalesItem=@SalesItem,Production=@Production,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId", param);


                        List<int> rezerveId = (await _db.QueryAsync<int>($"SELECT * FROM Rezerve where  CompanyId=@CompanyId and CustomerId=@ContactId and SalesOrderId=@id and LocationId=@location and Status=1 and SalesOrderItemId is null", param)).ToList();
                        param.Add("@RezerveId", rezerveId[0]);

                        await _db.QueryAsync($"Update Rezerve set SalesOrderItemId=@OrderItemId where  CompanyId=@CompanyId and CustomerId=@ContactId and SalesOrderId=@id and LocationId=@location and id=@RezerveId ", param);

                    }
                    else
                    {


                        await _satis.IngredientsControl(A, T.id, CompanyId);
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

                        await _db.ExecuteAsync($"Update SalesOrderItem set SalesItem=@SalesItem,Production=@Production,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId", param);

                    }

                }

            }
            else
            {


            }
        }



    }



}

