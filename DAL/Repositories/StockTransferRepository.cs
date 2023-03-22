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
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ItemDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.StockTransferDTO;

namespace DAL.Repositories
{
    public class StockTransferRepository : IStockTransferRepository
    {
        IDbConnection _db;
        ILocationStockRepository _loc;
        private readonly IStockControl _control;
        private readonly IStokHareket _stokhareket;
        private readonly IEvrakNumarasıOLusturucu _evrakolustur;


        public StockTransferRepository(IDbConnection db, ILocationStockRepository loc, IStockControl control, IStokHareket stokhareket, IEvrakNumarasıOLusturucu evrakolustur)
        {
            _db = db;
            _loc = loc;
            _control = control;
            _stokhareket = stokhareket;
            _evrakolustur = evrakolustur;
        }


        public async Task Delete(IdControl T,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Aktif", false);
            string sqls = $@"select st.id,st.StokId,st.Miktar,sa.BaslangicDepo,sa.HedefDepo,Urunler.Tip,
            DepoStoklar.StokAdeti as BaslangicStokAdeti,ds.StokAdeti as HedefStokAdeti,
            Urunler.Isim as UrunIsmi,Urunler.StokKodu,Urunler.VarsayilanFiyat,sa.SubeId,Urunler.OlcuId,st.EvrakNo  from StokAktarimDetay  st
            left join StokAktarim sa on sa.id=st.StokAktarimId
				left join Urunler on Urunler.id=st.StokId
			left join DepoStoklar on DepoStoklar.DepoId=sa.BaslangicDepo and DepoStoklar.StokId=st.StokId
			left join DepoStoklar ds on ds.DepoId=sa.HedefDepo and ds.StokId=st.StokId
            where sa.id=@id   ";
            var list = await _db.QueryAsync<StockTransferDetailsResponse>(sqls, prm);
            foreach (var item in list)
            {
                prm.Add("@StokId", item.StokId);

                float Quantity = item.Miktar;
                float CostPerUnit = item.VarsayilanFiyat;
                var Tip = item.Tip;
                float value = Quantity * CostPerUnit; //transfer value hesaplama
                prm.Add("@Total", value);
                int Origin = item.BaslangicDepo;
                int Destination = item.HedefDepo;
                prm.Add("@Destination", Destination);
                prm.Add("@Origin", Origin);
                prm.Add("@AktarimUcreti", value);
                prm.Add("@CostPerUnit", CostPerUnit);


                //Verilen konumlarda bu iteme ait stock değeri var mı kontrol edilir.Yoksa oluşturulur.

                float? OriginStockCount = item.HedefStokAdeti;

                //Baslangıc Hareket
                StokHareketDTO harekettablo = new();
                harekettablo.Miktar = Quantity;
                harekettablo.EvrakNo = item.EvrakNo;
                harekettablo.DepoId = item.BaslangicDepo;
                harekettablo.SubeId = item.SubeId;
                harekettablo.StokId = item.StokId;
                harekettablo.StokAd = item.UrunIsmi;
                harekettablo.StokKodu = item.StokKodu;
                harekettablo.OlcuId = item.OlcuId;
                harekettablo.BirimFiyat = CostPerUnit;
                harekettablo.Tutar = value;
                harekettablo.Giris = true;
                harekettablo.EvrakTipi = 9;
                await _stokhareket.StokHareketInsert(harekettablo, UserId);
                harekettablo.DepoId = item.HedefDepo;
                harekettablo.Giris = false;
                await _stokhareket.StokHareketInsert(harekettablo, UserId);

                float? DestinationStockCount = item.HedefStokAdeti;

                var NewOriginStock = OriginStockCount + Quantity;
                var NewDestinationStock = DestinationStockCount - Quantity;

                prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
                prm.Add("@NewDestinationStock", NewDestinationStock);

                await _db.ExecuteAsync($"Update StokAktarim set Toplam=@Total where id=@id ", prm);

                await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewOriginStock where DepoId = @Origin and StokId=@StokId ", prm);
                await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewDestinationStock where DepoId = @Destination and StokId=@StokId ", prm);

            }

            prm.Add("@DateTime", DateTime.Now);
            prm.Add("@User", UserId);
            await _db.ExecuteAsync($"Update StokAktarim Set Aktif=@Aktif,SilmeTarihi=@DateTime,SilenKullanici=@User where id = @id ", prm);
        }

