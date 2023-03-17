using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using DAL.StockControl;
using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.BomDTO;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.ProductOperationsBomDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;

namespace DAL.Repositories
{
    public class UretimRepository : IUretimRepository
    {
        private readonly IDbConnection _db;
        private readonly IStockControl _control;


        public UretimRepository(IDbConnection db, IStockControl control)
        {
            _db = db;
            _control = control;
        }

        public async Task<int> Insert(UretimDTO T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@Name", T.Isim);
            param.Add("@ItemId", T.StokId);
            if (T.PlananlananMiktar == 0)
            {
                param.Add("@PlannedQuantity", 1);//ilk insert edilirken 1 değerini veriyoruz daha update kısmından kullanıcı kendisi ayarlayabilecek.
            }
            else
            {
                param.Add("@PlannedQuantity", T.PlananlananMiktar);
            }

            param.Add("@ProductionDeadline", DateTime.Now);
            param.Add("@CreatedDate", T.OlusturmTarihi);
            param.Add("@LocationId", T.DepoId);
            param.Add("@Info", T.Bilgi);
            param.Add("@Status", 0);
            param.Add("@IsActive", true);
            param.Add("@Private", T.Ozel);
            param.Add("@Tip", T.Tip);
            param.Add("@ParentId", T.ParentId);

            param.Add("@ExpectedDate", T.BeklenenTarih);
            string sql = string.Empty;
            if (T.Ozel == true)
            {
                sql = $@"Insert into ManufacturingOrder (ParentId,Tip,Private,Name,IsActive,ItemId,PlannedQuantity,ExpectedDate,ProductionDeadline,CreatedDate,LocationId,Info,Status,CompanyId)  OUTPUT INSERTED.[id] values (@ParentId,@Tip,@Private,@Name,@IsActive,@ItemId,@PlannedQuantity,@ExpectedDate,@ProductionDeadline,@CreatedDate,@LocationId,@Info,@Status,@CompanyId)";
            }
            else
            {
                param.Add("@Private", false);
                sql = $@"Insert into ManufacturingOrder (ParentId,Tip,Private,Name,IsActive,ItemId,PlannedQuantity,ExpectedDate,ProductionDeadline,CreatedDate,LocationId,Info,Status,CompanyId)  OUTPUT INSERTED.[id] values (@ParentId,@Tip,@Private,@Name,@IsActive,@ItemId,@PlannedQuantity,@ExpectedDate,@ProductionDeadline,@CreatedDate,@LocationId,@Info,@Status,@CompanyId)";
            }


            int id = await _db.QuerySingleAsync<int>(sql, param);

            return id;
        }

