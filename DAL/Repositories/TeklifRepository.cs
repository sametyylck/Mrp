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
            prm.Add("@CariId", T.CariId);
            prm.Add("@TeslimSuresi", T.TeslimSuresi);
            prm.Add("@OlusturmaTarihi", T.OlusturmaTarihi);
            prm.Add("@DurumBelirteci", 0);
            prm.Add("@SatisIsmi", T.SatisIsmi);
            prm.Add("@Bilgi", T.Bilgi);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@DepoId", T.DepoId);
            prm.Add("@Aktif", true);

            return await _db.QuerySingleAsync<int>($"Insert into Satis (Tip,CariId,TeslimSuresi,OlusturmaTarihi,SatisIsmi,DepoId,Bilgi,Aktif,DurumBelirteci) OUTPUT INSERTED.[id] values (@Tip,@CariId,@TeslimSuresi,@OlusturmaTarihi,@SatisIsmi,@DepoId,@Bilgi,@Aktif,@DurumBelirteci)", prm);
        }
        public async Task<int> InsertPurchaseItem(TeklifInsertItem T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
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
            prm.Add("@SatisOgesi", 0);
            prm.Add("@Malzemeler", 0);
            prm.Add("@Uretme", 0);

            int id = await _db.QuerySingleAsync<int>($"Insert into SatisDetay(StokId,Miktar,BirimFiyat,VergiId,VergiOrani,SatisId,ToplamTutar,VergiTutari,TumToplam,Durus,SatisOgesi,Malzemeler,Uretme) OUTPUT INSERTED.[id] values (@StokId,@Miktar,@BirimFiyat,@VergiId,@VergiOrani,@OrdersId,@ToplamTutar,@VergiTutari,@TumToplam,@Durus,@SatisOgesi,@Malzemeler,@Uretme)", prm);

            prm.Add("@SatisDetayId", id);
            prm.Add("@id", T.SatisId);

            return id;


        }
        public async Task Update(SalesOrderUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@CariId", T.CariId);
            prm.Add("@TeslimSuresi", T.TeslimSuresi);
            prm.Add("@OlusturmaTarihi", T.OlusturmaTarihi);
            prm.Add("@SatisIsmi", T.SatisIsmi);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@DepoId", T.DepoId);
            prm.Add("@Total", T.Toplam);
            prm.Add("@Bilgi", T.Bilgi);
            var location = await _db.QueryAsync<int>($"Select DepoId from Satis where id=@id ", prm);
            prm.Add("@eskilocationId", location.First());

            await _db.ExecuteAsync($"Update Satis set CariId=@CariId,TeslimSuresi=@TeslimSuresi,OlusturmaTarihi=@OlusturmaTarihi,SatisIsmi=@SatisIsmi,Bilgi=@Bilgi,DepoId=@DepoId,TumToplam=@Total where  id=@id", prm);

        }
        public async Task UpdateItems(TeklifUpdateItems T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.SatisId);
            prm.Add("@OrderStokId", T.id);
            prm.Add("@StokId", T.StokId);
            prm.Add("@Miktar", T.Miktar);
            prm.Add("@BirimFiyat", T.BirimFiyat);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@VergiId", T.VergiId);
            prm.Add("@CustomerId", T.CariId);
            prm.Add("@location", T.DepoId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@StokId", T.StokId);

            string sqlv = $@"Select StokId  from  SatisDetay where  id=@OrderStokId";
            var Item = await _db.QuerySingleAsync<int>(sqlv, prm);
            if (T.StokId != Item)
            {
                var liste = await _db.QueryAsync<LocaVarmı>($@"select 
            (select VergiDegeri from Vergi where id=(select VarsayilanSatinAlimVergi from GenelAyarlar))as VergiDegeri,
            (select VarsayilanFiyat from Urunler where id =@StokId)as VarsayilanFiyat", prm);
                prm.Add("@VergiId", T.VergiId);
                var Birimfiyat = liste.First().VarsayilanFiyat;
                T.BirimFiyat = Birimfiyat;

            }

            var VergiDegeri = await _db.QueryFirstAsync<float>($"(select VergiDegeri from Vergi where id =@VergiId)", prm);
            float TaxRate = VergiDegeri;
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

            await _db.ExecuteAsync($@"Update SatisDetay set StokId=@StokId,Miktar=@Miktar,TumToplam=@TumToplam,BirimFiyat=@BirimFiyat,VergiId=@VergiId,VergiOrani=@VergiOrani where  id=@OrderStokId and SatisId=@id", prm);


        }
        public async Task<IEnumerable<SalesOrderDetail>> Detail(int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", id);
            string sql = $@"select o.id,o.CariId,Cari.AdSoyad,o.DepoId,DepoVeAdresler.Isim as DepoIsmi,o.TeslimSuresi,o.OlusturmaTarihi,o.SatisIsmi,o.FaturaAdresiId,
                o.Bilgi,o.DurumBelirteci
                from Satis o
                left join SatisDetay oi on oi.SatisId=o.id
                left join Cari on Cari.CariKod=o.CariId
	            LEFT join DepoVeAdresler on DepoVeAdresler.id=o.DepoId
                where o.id=@id
                group by o.id,o.CariId,Cari.AdSoyad,o.TeslimSuresi,o.OlusturmaTarihi,o.SatisIsmi,o.FaturaAdresiId,o.KargoAdresiId,o.Bilgi,o.DepoId,DepoVeAdresler.Isim,o.DurumBelirteci";
            var details = await _db.QueryAsync<SalesOrderDetail>(sql, prm);
            foreach (var item in details)
            {
                DynamicParameters prm1 = new DynamicParameters();
                prm1.Add("@CompanyId", CompanyId);
                prm1.Add("@id", id);
                string sqla = $@"Select DepoId from Satis where id=@id";
                var sorgu = await _db.QueryAsync<int>(sqla, prm);
                prm1.Add("@DepoId", sorgu.First());

                string sql1 = $@"
           Select SatisDetay.id as id,SatisDetay.StokId,Urunler.Isim as UrunIsmi,SatisDetay.Miktar,Urunler.Tip,
           SatisDetay.BirimFiyat, SatisDetay.TumToplam, SatisDetay.VergiId, Vergi.VergiIsim,SatisDetay.VergiOrani as VergiDegeri
		   from Satis 
        inner join SatisDetay on SatisDetay.SatisId = Satis.id 
		left join Urunler on Urunler.id = SatisDetay.StokId
		left join Vergi on Vergi.id = SatisDetay.VergiId
        where Satis.id = @id  
		Group by SatisDetay.id,SatisDetay.StokId,Urunler.Isim,SatisDetay.Miktar,Urunler.Tip,
           SatisDetay.BirimFiyat, SatisDetay.TumToplam, SatisDetay.VergiId, Vergi.VergiIsim,SatisDetay.VergiOrani";
                var ItemsDetail = await _db.QueryAsync<SatısDetail>(sql1, prm1);
                item.detay = ItemsDetail;
            }
            return details;
        }

        public async Task<IEnumerable<SatısList>> SatisList(SatısListFiltre T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@DepoId", T.DepoId);
            string sql = string.Empty;

            if (T.BaslangıcTarih == null || T.BaslangıcTarih == "" || T.SonTarih == null || T.SonTarih == "")
            {
                if (T.DepoId == null)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad as CariAdSoyad,SUM(sa.TumToplam)AS TumToplam,
        sa.TeslimSuresi,sa.DepoId,DepoVeAdresler.Isim as DepoIsmi,sa.DurumBelirteci
		FROM Satis sa
        left join Cari on Cari.CariKod=sa.CariId
        left join SatisDetay on SatisDetay.SatisId=sa.id
		left join DepoVeAdresler on DepoVeAdresler.id=sa.DepoId
       where sa.Tip='Teklif'  and sa.Aktif=1 and sa.DurumBelirteci!=4 and 
        ISNULL(SatisIsmi,0) like '%{T.SatisIsmi}%'  and ISNULL(Cari.AdSoyad,'') Like '%{T.CariAdSoyad}%' AND    ISNULL(sa.TumToplam,'') like '%{T.TumToplam}%' and ISNULL(sa.DurumBelirteci,'') like '%{T.DurumBelirteci}%' 
        group by sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad,sa.TeslimSuresi,sa.DurumBelirteci,sa.DepoId,DepoVeAdresler.Isim)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";
                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad as CariAdSoyad,SUM(sa.TumToplam)AS TumToplam,
        sa.TeslimSuresi,sa.DepoId,DepoVeAdresler.Isim as DepoIsmi,sa.DurumBelirteci
		FROM Satis sa
        left join Cari on Cari.CariKod=sa.CariId
        left join SatisDetay on SatisDetay.SatisId=sa.id
		left join DepoVeAdresler on DepoVeAdresler.id=sa.DepoId
       where sa.Tip='Teklif'  and sa.DepoId=@DepoId and sa.Aktif=1 and sa.DurumBelirteci!=4 and 
        ISNULL(SatisIsmi,0) like '%{T.SatisIsmi}%'  and ISNULL(Cari.AdSoyad,'') Like '%{T.CariAdSoyad}%' AND    ISNULL(sa.TumToplam,'') like '%{T.TumToplam}%' and ISNULL(sa.DurumBelirteci,'') like '%{T.DurumBelirteci}%' 
        group by sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad,sa.TeslimSuresi,sa.DurumBelirteci,sa.DepoId,DepoVeAdresler.Isim)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";
                }
            }
            else
            {

                if (T.DepoId == null)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad as CariAdSoyad,SUM(sa.TumToplam)AS TumToplam,
        sa.TeslimSuresi,sa.DepoId,DepoVeAdresler.Isim as DepoIsmi,sa.DurumBelirteci
		FROM Satis sa
        left join Cari on Cari.CariKod=sa.CariId
        left join SatisDetay on SatisDetay.SatisId=sa.id
		left join DepoVeAdresler on DepoVeAdresler.id=sa.DepoId
       where sa.Tip='Teklif'  and sa.Aktif=1 and sa.DurumBelirteci!=4 and 
        ISNULL(SatisIsmi,0) like '%{T.SatisIsmi}%'  and ISNULL(Cari.AdSoyad,'') Like '%{T.CariAdSoyad}%' AND    ISNULL(sa.TumToplam,'') like '%{T.TumToplam}%'  and ISNULL(sa.DurumBelirteci,'') like '%{T.DurumBelirteci}%'  and sa.TeslimSuresi BETWEEN '{T.BaslangıcTarih}' and '{T.SonTarih}'
        group by sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad,sa.TeslimSuresi,sa.DurumBelirteci,sa.DepoId,DepoVeAdresler.Isim)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";

                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad as CariAdSoyad,SUM(sa.TumToplam)AS TumToplam,
        sa.TeslimSuresi,sa.DepoId,DepoVeAdresler.Isim as DepoIsmi,sa.DurumBelirteci
		FROM Satis sa
        left join Cari on Cari.CariKod=sa.CariId
        left join SatisDetay on SatisDetay.SatisId=sa.id
		left join DepoVeAdresler on DepoVeAdresler.id=sa.DepoId
       where sa.Tip='Teklif'  and sa.DepoId=@DepoId and sa.Aktif=1 and sa.DurumBelirteci!=4 and 
        ISNULL(SatisIsmi,0) like '%{T.SatisIsmi}%'  and ISNULL(Cari.AdSoyad,'') Like '%{T.CariAdSoyad}%' AND    ISNULL(sa.TumToplam,'') like '%{T.TumToplam}%' and ISNULL(sa.DurumBelirteci,'') like '%{T.DurumBelirteci}%' and sa.TeslimSuresi BETWEEN '{T.BaslangıcTarih}' and '{T.SonTarih}'
        group by sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad,sa.TeslimSuresi,sa.DurumBelirteci,sa.DepoId,DepoVeAdresler.Isim)x
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
            prm.Add("@StokId", T.StokId);
            prm.Add("@CompanyId", CompanyId);
            if (T.StokId != 0)
            {
                await _db.ExecuteAsync($"Delete from SatisDetay where id=@id and SatisId=@OrdersId", prm);
            }

        }
        public async Task DeleteStockControl(List<SatısDelete> A, int CompanyId, int User)
        {
            foreach (var T in A)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@CompanyId", CompanyId);
                param.Add("@id", T.id);
                param.Add("@Aktif", false);
                param.Add("@User", User);
                param.Add("@Date", DateTime.Now);
                var Aktifd = await _db.QueryAsync<bool>($"select Aktif from Satis where id=@id  ", param);
                if (Aktifd.First() == false)
                {

                }
                else
                {
                    var detay = await _db.QueryAsync<TeklifUpdateItems>($"select * from SatisDetay where SatisId=@id", param);
                    foreach (var item in detay)
                    {
                        param.Add("@itemid", item.id);

                        await _db.ExecuteAsync($"Delete from  SatisDetay where id = @itemid ", param);

                    }

                    await _db.ExecuteAsync($"Delete from  Satis where id = @id ", param);
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
                await _db.ExecuteAsync($"Update Satis set Tip='Satis' where id=@id", prm);

                List<ManufacturingOrderItemsIngredientsUpdate> Itemdegerler = (await _db.QueryAsync<ManufacturingOrderItemsIngredientsUpdate>($@"select StokId,Miktar,SatisDetay.id from SatisDetay 
                  inner join Satis on Satis.id=SatisDetay.SatisId
                  where SatisDetay.SatisId=@id and Satis.Aktif=1 and Satis.DurumBelirteci!=4", prm)).ToList();
                foreach (var item in Itemdegerler)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@StokId", item.StokId);
                    param.Add("@CompanyId", CompanyId);
                    param.Add("@location", T.DepoId);
                    param.Add("@id", T.id);
                    param.Add("@OrderStokId", item.id);
                    param.Add("@CariId", T.CariId);

                    var RezerveCount = await _control.Count(item.StokId, T.DepoId);
                    string sqla = $@"select
                     (Select ISNULL(Tip,'') from Urunler where id = @StokId)as Tip";
                    var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, param);
                    var Tip = sorgu.First().Tip;
                    SatısInsertItem A = new SatısInsertItem();
                    A.StokId = item.StokId;
                    A.DepoId = T.DepoId;
                    A.CariId = T.CariId;
                    A.Miktar = item.Miktar;



                    await _satis.Control(A, T.id, Tip, CompanyId);
                    if (A.Durum == 3)
                    {
                        param.Add("@SatisOgesi", 3);
                    }
                    else
                    {
                        param.Add("@SatisOgesi", 1);
                    }

                    if (A.Durum == 3)
                    {
                        param.Add("@SatisOgesi", 3);
                        param.Add("@Uretme", 4);
                        param.Add("@Ingredient", 3);


                        await _db.ExecuteAsync($"Update SatisDetay set SatisOgesi=@SatisOgesi,Uretme=@Uretme,Malzemeler=@Ingredient where id=@OrderStokId", param);


                        List<int> rezerveId = (await _db.QueryAsync<int>($"SELECT * FROM Rezerve where  SatisId=@id and DepoId=@location and Durum=1 and SatisDetayId is null", param)).ToList();
                        param.Add("@RezerveId", rezerveId[0]);

                        await _db.QueryAsync($"Update Rezerve set SatisDetayId=@OrderStokId where SatisId=@id and DepoId=@location and id=@RezerveId ", param);

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
                        param.Add("@Uretme", 0);

                        await _db.ExecuteAsync($"Update Rezerve set SatisDetayId=@OrderStokId where  SatisId=@id and DepoId=@location and SatisDetayId is null ", param);

                        await _db.ExecuteAsync($"Update SatisDetay set SatisOgesi=@SatisOgesi,Uretme=@Uretme,Malzemeler=@Ingredient where id=@OrderStokId", param);

                    }

                }

            }
            else
            {


            }
        }
    }




}