        public async Task DeleteItems(StockTransferDeleteItems T,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@StokAktarimId", T.StokAktarimId);
            prm.Add("@StokId", T.StokId);
            string sqlf = $@"select st.id,st.StokId,st.Miktar,sa.BaslangicDepo,sa.HedefDepo,Urunler.Tip,
            DepoStoklar.StokAdeti as BaslangicStokAdeti,ds.StokAdeti as HedefStokAdeti,
            Urunler.Isim as UrunIsmi,Urunler.StokKodu,Urunler.VarsayilanFiyat,sa.SubeId,Urunler.OlcuId,st.EvrakNo  from StokAktarimDetay  st
            left join StokAktarim sa on sa.id=st.StokAktarimId
				left join Urunler on Urunler.id=@StokId
			left join DepoStoklar on DepoStoklar.DepoId=sa.BaslangicDepo and DepoStoklar.StokId=@StokId
			left join DepoStoklar ds on ds.DepoId=sa.HedefDepo and ds.StokId=@StokId
            where sa.id=@StokAktarimId and  st.id=@id";
            var sorgu = await _db.QueryAsync<StockTransferDetailsResponse>(sqlf, prm);
            float Quantity = sorgu.First().Miktar;
            var CostPerUnit = sorgu.First().VarsayilanFiyat;
            var Tip = sorgu.First().Tip;
            var value = Quantity * CostPerUnit; //transfer value hesaplama
            prm.Add("@Total", value);
            int Origin = sorgu.First().BaslangicDepo;
            int Destination = sorgu.First().HedefDepo;
            int stockId = sorgu.First().StokId;
            prm.Add("@stockId", stockId);
            prm.Add("@Destination", Destination);
            prm.Add("@Origin", Origin);
            prm.Add("@AktarimUcreti", value);
            prm.Add("@CostPerUnit", CostPerUnit);

            foreach (var item in sorgu)
            {
                StokHareketDTO harekettablo = new();
                harekettablo.Miktar = Quantity;
                harekettablo.EvrakNo = item.EvrakNo;
                harekettablo.DepoId = item.BaslangicDepo;
                harekettablo.SubeId = item.SubeId;
                harekettablo.StokId = item.StokId;
                harekettablo.StokAd = item.UrunIsmi;
                harekettablo.StokKodu = item.StokKodu;
                harekettablo.OlcuId = item.OlcuId;
                harekettablo.BirimFiyat = CostPerUnit;
                harekettablo.Tutar = value;
                harekettablo.Giris = true;
                harekettablo.EvrakTipi = 9;
                await _stokhareket.StokHareketInsert(harekettablo, UserId);
                harekettablo.DepoId = item.HedefDepo;
                harekettablo.Giris = false;
                await _stokhareket.StokHareketInsert(harekettablo, UserId);
            }

            float? OriginStockCount = sorgu.First().BaslangicStokAdeti;
            float? DestinationStockCount = sorgu.First().HedefStokAdeti;

            var NewOriginStock = OriginStockCount + Quantity;
            var NewDestinationStock = DestinationStockCount - Quantity;

            prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
            prm.Add("@NewDestinationStock", NewDestinationStock);
            await _db.ExecuteAsync($"Update StokAktarim set Toplam=@Total where id=@StokAktarimId ", prm);

            await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewOriginStock where DepoId = @Origin and StokId=@StokId", prm);
            await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewDestinationStock where DepoId = @Destination and StokId=@StokId", prm);

            await _db.ExecuteAsync($"Delete From StokAktarimDetay  where id = @id and StokId=@StokId and StokAktarimId=@StokAktarimId", prm);
        }

        public async Task<IEnumerable<StockTransferDetails>> Details(int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);

