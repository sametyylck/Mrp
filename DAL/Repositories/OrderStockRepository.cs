using DAL.Contracts;
using DAL.DTO;
using DAL.Hareket;
using DAL.Models;
using DAL.StockControl;
using DAL.StokHareket;
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
        private readonly IStokHareket _stokhareket;
        private readonly ICariHareket _carihareket;
        private readonly IEvrakNumarasıOLusturucu _evrakno;
        public OrderStockRepository(IDbConnection db, ILocationStockRepository locationStockRepository, IStockControl stockcontrol, IStokHareket stokhareket, ICariHareket carihareket, IEvrakNumarasıOLusturucu evrakno)
        {
            _db = db;
            _locationStockRepository = locationStockRepository;
            _stockcontrol = stockcontrol;
            _stokhareket = stokhareket;
            _carihareket = carihareket;
            _evrakno = evrakno;
        }

        public async Task<IEnumerable<PurchaseOrderLogsList>> DoneList(PurchaseOrderLogsList T, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SupplierName", T.TedarikciIsmi);
            string sql;
            if (T.DepoId == null)
            {
                sql = $"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}Select sa.id,sa.Tip,sa.SatinAlmaIsmi,Cari.AdSoyad as TedarikciIsmi,sa.BeklenenTarih, sa.TumTutar from SatinAlma sa inner join Cari on Cari.CariKod = sa.TedarikciId and sa.DurumBelirteci=1 where  sa.Aktif = 1 and  sa.Tip = '{T.Tip}' and ISNULL(Cari.AdSoyad,0) LIKE '%{T.TedarikciIsmi}%' and ISNULL(sa.SatinAlmaIsmi,0) LIKE '%{T.SatinAlmaIsmi}%' and ISNULL(sa.TumTutar,0) LIKE '%{T.TumToplam}%' ORDER BY sa.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";
            }
            else
            {


                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}Select sa.id,sa.Tip,sa.SatinAlmaIsmi,Cari.AdSoyad as TedarikciIsmi,sa.BeklenenTarih, sa.TumTutar from SatinAlma sa inner join Cari on Cari.CariKod = sa.TedarikciId and sa.DurumBelirteci=1 where  sa.DepoId={T.DepoId} and  sa.Aktif = 1 and  sa.Tip = '{T.Tip}' and ISNULL(Cari.AdSoyad,0) LIKE '%{T.TedarikciIsmi}%' and ISNULL(sa.SatinAlmaIsmi,0) LIKE '%{T.SatinAlmaIsmi}%' and ISNULL(sa.TumTutar,0) LIKE '%{T.TumToplam}%' ORDER BY sa.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";
            }

            var list = await _db.QueryAsync<PurchaseOrderLogsList>(sql);
            return list;
        }

        public async Task<IEnumerable<PurchaseOrderLogsList>> List(PurchaseOrderLogsList T,  int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SupplierName", T.TedarikciIsmi);
            string sql;
            if (T.DepoId == null)
            {
                sql = $"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}Select sa.id,sa.Tip,sa.SatinAlmaIsmi,Cari.AdSoyad as TedarikciIsmi,sa.BeklenenTarih, sa.TumTutar from SatinAlma sa inner join Cari on Cari.CariKod = sa.TedarikciId and sa.DurumBelirteci=1 where  sa.Aktif = 1 and  sa.Tip = '{T.Tip}' and ISNULL(Cari.AdSoyad,0) LIKE '%{T.TedarikciIsmi}%' and ISNULL(sa.SatinAlmaIsmi,0) LIKE '%{T.SatinAlmaIsmi}%' and ISNULL(sa.TumTutar,0) LIKE '%{T.TumToplam}%' ORDER BY sa.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";
            }
            else
            {
     

                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}Select sa.id,sa.Tip,sa.SatinAlmaIsmi,Cari.AdSoyad as TedarikciIsmi,sa.BeklenenTarih, sa.TumTutar from SatinAlma sa inner join Cari on Cari.CariKod = sa.TedarikciId and sa.DurumBelirteci=1 where  sa.DepoId={T.DepoId} and  sa.Aktif = 1 and  sa.Tip = '{T.Tip}' and ISNULL(Cari.AdSoyad,0) LIKE '%{T.TedarikciIsmi}%' and ISNULL(sa.SatinAlmaIsmi,0) LIKE '%{T.SatinAlmaIsmi}%' and ISNULL(sa.TumTutar,0) LIKE '%{T.TumToplam}%' ORDER BY sa.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";
            }






            var list = await _db.QueryAsync<PurchaseOrderLogsList>(sql);
            return list;
        }

        public async Task StockUpdate(PurchaseOrderId T, int user)
        {
            DynamicParameters prm = new DynamicParameters();
            var evrakno = await _evrakno.Olustur(T.id);
       
            prm.Add("@id", T.id);
            string sqla = $"Select DepoId,SubeId,DurumBelirteci From SatinAlma where id  = @id";
            var locationIdDeger = await _db.QueryAsync<PurchaseOrder>(sqla, prm); //gelen ordersId'nin locationId alıyoruz
            int locaitonId = locationIdDeger.First().DepoId;
            int SubeId = locationIdDeger.First().SubeId;
            int olddeliveryId = locationIdDeger.First().DurumBelirteci;
            prm.Add("@locationId", locaitonId);

            prm.Add("@DeliveryId", T.DurumBelirteci);

            //if (T.DurumBelirteci == 1 || T.DurumBelirteci == 2)
            //{
            //    await _db.ExecuteAsync($"Update SatinAlma SET DurumBelirteci = @DeliveryId where id = @id ", prm);
            //}


            string sql = @$"select SatinAlmaDetay.id,StokId,SatinAlmaDetay.OlcuId,Miktar,BirimFiyat,VergiId,VergiDegeri,ToplamTutar as AraToplam,VergiMiktari,TumToplam,Urunler.StokKodu,Urunler.Isim as StokAd from SatinAlmaDetay
               left join Urunler on Urunler.id=SatinAlmaDetay.StokId where SatinAlmaId=@id";
            var stokdegerler = await _db.QueryAsync<SatinAlmaDetays>(sql, prm); //gelen ordersId'nin OrdersItem tablosundaki itemId ve Quantity e erişerek hangi itemin kaç tane artacağına bakılacak.

            if (olddeliveryId < T.DurumBelirteci && T.DurumBelirteci == 2)
            {
                foreach (var item in stokdegerler)
                {
                    prm.Add("@Quantity", item.Miktar);
                    prm.Add("@ItemId", item.StokId);
                    prm.Add("@OrdersItemid", item.id);
                    float? StockLocationRezerve = await _stockcontrol.Count(item.StokId, locaitonId);
                    StockLocationRezerve = StockLocationRezerve >= 0 ? StockLocationRezerve : 0;


                    string sqlc = $"select * from SatinAlma where id=@id";
                    var sorgu3 = await _db.QueryAsync<PurchaseOrder>(sqlc, prm);//
                    int? salesorderId = sorgu3.First().SatisId;
                    int? salesorderitemId = sorgu3.First().SatisDetayId;
                    int? manufacturingorderId = sorgu3.First().UretimId;
                    prm.Add("@SalesOrderId", salesorderId);
                    prm.Add("@SalesOrderItemId", salesorderitemId);
                    int? ProductId = 0;
                    if (salesorderId != 0)
                    {
                        var ItemIdAL = await _db.QueryAsync<int>($"Select StokId From SatisDetay where id =@SalesOrderItemId  and SatisId=@SalesOrderId", prm);
                        ProductId = ItemIdAL.First();
                        prm.Add("@ProductId", ProductId);
                    }


                    float adet = item.Miktar;

                    if (salesorderId!=0)
                    {


                        prm.Add("@SalesOrderId", sorgu3.First().SatisId);
                        prm.Add("@SalesOrderItemId", sorgu3.First().SatisDetayId);
                        prm.Add("@RezerveCount", item.Miktar);
                        prm.Add("@LocationId", sorgu3.First().DepoId);
                        prm.Add("@Status", 1);
                        prm.Add("@Tip", sorgu3.First().Tip);
                        //prm.Add("@ManufacturingOrderItemId", sorgu3.First().ManufacturingOrderItemId);

                        string sqld = $@"select ISNULL(RezerveDegeri,0) as RezerveDeger from Rezerve where SatisId=@SalesOrderId and StokId=@ItemId and DepoId=@LocationId and Durum=1";
                        var rezervestockCount = await _db.QueryAsync<LocaVarmı>(sqld, prm);
                        float? rezervecount = rezervestockCount.First().RezerveDeger;


                        string sqlf = $"select Tip from SatisDetay where id=@SalesOrderItemId ";
                        var sorgu5 = await _db.QueryAsync<SalesOrderUpdateItems>(sqlf, prm);//
                        var tip = sorgu5.First().Tip;
                        if (sorgu3.First().UretimId == 0 && tip == null)
                        {
                            string missingsorgu = $@"
				    	 							Select 
                         ((select Miktar from SatisDetay where id=@SalesOrderItemId and SatisId=@SalesOrderId )*
                        (select Miktar from UrunRecetesi where UrunRecetesi.MamulId=@ProductId and UrunRecetesi.MalzemeId=@ItemId) -ISNULL((Rezerve.RezerveDeger),0))as Missing
                         
                         from SatinAlma
                         left join SatinAlmaDetay on SatinAlmaDetay.SatinAlmaId=SatinAlma.id  and DurumBelirteci = 1
				    	 LEFT join Rezerve on Rezerve.SatisId=@SalesOrderId and Rezerve.SatisDetayId=@SalesOrderItemId and Rezerve.StokId=@ItemId
                         where SatinAlma.Aktif=1 
                         Group by Rezerve.RezerveDeger";
                            var missingcount = await _db.QueryAsync<LocaVarmı>(missingsorgu, prm);
                            float missing = missingcount.Count() > 0 ? missingcount.First().Kayıp : 0;

                            if (missing == item.Miktar)
                            {

                                var counts = item.Miktar + rezervecount;
                                prm.Add("@RezerveCount", counts);
                                await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where  SatisId=@SalesOrderId and SatisDetayId=@SalesOrderItemId  and Durum=1 and StokId=@ItemId", prm);
                            }
                            else if (missing < item.Miktar)
                            {

                                var newstocks = item.Miktar - missing;

                                prm.Add("@RezerveCount", rezervecount + missing);
                                await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where  SatisId=@SalesOrderId and SatisDetayId=@SalesOrderItemId  and Durum=1 and StokId=@ItemId ", prm);
                                prm.Add("@Availability", 2);


                            }
                            else if (missing <= item.Miktar + StockLocationRezerve)
                            {
                                prm.Add("@RezerveCount", rezervecount + missing);
                                await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where  SatisId=@SalesOrderId and SatisDetayId=@SalesOrderItemId  and Durum=1 and StokId=@ItemId ", prm);
                                prm.Add("@Availability", 2);
                            }
                            else
                            {

                                prm.Add("@RezerveCount", item.Miktar + StockLocationRezerve + rezervecount);
                                prm.Add("@Availability", 0);

                                await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where  SatisId=@SalesOrderId and SatisDetayId=@SalesOrderItemId  and Durum=1 and StokId=@ItemId ", prm);

                            }

                        }
                        else if (tip != null)
                        {


                            float? kalan = item.Miktar;

                            string sqlquery = $@"select ma.id,me.id as UretimDetayId,me.PlanlananMiktar  from Uretim ma
                            left join UretimDetay me on me.UretimId=ma.id
                            where  SatisId=@SalesOrderId and SatisDetayId=@SalesOrderItemId and me.StokId=@ItemId and me.MalzemeDurum=0";
                            var sorgu6 = await _db.QueryAsync<UretimOzelClass>(sqlquery, prm);//
                            foreach (var list2 in sorgu6)
                            {
                                if (kalan > 0)
                                {


                                    prm.Add("@ManufacturingOrderId", sorgu6.First().id);
                                    prm.Add("@ManufacturingOrderItemId", sorgu6.First().UretimDetayId);
                                    string missingsorgu = $@"
				    	 	Select 
                         ((select Miktar from SatisDetayId where id=@SalesOrderItemId  and SatisId=@SalesOrderId)*
                        (select ISNULL(PlananlananMiktar,0) from UretimDetay where id=@ManufacturingOrderItemId and StokId=@ItemId) -ISNULL((Rezerve.RezerveDeger),0))as Missing
                        
                         from SatinAlma
                         left join SatinAlmaDetay on SatinAlmaDetay.SatinAlmaId=SatinAlma.id  and DurumBelirteci = 1
				    	 LEFT join Rezerve on Rezerve.SatisId=@SalesOrderId and Rezerve.SatisDetayId=@SalesOrderItemId  and Rezerve.StokId=@ItemId
                         where SatinAlma.Aktif=1
                         Group by Rezerve.RezerveDeger";
                                    var missingcount = await _db.QueryAsync<LocaVarmı>(missingsorgu, prm);
                                    float missing = missingcount.Count() > 0 ? missingcount.First().Kayıp : 0;

                                    if (missing <= item.Miktar)
                                    {
                                        kalan = item.Miktar - missing;
                                        prm.Add("@RezerveCount", missing + rezervecount);
                                        await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where UretimId=@ManufacturingOrderId and UretimDetayId=@ManufacturingOrderItemId and Durum=1", prm);
                                        prm.Add("@Availability", 2);
                                        await _db.ExecuteAsync($"Update UretimDetay set MalzemeDurum=@Availability where UretimId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and StokId=@ItemId", prm);

                                    }

                                    else if (missing <= item.Miktar + StockLocationRezerve)
                                    {
                                        prm.Add("@RezerveCount", rezervecount + missing);
                                        await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where  UretimId=@ManufacturingOrderId and UretimDetayId=@ManufacturingOrderItemId and Durum=1 ", prm);
                                        prm.Add("@Availability", 2);

                                        await _db.ExecuteAsync($"Update UretimDetay set MalzemeDurum=@Availability where UretimId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and StokId=@ItemId", prm);
                                    }
                                    else
                                    {
                                        prm.Add("@RezerveCount", item.Miktar + rezervecount);
                                        await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where  UretimId=@ManufacturingOrderId and UretimDetayId=@ManufacturingOrderItemId and Durum=1 ", prm);
                                        prm.Add("@Availability", 0);
                                        await _db.ExecuteAsync($"Update UretimDetay set MalzemeDurum=@Availability where UretimId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and StokId=@ItemId", prm);
                                    }
                                    prm.Add("@ItemId", ProductId);
                                    prm.Add("@SalesOrderId", sorgu3.First().SatisId);
                                    prm.Add("@SalesOrderItemId", sorgu3.First().SatisDetayId);

                                    string sqlr = $@"select MIN(UretimDetay.MalzemeDurum)as Ingredients from UretimDetay
                    left join Uretim ma on ma.id=UretimDetay.UretimId
                    where ma.SatisId=@SalesOrderId and ma.SatisDetayId=@SalesOrderItemId";
                                    var availability = await _db.QueryAsync(sqlr, prm);
                                    prm.Add("@Ingredients", availability.First());
                                    await _db.ExecuteAsync($"Update SatisDetay set Malzemeler=@Ingredients where  SatisId=@SalesOrderId and id=@SalesOrderItemId and StokId=@ItemId ", prm);
                                }

                            }
                        }









                    }
                    else if (salesorderId == 0 && manufacturingorderId != 0)
                    {
                        prm.Add("@ManufacturingOrderId", sorgu3.First().UretimId);
                        prm.Add("@ManufacturingOrderItemId", sorgu3.First().UretimDetayId);

                        float missing;
                        string sqlsorgu = $@" 
		select
        (-ISNULL(moi.PlanlananMiktar,0)+(ISNULL(rez.RezerveDeger,0)))AS missing
        from UretimDetay moi
        left join Uretim mao on mao.id=moi.UretimId
        left join Urunler on Urunler.id=moi.StokId
        left join SatinAlmaDetay on SatinAlmaDetay.StokId=moi.StokId 
        left join SatinAlma on SatinAlma.id=SatinAlmaDetay.SatinAlmaId and SatinAlma.UretimId=mao.id and SatinAlma.Aktif=1 and SatinAlma.DurumBelirteci=1
        left join Rezerve rez on rez.UretimId=mao.id and rez.UretimDetayId=moi.id and rez.StokId=@ItemId
        where mao.id=@ManufacturingOrderId and moi.Tip='Ingredients' and mao.DepoId=@locationId  and mao.Durum!=3 and  moi.id=@ManufacturingOrderItemId
        Group by moi.id,moi.Tip,moi.StokId,Urunler.ıSİM,moi.Bilgi,moi.PlanlananMiktar ,moi.Tutar,moi.MalzemeDurum,
        moi.PlanlananMiktar,rez.RezerveDeger";
                        List<int> missingdeger = (await _db.QueryAsync<int>(sqlsorgu, prm)).ToList();

                        if (missingdeger.Count() == 0)
                        {
                            missing = 0;
                        }
                        else
                        {
                            missing = missingdeger.First()*(-1);
                        }
                        prm.Add("@LocationId", sorgu3.First().DepoId);
                        prm.Add("@Status", 1);
                        prm.Add("@Tip", sorgu3.First().Tip);

                        string sqld = $@"select ISNULL(RezerveDeger,0) from Rezerve where UretimId=@ManufacturingOrderId  and StokId=@ItemId and DepoId=@LocationId and UretimDetayId=@ManufacturingOrderItemId and Durum=1";
                        var rezervestockCount = await _db.QueryAsync<float>(sqld, prm);
                        float rezervecount = 0;
                        rezervecount = rezervestockCount.First();

                        if (missing <= item.Miktar)
                        {
                            item.Miktar = item.Miktar - missing;
                            prm.Add("@RezerveCount", missing + rezervecount);
                            prm.Add("@Availability", 2);


                        }
                        else if (missing <= item.Miktar + StockLocationRezerve)
                        {
                            prm.Add("@RezerveCount", missing + rezervecount);
                            item.Miktar = 0;
                            prm.Add("@Availability", 2);

                        }

                        else
                        {
                            prm.Add("@RezerveCount", item.Miktar + rezervecount + StockLocationRezerve);
                            item.Miktar = 0;
                            prm.Add("@Availability", 0);

                        }
                        await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where UretimId=@ManufacturingOrderId and UretimDetayId=@ManufacturingOrderItemId and Durum=1", prm);
                        prm.Add("@Availability", 2);
                        await _db.ExecuteAsync($"Update UretimDetay set MalzemeDurum=@Availability where UretimId=@ManufacturingOrderId and id=@ManufacturingOrderItemId and StokId=@ItemId", prm);



                    }

                    prm.Add("@Quantity", adet);
                    prm.Add("@ItemId", item.StokId);
                    prm.Add("@OrdersItemid", item.id);
                    string sqlb = $" select (Select StokAdeti from DepoStoklar where StokId = @ItemId  and DepoId = @locationId )as LocationsStockCount,(Select id from DepoStoklar where StokId = @ItemId and DepoId = @locationId) as LocationStockId,(select Tip from Urunler where id=@ItemId) as Tip";
                    var sorgu = await _db.QueryAsync<Stock>(sqlb, prm);//


                   
                    var stocklocationId = sorgu.First().LocationStockId;
                    prm.Add("@stocklocationId", stocklocationId);
                    float? stockCount = 0;
                    string Tip = sorgu.First().Tip;
                    if (stocklocationId == 0)
                    {
                        int locationid = await _locationStockRepository.Insert(Tip, item.StokId, locaitonId); ;
                        prm.Add("@stocklocationId", locationid);
                        stockCount = 0;
                    }
                    else
                    {
                        stockCount = sorgu.First().LocationsStockCount;

                    }
                    float? NewStockCount = stockCount + adet;
                    prm.Add("@NewStockCount", NewStockCount); //Yeni count değerini tabloya güncelleştiriyoruz.
                    await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewStockCount where id = @stocklocationId", prm);
                    StokHareketDTO harekettablo = new();
                    harekettablo.Miktar = adet;
                    harekettablo.EvrakNo = evrakno;
                    harekettablo.DepoId = locaitonId;
                    harekettablo.SubeId = SubeId;
                    harekettablo.StokId = item.StokId;
                    harekettablo.StokAd = item.StokAd;
                    harekettablo.StokKodu = item.StokKodu;
                    harekettablo.OlcuId = item.OlcuId;
                    harekettablo.BirimFiyat = item.BirimFiyat;
                    harekettablo.Tutar = item.TumToplam;
                    harekettablo.AraToplam = item.AraToplam;
                    harekettablo.Giris = true;
                    harekettablo.EvrakTipi = 1;
                    harekettablo.KDVOrani = item.VergiDegeri;
                    harekettablo.KDVTutari = item.VergiMiktari;
                    harekettablo.OlcuId = item.OlcuId;
                    await _stokhareket.StokHareketInsert(harekettablo,user);
   


                }
                string sqlsorgu4 = @$"select 
                sa.DepoId,sa.SubeId,Sum(sad.TumToplam) as Tutar,Sum(sad.ToplamTutar)as AraToplam,Sum(sad.VergiMiktari)as KDVTutari,
				sa.TedarikciId as CariKod,Cari.AdSoyad as CariAdSoyad,Cari.VergiDairesi,Cari.VergiNumarası,Cari.FaturaAdresId
                from SatinAlmaDetay sad
                left join SatinAlma sa  on sa.id=sad.SatinAlmaId
                left join Cari on Cari.CariKod=sa.TedarikciId where sa.id=@id
                Group By sa.DepoId,sa.SubeId,sa.TedarikciId,Cari.AdSoyad,Cari.VergiDairesi,Cari.VergiNumarası,Cari.FaturaAdresId";
                var caridetay = await _db.QueryAsync<CariHareketDTO>(sqlsorgu4, prm);//
                CariHareketDTO cari = new();
                foreach (var carihareket in caridetay)
                {
                    cari.AraToplam = carihareket.AraToplam;
                    cari.CariAdSoyad = carihareket.CariAdSoyad;
                    cari.CariKod = carihareket.CariKod;
                    cari.EvrakNo = evrakno;
                    cari.EvrakTipi = 1;
                    cari.KDVTutari = carihareket.KDVTutari;
                    cari.SubeId = carihareket.SubeId;
                    cari.DepoId = carihareket.DepoId;
                    cari.Tutar = carihareket.Tutar;
                 
                   
                }
                await _carihareket.CariHareketInsert(cari, user);
            }
        }


    }
}
