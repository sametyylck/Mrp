using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.GeneralSettingsDTO;
using static DAL.DTO.ItemDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.TaxDTO;

namespace DAL.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        IDbConnection _db;
        private readonly IUretimRepository _uretim;
        public OrdersRepository(IDbConnection db, IUretimRepository uretim)
        {
            _db = db;
            _uretim = uretim;
        }

        public async Task Delete(List<Delete> A, int CompanyId, int user)
        {
            foreach (var T in A)
            {
                DynamicParameters prm = new DynamicParameters();
                prm.Add("@id", T.id);
                prm.Add("@Tip", T.Tip);
                prm.Add("@CompanyId", CompanyId);
                prm.Add("@IsActive", false);
                prm.Add("@DateTime", DateTime.Now);
                prm.Add("@User", user);

                string sql = $"select Orders.ManufacturingOrderId,Orders.ManufacturingOrderItemId,OrdersItem.id,ItemId from OrdersItem left join Orders on Orders.id=OrdersItem.OrdersId where OrdersItem.CompanyId=@CompanyId and Orders.Tip='PurchaseOrder' and Orders.id=@id";
                var idcontrol = await _db.QueryAsync<PurchaseOrder>(sql, prm);
                foreach (var item in idcontrol)
                {
                    DeleteItems B = new DeleteItems();
                    B.id = item.id;
                    B.ItemId = (int)item.ItemId;
                    B.OrdersId = T.id;
                    await DeleteItems(B, CompanyId);
                }
                var manuorderid = idcontrol.First().ManufacturingOrderId;
                var manuorderitemid = idcontrol.First().ManufacturingOrderItemId;

                string sql1 = $@"Select m.LocationId,mi.ItemId,mi.PlannedQuantity from ManufacturingOrder m 
                left join ManufacturingOrderItems mi on mi.OrderId=m.id
                where m.id={manuorderid} and mi.id={manuorderitemid} and m.CompanyId={CompanyId}";
                var manufacturing = await _db.QueryAsync<PurchaseOrder>(sql1, prm);
                await _db.ExecuteAsync($"Delete from Orders where id = @id and CompanyId = @CompanyId and Tip=@Tip", prm);
                foreach (var item in manufacturing)
                {
                    UretimIngredientsUpdate clas = new();
                    clas.id = manuorderitemid;
                    clas.OrderId = manuorderid;
                    clas.Quantity = item.Quantity;
                    clas.LocationId=item.LocationId;
                    clas.ItemId = item.ItemId;
                    await _uretim.IngredientsUpdate(clas, CompanyId);
                }



            }
          
        }

        public async Task DeleteItems(DeleteItems T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@OrdersId", T.OrdersId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@CompanyId", CompanyId);
            await _db.ExecuteAsync($"Delete From OrdersItem  where ItemId = @ItemId and CompanyId = @CompanyId and id=@id and OrdersId=@OrdersId", prm);
        }

        public async Task<IEnumerable<PurchaseDetails>> Details(int id, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@CompanyId", CompanyId);

            var list = await _db.QueryAsync<PurchaseDetails>($"Select Orders.id,Orders.Tip,Orders.ContactId,Contacts.DisplayName as SupplierName,Orders.ExpectedDate,Orders.CreateDate,Orders.OrderName, Orders.LocationId,Locations.LocationName, Orders.Info,Orders.CompanyId,Orders.DeliveryId From Orders left join Contacts on Contacts.id = Orders.ContactId left join Locations on Locations.id=Orders.LocationId left join OrdersItem on OrdersItem.OrdersId = Orders.id  where Orders.CompanyId = @CompanyId and Orders.id = @id Group By Orders.id, Orders.Tip, Orders.ContactId, Contacts.DisplayName, Orders.ExpectedDate, Orders.CreateDate,Orders.OrderName, Orders.LocationId, Orders.Info,Orders.CompanyId,Locations.LocationName,Orders.DeliveryId", prm);
            foreach (var item in list)
            {
                var list2 = await _db.QueryAsync<PurchaseOrdersItemDetails>($"  Select OrdersItem.id as id,OrdersItem.ItemId ,Items.Name as ItemName, OrdersItem.Quantity, OrdersItem.PricePerUnit, OrdersItem.TaxId, Tax.TaxName, OrdersItem.TaxValue, OrdersItem.TotalAll, OrdersItem.TotalPrice, OrdersItem.PlusTax , OrdersItem.OrdersId, OrdersItem.MeasureId, Measure.Name as MeasureName from OrdersItem  left join Items on Items.id = OrdersItem.ItemId  left    join Tax on Tax.id = OrdersItem.TaxId left  join Measure on Measure.id = OrdersItem.MeasureId  where OrdersItem.CompanyId = @CompanyId and OrdersItem.OrdersId = @id", prm);
                item.detay = list2;
            }
            return list;
        }

        public async Task<int> Insert(PurchaseOrderInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "PurchaseOrder");
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@ExpectedDate", T.ExpectedDate);
            prm.Add("@CreateDate", T.CreateDate);
            prm.Add("@OrderName", T.OrderName);
            prm.Add("@SalesOrderId", T.SalesOrderId);
            prm.Add("@ManufacturingOrderId", T.ManufacturingOrderId);
            prm.Add("@ManufacturingOrderItemId", T.ManufacturingOrderItemId);
            prm.Add("@SalesOrderItemId", T.SalesOrderItemId);
            prm.Add("@Info", T.Info);
            prm.Add("@DeliveryId", 1);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@LocationId", T.LocationId);
            prm.Add("@IsActive", true);

            return await _db.QuerySingleAsync<int>($"Insert into Orders (Tip,SalesOrderId,SalesOrderItemId,ManufacturingOrderId,ManufacturingOrderItemId,ContactId,ExpectedDate,CreateDate,OrderName,LocationId,DeliveryId,Info,CompanyId,IsActive) OUTPUT INSERTED.[id] values (@Tip,@SalesOrderId,@SalesOrderItemId,@ManufacturingOrderId,@ManufacturingOrderItemId,@ContactId,@ExpectedDate,@CreateDate,@OrderName,@LocationId,@DeliveryId,@Info,@CompanyId,@IsActive)", prm);
        }

        public async Task<int> InsertPurchaseItem(PurchaseOrderInsertItem T, int OrdersId, int CompanyId)
        {
            var prm = new DynamicParameters();
            prm.Add("CompanyId", CompanyId);

            prm.Add("ItemId", T.ItemId);

            var Rate = await _db.QueryFirstAsync<TaxClas>($"select Rate from Tax where id =@id and CompanyId=@CompanyId", new { id = T.TaxId, CompanyId = CompanyId });
            float TaxRate = Rate.Rate;

            var DefaultPricee = await _db.QueryFirstAsync<Items>($"select DefaultPrice from Items where id =@ItemId and CompanyId=@CompanyId", new { ItemId = T.ItemId, CompanyId = CompanyId });
            var PriceUnit = DefaultPricee.DefaultPrice;
            var TotalPrice = (T.Quantity * PriceUnit); //adet*fiyat
            float? PlusTax = (TotalPrice * TaxRate) / 100; //tax fiyatı hesaplama
            var TotalAll = TotalPrice + PlusTax; //toplam fiyat hesaplama  
            prm.Add("@Quantity", T.Quantity);
            prm.Add("@PricePerUnit", PriceUnit);
            prm.Add("@TaxId", T.TaxId);
            prm.Add("@TaxValue", TaxRate);
            prm.Add("@OrdersId", OrdersId);
            prm.Add("@PlusTax", PlusTax);
            prm.Add("@TotalPrice", TotalPrice);
            prm.Add("@TotalAll", TotalAll);

            return await _db.QuerySingleAsync<int>($"Insert into OrdersItem (ItemId,Quantity,PricePerUnit,TaxId,TaxValue,OrdersId,TotalPrice,PlusTax,TotalAll,CompanyId,MeasureId) OUTPUT INSERTED.[id] values (@ItemId,@Quantity,@PricePerUnit,@TaxId,@TaxValue,@OrdersId,@TotalPrice,@PlusTax,@TotalAll,@CompanyId,@MeasureId)", new { ItemId = T.ItemId, Quantity = T.Quantity, PricePerUnit = PriceUnit, TaxId = T.TaxId, TaxValue = TaxRate, OrdersId = OrdersId, TotalPrice = TotalPrice, PlusTax = PlusTax, TotalAll = TotalAll, CompanyId = CompanyId, MeasureId = T.MeasureId });

        }

        public async Task Update(PurchaseOrderUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "PurchaseOrder");
            prm.Add("@id", T.id);
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@ExpectedDate", T.ExpectedDate);
            prm.Add("@OrderName", T.OrderName);
            prm.Add("@Info", T.Info);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@LocationId", T.LocationId);
            prm.Add("@TotalAll", T.TotalAll);

            await _db.ExecuteAsync($"Update Orders SET ContactId = @ContactId,TotalAll=@TotalAll,ExpectedDate=@ExpectedDate,OrderName=@OrderName,LocationId=@LocationId,Info=@Info where id=@id  and CompanyId = @CompanyId", prm);
        }

        public async Task UpdatePurchaseItem(PurchaseItem T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", T.id);
            prm.Add("@TaxId", T.TaxId);
            prm.Add("@ItemId", T.ItemId);

            var Rate = _db.Query<TaxClas>($"(select Rate from Tax where id =@TaxId and CompanyId=@CompanyId)", prm);
            float TaxRate = Rate.First().Rate;

            prm.Add("@CompanyId", CompanyId);
            prm.Add("@PricePerUnit", T.PricePerUnit);

            var TotalPrice = (T.Quantity * T.PricePerUnit); //adet*fiyat
            float? PlusTax = (TotalPrice * TaxRate) / 100; //tax fiyatı hesaplama
            var TotalAll = TotalPrice + PlusTax; //toplam fiyat hesaplama  

            prm.Add("@PlusTax", PlusTax);
            prm.Add("@Quantity", T.Quantity);
            prm.Add("@TaxId", T.TaxId);
            prm.Add("@TaxValue", TaxRate);
            prm.Add("@OrdersId", T.OrdersId);
            prm.Add("@TotalPrice", TotalPrice);
            prm.Add("@TotalAll", TotalAll);
            prm.Add("@MeasureId", T.MeasureId);

            await _db.ExecuteAsync($"Update OrdersItem SET TotalPrice = @TotalPrice,ItemId=@ItemId,TotalAll=@TotalAll,MeasureId=@MeasureId,PlusTax=@PlusTax,TaxValue=@TaxValue,TaxId=@TaxId,PricePerUnit=@PricePerUnit,Quantity=@Quantity where OrdersId = @OrdersId and CompanyId = @CompanyId and id=@id", prm);
        }
    }
}