            var list =await _db.QueryAsync<StockTransferDetails>($@" Select x.* From 
            (Select StokAktarim.id,StokAktarim.AktarimIsmi,StokAktarim.AktarmaTarihi,StokAktarim.HedefDepo,da.Isim as HedefDepoIsmi,StokAktarim.Toplam,
                StokAktarim.BaslangicDepo,de.Isim as BaslangicDepoIsmi,StokAktarim.Bilgi from StokAktarim
            left join DepoVeAdresler de on de.id = StokAktarim.BaslangicDepo left join DepoVeAdresler da on da.id=StokAktarim.HedefDepo ) x  where x.id=@id", prm);

            foreach (var item in list)
            {
                prm.Add("@Id", id);

                string sqla = $@"Select StokAktarimDetay.id,StokAktarimDetay.StokId,StokAktarimDetay.Miktar,Urunler.Isim as UrunIsmi,
 StokAktarimDetay.BirimFiyat,StokAktarim.HedefDepo,l.Isim as HedefDepoIsmi,
 (v.StokAdeti-((select ISNULL(SUM(RezerveDeger),0) from Rezerve where StokId = StokAktarimDetay.StokId and DepoId = StokAktarim.HedefDepo and Durum = 1))) as HedefDepoStokAdeti,
StokAktarim.BaslangicDepo,m.Isim as BaslangicDepoIsmi,
 (c.StokAdeti-((select ISNULL(SUM(RezerveDeger),0) from Rezerve where StokId = StokAktarimDetay.StokId and DepoId = StokAktarim.BaslangicDepo and Durum = 1))) as BaslangicDepoStokAdeti,StokAktarimDetay.AktarimUcreti from StokAktarimDetay 
 inner join Urunler on Urunler.id = StokAktarimDetay.StokId 
 inner join StokAktarim on StokAktarim.id = StokAktarimDetay.StokAktarimId 
 inner join DepoVeAdresler l on l.id = StokAktarim.BaslangicDepo 
 inner join DepoVeAdresler m on m.id = StokAktarim.HedefDepo 
 inner join DepoStoklar c on c.StokId = Urunler.id and c.DepoId = StokAktarim.BaslangicDepo
 inner join DepoStoklar v on v.StokId = Urunler.id
 and v.DepoId = StokAktarim.HedefDepo 
 where StokAktarimDetay.StokAktarimId = @id
 Group BY StokAktarimDetay.id,StokAktarimDetay.StokId,StokAktarimDetay.Miktar,Urunler.Isim , StokAktarim.HedefDepo,l.Isim,v.StokAdeti, StokAktarim.BaslangicDepo,m.Isim,c.StokAdeti, StokAktarimDetay.AktarimUcreti,StokAktarimDetay.BirimFiyat";
                var list2 = await _db.QueryAsync<StockTransferDetailsItems>(sqla, prm);
                item.detay = list2;
            }

            return list.ToList();
        }

        public async Task<int> Insert(StockTransferInsert T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@AktarimIsmi", T.AktarimIsmi);
            prm.Add("@AktarimTarihi", T.AktarmaTarihi);
            prm.Add("@BaslangicDepo", T.BaslangicDepo);
            prm.Add("@HedefDepo", T.HedefDepo);
            prm.Add("@Info", T.Bilgi);
            prm.Add("@BaslangicDepo", T.BaslangicDepo);
            prm.Add("@SubeId", T.SubeId);

            prm.Add("Aktif", true);


            return await _db.QuerySingleAsync<int>($"Insert into StokAktarim (SubeId,AktarimIsmi,AktarmaTarihi,BaslangicDepo,HedefDepo,Bilgi,Aktif) OUTPUT INSERTED.[id] values (@SubeId,@AktarimIsmi,@AktarimTarihi,@BaslangicDepo,@HedefDepo,@Info,@Aktif)", prm);
        }

