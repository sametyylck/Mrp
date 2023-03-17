using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using DAL.StockControl;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.BomDTO;
using static DAL.DTO.ItemDTO;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.ProductOperationsBomDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;

namespace DAL.Repositories
{
    public class SatısRepository : ISatısRepository
    {
        private readonly IDbConnection _db;
        private readonly IStockControl _control;
        private readonly IUretimRepository _manufacturingOrderItem;

        public SatısRepository(IDbConnection db, IStockControl control, IUretimRepository manufacturingOrderItem)
        {
            _db = db;
            _control = control;
            _manufacturingOrderItem = manufacturingOrderItem;
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
        public async Task<int> InsertPurchaseItem(SatısInsertItem T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            var liste = await _db.QueryAsync<LocaVarmı>($@"select 
            (select Rate from Tax where id=(select DefaultTaxPurchaseOrderId from GeneralDefaultSettings where CompanyId=@CompanyId))as Rate,
            (select DefaultPrice from Items where id =@ItemId and CompanyId=@CompanyId)as DefaultPrice", prm);
            prm.Add("@TaxId", T.TaxId);
            float rate = await _db.QueryFirstAsync<int>($"select  Rate from Tax where id =@TaxId and CompanyId=@CompanyId", prm);


            var PriceUnit = liste.First().VarsayilanFiyat;

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



            string sqla = $@"select
                (Select ISNULL(Tip,'') from Items where id = @ItemId and CompanyId = @CompanyId)as Tip,     
                (Select ISNULL(id,0) from LocationStock where ItemId =  @ItemId  and LocationId = @location and CompanyId = @CompanyId)   as    LocationStockId,
               (select ISNULL(SUM(ManufacturingOrder.PlannedQuantity),0) as Quantity from ManufacturingOrder where     ManufacturingOrder.ItemId=@ItemId and ManufacturingOrder.CompanyId=@CompanyId and   ManufacturingOrder.CustomerId=@ContactId )as ManufacturingQuantity";
            var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, prm);
            var Stock =await _control.Count(T.ItemId,  T.LocationId);
            var locationStockId = sorgu.First().LocationStockId;
            var tip = sorgu.First().Tip;
            prm.Add("@LocationStockId", locationStockId);
            int rezervid = 0;

            //STOKDA ürün var mı kontrol
            rezervid = await Control(T, T.SalesOrderId, tip, CompanyId);
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
                prm.Add("@Ingredient", 3);

                await _db.ExecuteAsync($"Update SalesOrder set TotalAll=@TotalAll where CompanyId=@CompanyId and id=@OrdersId", prm);


                int itemid = await _db.QuerySingleAsync<int>($"Insert into SalesOrderItem(ItemId,Quantity,PricePerUnit,TaxId,TaxValue,SalesOrderId,SalesItem,TotalPrice,PlusTax,TotalAll,Stance,Ingredients,CompanyId,Production) OUTPUT INSERTED.[id] values (@ItemId,@Quantity,@PricePerUnit,@TaxId,@TaxValue,@OrdersId,@SalesItem,@TotalPrice,@PlusTax,@TotalAll,@Stance,@Ingredient,@CompanyId,@Production)", prm);
                prm.Add("@SalesOrderItemId", itemid);
                prm.Add("@RezerveId", rezervid);

                await _db.ExecuteAsync($"Update Rezerve set SalesOrderItemId=@SalesOrderItemId where  CompanyId=@CompanyId and CustomerId=@ContactId and SalesOrderId=@OrdersId and LocationId=@location and id=@RezerveId  ", prm);
                return itemid;
            }
            if (tip!="Material")
            {
                await IngredientsControl(T, T.SalesOrderId, CompanyId);
                if (T.Conditions == 3)
                {
                    prm.Add("@Ingredient", 2);
                }
                else
                {
                    prm.Add("@Ingredient", 0);
                }
            }
            if (tip=="Material")
            {
                prm.Add("@Ingredient", 0);

            }
            string sqlquery = $@"select * from ManufacturingOrder where CustomerId is null and SalesOrderId is null and SalesOrderItemId is null and Status!=3 and Private='false' and IsActive=1 and ItemId=@ItemId and LocationId=@location Order by id DESC";
            var EmptyManufacturing = await _db.QueryAsync<SalesOrderUpdateMakeBatchItems>(sqlquery, prm);


       
            await _db.ExecuteAsync($"Update SalesOrder set TotalAll=@TotalAll where CompanyId=@CompanyId and id=@OrdersId", prm);
            prm.Add("@Production", 0);

            int id = await _db.QuerySingleAsync<int>($"Insert into SalesOrderItem(ItemId,Quantity,PricePerUnit,TaxId,TaxValue,SalesOrderId,TotalPrice,PlusTax,TotalAll,Stance,SalesItem,Ingredients,CompanyId,Production) OUTPUT INSERTED.[id] values (@ItemId,@Quantity,@PricePerUnit,@TaxId,@TaxValue,@OrdersId,@TotalPrice,@PlusTax,@TotalAll,@Stance,@SalesItem,@Ingredient,@CompanyId,@Production)", prm);

            prm.Add("@SalesOrderItemId", id);
            prm.Add("@id", T.SalesOrderId);
            if (tip!="Material")
            {
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
                    if (aranandeger == 0)
                    {

                    }
                    else
                    {
                        foreach (var item in EmptyManufacturing)
                        {
                            float toplamuretimadeti = item.PlannedQuantity;
                            if (varmi == 0)
                            {


                                if (toplamuretimadeti >= aranandeger)
                                {
                                    prm.Add("@SalesOrderId", T.SalesOrderId);
                                    prm.Add("@CompanyId", CompanyId);
                                    prm.Add("@SalesOrderItemId", id);
                                    prm.Add("@ItemId", T.ItemId);
                                    prm.Add("@ManufacturingOrderId", item.id);
                                    prm.Add("@ContactId", T.ContactId);

                                    prm.Add("@SalesItem", 2);
                                    await _db.ExecuteAsync($@"Update SalesOrderItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@SalesOrderItemId and SalesOrderId=@SalesOrderId", prm);
                                    await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=@ContactId,SalesOrderId=@SalesOrderId,SalesOrderItemId=@SalesOrderItemId where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);

                                    string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId  and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.CompanyId=@CompanyId)";
                                    var availability = await _db.QueryAsync<int>(sqlr, prm);
                                    prm.Add("@Ingredients", availability.First());
                                    await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                                    varmi++;

                                }
                                else if (toplamuretimadeti < aranandeger)
                                {

                                    prm.Add("@SalesOrderId", T.SalesOrderId);
                                    prm.Add("@CompanyId", CompanyId);
                                    prm.Add("@SalesOrderItemId", id);
                                    prm.Add("@ItemId", T.ItemId);
                                    prm.Add("@ManufacturingOrderId", item.id);
                                    prm.Add("@ContactId", T.ContactId);
                                    prm.Add("@SalesItem", 1);
                                    await _db.ExecuteAsync($@"Update SalesOrderItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@SalesOrderItemId and SalesOrderId=@SalesOrderId", prm);
                                    await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=@ContactId,SalesOrderId=@SalesOrderId,SalesOrderItemId=@SalesOrderItemId where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);

                                    string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId  and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.CompanyId=@CompanyId)";
                                    var availability = await _db.QueryAsync<int>(sqlr, prm);
                                    prm.Add("@Ingredients", availability.First());
                                    await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                                    aranandeger = aranandeger - toplamuretimadeti;


                                }
                            }


                        }
                    }



                }
            }
       
            await _db.ExecuteAsync($"Update Rezerve set SalesOrderItemId=@SalesOrderItemId where  CompanyId=@CompanyId and CustomerId=@ContactId and SalesOrderId=@OrdersId and LocationId=@location and SalesOrderItemId is null ", prm);
            return id;


        }
        public async Task<int> Control(SatısInsertItem T, int OrdersId, string? Tip, int CompanyId)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SalesOrderId", OrdersId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@location", T.LocationId);
            prm.Add("@Tip", Tip);

            //kullanılabilir stok sayisi
            var rezervecount = await _control.Count(T.ItemId, T.LocationId);
            rezervecount = rezervecount >= 0 ? rezervecount : 0;

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
            if (Tip=="Material")
            {
                prm.Add("@Status", 3);

            }
            else
            {
                prm.Add("@Status", 1);

            }

            return await _db.QuerySingleAsync<int>($"Insert into Rezerve (SalesOrderId,Tip,ItemId,RezerveCount,CustomerId,LocationId,Status,LocationStockCount,CompanyId) OUTPUT INSERTED.[id]  values (@SalesOrderId,@Tip,@ItemId,@RezerveCount,@ContactId,@location,@Status,@LocationStockCount,@CompanyId)", prm);
        }
        public async Task IngredientsControl(SatısInsertItem T, int OrdersId, int CompanyId)
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
                prm2.Add("@ItemId", item.MalzemeId);
                prm2.Add("@location", T.LocationId);
                string sqlb = $@"select
                (Select ISNULL(Tip,'') from Items where id = @ItemId and CompanyId = @CompanyId)as Tip,
                 (Select ISNULL(id,0) from LocationStock where ItemId = @ItemId  and LocationId = @location and CompanyId = @CompanyId) as     LocationStockId ";
                var sorgu1 = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqlb, prm2);
                prm2.Add("@LocationStockId", sorgu1.First().LocationStockId);
                prm2.Add("@stockId", sorgu1.First().StockId);

                var RezerveCount = await _control.Count(item.MalzemeId, T.LocationId);//stocktaki adet
                RezerveCount = RezerveCount >= 0 ? RezerveCount : 0;

                var stokcontrol = T.Quantity * item.Miktar; //bir materialin kaç tane gideceği hesaplanıyor
                if (RezerveCount >= stokcontrol) //yeterli stok var mı
                {
                    var yenistockdeğeri = RezerveCount - stokcontrol;
                    var Rezerve = stokcontrol;
                    prm2.Add("@RezerveCount", Rezerve);
                    prm2.Add("@LocationStockCount", yenistockdeğeri);


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


            if (location.First() == T.LocationId)
            {
                await _db.ExecuteAsync($"Update SalesOrder set ContactId=@ContactId,DeliveryDeadLine=@DeliveryDeadLine,CreateDate=@CreateDate,OrderName=@OrderName,Info=@Info,LocationId=@LocationId,TotalAll=@Total where CompanyId=@CompanyId and id=@id", prm);
            }
            else
            {
                await _db.ExecuteAsync($"Update SalesOrder set ContactId=@ContactId,DeliveryDeadLine=@DeliveryDeadLine,CreateDate=@CreateDate,OrderName=@OrderName,Info=@Info,LocationId=@LocationId,TotalAll=@Total where CompanyId=@CompanyId and id=@id", prm);

                List<Manufacturing> ItemsCount = (await _db.QueryAsync<Manufacturing>($"select ItemId,RezerveCount from Rezerve where SalesOrderId=@id and CompanyId=@CompanyId and Status=1", prm)).ToList();
                foreach (var item in ItemsCount)
                {
                    prm.Add("@ItemId", item.StokId);
                    await _db.ExecuteAsync($"Delete from Rezerve where SalesOrderId=@id and CompanyId=@CompanyId and ItemId=@ItemId", prm);

                }
                List<ManufacturingOrderItemsIngredientsUpdate> Itemdegerler = (await _db.QueryAsync<ManufacturingOrderItemsIngredientsUpdate>($@"select ItemId,Quantity,SalesOrderItem.id from SalesOrderItem 
                inner join SalesOrder on SalesOrder.id=SalesOrderItem.SalesOrderId
                where SalesOrderItem.CompanyId=@CompanyId and SalesOrderItem.SalesOrderId=@id and SalesOrder.IsActive=1 and SalesOrder.DeliveryId!=4", prm)).ToList();
                foreach (var item in Itemdegerler)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@ItemId", item.StokId);
                    param.Add("@CompanyId", CompanyId);
                    param.Add("@location", T.LocationId);
                    param.Add("@id", T.id);
                    param.Add("@OrderItemId", item.id);
                    param.Add("@ContactId", T.ContactId);

                    string sqla = $@"select
                    (Select ISNULL(Tip,'') from Items where id = @ItemId and CompanyId = @CompanyId)as Tip,
                    (Select ISNULL(id,0) from LocationStock where ItemId=@ItemId and LocationId = @location and CompanyId = @CompanyId)   as    LocationStockId,
                     (select ISNULL(SUM(ManufacturingOrder.PlannedQuantity),0) as Quantity from ManufacturingOrder where  ManufacturingOrder.ItemId=@ItemId and ManufacturingOrder.CompanyId=@CompanyId and   ManufacturingOrder.CustomerId=@ContactId )as ManufacturingQuantity";
                    var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, param);
                    var RezerveCount = await _control.Count(item.StokId, T.LocationId);
                    RezerveCount = RezerveCount >= 0 ? RezerveCount : 0;

                    var locationStockId = sorgu.First().LocationStockId;
                    var tip = sorgu.First().Tip;
                    param.Add("@LocationStockId", locationStockId);
                    var stokkontrol = await _control.Count(item.StokId, T.LocationId);
                    int rezervid = 0;
                    SatısInsertItem A = new SatısInsertItem();
                    A.ItemId = item.StokId;
                    A.LocationId = T.LocationId;
                    A.ContactId = T.ContactId;
                    A.Quantity = item.Miktar;
                    if (RezerveCount > 0)
                    {


                        rezervid = await Control(A, T.id, tip, CompanyId);
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


                        await _db.ExecuteAsync($"Update SalesOrderItem set SalesItem=@SalesItem,Production=@Production,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId", param);

                        param.Add("@RezerveId", rezervid);

                        await _db.ExecuteAsync($"Update Rezerve set SalesOrderItemId=@OrderItemId where  CompanyId=@CompanyId and CustomerId=@ContactId and SalesOrderId=@id and LocationId=@location and id=@RezerveId ", param);

                    }
                    else
                    {


                        await IngredientsControl(A, T.id, CompanyId);
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
        }
        public async Task UpdateItems(SatısUpdateItems T, int CompanyId)
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
            var make = await _db.QueryAsync<SalesOrderRezerve>($"select mo.id from ManufacturingOrder mo where SalesOrderId=@id and SalesOrderItemId=@OrderItemId and CompanyId=@CompanyId and IsActive=1 and Status!=3", prm);
            var adetbul = await _db.QueryFirstAsync<float>($"Select Quantity From SalesOrderItem where CompanyId = {CompanyId} and id =@OrderItemId and SalesOrderId=@id", prm);
            float eski = adetbul;
            string sqlv = $@"Select ItemId  from  SalesOrderItem where CompanyId=@CompanyId and id=@OrderItemId";
            var Item = await _db.QuerySingleAsync<int>(sqlv, prm);
            if (T.ItemId != Item)
            {
                if (T.Tip=="Material")
                {
                    prm.Add("@Status", 3);

                }
                else
                {
                    prm.Add("@Status", 1);

                }
                List<Manufacturing> ItemsCount = (await _db.QueryAsync<Manufacturing>($"select ItemId,RezerveCount from Rezerve where SalesOrderId=@id and CompanyId=@CompanyId and SalesOrderItemId=@OrderItemId  and Status=@Status", prm)).ToList();
                foreach (var item in ItemsCount)
                {
                    prm.Add("@ItemId", item.StokId);
                    await _db.ExecuteAsync($"Delete from  Rezerve where SalesOrderId=@id and SalesOrderItemId=@OrderItemId and CompanyId=@CompanyId and ItemId=@ItemId", prm);
                }
                var liste = await _db.QueryAsync<LocaVarmı>($@"select 
            (select Rate from Tax where id=(select DefaultTaxPurchaseOrderId from GeneralDefaultSettings where CompanyId=@CompanyId))as Rate,
            (select DefaultPrice from Items where id =@ItemId and CompanyId=@CompanyId)as DefaultPrice", prm);
                prm.Add("@TaxId", T.TaxId);
                var Birimfiyat = liste.First().VarsayilanFiyat;
                T.PricePerUnit = Birimfiyat;

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

            if (makeorderId == 0 && T.Tip!="Material")
            {
                int degerler;
                string sqlp = $" select ISNULL(RezerveCount,0) from Rezerve where SalesOrderId=@id and SalesOrderItemId=@OrderItemId and LocationId=@location and ItemId=@ItemId and CompanyId=@CompanyId and Status=1";
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
                await _db.ExecuteAsync($"Update SalesOrderItem set Tip=@Null where CompanyId=@CompanyId and id=@OrderItemId and SalesOrderId=@id", prm);

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
                prm.Add("@OrdersId", T.SalesOrderId);
                prm.Add("@PlusTax", PlusTax);
                prm.Add("@TotalPrice", totalprice);
                prm.Add("@TotalAll", total);
                prm.Add("@ContactsId", T.ContactId);
                string sqla = $@"select
                  (Select ISNULL(Tip,'') from Items where id = @ItemId and CompanyId = @CompanyId)as Tip,
                  (Select ISNULL(id,0) from LocationStock where ItemId = @ItemId  and LocationId = @location and CompanyId = @CompanyId)   as    LocationStockId ";
                var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, prm);
                var RezerveCount = await _control.Count(T.ItemId, T.LocationId);
                RezerveCount = RezerveCount >= 0 ? RezerveCount : 0;
                var locationStockId = sorgu.First().LocationStockId;
                var tip = sorgu.First().Tip;
                prm.Add("@LocationStockId", locationStockId);
                var status = 0;


                if (degerler >= T.Quantity)
                {
                    prm.Add("@RezerveCount", T.Quantity);
                    prm.Add("@LocationStockCount", RezerveCount);
                    prm.Add("@SalesItem", 3);
                    status = 3;
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and SalesOrderId=@id and ItemId=@ItemId and SalesOrderItemId=@OrderItemId and Status=1", prm);
                }

                else if (RezerveCount >= T.Quantity - degerler && RezerveCount > 0)
                {
                    prm.Add("@RezerveCount", T.Quantity);
                    prm.Add("@LocationStockCount", RezerveCount);
                    prm.Add("@SalesItem", 3);
                    status = 3;
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and SalesOrderId=@id and ItemId=@ItemId and SalesOrderItemId=@OrderItemId and Status=1", prm);
                }
                else
                {
                    prm.Add("@RezerveCount", degerler + RezerveCount);
                    prm.Add("@LocationStockCount", RezerveCount);
                    prm.Add("@SalesItem", 1);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and SalesOrderId=@id and ItemId=@ItemId and SalesOrderItemId=@OrderItemId and Status=1", prm);
                }



                if (status == 3)
                {
                    prm.Add("@SalesItem", 3);
                    prm.Add("@Production", 4);

                    await _db.ExecuteAsync($"Update SalesOrder set TotalAll=@TotalAll where CompanyId=@CompanyId and id=@OrdersId", prm);
                    prm.Add("@Ingredient", 3);
                    await _db.ExecuteAsync($"Update SalesOrderItem set ItemId=@ItemId,Quantity=@Quantity,PricePerUnit=@PricePerUnit,TaxId=@TaxId,TaxValue=@TaxValue,SalesOrderId=@id,TotalPrice=@TotalPrice,PlusTax=@PlusTax,TotalAll=@TotalAll,SalesItem=@SalesItem,Ingredients=@Ingredient,Production=@Production where CompanyId=@CompanyId and id=@OrderItemId and SalesOrderId=@id ", prm);
                }
                else
                {
                    await UpdateIngredientsControl(T, T.SalesOrderId, CompanyId);
                    if (T.Conditions == 3)
                    {
                        prm.Add("@Ingredient", 2);
                    }
                    else
                    {
                        prm.Add("@Ingredient", 0);
                    }
                }

                await _db.ExecuteAsync($"Update SalesOrder set TotalAll=@TotalAll where CompanyId=@CompanyId and id=@OrdersId", prm);

                await _db.ExecuteAsync($@"Update SalesOrderItem set ItemId=@ItemId,Quantity=@Quantity,PricePerUnit=@PricePerUnit,TaxId=@TaxId,TaxValue=@TaxValue,TotalPrice=@TotalPrice,PlusTax=@PlusTax,
                TotalAll=@TotalAll,SalesItem=@SalesItem,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId and SalesOrderId=@id", prm);



            }
            else
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
                prm.Add("@OrdersId", T.SalesOrderId);
                prm.Add("@PlusTax", PlusTax);
                prm.Add("@TotalPrice", totalprice);
                prm.Add("@TotalAll", total);
                prm.Add("@ContactsId", T.ContactId);
                if (T.Tip=="MakeBatch")
                {
                    await _db.ExecuteAsync($@"Update SalesOrderItem set Quantity=@Quantity,TotalAll=@TotalAll,PricePerUnit=@PricePerUnit,TaxId=@TaxId,TaxValue=@TaxValue where CompanyId=@CompanyId and id=@OrderItemId and SalesOrderId=@id", prm);
                    await UpdateMakeBatchItems(T, CompanyId, eski);
                }
                else if (T.Tip == "MakeOrder")
                {
                    prm.Add("@SalesItem", 2);
                    await _db.ExecuteAsync($@"Update SalesOrderItem set Quantity=@Quantity,TotalAll=@TotalAll,PricePerUnit=@PricePerUnit,SalesItem=@SalesItem,TaxId=@TaxId,TaxValue=@TaxValue where CompanyId=@CompanyId and id=@OrderItemId and SalesOrderId=@id", prm);
                    await UpdateMakeItems(T, eski, CompanyId);
                }
                else if (T.Tip == "Material")
                {
                    await _db.ExecuteAsync($@"Update SalesOrderItem set Quantity=@Quantity,TotalAll=@TotalAll,PricePerUnit=@PricePerUnit,TaxId=@TaxId,TaxValue=@TaxValue where CompanyId=@CompanyId and id=@OrderItemId and SalesOrderId=@id", prm);
                    var StokMiktari = await _control.Count(T.ItemId,T.LocationId);
                    StokMiktari = StokMiktari >= 0 ? StokMiktari : 0;
                    //stocktaki adet

                    string sqlp = $" select id,ISNULL(RezerveCount,0) as RezerveCount from Rezerve where SalesOrderId=@OrdersId and SalesOrderItemId=@OrderItemId and LocationId=@location and ItemId=@ItemId and CompanyId=@CompanyId and Status=3";
                    var deger = await _db.QueryAsync<LocaVarmı>(sqlp, prm);
                    int rezerveid = deger.First().id;
                    prm.Add("@rezerveid", rezerveid);

                    float? rezervecount = deger.First().RezerveDeger;

                    string sqlb = $@"select ISNULL(SUM(Quantity),0) from Orders 
                left join OrdersItem on OrdersItem.OrdersId = Orders.id and Orders.LocationId=@location
                and OrdersItem.ItemId = @ItemId where Orders.CompanyId = @CompanyId
                and DeliveryId = 1 and Orders.ManufacturingOrderId is null and Orders.SalesOrderId is null  and Orders.IsActive=1";
                    var expected = await _db.QueryFirstAsync<float>(sqlb,prm);

                    if (T.Quantity<=rezervecount)
                    {
                        prm.Add("@SalesItem", 3);
                        prm.Add("@Production", 4);
                        prm.Add("@Ingredient", 3);
                        prm.Add("@RezerveCount", T.Quantity);

                        await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and id=@rezerveid ", prm);


                    }
                    else if (StokMiktari>=T.Quantity-rezervecount)
                    {
                        prm.Add("@SalesItem", 3);
                        prm.Add("@Production", 4);
                        prm.Add("@Ingredient", 3);

                        prm.Add("@RezerveCount", T.Quantity);
                        prm.Add("@LocationStockCount", StokMiktari-T.Quantity);

                        await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount ,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and id=@rezerveid ", prm);

                    }
                    else if (expected>=T.Quantity-rezervecount)
                    {
                        prm.Add("@SalesItem", 2);
                        prm.Add("@Production", 3);
                        prm.Add("@Ingredient", 1);

                    }
                    else
                    {
                        prm.Add("@SalesItem", 1);
                        prm.Add("@Production", 0);
                        prm.Add("@Ingredient", 0);

                    }
                    await _db.ExecuteAsync($@"Update SalesOrderItem set Quantity=@Quantity,TotalAll=@TotalAll,ItemId=@ItemId,PricePerUnit=@PricePerUnit,SalesItem=@SalesItem,Production=@Production,Ingredients=@Ingredient,TaxId=@TaxId,TaxValue=@TaxValue where CompanyId=@CompanyId and id=@OrderItemId and SalesOrderId=@id", prm);

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
        public async Task<int> InsertAddress(SalesOrderCloneAddress A,  int id)
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
            prm.Add("@IsActive", true);
            int adressid = 0;
            if (A.Tip == "BillingAddress")
            {
                adressid = await _db.QuerySingleAsync<int>($"Insert into Locations (Tip,FirstName,LastName,CompanyName,Phone, AddressLine1,AddressLine2,CityTown,StateRegion,ZipPostalCode,Country,CompanyId,IsActive)  OUTPUT INSERTED.[id]  values (@Tip,@FirstName,@LastName,@CompanyName,@Phone,@AddressLine1,@AddressLine2,@CityTown,@StateRegion,@ZipPostal,@Country,@CompanyId,@IsActive)", prm);
            }
            else if (A.Tip == "ShippingAddress")
            {
                adressid = await _db.QuerySingleAsync<int>($"Insert into Locations (Tip,FirstName,LastName,CompanyName,Phone, AddressLine1,AddressLine2,CityTown,StateRegion,ZipPostalCode,Country,CompanyId,IsActive)  OUTPUT INSERTED.[id]  values (@Tip,@FirstName,@LastName,@CompanyName,@Phone,@AddressLine1,@AddressLine2,@CityTown,@StateRegion,@ZipPostal,@Country,@CompanyId,@IsActive)", prm);
            }
            return adressid;

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

                await _db.ExecuteAsync($"Delete from  Rezerve  where SalesOrderId=@OrdersId and CompanyId=@CompanyId and SalesOrderItemId=@id and ItemId=@ItemId", prm);

                List<ManufacturingOrderA> MANU = (await _db.QueryAsync<ManufacturingOrderA>($@"select ManufacturingOrder.id from ManufacturingOrder 
                where  ManufacturingOrder.SalesOrderId=@OrdersId and ManufacturingOrder.SalesOrderItemId=@id and ManufacturingOrder.ItemId=@ItemId and ManufacturingOrder.IsActive=1 and ManufacturingOrder.CompanyId=@CompanyId ", prm)).ToList();
                int varmı = 1;
                if (MANU.Count() == 0)
                {
                    varmı = 0;
                }


                var status = await _db.QueryFirstAsync<int>($@"select SalesOrderItem.SalesItem from SalesOrderItem 
                    where SalesOrderItem.SalesOrderId=@OrdersId and SalesOrderItem.id=@id and CompanyId=@CompanyId", prm);
                await _db.ExecuteAsync($"Delete From SalesOrderItem  where ItemId = @ItemId and CompanyId = @CompanyId and id=@id and SalesOrderId=@OrdersId", prm);
                if (varmı == 0)
                {

                    if (status != 3)
                    {
                        var BomList = await _db.QueryAsync<BOM>($"Select * From Bom where CompanyId =@CompanyId and ProductId =@ItemId  and IsActive = 1", prm);
                        foreach (var item in BomList)
                        {
                            prm.Add("@MaterialId", item.MalzemeId);

                            prm.Add("@Status", 4);
                            await _db.ExecuteAsync($"Delete from  Rezerve where SalesOrderId=@OrdersId and SalesOrderItemId=@id and CompanyId=@CompanyId and ItemId=@MaterialId", prm);

                        }
                        await _db.ExecuteAsync($"Delete from SalesOrderItem where id=@id and SalesOrderId=@OrdersId and CompanyId=@CompanyId", prm);

                    }

                }
                else
                {
                    await _db.ExecuteAsync($"Delete from SalesOrderItem where id=@id and SalesOrderId=@OrdersId and CompanyId=@CompanyId", prm);
                }

            }
            else
            {
                await _db.ExecuteAsync($"Delete From SalesOrderItem  where  CompanyId = @CompanyId and id=@id and SalesOrderId=@OrdersId", prm);

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
                    List<OperationsUpdate> MANU = (await _db.QueryAsync<OperationsUpdate>($"select ManufacturingOrder.id from ManufacturingOrder where  SalesOrderId=@id and IsActive=1 and CompanyId=@CompanyId", param)).ToList();
                    foreach (var item in MANU)
                    {
                        param.Add("@manuid", item.id);
                        await _db.ExecuteAsync($"Update ManufacturingOrder Set SalesOrderId=NULL , SalesOrderItemId=NULL , CustomerId=NULL where id = @manuid and CompanyId = @CompanyId ", param);
                        await _db.ExecuteAsync($"Update Rezerve Set SalesOrderId=NULL , SalesOrderItemId=NULL , CustomerId=NULL where ManufacturingOrderId = @manuid and CompanyId = @CompanyId ", param);
                    }
                    var order = await _db.QueryAsync<OperationsUpdate>($"select Orders.id from Orders where  SalesOrderId=@id and IsActive=1 and CompanyId=@CompanyId", param);
                    foreach (var item in order)
                    {
                        param.Add("@orderid", item.id);

                        await _db.ExecuteAsync($"Update Order Set SalesOrderId=NULL , SalesOrderItemId=NULL  where id = @orderid and CompanyId = @CompanyId ", param);

                    }

                    List<Manufacturing> ItemsCount = (await _db.QueryAsync<Manufacturing>($"select ItemId,RezerveCount from Rezerve where SalesOrderId=@id and CompanyId=@CompanyId and Status=1", param)).ToList();
                    await _db.ExecuteAsync($"Update SalesOrder Set IsActive=@IsActive,DeleteDate=@Date,DeletedUser=@User where id = @id and CompanyId = @CompanyId ", param);
                    foreach (var item in ItemsCount)
                    {
                        param.Add("@ItemId", item.StokId);
                        await _db.ExecuteAsync($"Delete from  Rezerve where SalesOrderId=@id and CompanyId=@CompanyId and ItemId=@ItemId", param);
                    }
                }

            }
         
        }

        public async Task UpdateMakeItems(SatısUpdateItems T, float eski, int CompanyId)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.SalesOrderId);
            prm.Add("@ManufacturingOrderId", T.ManufacturingOrderId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@OrderItemId", T.id);
            prm.Add("@ItemsId", T.ItemId);


            var LocationId = await _db.QueryFirstAsync<int>($"Select LocationId From SalesOrder where CompanyId =@CompanyId and id =@id", prm);
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
                param.Add("@id", T.SalesOrderId);
                param.Add("@CompanyId", CompanyId);
                param.Add("@OrderItemId", T.id);
                param.Add("@ItemId", item.MalzemeId);
                param.Add("@Notes", item.Bilgi);
                if (adet == eski)
                {
                    param.Add("@PlannedQuantity", item.Miktar);
                }
                else if (adet > eski)
                {
                    float anadeger = item.Miktar / eski;
                    float yenideger = adet - eski;
                    var artışdegeri = yenideger * anadeger;
                    item.Miktar = item.Miktar + artışdegeri;
                    param.Add("@PlannedQuantity", item.Miktar);
                }
                else
                {
                    var yenideger = item.Miktar / eski;
                    var deger = eski - adet;
                    item.Miktar = item.Miktar - (yenideger * deger);
                    param.Add("@PlannedQuantity", item.Miktar);
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
                float DefaultPrice = sorgu.First().VarsayilanFiyat;
                param.Add("@Cost", DefaultPrice * item.Miktar);
                param.Add("@LocationStockId", sorgu.First().DepoStokId);

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
            ,LocationStock.StockCount,moi.PlannedQuantity,Rezerve.RezerveCount";
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


                float RezerveStockCount = await _control.Count(item.MalzemeId, LocationId);
                RezerveStockCount = RezerveStockCount >= 0 ? RezerveStockCount : 0;
                List<int> Count = (await _db.QueryAsync<int>($"Select ISNULL(Rezerve.RezerveCount,0)as Count from Rezerve where CompanyId=@CompanyId and ItemId=@ItemId and ManufacturingOrderId=@ManufacturingOrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and ItemId=@ItemId and SalesOrderId=@id and SalesOrderItemId=@OrderItemId and Rezerve.Status=1", param)).ToList();
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
                if (Counts >= item.Miktar)
                {
                    newQuantity = Count[0];
                    param.Add("@RezerveCount", newQuantity);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                    param.Add("@LocationStockCount", RezerveStockCount);
                    param.Add("@Availability", 2);
                }

                else if (RezerveStockCount >= item.Miktar - Counts)
                {

                    param.Add("@RezerveCount", item.Miktar);
                    param.Add("@LocationStockCount", RezerveStockCount);
                    param.Add("@Availability", 2);
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
                        param.Add("@RezerveCount", RezerveStockCount+Counts);

                    }
                    else
                    {
                        param.Add("@Availability", 0);
                        param.Add("@RezerveCount", RezerveStockCount+ Counts);

                    }

                }

                param.Add("@Status", 1);

                await _db.ExecuteAsync($"Update Rezerve set  Tip=@Tip,ItemId=@ItemId,RezerveCount=@RezerveCount,LocationId=@LocationId,Status=@Status,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and ManufacturingOrderId=@ManufacturingOrderId and ItemId=@ItemId and ManufacturingOrderItemId=@ManufacturingOrderItemId and SalesOrderId=@id and SalesOrderItemId=@OrderItemId ", param);





                string sqlw = $@"Update ManufacturingOrderItems Set Tip=@Tip,ItemId=@ItemId,Notes=@Notes,PlannedQuantity=@PlannedQuantity,Cost=@Cost,Availability=@Availability where CompanyId=@CompanyId and OrderId=@ManufacturingOrderId and ItemId=@ItemId and id=@ManufacturingOrderItemId  ";
                await _db.ExecuteAsync(sqlw, param);

                if (T.SalesOrderId != 0 || T.SalesOrderId != null)
                {
                    prm.Add("@OrderId", T.SalesOrderId);
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@ItemId", T.ItemId);
                    prm.Add("@SalesOrderId", T.id);
                    prm.Add("@SalesOrderItemId", T.id);
                    prm.Add("@ManufacturingOrderItemId", item.id);

                    string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId and ManufacturingOrderItems.id=@ManufacturingOrderItemId and ManufacturingOrderItems.Tip='Ingredients')";
                    var availability = await _db.QueryAsync<int>(sqlr, prm);
                    prm.Add("@Ingredients", availability.First());
                    await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                }

            }








            //Eklenen Ordera ait ıtemin Operation Bomlarını buluyoruz
            var OperationList = await _db.QueryAsync<ProductOperationsBOM>($"Select ISNULL(id,0)As id,ISNULL(OperationId,0) as OperationId,ISNULL(ResourceId,0)as ResourceId,ISNULL(CostPerHour,0)as CostHour,ISNULL(PlannedTime,0)as OperationTime  from ManufacturingOrderItems where CompanyId = {CompanyId} and ManufacturingOrderItems.OrderId = {T.SalesOrderId} and Tip = 'Operations'");

            foreach (var item in OperationList)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Operations");
                param.Add("@OrderId", T.SalesOrderId);
                param.Add("@OperationId", item.OperasyonId);
                param.Add("@ResourceId", item.KaynakId);
                if (adet == eski)
                {
                    param.Add("@PlannedQuantity", item.OperasyonZamani);
                }
                else if (adet > eski)
                {
                    float yenideger = adet - eski;
                    float artışdegeri = yenideger * item.OperasyonZamani;
                    item.OperasyonZamani = item.OperasyonZamani + artışdegeri;

                }
                else
                {
                    float yenideger = item.OperasyonZamani / eski;
                    float deger = eski - adet;
                    item.OperasyonZamani = item.OperasyonZamani - (yenideger * deger);

                }

                param.Add("@PlannedTime ", item.OperasyonZamani);
                param.Add("@Status", 0);
                param.Add("@Cost", (item.SaatlikUcret / 60 / 60) * item.OperasyonZamani);
                param.Add("@CompanyId", CompanyId);

                string sql = $@"Update ManufacturingOrderItems Set Tip=@Tip,OrderId=@OrderId,OperationId=@OperationId,ResourceId=@ResourceId,PlannedTime=@PlannedTime,Status=@Status,Cost=@Cost where CompanyId=@CompanyId and OrderId=@OrderId and OperationId=@OperationId and ResourceId=ResourceId  ";
                await _db.ExecuteAsync(sql, param);
            }
        }
        public async Task UpdateMakeBatchItems(SatısUpdateItems T, int CompanyId, float eskiQuantity)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.SalesOrderId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@OrderItemId", T.id);
            prm.Add("@ItemId", T.ItemId);
            var LocationIdAl = await _db.QueryFirstAsync<int>($"Select LocationId From SalesOrder where CompanyId =@CompanyId and id =@id", prm);
            int LocationId = LocationIdAl;
            prm.Add("@LocationId", LocationId);

            var status = await _db.QueryFirstAsync<int>($@"select SalesOrderItem.SalesItem from SalesOrderItem 
                    where SalesOrderItem.SalesOrderId=@id and SalesOrderItem.id=@OrderItemId and CompanyId=@CompanyId", prm);
            int statusId = status;
            prm.Add("@ItemsId", T.ItemId);


            //Adet azaltılırken hangi üretimin olacağı belirlenecek
            string sqlsorgu = $@"select ManufacturingOrder.id,ManufacturingOrder.PlannedQuantity from ManufacturingOrder 
            where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.SalesOrderId=@id  
            and ManufacturingOrder.SalesOrderItemId=@OrderItemId and ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1 and ManufacturingOrder.Private='false' and ManufacturingOrder.ItemId=@ItemId  Order by id DESC ";
            var Manufacturing = await _db.QueryAsync<SalesOrderUpdateMakeBatchItems>(sqlsorgu, prm);


            string sqlquerys = $@"select SUM(ISNULL(ManufacturingOrder.PlannedQuantity,0)) from ManufacturingOrder 
            where ManufacturingOrder.CompanyId=@CompanyId and ManufacturingOrder.SalesOrderId=@id  
            and ManufacturingOrder.SalesOrderItemId=@OrderItemId and ManufacturingOrder.Status!=3 and ManufacturingOrder.IsActive=1 and ManufacturingOrder.Private='false' and ManufacturingOrder.ItemId=@ItemId";
            var expected = await _db.QueryFirstAsync<int>(sqlquerys, prm);

            //adet yükseltilirken stokta yok ise boşta bir üretim var ise onu alır.
            string sqlquery = $@"select * from ManufacturingOrder where CustomerId is null and SalesOrderId is null and SalesOrderItemId is null and Status!=3 and Private='false' and IsActive=1 and ItemId=@ItemsId Order by id DESC";
            var EmptyManufacturing = await _db.QueryAsync<SalesOrderUpdateMakeBatchItems>(sqlquery, prm);

            string sqlb = $@"Select ISNULL(id,0) from LocationStock where ItemId =@ItemId and LocationId = @LocationId and CompanyId = @CompanyId";
            var locationstockid = await _db.QueryFirstAsync<int>(sqlb, prm);
            prm.Add("@LocationStockId", locationstockid);
            float degerler;
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

            var RezerveCount = await _control.Count(T.ItemId, LocationId);//stocktaki adet
            RezerveCount = RezerveCount >= 0 ? RezerveCount : 0;

            if (degerler > T.Quantity)
            {
                prm.Add("@SalesItem", 3);
                prm.Add("@Ingredient", 3);
                prm.Add("@ProductionId", 4);
                prm.Add("@RezerveCount", T.Quantity);
                prm.Add("@LocationStockCount", RezerveCount);
                await _db.ExecuteAsync($@"Update SalesOrderItem set SalesItem=@SalesItem,Production=@ProductionId,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);
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
            else if (RezerveCount >= T.Quantity - degerler)
            {
                prm.Add("@SalesItem", 3);
                prm.Add("@Ingredient", 3);
                prm.Add("@ProductionId", 4);
                prm.Add("@RezerveCount", T.Quantity);
                prm.Add("@LocationStockCount", RezerveCount);
                await _db.ExecuteAsync($@"Update SalesOrderItem set SalesItem=@SalesItem,Production=@ProductionId,Ingredients=@Ingredient where CompanyId=@CompanyId and id=@OrderItemId and OrdersId=@id", prm);
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
                var uretimfarki = eskiQuantity - expected - degerler;
                uretimfarki = Math.Abs(uretimfarki);

                float dusuculecekdeger = eskiQuantity - T.Quantity;
                foreach (var item in Manufacturing)
                {
                    prm.Add("@ManufacturingOrderId", item.id);
                    toplamuretimadeti = item.PlannedQuantity;
                    if (varmi == 0)
                    {

                        if (toplamuretimadeti <= dusuculecekdeger)
                        {
                            await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=NULL ,SalesOrderId=NULL,SalesOrderItemId=NULL where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);

                        }


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
                            await _db.ExecuteAsync($@"Update SalesOrderItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@OrderItemId and SalesOrderId=@id", prm);

                            prm.Add("@SalesOrderId", T.SalesOrderId);
                            prm.Add("@CompanyId", CompanyId);
                            prm.Add("@SalesOrderItemId", T.id);
                            prm.Add("@ItemId", T.ItemId);
                            prm.Add("@ManufacturingOrderId", item.id);
                            prm.Add("@ContactId", T.ContactId);
                            await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=@ContactId,SalesOrderId=@SalesOrderId,SalesOrderItemId=@SalesOrderItemId where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);

                            string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId  and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.CompanyId=@CompanyId)";
                            var availability = await _db.QueryAsync<int>(sqlr, prm);
                            prm.Add("@Ingredients", availability.First());
                            await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);

                            varmi++;

                        }
                        else if (toplamuretimadeti < aranandeger)
                        {
                            prm.Add("@SalesItem", 1);
                            await _db.ExecuteAsync($@"Update SalesOrderItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@OrderItemId and SalesOrderId=@id", prm);
                            prm.Add("@SalesOrderId", T.SalesOrderId);
                            prm.Add("@CompanyId", CompanyId);
                            prm.Add("@SalesOrderItemId", T.id);
                            prm.Add("@ItemId", T.ItemId);
                            prm.Add("@ManufacturingOrderId", item.id);
                            prm.Add("@ContactId", T.ContactId);
                            await _db.ExecuteAsync($@"Update ManufacturingOrder set CustomerId=@ContactId,SalesOrderId=@SalesOrderId,SalesOrderItemId=@SalesOrderItemId where CompanyId=@CompanyId and id=@ManufacturingOrderId", prm);

                            string sqlr = $@"(select MIN(ManufacturingOrderItems.Availability)as Ingredients from ManufacturingOrderItems
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ManufacturingOrderId  and ManufacturingOrderItems.Tip='Ingredients' and ManufacturingOrderItems.CompanyId=@CompanyId)";
                            var availability = await _db.QueryAsync<int>(sqlr, prm);
                            prm.Add("@Ingredients", availability.First());
                            await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                            aranandeger = aranandeger - toplamuretimadeti;


                        }
                        else
                        {
                            prm.Add("@SalesItem", 1);
                            await _db.ExecuteAsync($@"Update SalesOrderItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@OrderItemId and SalesOrderId=@id", prm);
                            await UpdateIngredientsControl(T, T.id, CompanyId);
                        }
                    }





                }

               
            }
            else
            {
                prm.Add("@SalesItem", 1);
                await _db.ExecuteAsync($@"Update SalesOrderItem set SalesItem=@SalesItem where CompanyId=@CompanyId and id=@OrderItemId and SalesOrderId=@id", prm);
                await UpdateIngredientsControl(T, T.id, CompanyId);
            }





        }
        public async Task UpdateIngredientsControl(SatısUpdateItems T, int OrdersId, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SalesOrderId", OrdersId);
            prm.Add("@OrderItemId", T.id);
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
                prm2.Add("@ItemId", item.MalzemeId);
                prm2.Add("@SalesOrderId", OrdersId);
                prm2.Add("@OrderItemId", T.id);
                prm2.Add("@location", T.LocationId);
                string sqlb = $@"select
                (Select ISNULL(Tip,'') from Items where id = @ItemId and CompanyId = @CompanyId)as Tip,
                 (Select ISNULL(id,0) from LocationStock where ItemId =  @ItemId and LocationId = @location and CompanyId = @CompanyId) as     LocationStockId";
                var sorgu1 = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqlb, prm2);
                int degerler;
                string sqlp = $" select ISNULL(RezerveCount,0) from Rezerve where SalesOrderId=@SalesOrderId and SalesOrderItemId=@OrderItemId and LocationId=@location and ItemId=@ItemId and CompanyId=@CompanyId and Status=1";
                var deger = await _db.QueryAsync<int>(sqlp, prm2);
                if (deger.Count() == 0)
                {
                    degerler = 0;
                }
                else
                {
                    degerler = deger.First();
                }




                var RezerveCount = await _control.Count(item.MalzemeId, T.LocationId);
                RezerveCount = RezerveCount >= 0 ? RezerveCount : 0;
                //stocktaki adet
                var stokcontrol = T.Quantity * item.Miktar; //bir materialin kaç tane gideceği hesaplanıyor
                if (deger.Count() == 0)
                {


                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@ItemId", item.MalzemeId);
                    param2.Add("@location", T.LocationId);
                    if (RezerveCount >= stokcontrol) //yeterli stok var mı
                    {

                        prm2.Add("@RezerveCount", stokcontrol);
                        prm2.Add("@LocationStockCount", RezerveCount);



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
                    prm2.Add("@OrderItemId", T.id);
                    await _db.ExecuteAsync($"Insert into Rezerve (SalesOrderId,SalesOrderItemId,Tip,ItemId,RezerveCount,CustomerId,LocationId,Status,LocationStockCount,CompanyId) values (@OrdersId,@OrderItemId,@Tip,@ItemId,@RezerveCount,@ContactsId,@location,@Status,@LocationStockCount,@CompanyId)", prm2);




                }
                else
                {
                    if (degerler >= stokcontrol)
                    {
                        prm2.Add("@RezerveCount", stokcontrol);
                        prm2.Add("@LocationStockCount", RezerveCount);

                    }
                    else if (RezerveCount >= stokcontrol - degerler) //yeterli stok var mı
                    {
                        prm2.Add("@RezerveCount", stokcontrol);
                        prm2.Add("@LocationStockCount", RezerveCount);



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
                await _db.ExecuteAsync($"Update Rezerve set Tip=@Tip,RezerveCount=@RezerveCount,CustomerId=@ContactsId,LocationId=@location,Status=@Status,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and SalesOrderId=@OrdersId and ItemId=@ItemId and Status=1", prm2);



            }
            if (T.id != 0)
            {
                prm.Add("@OrderId", OrdersId);
                prm.Add("@CompanyId", CompanyId);
                prm.Add("@ItemId", T.ItemId);
                prm.Add("@SalesOrderId", T.SalesOrderId);
                prm.Add("@SalesOrderItemId", T.id);

                if (b > 0)
                {
                    prm.Add("@Ingredients", 0);
                }
                else
                {
                    prm.Add("@Ingredients", 2);
                }

                await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
            }

        }
        public async Task<int> Make(SalesOrderMake T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@SalesOrderId", T.SalesOrderId);
            prm.Add("@SalesOrderItemId", T.SalesOrderItemId);
            prm.Add("@CustomerId", T.ContactId);
            prm.Add("@location", T.LocationId);
            prm.Add("@Tip", T.Tip);

            var ItemId = T.ItemId;
            int id = 0;
            UretimDTO A = new UretimDTO();
            A.StokId = T.ItemId;
            A.PlananlananMiktar = T.PlannedQuantity;
            A.DepoId = T.LocationId;
            A.Tip = T.Tip;
            A.BeklenenTarih = T.ExpectedDate;
            A.OlusturmTarihi = T.CreatedDate;
            A.UretimTarihi = T.ProductionDeadline;
            if (T.Tip == "MakeBatch")
            {
                if (T.SalesOrderId != 0)
                {
                    await _db.ExecuteAsync($"Update SalesOrderItem set Tip=@Tip where CompanyId=@CompanyId and id=@SalesOrderItemId and SalesOrderId=@SalesOrderId ", prm);
                }
                id = await _manufacturingOrderItem.Insert(A, CompanyId);
                prm.Add("@Manufacid", id);
                await _db.ExecuteAsync($"Update ManufacturingOrder set SalesOrderId=@SalesOrderId , SalesOrderItemId=@SalesOrderItemId , CustomerId=@CustomerId where id=@Manufacid and  CompanyId=@CompanyId", prm);
                await _manufacturingOrderItem.InsertOrderItems(id, T.ItemId, T.LocationId, T.PlannedQuantity,T.SalesOrderId, T.SalesOrderItemId);

            }
            else if (T.Tip == "MakeOrder")
            {
                if (T.SalesOrderId != 0)
                {
                    await _db.ExecuteAsync($"Update SalesOrderItem set Tip=@Tip where CompanyId=@CompanyId and id=@SalesOrderItemId and SalesOrderId=@SalesOrderId ", prm);
                }
                List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select (Select ISNULL(id, 0) from LocationStock where ItemId =  @ItemId  and LocationId = @location and CompanyId = @CompanyId)   as LocationStockId, (select ISNULL(DefaultPrice, 0) From Items where CompanyId = @CompanyId and id = @ItemId)as  DefaultPrice,(select Rezerve.RezerveCount from Rezerve where CompanyId=@CompanyId and ItemId=@ItemId and CustomerId=@CustomerId and LocationId=@location and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId)as RezerveCount", prm)).ToList();
                float? rezervecount = sorgu.First().RezerveDeger;
                float LocationStockId = sorgu.First().DepoStokId;
                prm.Add("@LocationStockId", LocationStockId);
                var make = await _db.QueryAsync<SalesOrderRezerve>($"select mo.id from ManufacturingOrder mo where SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and CompanyId=@CompanyId  and IsActive=1 and Status!=3", prm);

                foreach (var item in make)
                {
                    prm.Add("@manuid", item.id);
                    prm.Add("@Null", null);
                    await _db.ExecuteAsync($"Update ManufacturingOrder set SalesOrderId=@Null , SalesOrderItemId=@Null , CustomerId=@Null where id=@manuid and  CompanyId=@CompanyId", prm);

                }



                if (rezervecount != null)
                {
                    await _db.ExecuteAsync($"Delete from  Rezerve where SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and  CompanyId=@CompanyId AND ItemId!=@ItemId", prm);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=0  where SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and  CompanyId=@CompanyId AND ItemId=@ItemId", prm);

                    id = await _manufacturingOrderItem.Insert(A, CompanyId);
                    prm.Add("@Manufacid", id);

                    await _db.ExecuteAsync($"Update ManufacturingOrder set SalesOrderId=@SalesOrderId , SalesOrderItemId=@SalesOrderItemId , CustomerId=@CustomerId where id=@Manufacid and  CompanyId=@CompanyId", prm);
                    await _manufacturingOrderItem.InsertOrderItems(id, T.ItemId, T.LocationId, T.PlannedQuantity, T.SalesOrderId, T.SalesOrderItemId);
                }
                else
                {
                    id = await _manufacturingOrderItem.Insert(A, CompanyId);
                    prm.Add("@Manufacid", id);

                    await _db.ExecuteAsync($"Update ManufacturingOrder set SalesOrderId=@SalesOrderId , SalesOrderItemId=@SalesOrderItemId , CustomerId=@CustomerId where id=@Manufacid and  CompanyId=@CompanyId", prm);
                    await _manufacturingOrderItem.InsertOrderItems(id, T.ItemId, T.LocationId, T.PlannedQuantity, T.SalesOrderId, T.SalesOrderItemId);
                }


            }
            var sales = await _db.QueryAsync<int>($@"select SalesItem from SalesOrderItem where CompanyId=@CompanyId and id=@SalesOrderItemId", prm);
            int salesId = sales.First();
            if (salesId != 1)
            {
                if (salesId == 2)
                {
                    prm.Add("@ProductionId", 1);
                    await _db.ExecuteAsync($"Update SalesOrderItem set Production=@ProductionId where CompanyId=@CompanyId and id=@SalesOrderItemId", prm);
                }
                else if (salesId == 3)
                {
                    prm.Add("@ProductionId", 4);
                    await _db.ExecuteAsync($"Update SalesOrderItem set Production=@ProductionId where CompanyId=@CompanyId and id=@SalesOrderItemId", prm);
                }
            }
            else if (salesId == 1)
            {
                prm.Add("@ProductionId", 0);
                await _db.ExecuteAsync($"Update SalesOrderItem set Production=@ProductionId where CompanyId=@CompanyId and id=@SalesOrderItemId", prm);
            }



            return id;


        }

        public async Task DoneSellOrder(SalesDone T , int CompanyId, int UserId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@CompanyId", CompanyId);
            param.Add("@DeliveryId", T.DeliveryId);
            string sql = $@"select DeliveryId from SalesOrder where CompanyId=@CompanyId and id=@id ";
            var st = await _db.QueryAsync<int>(sql, param);
            int eskiStatus = st.First();

            if (eskiStatus == 0)
            {

                if (T.DeliveryId == 2)
                {
                    var List = await _db.QueryAsync<SalesOrderItem>($@"select SalesOrderItem.ItemId,SalesOrderItem.id,SalesOrderItem.SalesOrderId as SalesOrderId,SalesOrderItem.Quantity,LocationId from SalesOrder 
                     left join SalesOrderItem on SalesOrderItem.SalesOrderId=SalesOrder.id
                     where SalesOrder.CompanyId=@CompanyId and SalesOrder.id=@id and SalesOrderItem.Stance=0", param);
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
                                    UretimTamamlama tamam = new();
                                    tamam.id = items.id;
                                    tamam.Status = Status;
                                    await _manufacturingOrderItem.DoneStock(tamam, UserId);
                                }

                            }
                            param.Add("@Stance", 1);
                            param.Add("@Ingredients", 3);
                            param.Add("@SalesItem", 4);
                            param.Add("@Production", 4);
                            await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients,Stance=@Stance,SalesItem=@SalesItem,Production=@Production where CompanyId=@CompanyId and SalesOrderId=@id and id=@SalesOrderItemId and ItemId=@ItemId ", param);
                            param.Add("@Status", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Status=@Status where CompanyId=@CompanyId and SalesOrderId=@id and SalesOrderItemId=@SalesOrderItemId", param);

                        }
                        else
                        {

                            param.Add("@Stance", 1);
                            param.Add("@Ingredients", 3);
                            param.Add("@SalesItem", 4);
                            param.Add("@Production", 4);
                            await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients,Stance=@Stance,SalesItem=@SalesItem,Production=@Production where CompanyId=@CompanyId and SalesOrderId=@id and id=@SalesOrderItemId and ItemId=@ItemId ", param);
                            param.Add("@Status", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Status=@Status where CompanyId=@CompanyId and SalesOrderId=@id and SalesOrderItemId=@SalesOrderItemId", param);



                        }
                //        List<SalesOrderUpdateItems> aa = (await _db.QueryAsync<SalesOrderUpdateItems>($@"select o.id,oi.id as OrderItemId,oi.Quantity,oi.ItemId,oi.PricePerUnit,oi.TaxId,o.ContactId,o.LocationId,o.DeliveryDeadline from Orders o 
                //left join OrdersItem oi on oi.OrdersId = o.id where o.CompanyId = @CompanyId and oi.Stance = 0 and o.IsActive = 1 and o.id=@id and oi.ItemId = @ItemId and o.DeliveryId = 0 Order by o.DeliveryDeadline ", param)).ToList();
                //        SatısUpdateItems A = new SatısUpdateItems();
                //        foreach (var liste in aa)
                //        {
                //            A.id = item.id;
                //            A.SalesOrderId = liste.id;
                //            A.TaxId = liste.TaxId;
                //            A.Quantity = liste.Quantity;
                //            A.ItemId = liste.ItemId;
                //            A.ContactId = liste.ContactId;
                //            A.LocationId = liste.LocationId;
                //            A.DeliveryDeadline = liste.DeliveryDeadline;
                //            A.PricePerUnit = liste.PricePerUnit;
                //            param.Add("@RezerveCount", 0);
                //            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and SalesOrderId={liste.id} and SalesOrderItemId={liste.OrderItemId} and ItemId={liste.ItemId} ", param);
                //            await UpdateItems(A, CompanyId);
                //        }


                    }



                    param.Add("@DeliveryId", 2);
                    await _db.ExecuteAsync($"Update SalesOrder set DeliveryId=@DeliveryId where CompanyId=@CompanyId and id=@id", param);
                }
                else if (T.DeliveryId == 4)
                {

                    var List = await _db.QueryAsync<SalesOrderItem>($@"select SalesOrderItem.ItemId,SalesOrderItem.id,SalesOrderItem.SalesOrderId as SalesOrderId,SalesOrderItem.Quantity,LocationId from SalesOrder 
                left join SalesOrderItem on SalesOrderItem.SalesOrderId=SalesOrder.id
                where SalesOrder.CompanyId=@CompanyId and SalesOrder.id=@id and SalesOrderItem.Stance=0", param);
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
                                    UretimTamamlama tamam = new();
                                    tamam.id = items.id;
                                    tamam.Status=Status;    
                                    await _manufacturingOrderItem.DoneStock(tamam, UserId);
                                }

                            }



                            param.Add("@Stance", 2);
                            param.Add("@Ingredients", 3);
                            param.Add("@SalesItem", 4);
                            param.Add("@Production", 4);
                            await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients,Stance=@Stance,SalesItem=@SalesItem,Production=@Production where CompanyId=@CompanyId and SalesOrderId=@id and id=@SalesOrderItemId and ItemId=@ItemId ", param);
                            param.Add("@Status", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Status=@Status where CompanyId=@CompanyId and SalesOrderId=@id and SalesOrderItemId=@SalesOrderItemId", param);

                            List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select (Select ISNULL(AllStockQuantity, 0) from Items where id = @ItemId  and CompanyId = @CompanyId)as Quantity, (Select ISNULL(StockCount, 0) from LocationStock where ItemId = @ItemId  and LocationId = @LocationId and CompanyId = @CompanyId) as LocationStock, (Select ISNULL(id, 0) from LocationStock where ItemId = @ItemId and LocationId = @LocationId and CompanyId = @CompanyId)   as LocationStockId,(select RezerveCount from Rezerve where CompanyId=@CompanyId and SalesOrderId=@id and ItemId=@ItemId and Status!=4) as RezerveCount ", param)).ToList();
                            var stockall = sorgu.First().Miktari;
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
                            param.Add("@LocationStockId", sorgu.First().DepoStokId);
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
                            await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients,Stance=@Stance,SalesItem=@SalesItem,Production=@Production where CompanyId=@CompanyId and SalesOrderId=@id and id=@SalesOrderItemId and ItemId=@ItemId ", param);

                            param.Add("@Status", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Status=@Status where CompanyId=@CompanyId and SalesOrderId=@id and SalesOrderItemId=@SalesOrderItemId", param);



                        }
                //        List<SalesOrderUpdateItems> aa = (await _db.QueryAsync<SalesOrderUpdateItems>($@"select o.id,oi.id as OrderItemId,oi.Quantity,oi.ItemId,oi.PricePerUnit,oi.TaxId,o.ContactId,o.LocationId,o.DeliveryDeadline from Orders o 
                //left join OrdersItem oi on oi.OrdersId = o.id where o.CompanyId = @CompanyId and oi.Stance = 0 and o.IsActive = 1 and o.id=@id and oi.ItemId = @ItemId and o.DeliveryId = 0 Order by o.DeliveryDeadline ", param)).ToList();
                //        SalesOrderUpdateItems A = new SalesOrderUpdateItems();
                //        foreach (var liste in aa)
                //        {
                //            A.id = liste.id;
                //            A.OrderItemId = liste.OrderItemId;
                //            A.TaxId = liste.TaxId;
                //            A.Quantity = liste.Quantity;
                //            A.ItemId = liste.ItemId;
                //            A.ContactId = liste.ContactId;
                //            A.LocationId = liste.LocationId;
                //            A.DeliveryDeadline = liste.DeliveryDeadline;
                //            A.PricePerUnit = liste.PricePerUnit;
                //            param.Add("@RezerveCount", 0);
                //            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and SalesOrderId={liste.id} and SalesOrderItemId={liste.OrderItemId} and ItemId={liste.ItemId} ", param);
                //            await UpdateItems(A, T.id, CompanyId);
                //        }


                    }



                    param.Add("@DeliveryId", 4);
                    await _db.ExecuteAsync($"Update SalesOrder set DeliveryId=@DeliveryId where CompanyId=@CompanyId and id=@id", param);




                }
            }

            else if (eskiStatus == 2)
            {
                if (T.DeliveryId == 4)
                {
                    param.Add("@DeliveryId", 4);
                    await _db.ExecuteAsync($"Update SalesOrder set DeliveryId=@DeliveryId where CompanyId=@CompanyId and id=@id", param);
                }
            }
            else if (eskiStatus == 4)
            {

            }
        }

        

    }
}