        //Insert ile birlikte kullanılır.
        public async Task InsertOrderItems(int id, int? ItemId, int LocationId, float? adet, int? SalesOrderId,int? SalesOrderItemId)
        {

            int? ProductId = ItemId;
            var BomList = await _db.QueryAsync<BOM>($"Select * From Bom where ProductId = {ProductId} and IsActive = 1");

            foreach (var item in BomList)
            {

                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Ingredients");
                param.Add("@OrderId", id);
                param.Add("@ItemId", item.MalzemeId);
                param.Add("@Notes", item.Bilgi);
                param.Add("@SalesOrderId", SalesOrderId);
                param.Add("@SalesOrderItemId", SalesOrderItemId);

                param.Add("@LocationId", LocationId);
                // materyalin DefaultPrice,stockıd,locatinstock,locationstockId,RezerveCount
                // Bul
                List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($@" select (select ISNULL(DefaultPrice, 0) From Items where id = @ItemId)as  DefaultPrice,(Select ISNULL(id, 0) from LocationStock where ItemId=@ItemId and LocationId = @LocationId )   as LocationStockId,(select Tip from Items where id=@ItemId)as Tip", param)).ToList();

                param.Add("@PlannedQuantity", item.Miktar * adet);
                float DefaultPrice = sorgu.First().VarsayilanFiyat;
                param.Add("@Cost", DefaultPrice * item.Miktar * adet);
                //Avaibility Hesapla Stoktaki miktar işlemi gerçekleştirmeye yetiyormu kontrol et


                string sqlb = $@"select ISNULL(SUM(Quantity),0) from Orders 
                left join OrdersItem on OrdersItem.OrdersId = Orders.id
                and OrdersItem.ItemId = @ItemId where Orders.CompanyId = @CompanyId
                and DeliveryId = 1 and Orders.SalesOrderId is null and Orders.ManufacturingOrderId is null and Orders.IsActive=1 and Orders.Private=0";
                var expected = await _db.QueryFirstAsync<int>(sqlb, param);
                param.Add("@LocationStockId",
                sorgu.First().DepoStokId);
                int rezerveid = 0;

                int? rezerve = await _control.Count(item.MalzemeId, LocationId);
                rezerve = rezerve >= 0 ? rezerve : 0;

                if (SalesOrderItemId!=0)
                {
                    string sqld = $@"select id,ISNULL(RezerveCount,0) as RezerveCount from Rezerve where SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and ManufacturingOrderId is null and ManufacturingOrderItemId is null and ItemId=@ItemId and LocationId=@LocationId and Status=1";
                    var rezerv = await _db.QueryAsync<LocaVarmı>(sqld, param);

                    float? rezervestockCount = 0;
                    if (rezerv.Count() == 0)
                    {

                        param.Add("@RezervTip", sorgu.First().Tip);
                        param.Add("@RezerveDeger", 0);
                        param.Add("@Status", 1);
                        param.Add("@LocationStockCount", rezerve);
                        param.Add("@ContactId", sorgu.First().CariKod);

                        rezervestockCount = 0;
                       rezerveid= await _db.QuerySingleAsync<int>($"Insert into Rezerve (SalesOrderId,SalesOrderItemId,Tip,ItemId,RezerveCount,CustomerId,LocationId,Status,LocationStockCount,CompanyId) OUTPUT INSERTED.[id] values (@SalesOrderId,@SalesOrderItemId,@RezervTip,@ItemId,@RezerveDeger,@ContactId,@LocationId,@Status,@LocationStockCount,@CompanyId)", param);
                        param.Add("@Rezerveid", rezerveid);

                    }
                    else
                    {
                        rezerveid = rezerv.First().id;
                        param.Add("@Rezerveid", rezerveid);

                        rezervestockCount = rezerv.First().RezerveDeger;
                    }
                    if (rezervestockCount >= item.Miktar * adet)
                    {
                        param.Add("@Availability", 2);

                        var newStock = rezervestockCount - (item.Miktar * adet);
                        param.Add("@RezerveCount", item.Miktar * adet);
                        param.Add("@LocationStockCount", rezerve + newStock);
                        await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount,ManufacturingOrderId=@OrderId where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and LocationId=@LocationId and ItemId=@ItemId and id=@Rezerveid ", param);

                    }
                    else
                    {
                        float? newQuantity = (item.Miktar * adet) - rezervestockCount;
                        if (rezerve >= newQuantity)
                        {

                            var newStockCount = rezerve - newQuantity;
                            param.Add("@LocationStockCount", newStockCount);
                            var newrezervecount = rezervestockCount + newQuantity;
                            param.Add("@RezerveCount", newrezervecount);
                            param.Add("@Availability", 2);

                        }
                        else
                        {

                            param.Add("@RezerveCount", rezerve + rezervestockCount);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                            param.Add("@LocationStockCount", 0);
                            param.Add("@Availability", 0);

                        }
                        param.Add("@id", id);
                        await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount,ManufacturingOrderId=@OrderId where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and LocationId=@LocationId and ItemId=@ItemId and id=@Rezerveid  ", param);


                    }
                }

                else if (rezerve > 0)
                {


                    if (rezerve >= item.Miktar * adet)//Stok sayısı istenilenden büyük ise rezerve sayısı adet olur
                    {
                        param.Add("@RezerveCount", item.Miktar * adet);
                        param.Add("@Availability", 2);
                        param.Add("@LocationStockCount", rezerve - item.Miktar * adet);

                    }
                    else
                    {

                        param.Add("@RezerveCount", rezerve);//Stok sayısı adetten kücük ise rezer sayısı Stok adeti kadar olur.
                        param.Add("@Availability", 0);
                        param.Add("@LocationStockCount", 0);

                    }
                    param.Add("@Status", 1);
                    param.Add("@id", id);
                    await _db.ExecuteAsync($"Insert into Rezerve  (Tip,ManufacturingOrderId,ItemId,RezerveCount,LocationId,Status,LocationStockCount,CompanyId) values   (@Tip,@id,@ItemId,@RezerveCount,@LocationId,@Status,@LocationStockCount,@CompanyId)", param);

                }

                else if ((rezerve - (item.Miktar * adet)) * (-2) <= expected && expected != 0)
                {
                    param.Add("@id", id);

                    param.Add("@RezerveCount", rezerve);
                    param.Add("@LocationStockCount", rezerve);
                    param.Add("@Availability", 1);
                    param.Add("@Status", 1);

                    await _db.ExecuteAsync($"Insert into Rezerve  (Tip,ManufacturingOrderId,ItemId,RezerveCount,LocationId,Status,LocationStockCount,CompanyId) values   (@Tip,@id,@ItemId,@RezerveCount,@LocationId,@Status,@LocationStockCount,@CompanyId)", param);
                }
                else
                {
                    param.Add("@id", id);

                    param.Add("@RezerveCount", rezerve);
                    param.Add("@LocationStockCount", rezerve);
                    param.Add("@Availability", 0);
                    param.Add("@Status", 1);

                    await _db.ExecuteAsync($"Insert into Rezerve  (Tip,ManufacturingOrderId,ItemId,RezerveCount,LocationId,Status,LocationStockCount,CompanyId) values   (@Tip,@id,@ItemId,@RezerveCount,@LocationId,@Status,@LocationStockCount,@CompanyId)", param);
                }



                string sql = $@"Insert into ManufacturingOrderItems (Tip,OrderId,ItemId,Notes,PlannedQuantity,Cost,Availability,CompanyId) values (@Tip,@OrderId,@ItemId,@Notes,@PlannedQuantity,@Cost,@Availability,@CompanyId)";
                await _db.ExecuteAsync(sql, param);
                string sqlf = $@"(select ManufacturingOrderItems.id from ManufacturingOrderItems where CompanyId=@CompanyId and ItemId=@ItemId and OrderId=@OrderId)";
                var ManuItemId = await _db.QueryFirstAsync<int>(sqlf, param);
                param.Add("@ManufacturingOrderItemId", ManuItemId);
                if (SalesOrderId != 0)
                {
                    await _db.ExecuteAsync($"Update Rezerve set ManufacturingOrderItemId=@ManufacturingOrderItemId where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and SalesOrderItemId=@SalesOrderItemId and LocationId=@LocationId and ManufacturingOrderId=@OrderId and ItemId=@ItemId and id=@Rezerveid  ", param);
                }
                else
                {
                    await _db.ExecuteAsync($"Update Rezerve set ManufacturingOrderItemId=@ManufacturingOrderItemId where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId and LocationId=@LocationId and ItemId=@ItemId ", param);
                }

            }


            //Eklenen Ordera ait ıtemin Operation Bomlarını buluyoruz
            var OperationList = await _db.QueryAsync<ProductOperationsBomDTO.ProductOperationsBOM>($"Select * From ProductOperationsBom where  ItemId = {ProductId}");

            foreach (var item in OperationList)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Operations");
                param.Add("@OrderId", id);
                param.Add("@OperationId", item.OperasyonId);
                param.Add("@ResourceId", item.KaynakId);
                param.Add("@PlannedTime ", item.OperasyonZamani * adet);
                param.Add("@Status", 0);
                param.Add("@CostPerHour", item.SaatlikUcret);
                param.Add("@Cost", (item.SaatlikUcret / 60 / 60) * item.OperasyonZamani * adet);