        public  async Task<int> InsertStockTransferItem(StockTransferInsertItem T, int id,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            var evrakno = await _evrakolustur.Olustur(id);
            prm.Add("@StokId", T.StokId);
            prm.Add("@Quantity", T.Miktar);
            prm.Add("@StokAktarimId", id);


            //VarsayilanFiyat,Tip,BaslangicDepo,HedefDepo getiriliyor.
            string sqlf = $@"select sa.BaslangicDepo,sa.HedefDepo,Urunler.Tip,
            Urunler.Isim as UrunIsmi,Urunler.StokKodu,Urunler.VarsayilanFiyat,sa.SubeId,Urunler.OlcuId,dp.StokAdeti as BaslangicStokAdeti ,ds.StokAdeti as HedefStokAdeti   from StokAktarim  sa
		    left join Urunler on Urunler.id=@StokId
			left join DepoStoklar dp on dp.StokId=@StokId and dp.DepoId=sa.BaslangicDepo
			left join DepoStoklar ds on ds.StokId=@StokId and ds.DepoId=sa.HedefDepo
            where sa.id=@StokAktarimId ";
            var sorgu = await _db.QueryAsync<StockTransferDetailsResponse>(sqlf, prm);

            var CostPerUnit = sorgu.First().VarsayilanFiyat;
            var Tip = sorgu.First().Tip;
            var value = T.Miktar * CostPerUnit; //transfer value hesaplama
            prm.Add("@Total", value);
            int Origin = sorgu.First().BaslangicDepo;
            int Destination = sorgu.First().HedefDepo;
            prm.Add("@EvrakNo", evrakno);
            prm.Add("@Destination", Destination);
            prm.Add("@Origin", Origin);
            prm.Add("@AktarimUcreti", value);
            prm.Add("@CostPerUnit", CostPerUnit);

            float OriginStockCount = sorgu.First().BaslangicStokAdeti;
            float DestinationStockCount = sorgu.First().HedefStokAdeti;
            
            var NewOriginStock = OriginStockCount - T.Miktar;
            var NewDestinationStock = DestinationStockCount + T.Miktar;

            prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
            prm.Add("@NewDestinationStock", NewDestinationStock);
            await _db.ExecuteAsync($"Update StokAktarim set Toplam=@Total where id=@StokAktarimId", prm);


            StokHareketDTO harekettablo = new();
            harekettablo.Miktar = T.Miktar;
            harekettablo.EvrakNo = evrakno;
            harekettablo.DepoId = Origin;
            harekettablo.SubeId = sorgu.First().SubeId; ;
            harekettablo.StokId = T.StokId;
            harekettablo.StokAd = sorgu.First().UrunIsmi;
            harekettablo.StokKodu = sorgu.First().StokKodu;
            harekettablo.OlcuId = sorgu.First().OlcuId;
            harekettablo.BirimFiyat = CostPerUnit;
            harekettablo.Tutar = value;
            harekettablo.Giris = false;
            harekettablo.EvrakTipi = 8;

            await _stokhareket.StokHareketInsert(harekettablo, UserId);
            harekettablo.DepoId = Destination;
            harekettablo.Giris = true;
            await _stokhareket.StokHareketInsert(harekettablo, UserId);


            await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewOriginStock where DepoId = @Origin and StokId=@StokId ", prm);
            await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewDestinationStock where DepoId = @Destination and StokId=@StokId", prm);
       
            return await _db.QuerySingleAsync<int>($"Insert into StokAktarimDetay (EvrakNo,StokId,BirimFiyat,Miktar,AktarimUcreti,StokAktarimId) OUTPUT INSERTED.[id]  values (@EvrakNo,@StokId,@CostPerUnit,@Quantity,@AktarimUcreti,@StokAktarimId)", prm);
        }
        public async Task<IEnumerable<StockTransferList>> List(StockTransferList T, int KAYITSAYISI, int sayfa)
        {
            string sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {sayfa} 
Select x.* From (Select StokAktarim.id,StokAktarim.AktarimIsmi,StokAktarim.AktarmaTarihi,StokAktarim.Aktif,StokAktarim.HedefDepo
,da.Isim as HedefDepoIsmi,StokAktarim.BaslangicDepo,de.Isim as BaslangicDepoIsmi,StokAktarim.Toplam
from StokAktarim
inner join DepoVeAdresler de on de.id = StokAktarim.BaslangicDepo 
inner join DepoVeAdresler da on da.id=StokAktarim.HedefDepo ) x  
where x.Aktif=1  and 
ISNULL(x.AktarimIsmi,0) LIKE '%{T.AktarimIsmi}%'and
ISNULL(x.HedefDepo,0) LIKE '%{T.HedefDepoIsmi}%' and ISNULL(HedefDepoIsmi,0) 
LIKE '%{T.BaslangicDepoIsmi}%' and ISNULL(BaslangicDepoIsmi,0) LIKE
'%%' and ISNULL(x.Toplam,0) LIKE '%{T.Toplam}%' 
ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;   ";

            var list = await _db.QueryAsync<StockTransferList>(sql);
            return list.ToList();
        }

        public async Task Update(StockUpdate T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@AktarimTarihi", T.AktarmaTarihi);
            prm.Add("@AktarimIsmi", T.AktarimIsmi);
            prm.Add("@Info", T.Bilgi);
            prm.Add("@Total", T.Toplam);


            await _db.ExecuteAsync($"Update StokAktarim SET AktarmaTarihi = @AktarimTarihi,AktarimIsmi=@AktarimIsmi,Bilgi=@Info,Toplam=@Total where id=@id", prm);
        }

