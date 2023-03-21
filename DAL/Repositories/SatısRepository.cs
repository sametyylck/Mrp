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

        public async Task<int> Insert(SatısDTO T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@CariId", T.CariId);
            prm.Add("@TeslimSuresi", T.TeslimSuresi);
            prm.Add("@OlusturmaTarihi", T.OlusturmaTarihi);
            prm.Add("@DurumBelirteci", 0);
            prm.Add("@SatisIsmi", T.SatisIsmi);
            prm.Add("@Bilgi", T.Bilgi);
            prm.Add("@DepoId", T.DepoId);
            prm.Add("@Aktif", true);

            return await _db.QuerySingleAsync<int>($"Insert into Satis (Tip,CariId,TeslimSuresi,OlusturmaTarihi,SatisIsmi,DepoId,Bilgi,Aktif,DurumBelirteci) OUTPUT INSERTED.[id] values (@Tip,@CariId,@TeslimSuresi,@OlusturmaTarihi,@SatisIsmi,@DepoId,@Bilgi,@Aktif,@DurumBelirteci)", prm);
        }
        public async Task<int> InsertPurchaseItem(SatısInsertItem T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@StokId", T.StokId);
            var liste = await _db.QueryAsync<LocaVarmı>($@"select 
            (select VergiDegeri from Vergi where id=(select VarsayilanSatinAlimVergi from GenelAyarlar))as VergiDegeri,
            (select VarsayilanFiyat from Urunler where id =@StokId)as VarsayilanFiyat", prm);
            prm.Add("@VergiId", T.VergiId);
            float rate = await _db.QueryFirstAsync<int>($"select  VergiDegeri from Vergi where id =@VergiId", prm);


            var PriceUnit = liste.First().VarsayilanFiyat;

            var ToplamTutar = (T.Miktar * PriceUnit); //adet*fiyat
            float? VergiTutari = (ToplamTutar * rate) / 100; //tax fiyatı hesaplama
            var TumToplam = ToplamTutar + VergiTutari; //toplam fiyat hesaplama  
            prm.Add("@Miktar", T.Miktar);
            prm.Add("@BirimFiyat", PriceUnit);
            prm.Add("@VergiOrani", rate);
            prm.Add("@OrdersId", T.SatisId);
            prm.Add("@VergiTutari", VergiTutari);
            prm.Add("@ToplamTutar", ToplamTutar);
            prm.Add("@TumToplam", TumToplam);
            prm.Add("@location", T.DepoId);
            prm.Add("@CariId", T.CariId);
            prm.Add("@Durus", 0);



            string sqla = $@"select
                (Select ISNULL(Tip,'') from Urunler where id = @StokId)as Tip,     
                (Select ISNULL(id,0) from DepoStoklar where StokId =  @StokId  and DepoId = @location)   as    DepoStokId,
               (select ISNULL(SUM(Uretim.PlanlananMiktar),0) as Miktar from Uretim where     Uretim.StokId=@StokId and  Uretim.CariId=@CariId )as PlanlananMiktar";
            var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, prm);
            var Stock = await _control.Count(T.StokId, T.DepoId);
            var locationStockId = sorgu.First().DepoStokId;
            var tip = sorgu.First().Tip;
            prm.Add("@LocationStockId", locationStockId);
            int rezervid = 0;

            //STOKDA ürün var mı kontrol
            rezervid = await Control(T, T.SatisId, tip, CompanyId);
            if (T.Durum == 3)
            {
                prm.Add("@SatisOgesi", 3);
            }
            else
            {
                prm.Add("@SatisOgesi", 1);
            }


            if (T.Durum == 3)
            {
                prm.Add("@SatisOgesi", 3);
                prm.Add("@Uretme", 4);
                prm.Add("@Ingredient", 3);

                await _db.ExecuteAsync($"Update Satis set TumToplam=@TumToplam where id=@OrdersId", prm);


                int itemid = await _db.QuerySingleAsync<int>($"Insert into SatisDetay(StokId,Miktar,BirimFiyat,VergiId,VergiOrani,SatisId,SatisOgesi,ToplamTutar,VergiTutari,TumToplam,Durus,Malzemeler,Uretme) OUTPUT INSERTED.[id] values (@StokId,@Miktar,@BirimFiyat,@VergiId,@VergiOrani,@OrdersId,@SatisOgesi,@ToplamTutar,@VergiTutari,@TumToplam,@Durus,@Ingredient,@Uretme)", prm);
                prm.Add("@SatisDetayId", itemid);
                prm.Add("@RezerveId", rezervid);

                await _db.ExecuteAsync($"Update Rezerve set SatisDetayId=@SatisDetayId where  SatisId=@OrdersId and DepoId=@location and id=@RezerveId  ", prm);
                return itemid;
            }
            if (tip != "Material")
            {
                await IngredientsControl(T, T.SatisId, CompanyId);
                if (T.Conditions == 3)
                {
                    prm.Add("@Ingredient", 2);
                }
                else
                {
                    prm.Add("@Ingredient", 0);
                }
            }
            if (tip == "Material")
            {
                prm.Add("@Ingredient", 0);

            }
            string sqlquery = $@"select * from Uretim where CariId is null and SatisId is null and SatisDetayId is null and Durum!=3 and Ozel='false' and Aktif=1 and StokId=@StokId and DepoId=@location Order by id DESC";
            var EmptyManufacturing = await _db.QueryAsync<SalesOrderUpdateMakeBatchItems>(sqlquery, prm);



            await _db.ExecuteAsync($"Update Satis set TumToplam=@TumToplam where id=@OrdersId", prm);
            prm.Add("@Uretme", 0);

            int id = await _db.QuerySingleAsync<int>($"Insert into SatisDetay(StokId,Miktar,BirimFiyat,VergiId,VergiOrani,SatisId,ToplamTutar,VergiTutari,TumToplam,Durus,SatisOgesi,Malzemeler,Uretme) OUTPUT INSERTED.[id] values (@StokId,@Miktar,@BirimFiyat,@VergiId,@VergiOrani,@OrdersId,@ToplamTutar,@VergiTutari,@TumToplam,@Durus,@SatisOgesi,@Ingredient,@Uretme)", prm);

            prm.Add("@SatisDetayId", id);
            prm.Add("@id", T.SatisId);
            if (tip != "Material")
            {
                if (EmptyManufacturing.Count() != 0)
                {
                    var degerler = 0;
                    string sqlp = $" select ISNULL(RezerveDeger,0) from Rezerve where SatisId=@id and  DepoId=@location and StokId=@StokId ";
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
                    float? aranandeger = T.Miktar - degerler;
                    if (aranandeger == 0)
                    {

                    }
                    else
                    {
                        foreach (var item in EmptyManufacturing)
                        {
                            float toplamuretimadeti = item.PlanlananMiktar;
                            if (varmi == 0)
                            {


                                if (toplamuretimadeti >= aranandeger)
                                {
                                    prm.Add("@SatisId", T.SatisId);
                                    prm.Add("@SatisDetayId", id);
                                    prm.Add("@StokId", T.StokId);
                                    prm.Add("@UretimId", item.id);
                                    prm.Add("@CariId", T.CariId);

                                    prm.Add("@SatisOgesi", 2);
                                    await _db.ExecuteAsync($@"Update SatisDetay set SatisOgesi=@SatisOgesi where id=@SatisDetayId and SatisId=@SatisId", prm);
                                    await _db.ExecuteAsync($@"Update Uretim set CariId=@CariId,SatisId=@SatisId,SatisDetayId=@SatisDetayId where id=@UretimId", prm);

                                    string sqlr = $@"(select MIN(UretimDetay.MalzemeDurum)as Malzemeler from UretimDetay
                    left join Uretim on Uretim.id=UretimDetay.UretimId
                    where Uretim.id=@UretimId  and UretimDetay.Tip='Malzemeler' and UretimDetay.)";
                                    var availability = await _db.QueryAsync<int>(sqlr, prm);
                                    prm.Add("@Malzemeler", availability.First());
                                    await _db.ExecuteAsync($"Update SatisDetay set Malzemeler=@Malzemeler where  SatisId=@SatisId and id=@SatisDetayId and StokId=@StokId ", prm);
                                    varmi++;

                                }
                                else if (toplamuretimadeti < aranandeger)
                                {

                                    prm.Add("@SatisId", T.SatisId);
                                    prm.Add("@SatisDetayId", id);
                                    prm.Add("@StokId", T.StokId);
                                    prm.Add("@UretimId", item.id);
                                    prm.Add("@CariId", T.CariId);
                                    prm.Add("@SatisOgesi", 1);
                                    await _db.ExecuteAsync($@"Update SatisDetay set SatisOgesi=@SatisOgesi where id=@SatisDetayId and SatisId=@SatisId", prm);
                                    await _db.ExecuteAsync($@"Update Uretim set CariId=@CariId,SatisId=@SatisId,SatisDetayId=@SatisDetayId where id=@UretimId", prm);

                                    string sqlr = $@"(select MIN(UretimDetay.MalzemeDurum)as Malzemeler from UretimDetay
                    left join Uretim on Uretim.id=UretimDetay.retimId
                    where Uretim.id=@UretimId  and UretimDetay.Tip='Malzemeler' and UretimDetay.)";
                                    var availability = await _db.QueryAsync<int>(sqlr, prm);
                                    prm.Add("@Malzemeler", availability.First());
                                    await _db.ExecuteAsync($"Update SatisDetay set Malzemeler=@Malzemeler where SatisId=@SatisId and id=@SatisDetayId and StokId=@StokId ", prm);
                                    aranandeger = aranandeger - toplamuretimadeti;


                                }
                            }


                        }
                    }



                }
            }

            await _db.ExecuteAsync($"Update Rezerve set SatisDetayId=@SatisDetayId where   SatisId=@OrdersId and DepoId=@location and SatisDetayId is null ", prm);
            return id;


        }
        public async Task<int> Control(SatısInsertItem T, int OrdersId, string? Tip, int CompanyId)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SatisId", OrdersId);
            prm.Add("@StokId", T.StokId);
            prm.Add("@CariId", T.CariId);
            prm.Add("@location", T.DepoId);
            prm.Add("@Tip", Tip);

            //kullanılabilir stok sayisi
            var rezervecount = await _control.Count(T.StokId, T.DepoId);
            rezervecount = rezervecount >= 0 ? rezervecount : 0;

            if (rezervecount >= T.Miktar)//Stok sayısı istesnilenden büyük ise rezerve sayısı adet olur
            {
                prm.Add("@RezerveCount", T.Miktar);
                prm.Add("@LocationStockCount", rezervecount);
                T.Durum = 3;

            }
            else
            {
                prm.Add("@RezerveCount", rezervecount);//Stok sayısı adetten kücük ise rezer sayısı Stok adeti kadar olur.
                prm.Add("@LocationStockCount", rezervecount);
                T.Durum = 1;
            }
            if (Tip == "Material")
            {
                prm.Add("@Durum", 3);

            }
            else
            {
                prm.Add("@Durum", 1);

            }

            return await _db.QuerySingleAsync<int>($"Insert into Rezerve (SatisId,Tip,StokId,RezerveDeger,DepoId,Durum) OUTPUT INSERTED.[id]  values (@SatisId,@Tip,@StokId,@RezerveCount,@location,@Durum)", prm);
        }
        public async Task IngredientsControl(SatısInsertItem T, int OrdersId, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SatisId", OrdersId);
            prm.Add("@StokId", T.StokId);
            prm.Add("@CariId", T.CariId);
            prm.Add("@location", T.DepoId);
            var BomList = await _db.QueryAsync<BOM>($"Select * From UrunRecetesi where  MamulId = {T.StokId} and Aktif = 1");
            var b = 0;


            foreach (var item in BomList)
            {

                DynamicParameters prm2 = new DynamicParameters();
                prm2.Add("@StokId", item.MalzemeId);
                prm2.Add("@location", T.DepoId);
                string sqlb = $@"select
                (Select ISNULL(Tip,'') from Urunler where id = @StokId)as Tip,
                 (Select ISNULL(id,0) from DepoStoklar where StokId = @StokId  and DepoId = @location) as     DepoStokId ";
                var sorgu1 = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqlb, prm2);
                prm2.Add("@LocationStockId", sorgu1.First().DepoStokId);
                prm2.Add("@stockId", sorgu1.First().StokId);

                var RezerveCount = await _control.Count(item.MalzemeId, T.DepoId);//stocktaki adet
                RezerveCount = RezerveCount >= 0 ? RezerveCount : 0;

                var stokcontrol = T.Miktar * item.Miktar; //bir materialin kaç tane gideceği hesaplanıyor
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
                prm2.Add("@Durum", 1);
                prm2.Add("@OrdersId", OrdersId);
                prm2.Add("@Tip", sorgu1.First().Tip);
                prm2.Add("@ContactsId", T.CariId);
                await _db.ExecuteAsync($"Insert into Rezerve(SatisId,Tip,StokId,RezerveDeger,DepoId,Durum) values (@OrdersId,@Tip,@StokId,@RezerveCount,@location,@Durum)", prm2);



            }
        }
        public async Task Update(SalesOrderUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@CariId", T.CariId);
            prm.Add("@TeslimSuresi", T.TeslimSuresi);
            prm.Add("@OlusturmaTarihi", T.OlusturmaTarihi);
            prm.Add("@SatisIsmi", T.SatisIsmi);
            prm.Add("@DepoId", T.DepoId);
            prm.Add("@Total", T.Toplam);
            prm.Add("@Bilgi", T.Bilgi);
            var location = await _db.QueryAsync<int>($"Select DepoId from Satis where id=@id", prm);
            prm.Add("@eskilocationId", location.First());


            if (location.First() == T.DepoId)
            {
                await _db.ExecuteAsync($"Update Satis set CariId=@CariId,TeslimSuresi=@TeslimSuresi,OlusturmaTarihi=@OlusturmaTarihi,SatisIsmi=@SatisIsmi,Bilgi=@Bilgi,DepoId=@DepoId,TumToplam=@Total where id=@id", prm);
            }
            else
            {
                await _db.ExecuteAsync($"Update Satis set CariId=@CariId,TeslimSuresi=@TeslimSuresi,OlusturmaTarihi=@OlusturmaTarihi,SatisIsmi=@SatisIsmi,Bilgi=@Bilgi,DepoId=@DepoId,TumToplam=@Total where id=@id", prm);

                List<Manufacturing> ItemsCount = (await _db.QueryAsync<Manufacturing>($"select StokId,RezerveDeger from Rezerve where SatisId=@id and Durum=1", prm)).ToList();
                foreach (var item in ItemsCount)
                {
                    prm.Add("@StokId", item.StokId);
                    await _db.ExecuteAsync($"Delete from Rezerve where SatisId=@id and StokId=@StokId", prm);

                }
                List<ManufacturingOrderItemsIngredientsUpdate> Itemdegerler = (await _db.QueryAsync<ManufacturingOrderItemsIngredientsUpdate>($@"select StokId,Miktar,SatisDetay.id from SatisDetay 
                inner join Satis on Satis.id=SatisDetay.SatisId
                where SatisDetay.SatisId=@id and Satis.Aktif=1 and Satis.DurumBelirteci!=4", prm)).ToList();
                foreach (var item in Itemdegerler)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@StokId", item.StokId);
                    param.Add("@location", T.DepoId);
                    param.Add("@id", T.id);
                    param.Add("@OrderItemId", item.id);
                    param.Add("@CariId", T.CariId);

                    string sqla = $@"select
                    (Select ISNULL(Tip,'') from Urunler where id = @StokId )as Tip,
                    (Select ISNULL(id,0) from DepoStoklar where StokId=@StokId and DepoId = @location)   as    DepoStokId,
                     (select ISNULL(SUM(Uretim.PlanlananMiktar),0) as Miktar from Uretim where  Uretim.StokId=@StokId and Uretim.CariId=@CariId )as ManufacturingQuantity";
                    var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, param);
                    var RezerveCount = await _control.Count(item.StokId, T.DepoId);
                    RezerveCount = RezerveCount >= 0 ? RezerveCount : 0;

                    var locationStockId = sorgu.First().DepoStokId;
                    var tip = sorgu.First().Tip;
                    param.Add("@LocationStockId", locationStockId);
                    var stokkontrol = await _control.Count(item.StokId, T.DepoId);
                    int rezervid = 0;
                    SatısInsertItem A = new SatısInsertItem();
                    A.StokId = item.StokId;
                    A.DepoId = T.DepoId;
                    A.CariId = T.CariId;
                    A.Miktar = item.Miktar;
                    if (RezerveCount > 0)
                    {


                        rezervid = await Control(A, T.id, tip, CompanyId);
                        if (A.Durum == 3)
                        {
                            param.Add("@SatisOgesi", 3);
                        }
                        else
                        {
                            param.Add("@SatisOgesi", 1);
                        }

                    }

                    else
                        param.Add("@SatisOgesi", 1);
                    if (A.Durum == 3)
                    {
                        param.Add("@SatisOgesi", 3);
                        param.Add("@Uretme", 4);
                        param.Add("@Ingredient", 3);


                        await _db.ExecuteAsync($"Update SatisDetay set SatisOgesi=@SatisOgesi,Uretme=@Uretme,Malzemeler=@Ingredient where id=@OrderItemId", param);

                        param.Add("@RezerveId", rezervid);

                        await _db.ExecuteAsync($"Update Rezerve set SatisDetayId=@OrderItemId where CariId=@CariId and SatisId=@id and DepoId=@location and id=@RezerveId ", param);

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
                        param.Add("@Uretme", 0);

                        await _db.ExecuteAsync($"Update Rezerve set SatisDetayId=@OrderItemId where  SatisId=@id and DepoId=@location and SatisDetayId is null ", param);

                        await _db.ExecuteAsync($"Update SatisDetay set SatisOgesi=@SatisOgesi,Uretme=@Uretme,Malzemeler=@Ingredient where id=@OrderItemId", param);

                    }

                }


            }
        }
        public async Task UpdateItems(SatısUpdateItems T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.SatisId);
            prm.Add("@OrderItemId", T.id);
            prm.Add("@StokId", T.StokId);
            prm.Add("@Miktar", T.Miktar);
            prm.Add("@BirimFiyat", T.BirimFiyat);

            prm.Add("@VergiId", T.VergiId);
            prm.Add("@CariId", T.CariId);
            var make = await _db.QueryAsync<SalesOrderRezerve>($"select mo.id from Uretim mo where SatisId=@id and SatisDetayId=@OrderItemId and Aktif=1 and Durum!=3", prm);
            var adetbul = await _db.QueryFirstAsync<float>($"Select Miktar From SatisDetay where id =@OrderItemId and SatisId=@id", prm);
            float eski = adetbul;
            string sqlv = $@"Select StokId  from  SatisDetay where id=@OrderItemId";
            var Item = await _db.QuerySingleAsync<int>(sqlv, prm);
            if (T.StokId != Item)
            {
                if (T.Tip == "Material")
                {
                    prm.Add("@Durum", 3);

                }
                else
                {
                    prm.Add("@Durum", 1);

                }
                List<Manufacturing> ItemsCount = (await _db.QueryAsync<Manufacturing>($"select StokId,RezerveDeger from Rezerve where SatisId=@id and  SatisDetayId=@OrderItemId  and Durum=@Durum", prm)).ToList();
                foreach (var item in ItemsCount)
                {
                    prm.Add("@StokId", item.StokId);
                    await _db.ExecuteAsync($"Delete from  Rezerve where SatisId=@id and SatisDetayId=@OrderItemId and StokId=@StokId", prm);
                }
                var liste = await _db.QueryAsync<LocaVarmı>($@"select 
            (select VergiDegeri from Vergi where id=(select VarsayilanSatinAlimVergi from GenelAyarlar where ))as VergiDegeri,
            (select VarsayilanFiyat from Urunler where id =@StokId and )as VarsayilanFiyat", prm);
                prm.Add("@VergiId", T.VergiId);
                var Birimfiyat = liste.First().VarsayilanFiyat;
                T.BirimFiyat = Birimfiyat;

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
            T.UretimId = makeorderId;



            prm.Add("@location", T.DepoId);

            if (makeorderId == 0 && T.Tip != "Material")
            {
                int degerler;
                string sqlp = $" select ISNULL(RezerveDeger,0) from Rezerve where SatisId=@id and SatisDetayId=@OrderItemId and DepoId=@location and StokId=@StokId and Durum=1";
                var deger = await _db.QueryAsync<int>(sqlp, prm);
                if (deger.Count() == 0)
                {
                    degerler = 0;
                }
                else
                {
                    degerler = deger.First();
                }
                prm.Add("@StokId", T.StokId);
                prm.Add("@Null", null);
                await _db.ExecuteAsync($"Update SatisDetay set Tip=@Null where id=@OrderItemId and SatisId=@id", prm);

                var Rate = await _db.QuerySingleAsync<float>($"(select VergiDegeri from Vergi where id =@VergiId)", prm);
                float TaxRate = Rate;


                var PriceUnit = T.BirimFiyat;
                float totalprice = (T.Miktar * PriceUnit); //adet*fiyat
                float? VergiTutari = (totalprice * TaxRate) / 100; //tax fiyatı hesaplama
                float? total = totalprice + VergiTutari; //toplam fiyat hesaplama  
                prm.Add("@Miktar", T.Miktar);
                prm.Add("@BirimFiyat", PriceUnit);
                prm.Add("@VergiId", T.VergiId);
                prm.Add("@VergiOrani", TaxRate);
                prm.Add("@OrdersId", T.SatisId);
                prm.Add("@VergiTutari", VergiTutari);
                prm.Add("@ToplamTutar", totalprice);
                prm.Add("@TumToplam", total);
                prm.Add("@ContactsId", T.CariId);
                string sqla = $@"select
                  (Select ISNULL(Tip,'') from Urunler where id = @StokId ) as Tip,
                  (Select ISNULL(id,0) from DepoStoklar where StokId = @StokId  and DepoId = @location )   as    DepoStokId ";
                var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, prm);
                var RezerveCount = await _control.Count(T.StokId, T.DepoId);
                RezerveCount = RezerveCount >= 0 ? RezerveCount : 0;
                var locationStockId = sorgu.First().DepoStokId;
                var tip = sorgu.First().Tip;
                prm.Add("@LocationStockId", locationStockId);
                var status = 0;


                if (degerler >= T.Miktar)
                {
                    prm.Add("@RezerveCount", T.Miktar);
                    prm.Add("@LocationStockCount", RezerveCount);
                    prm.Add("@SatisOgesi", 3);
                    status = 3;
                    await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where SatisId=@id and StokId=@StokId and SatisDetayId=@OrderItemId and Durum=1", prm);
                }

                else if (RezerveCount >= T.Miktar - degerler && RezerveCount > 0)
                {
                    prm.Add("@RezerveCount", T.Miktar);
                    prm.Add("@LocationStockCount", RezerveCount);
                    prm.Add("@SatisOgesi", 3);
                    status = 3;
                    await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where SatisId=@id and StokId=@StokId and SatisDetayId=@OrderItemId and Durum=1", prm);
                }
                else
                {
                    prm.Add("@RezerveCount", degerler + RezerveCount);
                    prm.Add("@LocationStockCount", RezerveCount);
                    prm.Add("@SatisOgesi", 1);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where SatisId=@id and StokId=@StokId and SatisDetayId=@OrderItemId and Durum=1", prm);
                }



                if (status == 3)
                {
                    prm.Add("@SatisOgesi", 3);
                    prm.Add("@Uretme", 4);

                    await _db.ExecuteAsync($"Update Satis set TumToplam=@TumToplam where id=@OrdersId", prm);
                    prm.Add("@Ingredient", 3);
                    await _db.ExecuteAsync($"Update SatisDetay set StokId=@StokId,Miktar=@Miktar,BirimFiyat=@BirimFiyat,VergiId=@VergiId,VergiOrani=@VergiOrani,SatisId=@id,ToplamTutar=@ToplamTutar,VergiTutari=@VergiTutari,TumToplam=@TumToplam,SatisOgesi=@SatisOgesi,Malzemeler=@Ingredient,Uretme=@Uretme where id=@OrderItemId and SatisId=@id ", prm);
                }
                else
                {
                    await UpdateIngredientsControl(T, T.SatisId, CompanyId);
                    if (T.Conditions == 3)
                    {
                        prm.Add("@Ingredient", 2);
                    }
                    else
                    {
                        prm.Add("@Ingredient", 0);
                    }
                }

                await _db.ExecuteAsync($"Update Satis set TumToplam=@TumToplam where id=@OrdersId", prm);

                await _db.ExecuteAsync($@"Update SatisDetay set StokId=@StokId,Miktar=@Miktar,BirimFiyat=@BirimFiyat,VergiId=@VergiId,VergiOrani=@VergiOrani,ToplamTutar=@ToplamTutar,VergiTutari=@VergiTutari,
                TumToplam=@TumToplam,SatisOgesi=@SatisOgesi,Malzemeler=@Ingredient where  id=@OrderItemId and SatisId=@id", prm);



            }
            else
            {
                prm.Add("@StokId", T.StokId);
                var Rate = await _db.QueryFirstAsync<float>($"(select VergiDegeri from Vergi where id =@VergiId)", prm);
                float TaxRate = Rate;
                var PriceUnit = T.BirimFiyat;
                float totalprice = (T.Miktar * PriceUnit); //adet*fiyat
                float? VergiTutari = (totalprice * TaxRate) / 100; //tax fiyatı hesaplama
                float? total = totalprice + VergiTutari; //toplam fiyat hesaplama  
                prm.Add("@Miktar", T.Miktar);
                prm.Add("@BirimFiyat", PriceUnit);
                prm.Add("@VergiId", T.VergiId);
                prm.Add("@VergiOrani", TaxRate);
                prm.Add("@OrdersId", T.SatisId);
                prm.Add("@VergiTutari", VergiTutari);
                prm.Add("@ToplamTutar", totalprice);
                prm.Add("@TumToplam", total);
                prm.Add("@ContactsId", T.CariId);
                if (T.Tip == "MakeBatch")
                {
                    await _db.ExecuteAsync($@"Update SatisDetay set Miktar=@Miktar,TumToplam=@TumToplam,BirimFiyat=@BirimFiyat,VergiId=@VergiId,VergiOrani=@VergiOrani where id=@OrderItemId and SatisId=@id", prm);
                    await UpdateMakeBatchItems(T, CompanyId, eski);
                }
                else if (T.Tip == "MakeOrder")
                {
                    prm.Add("@SatisOgesi", 2);
                    await _db.ExecuteAsync($@"Update SatisDetay set Miktar=@Miktar,TumToplam=@TumToplam,BirimFiyat=@BirimFiyat,SatisOgesi=@SatisOgesi,VergiId=@VergiId,VergiOrani=@VergiOrani where  id=@OrderItemId and SatisId=@id", prm);
                    await UpdateMakeItems(T, eski, CompanyId);
                }
                else if (T.Tip == "Material")
                {
                    await _db.ExecuteAsync($@"Update SatisDetay set Miktar=@Miktar,TumToplam=@TumToplam,BirimFiyat=@BirimFiyat,VergiId=@VergiId,VergiOrani=@VergiOrani where id=@OrderItemId and SatisId=@id", prm);
                    var StokMiktari = await _control.Count(T.StokId, T.DepoId);
                    StokMiktari = StokMiktari >= 0 ? StokMiktari : 0;
                    //stocktaki adet

                    string sqlp = $" select id,ISNULL(RezerveCount,0) as RezerveDeger from Rezerve where SatisId=@OrdersId and SatisDetayId=@OrderItemId and DepoId=@location and StokId=@StokId and Durum=3";
                    var deger = await _db.QueryAsync<LocaVarmı>(sqlp, prm);
                    int rezerveid = deger.First().id;
                    prm.Add("@rezerveid", rezerveid);

                    float? rezervecount = deger.First().RezerveDeger;

                    string sqlb = $@"select ISNULL(SUM(Miktar),0) from SatinAlma 
                left join SatinAlmaDetay on SatinAlmaDetay.SatisId = SatinAlma.id and SatinAlma.DepoId=@location
                and SatinAlmaDetay.StokId = @StokId where DurumBelirteci = 1 and SatinAlma.UretimId is null and SatinAlma.SatisId is null  and SatinAlma.Aktif=1";
                    var expected = await _db.QueryFirstAsync<float>(sqlb, prm);

                    if (T.Miktar <= rezervecount)
                    {
                        prm.Add("@SatisOgesi", 3);
                        prm.Add("@Uretme", 4);
                        prm.Add("@Ingredient", 3);
                        prm.Add("@RezerveCount", T.Miktar);

                    }
                    else if (StokMiktari >= T.Miktar - rezervecount)
                    {
                        prm.Add("@SatisOgesi", 3);
                        prm.Add("@Uretme", 4);
                        prm.Add("@Ingredient", 3);

                        prm.Add("@RezerveCount", T.Miktar);


                    }
                    else if (expected >= T.Miktar - rezervecount)
                    {
                        prm.Add("@SatisOgesi", 2);
                        prm.Add("@Uretme", 3);
                        prm.Add("@Ingredient", 1);

                    }
                    else
                    {
                        prm.Add("@SatisOgesi", 1);
                        prm.Add("@Uretme", 0);
                        prm.Add("@Ingredient", 0);

                    }
                    await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where id=@rezerveid ", prm);
                    await _db.ExecuteAsync($@"Update SatisDetay set Miktar=@Miktar,TumToplam=@TumToplam,StokId=@StokId,BirimFiyat=@BirimFiyat,SatisOgesi=@SatisOgesi,Uretme=@Uretme,Malzemeler=@Ingredient,VergiId=@VergiId,VergiOrani=@VergiOrani where  and id=@OrderItemId and SatisId=@id", prm);

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
            prm.Add("@Aktif", true);
            if (A.Tip == "ShippingAddress")
            {
                prm.Add("@Tip", "ShippingAddress");

                await _db.ExecuteAsync($"Update DepoVeAdresler set Tip=@Tip,Ad=@FirstName,Soyisim=@LastName,SirketIsmi=@CompanyName,Telefon=@Phone, Adres1=@AddressLine1,Adres2=@AddressLine2,Sehir=@CityTown,Cadde=@StateRegion,PostaKodu=@ZipPostal,Ulke=@Country where id=@id", prm);
            }
            else if (A.Tip == "BillingAddress")
            {
                prm.Add("@Tip", "BillingAddress");
                await _db.ExecuteAsync($"Update DepoVeAdresler set Tip=@Tip,Ad=@FirstName,Soyisim=@LastName,SirketIsmi=@CompanyName,Telefon=@Phone, Adres1=@AddressLine1,Adres2=@AddressLine2,Sehir=@CityTown,Cadde=@StateRegion,PostaKodu=@ZipPostal,Ulke=@Country where id=@id", prm);
            }
        }
        public async Task<int> InsertAddress(SalesOrderCloneAddress A, int id)
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
            prm.Add("@Aktif", true);
            int adressid = 0;
            if (A.Tip == "BillingAddress")
            {
                adressid = await _db.QuerySingleAsync<int>($"Insert into DepoVeAdresler (Tip,Ad,Soyisim,SirketIsmi,Telefon, Adres1,Adres2,Sehir,Cadde,PostaKodu,Ulke,Aktif)  OUTPUT INSERTED.[id]  values (@Tip,@FirstName,@LastName,@CompanyName,@Phone,@AddressLine1,@AddressLine2,@CityTown,@StateRegion,@ZipPostal,@Country,@Aktif)", prm);
            }
            else if (A.Tip == "ShippingAddress")
            {
                adressid = await _db.QuerySingleAsync<int>($"Insert into DepoVeAdresler (Tip,Ad,Soyisim,SirketIsmi,Telefon, Adres1,Adres2,Sehir,Cadde,PostaKodu,Ulke,Aktif)  OUTPUT INSERTED.[id]  values (@Tip,@FirstName,@LastName,@CompanyName,@Phone,@AddressLine1,@AddressLine2,@CityTown,@StateRegion,@ZipPostal,@Country,@Aktif)", prm);
            }
            return adressid;

        }
        public async Task DeleteItems(SatısDeleteItems T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@OrdersId", T.OrdersId);
            prm.Add("@StokId", T.StokId);
            if (T.StokId != 0)
            {

                await _db.ExecuteAsync($"Delete from  Rezerve  where SatisId=@OrdersId and  SatisDetayId=@id and StokId=@StokId", prm);

                List<ManufacturingOrderA> MANU = (await _db.QueryAsync<ManufacturingOrderA>($@"select Uretim.id from Uretim 
                where  Uretim.SatisId=@OrdersId and Uretim.SatisDetayId=@id and Uretim.StokId=@StokId and Uretim.Aktif=1 ", prm)).ToList();
                int varmı = 1;
                if (MANU.Count() == 0)
                {
                    varmı = 0;
                }


                var status = await _db.QueryFirstAsync<int>($@"select SatisDetay.SatisOgesi from SatisDetay 
                    where SatisDetay.SatisId=@OrdersId and SatisDetay.id=@id", prm);
                await _db.ExecuteAsync($"Delete From SatisDetay  where StokId = @StokId and id=@id and SatisId=@OrdersId", prm);
                if (varmı == 0)
                {

                    if (status != 3)
                    {
                        var BomList = await _db.QueryAsync<BOM>($"Select * From UrunRecetesi where  MamulId =@StokId  and Aktif = 1", prm);
                        foreach (var item in BomList)
                        {
                            prm.Add("@MaterialId", item.MalzemeId);

                            prm.Add("@Durum", 4);
                            await _db.ExecuteAsync($"Delete from  Rezerve where SatisId=@OrdersId and SatisDetayId=@id and  StokId=@MaterialId", prm);

                        }
                        await _db.ExecuteAsync($"Delete from SatisDetay where id=@id and SatisId=@OrdersId  ", prm);

                    }

                }
                else
                {
                    await _db.ExecuteAsync($"Delete from SatisDetay where id=@id and SatisId=@OrdersId ", prm);
                }

            }
            else
            {
                await _db.ExecuteAsync($"Delete From SatisDetay  where id=@id and SatisId=@OrdersId", prm);

            }
        }
        public async Task DeleteStockControl(List<SatısDelete> A, int CompanyId, int User)
        {
            foreach (var T in A)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@id", T.id);
                param.Add("@Aktif", false);
                param.Add("@User", User);
                param.Add("@Date", DateTime.Now);
                var IsActived = await _db.QueryAsync<bool>($"select Aktif from Satis where id=@id", param);
                if (IsActived.First() == false)
                {

                }
                else
                {
                    List<OperationsUpdate> MANU = (await _db.QueryAsync<OperationsUpdate>($"select Uretim.id from Uretim where  SatisId=@id and Aktif=1", param)).ToList();
                    foreach (var item in MANU)
                    {
                        param.Add("@manuid", item.id);
                        await _db.ExecuteAsync($"Update Uretim Set SatisId=NULL , SatisDetayId=NULL , CariId=NULL where id = @manuid  ", param);
                        await _db.ExecuteAsync($"Update Rezerve Set SatisId=NULL , SatisDetayId=NULL , CariId=NULL where UretimId = @manuid ", param);
                    }
                    var order = await _db.QueryAsync<OperationsUpdate>($"select SatinAlma.id from SatinAlma where  SatisId=@id and Aktif=1", param);
                    foreach (var item in order)
                    {
                        param.Add("@orderid", item.id);

                        await _db.ExecuteAsync($"Update SatinAlma Set SatisId=NULL , SatisDetayId=NULL  where id = @orderid", param);

                    }

                    List<Manufacturing> ItemsCount = (await _db.QueryAsync<Manufacturing>($"select StokId,RezerveDeger from Rezerve where SatisId=@id and  Durum=1", param)).ToList();
                    await _db.ExecuteAsync($"Update Satis Set Aktif=@Aktif,SilinmeTarihi=@Date,SilenKullanici=@User where id = @id ", param);
                    foreach (var item in ItemsCount)
                    {
                        param.Add("@StokId", item.StokId);
                        await _db.ExecuteAsync($"Delete from  Rezerve where SatisId=@id and  StokId=@StokId", param);
                    }
                }

            }

        }
        public async Task UpdateMakeItems(SatısUpdateItems T, float eski, int CompanyId)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.SatisId);
            prm.Add("@UretimId", T.UretimId);
            prm.Add("@OrderItemId", T.id);
            prm.Add("@ItemsId", T.StokId);


            var DepoId = await _db.QueryFirstAsync<int>($"Select DepoId From Satis where id =@id", prm);
            prm.Add("@DepoId", DepoId);

            prm.Add("@PlanlananMiktar", T.Miktar);
            string sqlp = $@"Update Uretim Set PlanlananMiktar=@PlanlananMiktar where  id=@UretimId and SatisId=@id and SatisDetayId=@OrderItemId and StokId=@ItemsId  ";
            await _db.ExecuteAsync(sqlp, prm);


            //Eklenen Ordera ait ıtemin  Bomlarını buluyoruz
            var BomList = await _db.QueryAsync<BOM>($"Select id,ISNULL(StokId,0) as MalzemeId,ISNULL(PlanlananMiktar,0) as Miktar,ISNULL(Bilgi,'') as Bilgi from UretimDetay where  UretimDetay.UretimId=@UretimId  and Tip='Malzemeler'", prm);
            float adet = T.Miktar;


            foreach (var item in BomList)
            {

                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Malzemeler");
                param.Add("@UretimDetayId", item.id);
                param.Add("@UretimId", T.UretimId);
                param.Add("@id", T.SatisId);
                param.Add("@OrderItemId", T.id);
                param.Add("@StokId", item.MalzemeId);
                param.Add("@Notes", item.Bilgi);
                if (adet == eski)
                {
                    param.Add("@PlanlananMiktar", item.Miktar);
                }
                else if (adet > eski)
                {
                    float anadeger = item.Miktar / eski;
                    float yenideger = adet - eski;
                    var artışdegeri = yenideger * anadeger;
                    item.Miktar = item.Miktar + artışdegeri;
                    param.Add("@PlanlananMiktar", item.Miktar);
                }
                else
                {
                    var yenideger = item.Miktar / eski;
                    var deger = eski - adet;
                    item.Miktar = item.Miktar - (yenideger * deger);
                    param.Add("@PlanlananMiktar", item.Miktar);
                }


                param.Add("@DepoId", DepoId);
                string sql = $@"Update UretimDetay Set PlanlananMiktar=@PlanlananMiktar where   UretimId=@UretimId and id=@UretimDetayId and StokId=@StokId ";
                await _db.ExecuteAsync(sql, param);
                // materyalin VarsayilanFiyat
                // Bul
                string sqlb = $@"select ISNULL(SUM(Miktar),0) from SatinAlma 
                left join SatinAlmaDetay on SatinAlmaDetay.SatinAlmaId = SatinAlma.id and SatinAlma.DepoId=@DepoId
                and SatinAlmaDetay.StokId = @StokId where  DurumBelirteci = 1 and SatinAlma.UretimId=@UretimId and SatinAlma.Aktif=1 and SatinAlma.Tip='PurchaseOrder' ";
                var expectedsorgu = await _db.QueryAsync<float>(sqlb, param);
                float expected = expectedsorgu.First();



                List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select (Select ISNULL(id, 0) from DepoStoklar where StokId=@StokId and DepoId = @DepoId ) as DepoStokId, (select ISNULL(VarsayilanFiyat, 0) From Urunler where id = @StokId)as  VarsayilanFiyat", param)).ToList();
                float VarsayilanFiyat = sorgu.First().VarsayilanFiyat;
                param.Add("@Tutar", VarsayilanFiyat * item.Miktar);
                param.Add("@LocationStockId", sorgu.First().DepoStokId);

                string sqlTY = $@"select
           (moi.PlanlananMiktar-SUM(ISNULL(SatinAlmaDetay.Miktar,0))-ISNULL((Rezerve.RezerveDeger),0))as Missing
            
            from UretimDetay moi
            left join Urunler on Urunler.id=moi.StokId
            left join Uretim on Uretim.id=moi.UretimId 
			LEFT join Rezerve on Rezerve.UretimId=Uretim.id and Rezerve.UretimDetayId=moi.id and Rezerve.Durum=1
            left join DepoStoklar on DepoStoklar.StokId=Urunler.id and DepoStoklar.DepoId=@DepoId
            left join SatinAlma on SatinAlma.UretimId=Uretim.id and  SatinAlma.SatisId is null  and moi.id=SatinAlma.UretimDetayId
            left join SatinAlmaDetay on SatinAlmaDetay.SatinAlmaId=SatinAlma.id  and DurumBelirteci = 1 and SatinAlmaDetay.StokId=moi.StokId
            where  moi.UretimId = @UretimId and moi.Tip='Malzemeler'  and Uretim.id=@UretimId and moi.id=@UretimDetayId and 
			 Uretim.Durum!=3
            Group by moi.PlanlananMiktar,Rezerve.RezerveDeger";
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


                float RezerveStockCount = await _control.Count(item.MalzemeId, DepoId);
                RezerveStockCount = RezerveStockCount >= 0 ? RezerveStockCount : 0;
                List<int> Count = (await _db.QueryAsync<int>($"Select ISNULL(Rezerve.RezerveDeger,0)as Count from Rezerve where StokId=@StokId and UretimId=@UretimId and UretimDetayId=@UretimDetayId and StokId=@StokId and SatisId=@id and SatisDetayId=@OrderItemId and Rezerve.Durum=1", param)).ToList();
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
                    param.Add("@MalzemeDurum", 2);
                }

                else if (RezerveStockCount >= item.Miktar - Counts)
                {

                    param.Add("@RezerveCount", item.Miktar);
                    param.Add("@LocationStockCount", RezerveStockCount);
                    param.Add("@MalzemeDurum", 2);
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
                        param.Add("@MalzemeDurum", 1);
                        param.Add("@RezerveCount", RezerveStockCount + Counts);

                    }
                    else
                    {
                        param.Add("@MalzemeDurum", 0);
                        param.Add("@RezerveCount", RezerveStockCount + Counts);

                    }

                }

                param.Add("@Durum", 1);

                await _db.ExecuteAsync($"Update Rezerve set  Tip=@Tip,StokId=@StokId,RezerveDeger=@RezerveCount,DepoId=@DepoId,Durum=@Durum where  UretimId=@UretimId and StokId=@StokId and UretimDetayId=@UretimDetayId and SatisId=@id and SatisDetayId=@OrderItemId ", param);





                string sqlw = $@"Update UretimDetay Set Tip=@Tip,StokId=@StokId,Bilgi=@Notes,PlanlananMiktar=@PlanlananMiktar,Tutar=@Tutar,MalzemeDurum=@MalzemeDurum where  UretimId=@UretimId and StokId=@StokId and id=@UretimDetayId  ";
                await _db.ExecuteAsync(sqlw, param);

                if (T.SatisId != 0 || T.SatisId != null)
                {
                    prm.Add("@OrderId", T.SatisId);
                    prm.Add("@StokId", T.StokId);
                    prm.Add("@SatisId", T.id);
                    prm.Add("@SatisDetayId", T.id);
                    prm.Add("@UretimDetayId", item.id);

                    string sqlr = $@"(select MIN(UretimDetay.MalzemeDurum)as Malzemeler from UretimDetay
                    left join Uretim on Uretim.id=UretimDetay.UretimId
                    where Uretim.id=@UretimId and UretimDetay.id=@UretimDetayId and UretimDetay.Tip='Malzemeler')";
                    var availability = await _db.QueryAsync<int>(sqlr, prm);
                    prm.Add("@Malzemeler", availability.First());
                    await _db.ExecuteAsync($"Update SatisDetay set Malzemeler=@Malzemeler where SatisId=@SatisId and id=@SatisDetayId and StokId=@StokId ", prm);
                }

            }








            //Eklenen Ordera ait ıtemin Operation Bomlarını buluyoruz
            var OperationList = await _db.QueryAsync<ProductOperationsBOM>($"Select ISNULL(id,0)As id,ISNULL(OperasyonId,0) as OperasyonId,ISNULL(KaynakId,0)as KaynakId,ISNULL(SaatlikUcret,0)as SaatlikUcret,ISNULL(PlanlananZaman,0)as OperationTime  from UretimDetay where  UretimDetay.UretimId = {T.SatisId} and Tip = 'Operasyonlar'");

            foreach (var item in OperationList)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Operasyonlar");
                param.Add("@OrderId", T.SatisId);
                param.Add("@OperasyonId", item.OperasyonId);
                param.Add("@KaynakId", item.KaynakId);
                if (adet == eski)
                {
                    param.Add("@PlanlananMiktar", item.OperasyonZamani);
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

                param.Add("@PlanlananZaman ", item.OperasyonZamani);
                param.Add("@Durum", 0);
                param.Add("@Tutar", (item.SaatlikUcret / 60 / 60) * item.OperasyonZamani);

                string sql = $@"Update UretimDetay Set Tip=@Tip,OrderId=@OrderId,OperasyonId=@OperasyonId,KaynakId=@KaynakId,PlanlananZaman=@PlanlananZaman,Durum=@Durum,Tutar=@Tutar where UretimId=@OrderId and OperasyonId=@OperasyonId and KaynakId=KaynakId  ";
                await _db.ExecuteAsync(sql, param);
            }
        }
        public async Task UpdateMakeBatchItems(SatısUpdateItems T, int CompanyId, float eskiQuantity)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.SatisId);
            prm.Add("@OrderItemId", T.id);
            prm.Add("@StokId", T.StokId);
            var LocationIdAl = await _db.QueryFirstAsync<int>($"Select DepoId From Satis where id =@id", prm);
            int DepoId = LocationIdAl;
            prm.Add("@DepoId", DepoId);

            var status = await _db.QueryFirstAsync<int>($@"select SatisDetay.SatisOgesi from SatisDetay 
                    where SatisDetay.SatisId=@id and SatisDetay.id=@OrderItemId ", prm);
            int statusId = status;
            prm.Add("@ItemsId", T.StokId);


            //Adet azaltılırken hangi üretimin olacağı belirlenecek
            string sqlsorgu = $@"select Uretim.id,Uretim.PlanlananMiktar from Uretim 
            where Uretim.SatisId=@id  
            and Uretim.SatisDetayId=@OrderItemId and Uretim.Durum!=3 and Uretim.Aktif=1 and Uretim.Ozel='false' and Uretim.StokId=@StokId  Order by id DESC ";
            var Manufacturing = await _db.QueryAsync<SalesOrderUpdateMakeBatchItems>(sqlsorgu, prm);


            string sqlquerys = $@"select SUM(ISNULL(Uretim.PlanlananMiktar,0)) from Uretim 
            where Uretim.SatisId=@id  
            and Uretim.SatisDetayId=@OrderItemId and Uretim.Durum!=3 and Uretim.Aktif=1 and Uretim.Ozel='false' and Uretim.StokId=@StokId";
            var expected = await _db.QueryFirstAsync<int>(sqlquerys, prm);

            //adet yükseltilirken stokta yok ise boşta bir üretim var ise onu alır.
            string sqlquery = $@"select * from Uretim where CariId is null and SatisId is null and SatisDetayId is null and Durum!=3 and Ozel='false' and Aktif=1 and StokId=@ItemsId Order by id DESC";
            var EmptyManufacturing = await _db.QueryAsync<SalesOrderUpdateMakeBatchItems>(sqlquery, prm);

            string sqlb = $@"Select ISNULL(id,0) from DepoStoklar where StokId =@StokId and DepoId = @DepoId";
            var locationstockid = await _db.QueryFirstAsync<int>(sqlb, prm);
            prm.Add("@LocationStockId", locationstockid);
            float degerler;
            string sqlp = $" select ISNULL(RezerveDeger,0) from Rezerve where SatisId=@id and SatisDetayId=@OrderItemId and DepoId=@DepoId and StokId=@StokId";
            var deger = await _db.QueryAsync<int>(sqlp, prm);
            if (deger.Count() == 0)
            {
                degerler = 0;
            }
            else
            {
                degerler = deger.First();
            }

            var RezerveCount = await _control.Count(T.StokId, DepoId);//stocktaki adet
            RezerveCount = RezerveCount >= 0 ? RezerveCount : 0;

            if (degerler > T.Miktar)
            {
                prm.Add("@SatisOgesi", 3);
                prm.Add("@Ingredient", 3);
                prm.Add("@ProductionId", 4);
                prm.Add("@RezerveCount", T.Miktar);
                prm.Add("@LocationStockCount", RezerveCount);
                await _db.ExecuteAsync($@"Update SatisDetay set SatisOgesi=@SatisOgesi,Uretme=@ProductionId,Malzemeler=@Ingredient where id=@OrderItemId and SatisId=@id", prm);
                await _db.ExecuteAsync($"Update Rezerve set  RezerveDeger=@RezerveCount where UretimId=@UretimId and StokId=@StokId and UretimDetayId=@UretimDetayId and SatisId=@id and SatisDetayId=@OrderItemId ", prm);
                if (statusId == 2)
                {

                    if (T.Miktar < eskiQuantity)
                    {
                        foreach (var item in Manufacturing)
                        {
                            await _db.ExecuteAsync($@"Update Uretim set CariId=NULL ,SatisId=NULL,SatisDetayId=NULL where id=@UretimId", prm);
                        }
                    }

                }
            }
            else if (RezerveCount >= T.Miktar - degerler)
            {
                prm.Add("@SatisOgesi", 3);
                prm.Add("@Ingredient", 3);
                prm.Add("@ProductionId", 4);
                prm.Add("@RezerveCount", T.Miktar);
                prm.Add("@LocationStockCount", RezerveCount);
                await _db.ExecuteAsync($@"Update SatisDetay set SatisOgesi=@SatisOgesi,Uretme=@ProductionId,Malzemeler=@Ingredient where id=@OrderItemId and SatisId=@id", prm);
                await _db.ExecuteAsync($"Update Rezerve set  RezerveCount=@RezerveCount where UretimId=@UretimId and StokId=@StokId and UretimDetayId=@UretimDetayId and SatisId=@id and SatisDetayId=@OrderItemId ", prm);
                if (statusId == 2)
                {

                    if (T.Miktar < eskiQuantity)
                    {
                        foreach (var item in Manufacturing)
                        {
                            await _db.ExecuteAsync($@"Update Uretim set CariId=NULL ,SatisId=NULL,SatisDetayId=NULL where id=@UretimId", prm);
                        }
                    }

                }
            }
            else if (eskiQuantity > T.Miktar && Manufacturing.Count() != 0)
            {

                int varmi = 0;
                float toplamuretimadeti = 0;
                var dususmıktarı = eskiQuantity - T.Miktar;
                var uretimfarki = eskiQuantity - expected - degerler;
                uretimfarki = Math.Abs(uretimfarki);

                float dusuculecekdeger = eskiQuantity - T.Miktar;
                foreach (var item in Manufacturing)
                {
                    prm.Add("@UretimId", item.id);
                    toplamuretimadeti = item.PlanlananMiktar;
                    if (varmi == 0)
                    {

                        if (toplamuretimadeti <= dusuculecekdeger)
                        {
                            await _db.ExecuteAsync($@"Update Uretim set CariId=NULL ,SatisId=NULL,SatisDetayId=NULL where id=@UretimId", prm);

                        }


                    }

                }



            }
            else if (eskiQuantity < T.Miktar && EmptyManufacturing.Count() != 0)
            {
                int varmi = 0;
                float toplamuretimadeti = 0;
                float aranandeger = T.Miktar - degerler;
                foreach (var item in EmptyManufacturing)
                {
                    toplamuretimadeti = item.PlanlananMiktar;
                    if (varmi == 0)
                    {


                        if (toplamuretimadeti >= aranandeger)
                        {
                            prm.Add("@SatisOgesi", 2);
                            await _db.ExecuteAsync($@"Update SatisDetay set SatisOgesi=@SatisOgesi where id=@OrderItemId and SatisId=@id", prm);

                            prm.Add("@SatisId", T.SatisId);
                            prm.Add("@SatisDetayId", T.id);
                            prm.Add("@StokId", T.StokId);
                            prm.Add("@UretimId", item.id);
                            prm.Add("@CariId", T.CariId);
                            await _db.ExecuteAsync($@"Update Uretim set CariId=@CariId,SatisId=@SatisId,SatisDetayId=@SatisDetayId where id=@UretimId", prm);

                            string sqlr = $@"(select MIN(UretimDetay.MalzemeDurum)as Malzemeler from UretimDetay
                    left join Uretim on Uretim.id=UretimDetay.UretimId
                    where Uretim.id=@UretimId  and UretimDetay.Tip='Malzemeler' and UretimDetay.)";
                            var availability = await _db.QueryAsync<int>(sqlr, prm);
                            prm.Add("@Malzemeler", availability.First());
                            await _db.ExecuteAsync($"Update SatisDetay set Malzemeler=@Malzemeler where  SatisId=@SatisId and id=@SatisDetayId and StokId=@StokId ", prm);

                            varmi++;

                        }
                        else if (toplamuretimadeti < aranandeger)
                        {
                            prm.Add("@SatisOgesi", 1);
                            await _db.ExecuteAsync($@"Update SatisDetay set SatisOgesi=@SatisOgesi where id=@OrderItemId and SatisId=@id", prm);
                            prm.Add("@SatisId", T.SatisId);
                            prm.Add("@SatisDetayId", T.id);
                            prm.Add("@StokId", T.StokId);
                            prm.Add("@UretimId", item.id);
                            prm.Add("@CariId", T.CariId);
                            await _db.ExecuteAsync($@"Update Uretim set CariId=@CariId,SatisId=@SatisId,SatisDetayId=@SatisDetayId where id=@UretimId", prm);

                            string sqlr = $@"(select MIN(UretimDetay.MalzemeDurum)as Malzemeler from UretimDetay
                    left join Uretim on Uretim.id=UretimDetay.UretimId
                    where Uretim.id=@UretimId  and UretimDetay.Tip='Malzemeler' and UretimDetay.)";
                            var availability = await _db.QueryAsync<int>(sqlr, prm);
                            prm.Add("@Malzemeler", availability.First());
                            await _db.ExecuteAsync($"Update SatisDetay set Malzemeler=@Malzemeler where SatisId=@SatisId and id=@SatisDetayId and StokId=@StokId ", prm);
                            aranandeger = aranandeger - toplamuretimadeti;


                        }
                        else
                        {
                            prm.Add("@SatisOgesi", 1);
                            await _db.ExecuteAsync($@"Update SatisDetay set SatisOgesi=@SatisOgesi where  id=@OrderItemId and SatisId=@id", prm);
                            await UpdateIngredientsControl(T, T.id, CompanyId);
                        }
                    }





                }


            }
            else
            {
                prm.Add("@SatisOgesi", 1);
                await _db.ExecuteAsync($@"Update SatisDetay set SatisOgesi=@SatisOgesi where id=@OrderItemId and SatisId=@id", prm);
                await UpdateIngredientsControl(T, T.id, CompanyId);
            }





        }
        public async Task UpdateIngredientsControl(SatısUpdateItems T, int OrdersId, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SatisId", OrdersId);
            prm.Add("@OrderItemId", T.id);
            prm.Add("@StokId", T.StokId);
            prm.Add("@CariId", T.CariId);
            prm.Add("@location", T.DepoId);
            string sqla = $@"select
        (Select Tip from Urunler where id = @StokId) as Tip,
       (Select ISNULL(id,0) from DepoStoklar where StokId = @StokId  and DepoId = @location ) as  LocationStockId";
            var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, prm);
            var BomList = await _db.QueryAsync<BOM>($"Select * From UrunRecetesi where  MamulId = {T.StokId} and Aktif = 1");
            var b = 0;
            foreach (var item in BomList)
            {

                DynamicParameters prm2 = new DynamicParameters();
                prm2.Add("@StokId", item.MalzemeId);
                prm2.Add("@SatisId", OrdersId);
                prm2.Add("@OrderItemId", T.id);
                prm2.Add("@location", T.DepoId);
                string sqlb = $@"select
                (Select ISNULL(Tip,'') from Urunler where id = @StokId)as Tip,
                 (Select ISNULL(id,0) from DepoStoklar where StokId =  @StokId and DepoId = @location ) as     DepoStokId";
                var sorgu1 = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqlb, prm2);
                int degerler;
                string sqlp = $" select ISNULL(RezerveDeger,0) from Rezerve where SatisId=@SatisId and SatisDetayId=@OrderItemId and DepoId=@location and StokId=@StokId and  Durum=1";
                var deger = await _db.QueryAsync<int>(sqlp, prm2);
                if (deger.Count() == 0)
                {
                    degerler = 0;
                }
                else
                {
                    degerler = deger.First();
                }




                var RezerveCount = await _control.Count(item.MalzemeId, T.DepoId);
                RezerveCount = RezerveCount >= 0 ? RezerveCount : 0;
                //stocktaki adet
                var stokcontrol = T.Miktar * item.Miktar; //bir materialin kaç tane gideceği hesaplanıyor
                if (deger.Count() == 0)
                {


                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@StokId", item.MalzemeId);
                    param2.Add("@location", T.DepoId);
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
                    prm2.Add("@Durum", 1);
                    prm2.Add("@OrdersId", OrdersId);
                    prm2.Add("@Tip", sorgu1.First().Tip);
                    prm2.Add("@ContactsId", T.CariId);
                    prm2.Add("@OrderItemId", T.id);
                    await _db.ExecuteAsync($"Insert into Rezerve (SatisId,SatisDetayId,Tip,StokId,RezerveDeger,DepoId,Durum) values (@OrdersId,@OrderItemId,@Tip,@StokId,@RezerveCount,@location,@Durum)", prm2);




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
                prm2.Add("@Durum", 1);
                prm2.Add("@OrdersId", OrdersId);
                prm2.Add("@Tip", sorgu1.First().Tip);
                prm2.Add("@ContactsId", T.CariId);
                await _db.ExecuteAsync($"Update Rezerve set Tip=@Tip,RezerveDeger=@RezerveCount,DepoId=@location,Durum=@Durum where SatisId=@OrdersId and StokId=@StokId and Durum=1", prm2);



            }
            if (T.id != 0)
            {
                prm.Add("@OrderId", OrdersId);
                prm.Add("@StokId", T.StokId);
                prm.Add("@SatisId", T.SatisId);
                prm.Add("@SatisDetayId", T.id);

                if (b > 0)
                {
                    prm.Add("@Malzemeler", 0);
                }
                else
                {
                    prm.Add("@Malzemeler", 2);
                }

                await _db.ExecuteAsync($"Update SatisDetay set Malzemeler=@Malzemeler where  SatisId=@SatisId and id=@SatisDetayId and StokId=@StokId ", prm);
            }

        }
        public async Task<int> Make(SalesOrderMake T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@StokId", T.StokId);
            prm.Add("@SatisId", T.SatisId);
            prm.Add("@SatisDetayId", T.SatisDetayId);
            prm.Add("@CariId", T.CariId);
            prm.Add("@location", T.DepoId);
            prm.Add("@Tip", T.Tip);

            var StokId = T.StokId;
            int id = 0;
            UretimDTO A = new UretimDTO();
            A.StokId = T.StokId;
            A.PlananlananMiktar = T.PlanlananMiktar;
            A.DepoId = T.DepoId;
            A.Tip = T.Tip;
            A.BeklenenTarih = T.BeklenenTarih;
            A.OlusturmTarihi = T.OlusturmaTarihi;
            A.UretimTarihi = T.UretimTarihi;
            if (T.Tip == "MakeBatch")
            {
                if (T.SatisId != 0)
                {
                    await _db.ExecuteAsync($"Update SatisDetay set Tip=@Tip where id=@SatisDetayId and SatisId=@SatisId ", prm);
                }
                id = await _manufacturingOrderItem.Insert(A, CompanyId);
                prm.Add("@Manufacid", id);
                await _db.ExecuteAsync($"Update Uretim set SatisId=@SatisId , SatisDetayId=@SatisDetayId , CariId=@CariId where id=@Manufacid ", prm);
                await _manufacturingOrderItem.InsertOrderItems(id, T.StokId, T.DepoId, T.PlanlananMiktar, T.SatisId, T.SatisDetayId);

            }
            else if (T.Tip == "MakeOrder")
            {
                if (T.SatisId != 0)
                {
                    await _db.ExecuteAsync($"Update SatisDetay set Tip=@Tip where id=@SatisDetayId and SatisId=@SatisId ", prm);
                }
                List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select (Select ISNULL(id, 0) from DepoStoklar where StokId =  @StokId  and DepoId = @location )   as DepoStokId, (select ISNULL(VarsayilanFiyat, 0) From Urunler where id = @StokId)as  VarsayilanFiyat,(select Rezerve.RezerveDeger from Rezerve where StokId=@StokId and DepoId=@location and SatisId=@SatisId and SatisDetayId=@SatisDetayId)as RezerveDeger", prm)).ToList();
                float? rezervecount = sorgu.First().RezerveDeger;
                float LocationStockId = sorgu.First().DepoStokId;
                prm.Add("@LocationStockId", LocationStockId);
                var make = await _db.QueryAsync<SalesOrderRezerve>($"select mo.id from Uretim mo where SatisId=@SatisId and SatisDetayId=@SatisDetayId  and Aktif=1 and Durum!=3", prm);

                foreach (var item in make)
                {
                    prm.Add("@manuid", item.id);
                    prm.Add("@Null", null);
                    await _db.ExecuteAsync($"Update Uretim set SatisId=@Null , SatisDetayId=@Null , CariId=@Null where id=@manuid", prm);

                }



                if (rezervecount != null)
                {
                    await _db.ExecuteAsync($"Delete from  Rezerve where SatisId=@SatisId and SatisDetayId=@SatisDetayId and  StokId!=@StokId", prm);
                    await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=0  where SatisId=@SatisId and SatisDetayId=@SatisDetayId and  StokId=@StokId", prm);

                    id = await _manufacturingOrderItem.Insert(A, CompanyId);
                    prm.Add("@Manufacid", id);

                    await _db.ExecuteAsync($"Update Uretim set SatisId=@SatisId , SatisDetayId=@SatisDetayId , CariId=@CariId where id=@Manufacid ", prm);
                    await _manufacturingOrderItem.InsertOrderItems(id, T.StokId, T.DepoId, T.PlanlananMiktar, T.SatisId, T.SatisDetayId);
                }
                else
                {
                    id = await _manufacturingOrderItem.Insert(A, CompanyId);
                    prm.Add("@Manufacid", id);

                    await _db.ExecuteAsync($"Update Uretim set SatisId=@SatisId , SatisDetayId=@SatisDetayId , CariId=@CariId where id=@Manufacid  ", prm);
                    await _manufacturingOrderItem.InsertOrderItems(id, T.StokId, T.DepoId, T.PlanlananMiktar, T.SatisId, T.SatisDetayId);
                }


            }
            var sales = await _db.QueryAsync<int>($@"select SatisOgesi from SatisDetay where id=@SatisDetayId", prm);
            int salesId = sales.First();
            if (salesId != 1)
            {
                if (salesId == 2)
                {
                    prm.Add("@ProductionId", 1);
                    await _db.ExecuteAsync($"Update SatisDetay set Uretme=@ProductionId where id=@SatisDetayId", prm);
                }
                else if (salesId == 3)
                {
                    prm.Add("@ProductionId", 4);
                    await _db.ExecuteAsync($"Update SatisDetay set Uretme=@ProductionId where id=@SatisDetayId", prm);
                }
            }
            else if (salesId == 1)
            {
                prm.Add("@ProductionId", 0);
                await _db.ExecuteAsync($"Update SatisDetay set Uretme=@ProductionId where id=@SatisDetayId", prm);
            }



            return id;


        }
        public async Task DoneSellOrder(SalesDone T, int CompanyId, int UserId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@DurumBelirteci", T.DurumBelirteci);
            string sql = $@"select DurumBelirteci from Satis where id=@id ";
            var st = await _db.QueryAsync<int>(sql, param);
            int eskiStatus = st.First();

            if (eskiStatus == 0)
            {

                if (T.DurumBelirteci == 2)
                {
                    var List = await _db.QueryAsync<SatisDetay>($@"select SatisDetay.StokId,SatisDetay.id,SatisDetay.SatisId,SatisDetay.Miktar,DepoId from Satis 
                     left join SatisDetay on SatisDetay.SatisId=Satis.id
                     where Satis.id=@id ", param);
                    param.Add("@DepoId", List.First().DepoId);
                    foreach (var item in List)
                    {
                        param.Add("@SatisDetayId", item.id);
                        param.Add("@StokId", item.StokId);
                        var make = await _db.QueryAsync<SalesOrderRezerve>($"select mo.id,Durum from Uretim mo where SatisId=@id and SatisDetayId=@SatisDetayId and StokId=@StokId and  Durum!=3 and Aktif=1", param);


                        if (make.Count() != 0)
                        {
                            foreach (var items in make)
                            {
                                if (items.Durum != 3)
                                {
                                    int Durum = 3;
                                    UretimTamamlama tamam = new();
                                    tamam.id = items.id;
                                    tamam.Durum = Durum;
                                    await _manufacturingOrderItem.DoneStock(tamam, UserId);
                                }

                            }
                            param.Add("@Durus", 1);
                            param.Add("@Malzemeler", 3);
                            param.Add("@SatisOgesi", 4);
                            param.Add("@Uretme", 4);
                            await _db.ExecuteAsync($"Update SatisDetay set Malzemeler=@Malzemeler,Durus=@Durus,SatisOgesi=@SatisOgesi,Uretme=@Uretme where SatisId=@id and id=@SatisDetayId and StokId=@StokId ", param);
                            param.Add("@Durum", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Durum=@Durum where  SatisId=@id and SatisDetayId=@SatisDetayId", param);

                        }
                        else
                        {

                            param.Add("@Durus", 1);
                            param.Add("@Malzemeler", 3);
                            param.Add("@SatisOgesi", 4);
                            param.Add("@Uretme", 4);
                            await _db.ExecuteAsync($"Update SatisDetay set Malzemeler=@Malzemeler,Durus=@Durus,SatisOgesi=@SatisOgesi,Uretme=@Uretme where SatisId=@id and id=@SatisDetayId and StokId=@StokId ", param);
                            param.Add("@Durum", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Durum=@Durum where SatisId=@id and SatisDetayId=@SatisDetayId", param);



                        }
                        //        List<SalesOrderUpdateItems> aa = (await _db.QueryAsync<SalesOrderUpdateItems>($@"select o.id,oi.id as OrderItemId,oi.Miktar,oi.StokId,oi.BirimFiyat,oi.VergiId,o.CariId,o.DepoId,o.TeslimSuresi from SatinAlma o 
                        //left join SatinAlmaDetay oi on oi.OrdersId = o.id where o. and oi.Durus = 0 and o.Aktif = 1 and o.id=@id and oi.StokId = @StokId and o.DurumBelirteci = 0 Order by o.TeslimSuresi ", param)).ToList();
                        //        SatısUpdateItems A = new SatısUpdateItems();
                        //        foreach (var liste in aa)
                        //        {
                        //            A.id = item.id;
                        //            A.SatisId = liste.id;
                        //            A.VergiId = liste.VergiId;
                        //            A.Miktar = liste.Miktar;
                        //            A.StokId = liste.StokId;
                        //            A.CariId = liste.CariId;
                        //            A.DepoId = liste.DepoId;
                        //            A.TeslimSuresi = liste.TeslimSuresi;
                        //            A.BirimFiyat = liste.BirimFiyat;
                        //            param.Add("@RezerveCount", 0);
                        //            await _db.ExecuteAsync($"Update Rezerve set RezerveCount=@RezerveCount where  and SatisId={liste.id} and SatisDetayId={liste.OrderItemId} and StokId={liste.StokId} ", param);
                        //            await UpdateItems(A, CompanyId);
                        //        }


                    }



                    param.Add("@DurumBelirteci", 2);
                    await _db.ExecuteAsync($"Update Satis set DurumBelirteci=@DurumBelirteci where id=@id", param);
                }
                else if (T.DurumBelirteci == 4)
                {

                    var List = await _db.QueryAsync<SatisDetay>($@"select SatisDetay.StokId,SatisDetay.id,SatisDetay.SatisId as SatisId,SatisDetay.Miktar,DepoId from Satis 
                left join SatisDetay on SatisDetay.SatisId=Satis.id
                where Satis.id=@id ", param);
                    param.Add("@DepoId", List.First().DepoId);
                    foreach (var item in List)
                    {
                        param.Add("@SatisDetayId", item.id);
                        param.Add("@StokId", item.StokId);
                        var make = await _db.QueryAsync<SalesOrderRezerve>($"select mo.id,Durum from Uretim mo where SatisId=@id and SatisDetayId=@SatisDetayId and StokId=@StokId and  Durum!=3 and Aktif=1", param);


                        if (make.Count() != 0)
                        {
                            foreach (var items in make)
                            {
                                if (items.Durum != 3)
                                {
                                    int Durum = 3;
                                    UretimTamamlama tamam = new();
                                    tamam.id = items.id;
                                    tamam.Durum = Durum;
                                    await _manufacturingOrderItem.DoneStock(tamam, UserId);
                                }

                            }



                            param.Add("@Durus", 2);
                            param.Add("@Malzemeler", 3);
                            param.Add("@SatisOgesi", 4);
                            param.Add("@Uretme", 4);
                            await _db.ExecuteAsync($"Update SatisDetay set Malzemeler=@Malzemeler,Durus=@Durus,SatisOgesi=@SatisOgesi,Uretme=@Uretme where SatisId=@id and id=@SatisDetayId and StokId=@StokId ", param);
                            param.Add("@Durum", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Durum=@Durum where SatisId=@id and SatisDetayId=@SatisDetayId", param);

                            List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($" select (Select ISNULL(StokAdeti, 0) from DepoStoklar where StokId = @StokId  and DepoId = @DepoId) as StokMiktar, (Select ISNULL(id, 0) from DepoStoklar where StokId = @StokId and DepoId = @DepoId ) as DepoStokId,(select RezerveDeger from Rezerve where SatisId=@id and StokId=@StokId and Durum!=4) as RezerveDeger ", param)).ToList();
                            var stockall = sorgu.First().Miktari;




                            float? newstock = sorgu.First().StokMiktar - item.Miktar;
                            param.Add("@LocationStockCount", newstock);
                            param.Add("@LocationStockId", sorgu.First().DepoStokId);
                            await _db.ExecuteAsync($"Update DepoStoklar set StockCount=@LocationStockCount where  and  id=@LocationStockId", param);


                        }
                        else
                        {


                            param.Add("@Durus", 2);
                            param.Add("@Malzemeler", 3);
                            param.Add("@SatisOgesi", 4);
                            param.Add("@Uretme", 4);
                            await _db.ExecuteAsync($"Update SatisDetay set Malzemeler=@Malzemeler,Durus=@Durus,SatisOgesi=@SatisOgesi,Uretme=@Uretme where SatisId=@id and id=@SatisDetayId and StokId=@StokId ", param);

                            param.Add("@Durum", 4);
                            await _db.ExecuteAsync($"Update Rezerve set Durum=@Durum where SatisId=@id and SatisDetayId=@SatisDetayId", param);

                        }




                        param.Add("@DurumBelirteci", 4);
                        await _db.ExecuteAsync($"Update Satis set DurumBelirteci=@DurumBelirteci where id=@id", param);




                    }
                }

                else if (eskiStatus == 2)
                {
                    if (T.DurumBelirteci == 4)
                    {
                        param.Add("@DurumBelirteci", 4);
                        await _db.ExecuteAsync($"Update Satis set DurumBelirteci=@DurumBelirteci where id=@id", param);
                    }
                }
                else if (eskiStatus == 4)
                {

                }
            }



        }
    }
}