                string sql = $@"Insert into ManufacturingOrderItems (Tip,OrderId,OperationId,ResourceId,PlannedTime,Status,CostPerHour,Cost,CompanyId) values (@Tip,@OrderId,@OperationId,@ResourceId,@PlannedTime,@Status,@CostPerHour,@Cost,@CompanyId)";
                await _db.ExecuteAsync(sql, param);
            }
        }
        public async Task Update(UretimUpdate T)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@Name", T.Isim);
            param.Add("@ItemId", T.StokId);
            param.Add("@ProductionDeadline", DateTime.Now);
            param.Add("@CreatedDate", T.OlusturmaTarihi);
            param.Add("@LocationId", T.DepoId);
            param.Add("@MaterialCost", T.MalzemeTutarı);
            param.Add("@OperationCost", T.OperasyonTutarı);
            param.Add("@TotalCost", T.ToplamTutar);
            param.Add("@Info", T.Bilgi);
            param.Add("@Status", 0);
            param.Add("@PlannedQuantity", T.PlanlananMiktar);
            param.Add("@ExpectedDate", T.BeklenenTarih);
            string sqlv = $@"Select ISNULL(PlannedQuantity,0)as PlannedQuantity ,ItemId,LocationId from  ManufacturingOrder where CompanyId=@CompanyId and id=@id and IsActive=1 and Status!=3";
            var deger = await _db.QueryAsync<ManufacturingOrderA>(sqlv, param);
            T.eskiPlanned = (float)deger.First().PlannedQuantity;
            T.eskiLocation = deger.First().LocationId;
            if (T.StokId != deger.First().ItemId && deger.First().ItemId != null && T.StokId != null)
            {
                string sqlsorgu = $@"Select * from ManufacturingOrderItems where CompanyId=@CompanyId and ManufacturingOrderItems.OrderId=@id";
                var ManuItems = await _db.QueryAsync<ManufacturingOrderResponse>(sqlsorgu, param);
                foreach (var item in ManuItems)
                {
                    UretimDeleteItems A = new UretimDeleteItems();

                    A.UretimId = T.id;
                    A.id = item.id;
                    if (item.Tip == "Ingredients")
                    {
                        A.StokId = (int)item.ItemId;
                    }
                    await DeleteItems(A);
                }
                param.Add("@ItemId", T.StokId);
                string sql = $@"Update ManufacturingOrder Set Name=@Name,MaterialCost=@MaterialCost,OperationCost=@OperationCost,TotalCost=@TotalCost,ItemId=@ItemId,ProductionDeadline=@ProductionDeadline,ExpectedDate=@ExpectedDate,PlannedQuantity=@PlannedQuantity,CreatedDate=@CreatedDate,LocationId=@LocationId,Info=@Info,Status=@Status where CompanyId=@CompanyId and id=@id";
                await _db.ExecuteAsync(sql, param);
                await InsertOrderItems(T.id, T.StokId, T.DepoId, T.PlanlananMiktar,0,0);





            }

            else
            {
                if (T.DepoId != T.eskiLocation)
                {
                    var rezervedegerler = await _db.QueryAsync<Manufacturing>($"select * from Rezerve where ManufacturingOrderId={T.id} and Status=1");
                    foreach (var item in rezervedegerler)
                    {
                        param.Add("@ItemsId", item.StokId);
                        await _db.ExecuteAsync($"Delete from Rezerve where ManufacturingOrderId=@id and CompanyId=@CompanyId and ItemId=@ItemsId", param);
                    }
                }

                string sql = $@"Update ManufacturingOrder Set Name=@Name,MaterialCost=@MaterialCost,OperationCost=@OperationCost,TotalCost=@TotalCost,ItemId=@ItemId,ProductionDeadline=@ProductionDeadline,ExpectedDate=@ExpectedDate,PlannedQuantity=@PlannedQuantity,CreatedDate=@CreatedDate,LocationId=@LocationId,Info=@Info,Status=@Status where CompanyId=@CompanyId and id=@id";
                await _db.ExecuteAsync(sql, param);
            }
        }
        public async Task DeleteItems(UretimDeleteItems T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@ManufacturingOrderId", T.UretimId);
            prm.Add("@ItemId", T.StokId);

            if (T.StokId != 0)
            {
                var sorgu = await _db.QueryAsync<ItemKontrol>($"select id from Rezerve where ManufacturingOrderId = @ManufacturingOrderId and CompanyId = @CompanyId and ItemId = @ItemId and Status=1 and ManufacturingOrderItemId=@id", prm);
                await _db.ExecuteAsync($"Update Orders set ManufacturingOrderId=NULL,ManufacturingOrderItemId=NULL where ManufacturingOrderId=@ManufacturingOrderId and CompanyId=@CompanyId  and ManufacturingOrderItemId=@id and Orders.IsActive=1 and Orders.DeliveryId=1", prm);
                await _db.ExecuteAsync($"Delete From ManufacturingOrderItems  where ItemId = @ItemId and CompanyId = @CompanyId and id=@id and OrderId=@ManufacturingOrderId", prm);

                int? rezervid = sorgu.First().id;
                prm.Add("@RezerveId", rezervid);

                await _db.ExecuteAsync($"Delete From Rezerve where id=@RezerveId and CompanyId=@CompanyId", prm);
            }
            else
            {
                await _db.ExecuteAsync($"Delete From ManufacturingOrderItems  where CompanyId = @CompanyId and id=@id and OrderId=@ManufacturingOrderId", prm);

            }

        }
        public async Task UpdateOrderItems(int id, int LocationId, float adetbul, float eski)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@LocationId", LocationId);
            prm.Add("@id", id);

            //Eklenen Ordera ait ıtemin  Bomlarını buluyoruz
            var BomList = await _db.QueryAsync<BOM>($"Select id,ISNULL(ItemId,0) as MaterialId,ISNULL(PlannedQuantity,0) as Quantity,ISNULL(Notes,'') as Note from ManufacturingOrderItems where  ManufacturingOrderItems.OrderId={id} and Tip='Ingredients'");

            foreach (var item in BomList)
            {

                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Ingredients");
                param.Add("@ManufacturingOrderItemId", item.id);
                param.Add("@OrderId", id);
                param.Add("@ItemId", item.MalzemeId);
                param.Add("@Notes", item.Bilgi);
                if (adetbul == eski)
                {

                    param.Add("@PlannedQuantity", item.Miktar);
                }
                else if (adetbul > eski)
                {
                    float anadeger = item.Miktar / eski;
                    float yenideger = adetbul - eski;
                    var artışdegeri = yenideger * anadeger;
                    item.Miktar = item.Miktar + artışdegeri;
                    param.Add("@PlannedQuantity", item.Miktar);
                }
                else
                {
                    var yenideger = item.Miktar / eski;
                    var degerler = eski - adetbul;
                    item.Miktar = item.Miktar - (yenideger * degerler);
                    param.Add("@PlannedQuantity", item.Miktar);
                }


                param.Add("@LocationId", LocationId);
                string sql = $@"Update ManufacturingOrderItems Set PlannedQuantity=@PlannedQuantity where CompanyId=@CompanyId and OrderId=@OrderId and ItemId=@ItemId and id=@ManufacturingOrderItemId  ";
                await _db.ExecuteAsync(sql, param);
                // materyalin DefaultPrice
                // Bul
                string sqlb = $@"select ISNULL(SUM(Quantity),0) from Orders 
                left join OrdersItem on OrdersItem.OrdersId = Orders.id and Orders.LocationId=@LocationId
                and OrdersItem.ItemId = @ItemId where Orders.CompanyId = @CompanyId
                and DeliveryId = 1 and Orders.ManufacturingOrderId={id} and Orders.IsActive=1";
                var expected = await _db.QueryFirstAsync<float>(sqlb, param);

                List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select   (Select ISNULL(id, 0) from LocationStock where ItemId = @ItemId and LocationId = @LocationId and CompanyId = @CompanyId)   as LocationStockId, (select ISNULL(DefaultPrice, 0) From Items where CompanyId = @CompanyId and id = @ItemId)as  DefaultPrice", param)).ToList();
                float DefaultPrice = sorgu.First().VarsayilanFiyat;
                param.Add("@Cost", DefaultPrice * item.Miktar);
                param.Add("@LocationStockId", sorgu.First().DepoStokId);


                float LocationStock = await _control.Count(item.MalzemeId, LocationId);
                LocationStock = LocationStock >= 0 ? LocationStock : 0;


                var Count = await _db.QueryAsync<int>($"Select ISNULL(Rezerve.RezerveCount,0)as Count from Rezerve where CompanyId=@CompanyId and ItemId=@ItemId and ManufacturingOrderId=@OrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Rezerve.Status=1 and Rezerve.LocationId=@LocationId", param);
                float? RezerveCounts = 0;
                float? deger;
                if (Count.Count() == 0)
                {
                    if (LocationStock >= item.Miktar)
                    {
                        deger = item.Miktar;
                    }

                    else
                    {
                        deger = LocationStock;

                    }
                    DynamicParameters prm2 = new DynamicParameters();
                    prm2.Add("@Status", 1);
                    prm2.Add("@LocationStockCount", LocationStock);
                    prm2.Add("@Tip", "Ingredients");
                    prm2.Add("@OrderId", id);
                    prm2.Add("@ManufacturingOrderItemId", item.id);
                    prm2.Add("@RezerveCount", deger);
                    prm2.Add("@ItemId", item.MalzemeId);
                    prm2.Add("@LocationId", LocationId);

                    await _db.ExecuteAsync($"Insert into Rezerve  (Tip,ManufacturingOrderId,ManufacturingOrderItemId,ItemId,RezerveCount,LocationId,Status,LocationStockCount,CompanyId) values   (@Tip,@OrderId,@ManufacturingOrderItemId,@ItemId,@RezerveCount,@LocationId,@Status,@LocationStockCount,@CompanyId)", prm2);



                }
                else
                {
                    RezerveCounts = Count.First();
                }




                if (RezerveCounts == item.Miktar)
                {

                    param.Add("@RezerveCount", RezerveCounts);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId  and ManufacturingOrderItemId=@ManufacturingOrderItemId and ItemId=@ItemId and Rezerve.LocationId=@LocationId", param);
                    param.Add("@Availability", 2);
                }
                else if (item.Miktar < RezerveCounts)
                {
                    param.Add("@RezerveCount", item.Miktar);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId  and ManufacturingOrderItemId=@ManufacturingOrderItemId and ItemId=@ItemId and Rezerve.LocationId=@LocationId", param);
                    param.Add("@Availability", 2);
                }
                else if (LocationStock > item.Miktar - RezerveCounts)
                {
                    param.Add("@RezerveCount", item.Miktar);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId  and ManufacturingOrderItemId=@ManufacturingOrderItemId and ItemId=@ItemId and Rezerve.LocationId=@LocationId", param);
                    param.Add("@Availability", 2);
                }
                else if (LocationStock == item.Miktar - RezerveCounts)
                {
                    param.Add("@RezerveCount", item.Miktar);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId  and ManufacturingOrderItemId=@ManufacturingOrderItemId and ItemId=@ItemId and Rezerve.LocationId=@LocationId", param);
                    param.Add("@Availability", 2);
                }
                else if (item.Miktar - RezerveCounts <= expected && expected > 0)
                {
                    param.Add("@Availability", 1);
                }
                else
                {

                    param.Add("@RezerveCount", LocationStock + RezerveCounts);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                    await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId  and ManufacturingOrderItemId=@ManufacturingOrderItemId and ItemId=@ItemId and Rezerve.LocationId=@LocationId", param);
                    param.Add("@Availability", 0);
                }


                string sqlw = $@"Update ManufacturingOrderItems Set Tip=@Tip,ItemId=@ItemId,Notes=@Notes,PlannedQuantity=@PlannedQuantity,Cost=@Cost,Availability=@Availability where CompanyId=@CompanyId and OrderId=@OrderId and ItemId=@ItemId and id=@ManufacturingOrderItemId  ";
                await _db.ExecuteAsync(sqlw, param);

            }


            //Eklenen Ordera ait ıtemin Operation Bomlarını buluyoruz
            var OperationList = await _db.QueryAsync<ProductOperationsBOM>($"Select ISNULL(id,0)As id,ISNULL(OperationId,0) as OperationId,ISNULL(ResourceId,0)as ResourceId,ISNULL(CostPerHour,0)as CostHour,ISNULL(PlannedTime,0)as OperationTime  from ManufacturingOrderItems where  ManufacturingOrderItems.OrderId = {id} and Tip = 'Operations'");

            foreach (var item in OperationList)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Operations");
                param.Add("@OrderId", id);
                param.Add("@OperationId", item.OperasyonId);
                param.Add("@ResourceId", item.KaynakId);
                if (adetbul == eski)
                {
                    param.Add("@PlannedQuantity", item.OperasyonZamani);
                }
                else
                {
                    var saatlik = item.OperasyonZamani / eski;
                    item.OperasyonZamani = saatlik * adetbul;

                }

                param.Add("@PlannedTime ", item.OperasyonZamani);
                param.Add("@Cost", (item.SaatlikUcret / 60 / 60) * item.OperasyonZamani);

                string sql = $@"Update ManufacturingOrderItems Set Tip=@Tip,OrderId=@OrderId,OperationId=@OperationId,ResourceId=@ResourceId,PlannedTime=@PlannedTime,Cost=@Cost where CompanyId=@CompanyId and OrderId=@OrderId and OperationId=@OperationId and ResourceId=ResourceId  ";
                await _db.ExecuteAsync(sql, param);
            }
        }
        public async Task<int> IngredientsInsert(UretimIngredientsInsert T)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@Tip", "Ingredients");
            param.Add("@OrderId", T.UretimId);
            param.Add("@ItemId", T.StokId);
            param.Add("@Notes", T.Bilgi);
            param.Add("@LocationId", T.DepoId);
            param.Add("@PlannedQuantity", T.Miktar);
            int rezerveid = 0;
            //param.Add("@Cost", T.Cost);
            //param.Add("@Availability", T.Availability);
            List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select  (Select ISNULL(id, 0) from LocationStock where ItemId = @ItemId  and LocationId = (Select LocationId From ManufacturingOrder where CompanyId =@CompanyId  and id = @OrderId) and CompanyId = @CompanyId)   as LocationStockId, (select ISNULL(DefaultPrice, 0) From Items where CompanyId = @CompanyId and id = @ItemId)as  DefaultPrice", param)).ToList();

            param.Add("@LocationStockId", sorgu.First().DepoStokId);

            //yeni costu buluyoruz.
            float DefaultPrice = sorgu.First().VarsayilanFiyat;
            var newCost = T.Miktar * DefaultPrice;
            param.Add("@Cost", newCost);

            float? rezerve = await _control.Count(T.StokId, T.DepoId);
            rezerve = rezerve >= 0 ? rezerve : 0;


            if (rezerve >= 0)
            {

                if (rezerve >= T.Miktar)//Stok sayısı istesnilenden büyük ise rezerve sayısı adet olur
                {
                    param.Add("@RezerveCount", T.Miktar);
                    var newStockCount = rezerve - (T.Miktar);
                    param.Add("@LocationStockCount", newStockCount);
                    param.Add("@Availability", 2);

                }
                else
                {

                    param.Add("@RezerveCount", rezerve);//Stok sayısı adetten kücük ise rezer sayısı Stokadeti kadar olur.
                    param.Add("@LocationStockCount", 0);
                    param.Add("@Availability", 0);
                }
                param.Add("@Status", 1);

                rezerveid = await _db.QuerySingleAsync<int>($"Insert into Rezerve  (Tip,ManufacturingOrderId,ItemId,RezerveCount,LocationId,Status,LocationStockCount,CompanyId) OUTPUT INSERTED.[id] values (@Tip,@OrderId,@ItemId,@RezerveCount,@LocationId,@Status,@LocationStockCount,@CompanyId)", param);


            }
            else
            {
                param.Add("@Availability", 0);
                rezerveid = await _db.QuerySingleAsync<int>($"Insert into Rezerve  (Tip,ManufacturingOrderId,ItemId,RezerveCount,LocationId,Status,LocationStockCount,CompanyId) OUTPUT INSERTED.[id] values (@Tip,@OrderId,@ItemId,@RezerveCount,@LocationId,@Status,@LocationStockCount,@CompanyId)", param);
            }

            string sql = $@"Insert into ManufacturingOrderItems (Tip,OrderId,ItemId,Notes,PlannedQuantity,Cost,Availability,CompanyId) OUTPUT INSERTED.[id] values (@Tip,@OrderId,@ItemId,@Notes,@PlannedQuantity,@Cost,@Availability,@CompanyId)";
            int ManuId = await _db.QuerySingleAsync<int>(sql, param);
            param.Add("@ManufacturingOrderItemId", ManuId);



            await _db.ExecuteAsync($"Update Rezerve set ManufacturingOrderItemId=@ManufacturingOrderItemId where CompanyId=@CompanyId and id={rezerveid}", param);
            return ManuId;
        }
        public async Task<int> OperationsInsert(UretimOperationsInsert T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "Operations");
            prm.Add("@ResourceId", T.KaynakId);
            prm.Add("@OperationId", T.OperasyonId);
            prm.Add("@PlannedTime", T.PlanlananZaman);
            prm.Add("@OrderId", T.UretimId);
            return await _db.QuerySingleAsync<int>($"Insert into ManufacturingOrderItems (Tip,OrderId,ResourceId, OperationId,PlannedTime,CompanyId) OUTPUT INSERTED.[id] values (@Tip,@OrderId,@ResourceId, @OperationId,@PlannedTime, @CompanyId)", prm);
        }
        public async Task OperationsUpdate(UretimOperationsUpdate T)
        {
            var Costbul = await _db.QueryAsync<float>($"Select CAST(({T.SaatlikUcret}*{T.PlanlananZaman})/60/60 as decimal(15,4)) ");
            float cost = Costbul.First();
            var newcost = Convert.ToDecimal(cost);
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@OrderId", T.UretimId);
            param.Add("@OperationId", T.OperasyonId);
            param.Add("@ResourceId", T.KaynakId);
            param.Add("@PlannedTime", T.PlanlananZaman);
            param.Add("@Status", T.Durum);
            param.Add("CostPerHour", T.SaatlikUcret);
            param.Add("@Cost", newcost);
            string sql = $@"Update ManufacturingOrderItems SET OrderId = @OrderId, OperationId = @OperationId, ResourceId = @ResourceId, PlannedTime = @PlannedTime, Status = @Status,Cost = @Cost,CostPerHour=@CostPerHour
                            where CompanyId = @CompanyId and id = @id and OrderId = @OrderId";
            await _db.ExecuteAsync(sql, param);
        }
        public async Task IngredientsUpdate(UretimIngredientsUpdate T)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@OrderId", T.UretimId);
            param.Add("@ItemId", T.StokId);
            param.Add("@Tip", " Ingredients");
            param.Add("@LocationId", T.DepoId);
            param.Add("@PlannedQuantity", T.Miktar);
            string sqlu = $@"Update ManufacturingOrderItems SET  PlannedQuantity = @PlannedQuantity where CompanyId = @CompanyId and id = @id and OrderId = @OrderId";
            await _db.ExecuteAsync(sqlu, param);

            List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select (Select ISNULL(id, 0) from LocationStock where ItemId = @ItemId  and LocationId = (Select LocationId From ManufacturingOrder where CompanyId =@CompanyId  and id = @OrderId) and CompanyId = @CompanyId)   as LocationStockId, (select ISNULL(DefaultPrice, 0) From Items where CompanyId = @CompanyId and id = @ItemId)as  DefaultPrice", param)).ToList();

            param.Add("@LocationStockId", sorgu.First().DepoStokId);

            float DefaultPrice = sorgu.First().VarsayilanFiyat;
            var newCost = T.Miktar * DefaultPrice;

            float? rezerve = await _control.Count(T.StokId, T.DepoId);
            rezerve = rezerve >= 0 ? rezerve : 0;

            string sqld = $@"select ISNULL(RezerveCount,0) from Rezerve where ManufacturingOrderId=@OrderId and ItemId=@ItemId and LocationId=@LocationId and ManufacturingOrderItemId=@id and Status=1";
            var rezervestockCount = await _db.QueryAsync<float>(sqld, param);
            string sqlb = $@"select ISNULL(SUM(Quantity),0) from Orders 
                left join OrdersItem on OrdersItem.OrdersId = Orders.id and Orders.LocationId=@LocationId
                and OrdersItem.ItemId = @ItemId where Orders.CompanyId = @CompanyId
                and DeliveryId = 1 and Orders.ManufacturingOrderId={T.UretimId} and Orders.IsActive=1 ";
            var expectedsorgu = await _db.QueryAsync<float>(sqlb, param);
            float expected = expectedsorgu.First();

            float rezervecount = 0;
            if (rezervestockCount.Count() == 0)
            {
                rezervecount = 0;
            }
            else
            {
                rezervecount = rezervestockCount.First();
            }






            if (rezervecount >= T.Miktar)
            {
                param.Add("@Availability", 2);
                var newStock = rezervecount - T.Miktar;
                param.Add("@LocationStockCount", rezerve + newStock);
                param.Add("@RezerveCount", T.Miktar);
                param.Add("@Status", 1);
                await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId and LocationId=@LocationId and ManufacturingOrderItemId=@id and ItemId=@ItemId  and Status=1 ", param);

            }

            else if (rezervecount < T.Miktar)
            {
                float? newQuantity = T.Miktar - rezervecount;
                if (rezerve >= newQuantity)
                {

                    var newStockCount = rezerve - newQuantity;
                    param.Add("@LocationStockCount", newStockCount);
                    var newrezervecount = rezervecount + newQuantity;
                    param.Add("@RezerveCount", newrezervecount);
                    param.Add("@Availability", 2);
                }
                else if ((T.Miktar - rezervecount <= expected && expected > 0))
                {
                    param.Add("@Availability", 1);
                    param.Add("@RezerveCount", rezervecount);
                }
                else
                {
                    param.Add("@Availability", 0);
                    param.Add("@RezerveCount", rezerve + rezervecount);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                    param.Add("@LocationStockCount", 0);

                }
                param.Add("@Status", 1);
                await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId and LocationId=@LocationId and ManufacturingOrderItemId=@id and ItemId=@ItemId  and Status=1 ", param);

            }
            else if ((T.Miktar - rezervecount <= expected && expected > 0))
            {
                param.Add("@Availability", 1);
            }
            param.Add("@id", T.id);
            param.Add("@OrderId", T.UretimId);
            param.Add("@ItemId", T.StokId);
            param.Add("@Notes", T.Bilgi);
            param.Add("@PlannedQuantity", T.Miktar);
            param.Add("@Cost", newCost);
            string sql = $@"Update ManufacturingOrderItems SET ItemId = @ItemId , Notes = @Notes , PlannedQuantity = @PlannedQuantity, Cost = @Cost,Availability = @Availability
                            where CompanyId = @CompanyId and id = @id and OrderId = @OrderId";
            await _db.ExecuteAsync(sql, param);


        }
        public async Task Delete(List<UretimDeleteKontrol> T, int UserId)
        {
            foreach (var A in T)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@id", A.id);
                param.Add("@DateTime", DateTime.Now);
                param.Add("@User", UserId);
                param.Add("@IsActive", false);
                List<bool> IsActived = (await _db.QueryAsync<bool>($"select IsActive from ManufacturingOrder where id=@id and CompanyId=@CompanyId ", param)).ToList();
                if (IsActived[0] == false)
                {

                }
                else
                {
                    List<int> ItemsCount = (await _db.QueryAsync<int>($"select id from Rezerve where ManufacturingOrderId=@id and CompanyId=@CompanyId and Status=1", param)).ToList();
                    var detay = await _db.QueryAsync<UretimIngredientsUpdate>($"select * from ManufacturingOrderItem where OrderId=@id and CompanyId=@CompanyId ", param);

                    var order = await _db.QueryAsync<OperationsUpdate>($"select Orders.id from Orders where  SalesOrderId=@id and IsActive=1 and CompanyId=@CompanyId", param);
                    foreach (var item in order)
                    {
                        param.Add("@orderid", item.id);

                        await _db.ExecuteAsync($"Update Order Set SalesOrderId=NULL , SalesOrderItemId=NULL  where id = @orderid and CompanyId = @CompanyId ", param);

                    }
                    foreach (var item in detay)
                    {
                        await _db.ExecuteAsync($"Delete from  ManufacturingOrderItem where  CompanyId=@CompanyId and id={item.id}");

                    }
                    await _db.QueryAsync($"Delete from ManufacturingOrder where id = @id and CompanyId = @CompanyId ", param);
                    foreach (var item in ItemsCount)
                    {
                        param.Add("@RezerveId", item);
                        await _db.ExecuteAsync($"Delete from  Rezerve where  CompanyId=@CompanyId and id=@RezerveId", param);

                    }

                }
            }
           
        }
        public async Task DoneStock(UretimTamamlama T , int UserId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            var BomList = await _db.QueryAsync<DoneStock>($@"Select moi.id,moi.ItemId,moi.PlannedQuantity,moi.Tip,Rezerve.id as RezerveId,ManufacturingOrder.Status,ManufacturingOrder.LocationId,ManufacturingOrder.SalesOrderId,ManufacturingOrder.SalesOrderItemId,ManufacturingOrder.ParentId     from ManufacturingOrderItems moi 
            left join ManufacturingOrder on ManufacturingOrder.id=moi.OrderId 
            left join Rezerve on Rezerve.ManufacturingOrderItemId=moi.id 
            where moi.CompanyId = @CompanyId and moi.OrderId=@id and ManufacturingOrder.IsActive=1", param);
            param.Add("@Status", T.Status);
            int eskiStatus = BomList.First().Status;
            if (T.Status == 3 && eskiStatus != 3)
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
                var ManufacturingQuantity = sorgu4.First().ManufacturingQuantity;
                var AllStock = sorgu4.First().Quantity;
                param.Add("@ItemId", sorgu4.First().ItemId);

                if (BomList.First().SalesOrderId!=0)
                {
                    var stokcontrol = await _control.Count(sorgu4.First().ItemId, BomList.First().LocationId);
                    stokcontrol = stokcontrol >= 0 ? stokcontrol : 0;

                    DynamicParameters prm = new();
                    prm.Add("@SalesOrderId", BomList.First().SalesOrderId);
                    prm.Add("@SalesOrderItemId", BomList.First().SalesOrderItemId);
                    prm.Add("@ItemId", sorgu4.First().ItemId);
                    prm.Add("@ItemId", sorgu4.First().ItemId);
                    prm.Add("@LocationId", BomList.First().LocationId);

                    string sqld = $@"select id,ISNULL(RezerveCount,0) as RezerveCount from Rezerve where SalesOrderId=@SalesOrderId AND SalesOrderItemId=@SalesOrderItemId and ItemId=@ItemId and LocationId=@LocationId and Status=1";
                    var rezervestockCount = await _db.QueryAsync<LocaVarmı>(sqld, prm);
                    float? rezervecount = rezervestockCount.First().RezerveDeger;
                    int rezerveid = rezervestockCount.First().id;
                    prm.Add("@rezerveid", rezerveid);

                    float missing;
                    string missingsorgu = $@"
                         Select 
                         ((select Quantity from SalesOrderItem where id=@SalesOrderItemId and SalesOrderId=@SalesOrderId )-ISNULL((Rezerve.RezerveCount),0))as Missing
                        
                         from Orders
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
                        missing = missingcount.First().Kayıp;
                    }
                    if (missing<=rezervecount)
                    {
                        prm.Add("@RezerveCount", missing);

                        prm.Add("@SalesItem", 3);
                        prm.Add("@Production", 4);
                        prm.Add("@Ingredients", 3);

                        await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);
                    }
                   else if (missing<=ManufacturingQuantity+rezervecount)
                    {
                        prm.Add("@RezerveCount", missing);

                        prm.Add("@SalesItem", 3);
                        prm.Add("@Production", 4);
                        prm.Add("@Ingredients", 3);

                        await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and id=@rezerveid and CompanyId=@CompanyId ", prm);
                    
                    }
                    else if (missing<=ManufacturingQuantity+stokcontrol+rezervecount)
                    {
                        prm.Add("@RezerveCount", missing);

                        prm.Add("@SalesItem", 3);
                        prm.Add("@Production", 4);
                        prm.Add("@Ingredient", 3);

                        await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and id=@rezerveid and CompanyId=@CompanyId ", prm);
                      
                    }
                    else
                    {
                        prm.Add("@RezerveCount", ManufacturingQuantity+stokcontrol+rezervecount);

                        prm.Add("@SalesItem", 0);
                        prm.Add("@Production", 0);
                        prm.Add("@Ingredient", 0);
                        await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and id=@rezerveid and CompanyId=@CompanyId ", prm);
                     
                    }
                    await _db.ExecuteAsync($"Update SalesOrderItem set Ingredients=@Ingredients,SalesItem=@SalesItem,Production=@Production where CompanyId=@CompanyId and SalesOrderId=@SalesOrderId and id=@SalesOrderItemId and ItemId=@ItemId ", prm);

                }
                var newstock = AllStock + ManufacturingQuantity;
                var LocationStock = sorgu4.First().LocationsStockCount;
                var newlocationstock = ManufacturingQuantity + LocationStock;
                param.Add("@StockCount", newlocationstock);
                param.Add("@AllStockQuantity", newstock);
                if (BomList.First().ParentId!=null)
                {
                    param.Add("@ParentId", BomList.First().ParentId);

                    string sql4c = $@"select ManufacturingOrderItems.id,ManufacturingOrderItems.OrderId,ManufacturingOrderItems.ItemId,ManufacturingOrderItems.PlannedQuantity from ManufacturingOrderItems 
                    left join ManufacturingOrder on ManufacturingOrder.id=ManufacturingOrderItems.OrderId
                    where ManufacturingOrder.id=@ParentId and ManufacturingOrderItems.ItemId=@ItemId and ManufacturingOrder.CompanyId=@CompanyId ";
                    var sorgu9 = await _db.QueryAsync<UretimMake>(sql4c, param);
                    foreach (var parent in sorgu9)
                    {
                        param.Add("LocationId",BomList.First().LocationId);

                        var stokmiktari = await _control.Count(sorgu4.First().ItemId, BomList.First().LocationId);
                        stokmiktari = stokmiktari >= 0 ? stokmiktari : 0;

                        param.Add("@Manuid", parent.OrderId);
                        param.Add("@Manuitemid", parent.id);
                        string sqld = $@"select id,ISNULL(RezerveCount,0) as RezerveCount from Rezerve where ManufacturingOrderId=@Manuid AND ManufacturingOrderItemId=@Manuitemid and ItemId=@ItemId and LocationId=@LocationId and Status=1";
                        
                        var rezervestockCount = await _db.QueryAsync<LocaVarmı>(sqld, param);
                        var rezervemiktar = rezervestockCount.First().RezerveDeger;
                        if (rezervemiktar >= parent.PlannedQuantity)
                        {
                            param.Add("@RezerveCount", parent.PlannedQuantity);
                            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@Manuid and ManufacturingOrderItemId=@Manuitemid  and Status=1 and ItemId=@ItemId ", param);
                            param.Add("@Availability", 2);
                        }
                        else if (rezervemiktar+ ManufacturingQuantity>=parent.PlannedQuantity)
                        {
                            param.Add("@RezerveCount", parent.PlannedQuantity);
                            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@Manuid and ManufacturingOrderItemId=@Manuitemid  and Status=1 and ItemId=@ItemId ", param);
                            param.Add("@Availability", 2);
                        }
                        else if (rezervemiktar+ ManufacturingQuantity+stokmiktari>=parent.PlannedQuantity)
                        {
                            param.Add("@RezerveCount", parent.PlannedQuantity);
                            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@Manuid and ManufacturingOrderItemId=@Manuitemid  and Status=1 and ItemId=@ItemId ", param);
                            param.Add("@Availability", 2);
                        }
                        else
                        {
                            param.Add("@RezerveCount", rezervemiktar+stokmiktari+ManufacturingQuantity);
                            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where CompanyId=@CompanyId and ManufacturingOrderId=@Manuid and ManufacturingOrderItemId=@Manuitemid  and Status=1 and ItemId=@ItemId ", param);
                            param.Add("@Availability", 0);
                        }
                        await _db.ExecuteAsync($"Update ManufacturingOrderItems set Availability=@Availability where OrderId=@Manuid and id=@Manuitemid and Itemıd=@ItemId and CompanyId=@CompanyId", param);

                    }

                }
                param.Add("@LocationStockId", sorgu4.First().LocationStockId);

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

                await _db.ExecuteAsync($"Update LocationStock Set StockCount=@StockCount where id=@LocationStockId and CompanyId=@CompanyId ", param);
                await _db.ExecuteAsync($"Update Items Set AllStockQuantity=@AllStockQuantity where id=@ItemId and CompanyId=@CompanyId ", param);
                await _db.ExecuteAsync($"Update ManufacturingOrder Set Status=@Status where id=@id and CompanyId=@CompanyId ", param);


            }
            else if (T.Status != 3 && eskiStatus == 3 && T.Status != eskiStatus)
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


                await _db.ExecuteAsync($"Update ManufacturingOrder Set Status=@Status where id=@id and CompanyId=@CompanyId ", param);
            }
            else
            {
                await _db.ExecuteAsync($"Update ManufacturingOrder Set Status=@Status where id=@id and CompanyId=@CompanyId ", param);
            }
        }
        public async Task BuyStockControl(PurchaseOrderInsert T, int? missing)
        {
            int? LocationId = T.DepoId;
            DynamicParameters param = new DynamicParameters();
            param.Add("@Tip", "Ingredients");
            param.Add("@ManufacturingOrderItemId", T.UretimDetayId);
            param.Add("@OrderId", T.UretimId);
            param.Add("@ItemId", T.StokId);
            param.Add("@LocationId", LocationId);
            // materyalin DefaultPrice
            // Bul

             var expectedsorgu = await _db.QueryAsync<LocaVarmı>($@"select ISNULL(SUM(Quantity),0) as Miktar from SatinAlma 
                left join SatinAlmaDetay on SatinAlmaDetay.SatinAlmaId = SatinAlma.id and SatinAlma.DepoId={T.DepoId}
                and OrdersItem.StokId = {T.StokId} where  DurumBelirteci = 1 and SatinAlma.UretimId= {T.UretimId} and SatinAlma.UretimDetayId={T.UretimDetayId}  and SatinAlma.Aktif=1");

            float? expected = expectedsorgu.First().Miktari;

           var sorgu = await _db.QueryAsync<LocaVarmı>($"   select  (Select ISNULL(id, 0) from LocationStock where ItemId =@ItemId and LocationId = @LocationId and CompanyId = @CompanyId)   as LocationStockId, (select ISNULL(DefaultPrice, 0) From Items where CompanyId = @CompanyId and id = @ItemId)as  DefaultPrice,(Select ManufacturingOrderItems.[Availability] from ManufacturingOrderItems where CompanyId=@CompanyId and Tip='Ingredients' and id=@ManufacturingOrderItemId and OrderId=@OrderId and ItemId=@ItemId)as [Availability]", param);
            float DefaultPrice = sorgu.First().VarsayilanFiyat;
            int Availability = sorgu.First().MalzemeDurum;
            float locationstock = await _control.Count(T.StokId, T.DepoId);
            locationstock = locationstock >= 0 ? locationstock : 0;

            param.Add("@Cost", DefaultPrice * T.Miktar);
            param.Add("@LocationStockId", sorgu.First().DepoStokId);


            var Count = await _db.QueryAsync<LocaVarmı>($"Select ISNULL(Rezerve.RezerveCount,0)as RezerveCount from Rezerve where CompanyId=@CompanyId and ItemId=@ItemId and ManufacturingOrderId=@OrderId and ManufacturingOrderItemId=@ManufacturingOrderItemId and Rezerve.Status=1", param);
         
           var Counts = Count.Count()>0 ? Count.First().RezerveDeger :0;

            

            if (Availability == 2)
            {
                param.Add("@Availability", 2);
                param.Add("@RezerveCount", Counts);
                param.Add("@LocationStockCount", locationstock);

            }
            else if (missing * (-1) <= expected && expected > 0)
            {
                param.Add("@Availability", 1);
                param.Add("@RezerveCount", Counts);
                param.Add("@LocationStockCount", locationstock);
            }

            else
            {
                param.Add("@RezerveCount", Counts + locationstock);
                param.Add("@LocationStockCount", 0);
                await _db.ExecuteAsync($"Update LocationStock set StockCount=@LocationStockCount where CompanyId=@CompanyId and   id=@LocationStockId", param);
                param.Add("@Availability", 0);
            }


            param.Add("@Status", 1);

            await _db.ExecuteAsync($"Update Rezerve set  Tip=@Tip,ItemId=@ItemId,RezerveCount=@RezerveCount,LocationId=@LocationId,Status=@Status,LocationStockCount=@LocationStockCount where CompanyId=@CompanyId and ManufacturingOrderId=@OrderId and ItemId=@ItemId and ManufacturingOrderItemId=@ManufacturingOrderItemId ", param);


            string sql = $@"Update ManufacturingOrderItems Set Tip=@Tip,ItemId=@ItemId,Cost=@Cost,Availability=@Availability where CompanyId=@CompanyId and OrderId=@OrderId and ItemId=@ItemId and id=@ManufacturingOrderItemId  ";
            await _db.ExecuteAsync(sql, param);

        }







    }
}