        public async Task<int> UpdateStockTransferItem(StokAktarimDetay T,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            double? Total = 0;
            prm.Add("@id", T.id);
            prm.Add("@StokId", T.StokId);
            prm.Add("@Quantity", T.Miktar);
            prm.Add("@StokAktarimId", T.StokAktarimId);


            var deger = await _db.QueryAsync<int>($"Select  StokAktarimDetay.Miktar from StokAktarimDetay where id=@id", prm);



            //VarsayilanFiyat,Tip,BaslangicDepo,HedefDepo getiriliyor.Gelen id ile originvarmı,destinationvarmı kontrol edilir.Eğer var ise StockCount değerleri çekilir.
            string sqlf = $@"select st.id,st.StokId,st.Miktar,sa.BaslangicDepo,sa.HedefDepo,Urunler.Tip,
            DepoStoklar.StokAdeti as BaslangicStokAdeti,ds.StokAdeti as HedefStokAdeti,
            Urunler.Isim as UrunIsmi,Urunler.StokKodu,Urunler.VarsayilanFiyat,sa.SubeId,Urunler.OlcuId,st.EvrakNo  from StokAktarimDetay  st
            left join StokAktarim sa on sa.id=st.StokAktarimId
				left join Urunler on Urunler.id=@StokId
			left join DepoStoklar on DepoStoklar.DepoId=sa.BaslangicDepo and DepoStoklar.StokId=@StokId
			left join DepoStoklar ds on ds.DepoId=sa.HedefDepo and ds.StokId=@StokId
            where sa.id=@StokAktarimId and st.id=@id           ";
            var sorgu = await _db.QueryAsync<StockTransferDetailsResponse>(sqlf, prm);
            int id = T.StokAktarimId; ;
            if (T.Miktar != deger.First())
            {

                var CostPerUnit = sorgu.First().VarsayilanFiyat;
                var Tip = sorgu.First().Tip;
                var value = T.Miktar * CostPerUnit; //transfer value hesaplama
                prm.Add("@Total", value);
                int Origin = sorgu.First().BaslangicDepo;
                int Destination = sorgu.First().HedefDepo;
                prm.Add("@Destination", Destination);
                prm.Add("@Origin", Origin);
                prm.Add("@AktarimUcreti", value);
                prm.Add("@CostPerUnit", CostPerUnit);

                float OriginStockCount = sorgu.First().BaslangicStokAdeti;
                float DestinationStockCount = sorgu.First().HedefStokAdeti;

                if (deger.First() < T.Miktar)
                {
                    var NewOriginStock = OriginStockCount - T.Miktar;
                    var NewDestinationStock = DestinationStockCount + T.Miktar;
                    prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
                    prm.Add("@NewDestinationStock", NewDestinationStock);

                    StokHareketDTO harekettablo = new();
                    harekettablo.Miktar = T.Miktar;
                    harekettablo.EvrakNo = sorgu.First().EvrakNo;
                    harekettablo.DepoId = Origin;
                    harekettablo.SubeId = sorgu.First().SubeId; ;
                    harekettablo.StokId = T.StokId;
                    harekettablo.StokAd = sorgu.First().UrunIsmi;
                    harekettablo.StokKodu = sorgu.First().StokKodu;
                    harekettablo.OlcuId = sorgu.First().OlcuId;
                    harekettablo.BirimFiyat = CostPerUnit;
                    harekettablo.Tutar = value;
                    harekettablo.Giris = false;
                    harekettablo.EvrakTipi = 8;

                    await _stokhareket.StokHareketInsert(harekettablo, UserId);
                    harekettablo.DepoId = Destination;
                    harekettablo.Giris = true;
                    await _stokhareket.StokHareketInsert(harekettablo, UserId);
                }
                else
                {
                    var NewOriginStock = OriginStockCount + T.Miktar;
                    var NewDestinationStock = DestinationStockCount - T.Miktar;
                    prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
                    prm.Add("@NewDestinationStock", NewDestinationStock);

                    StokHareketDTO harekettablo = new();
                    harekettablo.Miktar = T.Miktar;
                    harekettablo.EvrakNo = sorgu.First().EvrakNo;
                    harekettablo.DepoId = Origin;
                    harekettablo.SubeId = sorgu.First().SubeId; ;
                    harekettablo.StokId = T.StokId;
                    harekettablo.StokAd = sorgu.First().UrunIsmi;
                    harekettablo.StokKodu = sorgu.First().StokKodu;
                    harekettablo.OlcuId = sorgu.First().OlcuId;
                    harekettablo.BirimFiyat = CostPerUnit;
                    harekettablo.Tutar = value;
                    harekettablo.Giris = true;
                    harekettablo.EvrakTipi = 8;

                    await _stokhareket.StokHareketInsert(harekettablo, UserId);
                    harekettablo.DepoId = Destination;
                    harekettablo.Giris = false;
                    await _stokhareket.StokHareketInsert(harekettablo, UserId);
                }

    
                await _db.ExecuteAsync($"Update StokAktarim set Toplam=@Total where id=@StokAktarimId", prm);

                await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewOriginStock where DepoId = @Origin and StokId=@StokId ", prm);
                await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewDestinationStock where DepoId = @Destination and StokId=@StokId", prm);
            }

            return id;
          
        }
    }
}
