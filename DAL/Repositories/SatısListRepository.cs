using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockListDTO;

namespace DAL.Repositories
{
    public class SatısListRepository:ISatısListRepository
    {
        private readonly IDbConnection _db;

        public SatısListRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SatısList>> SalesOrderList(SatısListFiltre T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@DepoId", T.DepoId);
            string sql = string.Empty;

            if (T.BaslangıcTarih==null || T.BaslangıcTarih=="" || T.SonTarih==null || T.SonTarih=="" )
            {
                if (T.DepoId == null)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad as CariAdSoyad,SUM(sa.TumToplam)AS TumToplam,
        sa.TeslimSuresi,MIN(SatisDetay.SatisOgesi)as SatisOgesi,Min(SatisDetay.Malzemeler)as Malzemeler,Min(SatisDetay.Uretme)as Uretme,sa.DepoId,DepoVeAdresler.Isim,sa.DurumBelirteci
		FROM Satis sa
        left join Cari on Cari.CariKod=sa.CariId
        left join SatisDetay on SatisDetay.SatisId=sa.id
		left join DepoVeAdresler on DepoVeAdresler.id=sa.DepoId
       where sa.Tip='Satis'  and sa.Aktif=1 and sa.DurumBelirteci!=4 and 
        ISNULL(SatisIsmi,0) like '%{T.SatisIsmi}%'  and ISNULL(Cari.AdSoyad,'') Like '%{T.CariAdSoyad}%' AND    ISNULL(sa.TumToplam,'') like '%{T.TumToplam}%' and
        ISNULL(SatisOgesi,'') like '%{T.SatisOgesi}%' and ISNULL(Malzemeler,'') like '%{T.Malzemeler}%' and ISNULL(Uretme,'') like '%{T.Uretme}%' and ISNULL(sa.DurumBelirteci,'') like '%{T.DurumBelirteci}%' 
        group by sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad,sa.TeslimSuresi,sa.DurumBelirteci,sa.DepoId,DepoVeAdresler.Isim)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";
                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad as CariAdSoyad,SUM(sa.TumToplam)AS TumToplam,
        sa.TeslimSuresi,MIN(SatisDetay.SatisOgesi)as SatisOgesi,Min(SatisDetay.Malzemeler)as Malzemeler,Min(SatisDetay.Uretme)as Uretme,sa.DepoId,DepoVeAdresler.Isim,sa.DurumBelirteci
		FROM Satis sa
        left join Cari on Cari.CariKod=sa.CariId
        left join SatisDetay on SatisDetay.SatisId=sa.id
		left join DepoVeAdresler on DepoVeAdresler.id=sa.DepoId
       where sa.Tip='Satis'  and sa.DepoId=@DepoId and sa.Aktif=1 and sa.DurumBelirteci!=4 and 
        ISNULL(SatisIsmi,0) like '%{T.SatisIsmi}%'  and ISNULL(Cari.AdSoyad,'') Like '%{T.CariAdSoyad}%' AND    ISNULL(sa.TumToplam,'') like '%{T.TumToplam}%' and
        ISNULL(SatisOgesi,'') like '%{T.SatisOgesi}%' and ISNULL(Malzemeler,'') like '%{T.Malzemeler}%' and ISNULL(Uretme,'') like '%{T.Uretme}%' and ISNULL(sa.DurumBelirteci,'') like '%{T.DurumBelirteci}%' 
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
        sa.TeslimSuresi,MIN(SatisDetay.SatisOgesi)as SatisOgesi,Min(SatisDetay.Malzemeler)as Malzemeler,Min(SatisDetay.Uretme)as Uretme,sa.DepoId,DepoVeAdresler.Isim,sa.DurumBelirteci
		FROM Satis sa
        left join Cari on Cari.CariKod=sa.CariId
        left join SatisDetay on SatisDetay.SatisId=sa.id
		left join DepoVeAdresler on DepoVeAdresler.id=sa.DepoId
       where  sa.Tip='Satis'  and sa.Aktif=1 and sa.DurumBelirteci!=4 and 
        ISNULL(SatisIsmi,0) like '%{T.SatisIsmi}%'  and ISNULL(Cari.AdSoyad,'') Like '%{T.CariAdSoyad}%' AND    ISNULL(sa.TumToplam,'') like '%{T.TumToplam}%' and
        ISNULL(SatisOgesi,'') like '%{T.SatisOgesi}%' and ISNULL(Malzemeler,'') like '%{T.Malzemeler}%' and ISNULL(Uretme,'') like '%{T.Uretme}%' and ISNULL(sa.DurumBelirteci,'') like '%{T.DurumBelirteci}%'  and sa.TeslimSuresi BETWEEN '{T.BaslangıcTarih}' and '{T.SonTarih}'
        group by sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad,sa.TeslimSuresi,sa.DurumBelirteci,sa.DepoId,DepoVeAdresler.Isim)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";

                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad as CariAdSoyad,SUM(sa.TumToplam)AS TumToplam,
        sa.TeslimSuresi,MIN(SatisDetay.SatisOgesi)as SatisOgesi,Min(SatisDetay.Malzemeler)as Malzemeler,Min(SatisDetay.Uretme)as Uretme,sa.DepoId,DepoVeAdresler.Isim,sa.DurumBelirteci
		FROM Satis sa
        left join Cari on Cari.CariKod=sa.CariId
        left join SatisDetay on SatisDetay.SatisId=sa.id
		left join DepoVeAdresler on DepoVeAdresler.id=sa.DepoId
       where  sa.Tip='Satis'  and sa.DepoId=@DepoId and sa.Aktif=1 and sa.DurumBelirteci!=4 and 
        ISNULL(SatisIsmi,0) like '%{T.SatisIsmi}%'  and ISNULL(Cari.AdSoyad,'') Like '%{T.CariAdSoyad}%' AND    ISNULL(sa.TumToplam,'') like '%{T.TumToplam}%' and
        ISNULL(SatisOgesi,'') like '%{T.SatisOgesi}%' and ISNULL(Malzemeler,'') like '%{T.Malzemeler}%' and ISNULL(Uretme,'') like '%{T.Uretme}%' and ISNULL(sa.DurumBelirteci,'') like '%{T.DurumBelirteci}%' and sa.TeslimSuresi BETWEEN '{T.BaslangıcTarih}' and '{T.SonTarih}'
        group by sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad,sa.TeslimSuresi,sa.DurumBelirteci,sa.DepoId,DepoVeAdresler.Isim)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";

                }
            }

        

            var ScheludeOpenList = await _db.QueryAsync<SatısList>(sql, param);


            foreach (var item in ScheludeOpenList)
            {
              
                param.Add("@Listid", item.id);
                param.Add("@SatisId", item.id);
                string sqlsorgu = $@"SELECT ma.id,ma.Isim,ma.StokId,Urunler.Isim,ma.BeklenenTarih,ma.UretimTarihi,ma.OlusturmaTarihi,ma.DepoId,ma.PlanlananMiktar,ma.ToplamMaliyet,ma.[Durum] FROM Uretim ma  
left join Urunler on Urunler.id=ma.StokId
Where ma.SatisId=@SatisId and  ma.Aktif=1 and ma.Durum!=3";
                var Manufacturing = await _db.QueryAsync<ManufacturingOrderDetail>(sqlsorgu, param);
                item.MOList = Manufacturing;

            }

            return ScheludeOpenList;
        }
        public async Task<IEnumerable<SatısList>> SalesOrderDoneList(SatısListFiltre T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@DepoId", T.DepoId);
            string sql = string.Empty;

            if (T.BaslangıcTarih == null || T.BaslangıcTarih == "" || T.SonTarih == null || T.SonTarih == "")
            {
                if (T.DepoId == null)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad as CariAdSoyad,SUM(sa.TumToplam)AS TumToplam,
        sa.TeslimSuresi,MIN(SatisDetay.SatisOgesi)as SatisOgesi,Min(SatisDetay.Malzemeler)as Malzemeler,Min(SatisDetay.Uretme)as Uretme,sa.DepoId,DepoVeAdresler.Isim,sa.DurumBelirteci
		FROM Satis sa
        left join Cari on Cari.CariKod=sa.CariId
        left join SatisDetay on SatisDetay.SatisId=sa.id
		left join DepoVeAdresler on DepoVeAdresler.id=sa.DepoId
       where  sa.Tip='Satis'  and sa.Aktif=1 and sa.DurumBelirteci=4 and 
        ISNULL(SatisIsmi,0) like '%{T.SatisIsmi}%'  and ISNULL(Cari.AdSoyad,'') Like '%{T.CariAdSoyad}%' AND    ISNULL(sa.TumToplam,'') like '%{T.TumToplam}%' and
        ISNULL(SatisOgesi,'') like '%{T.SatisOgesi}%' and ISNULL(Malzemeler,'') like '%{T.Malzemeler}%' and ISNULL(Uretme,'') like '%{T.Uretme}%' and ISNULL(sa.DurumBelirteci,'') like '%{T.DurumBelirteci}%' 
        group by sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad,sa.TeslimSuresi,sa.DurumBelirteci,sa.DepoId,DepoVeAdresler.Isim)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";
                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad as CariAdSoyad,SUM(sa.TumToplam)AS TumToplam,
        sa.TeslimSuresi,MIN(SatisDetay.SatisOgesi)as SatisOgesi,Min(SatisDetay.Malzemeler)as Malzemeler,Min(SatisDetay.Uretme)as Uretme,sa.DepoId,DepoVeAdresler.Isim,sa.DurumBelirteci
		FROM Satis sa
        left join Cari on Cari.CariKod=sa.CariId
        left join SatisDetay on SatisDetay.SatisId=sa.id
		left join DepoVeAdresler on DepoVeAdresler.id=sa.DepoId
       where sa.Tip='Satis'  and sa.DepoId=@DepoId and sa.Aktif=1 and sa.DurumBelirteci=4 and 
        ISNULL(SatisIsmi,0) like '%{T.SatisIsmi}%'  and ISNULL(Cari.AdSoyad,'') Like '%{T.CariAdSoyad}%' AND    ISNULL(sa.TumToplam,'') like '%{T.TumToplam}%' and
        ISNULL(SatisOgesi,'') like '%{T.SatisOgesi}%' and ISNULL(Malzemeler,'') like '%{T.Malzemeler}%' and ISNULL(Uretme,'') like '%{T.Uretme}%' and ISNULL(sa.DurumBelirteci,'') like '%{T.DurumBelirteci}%' 
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
        sa.TeslimSuresi,MIN(SatisDetay.SatisOgesi)as SatisOgesi,Min(SatisDetay.Malzemeler)as Malzemeler,Min(SatisDetay.Uretme)as Uretme,sa.DepoId,DepoVeAdresler.Isim,sa.DurumBelirteci
		FROM Satis sa
        left join Cari on Cari.CariKod=sa.CariId
        left join SatisDetay on SatisDetay.SatisId=sa.id
		left join DepoVeAdresler on DepoVeAdresler.id=sa.DepoId
       where sa.Tip='Satis'  and sa.Aktif=1 and sa.DurumBelirteci=4 and 
        ISNULL(SatisIsmi,0) like '%{T.SatisIsmi}%'  and ISNULL(Cari.AdSoyad,'') Like '%{T.CariAdSoyad}%' AND    ISNULL(sa.TumToplam,'') like '%{T.TumToplam}%' and
        ISNULL(SatisOgesi,'') like '%{T.SatisOgesi}%' and ISNULL(Malzemeler,'') like '%{T.Malzemeler}%' and ISNULL(Uretme,'') like '%{T.Uretme}%' and ISNULL(sa.DurumBelirteci,'') like '%{T.DurumBelirteci}%'  and sa.TeslimSuresi BETWEEN '{T.BaslangıcTarih}' and '{T.SonTarih}'
        group by sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad,sa.TeslimSuresi,sa.DurumBelirteci,sa.DepoId,DepoVeAdresler.Isim)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";

                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (
        select sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad as CariAdSoyad,SUM(sa.TumToplam)AS TumToplam,
        sa.TeslimSuresi,MIN(SatisDetay.SatisOgesi)as SatisOgesi,Min(SatisDetay.Malzemeler)as Malzemeler,Min(SatisDetay.Uretme)as Uretme,sa.DepoId,DepoVeAdresler.Isim,sa.DurumBelirteci
		FROM Satis sa
        left join Cari on Cari.CariKod=sa.CariId
        left join SatisDetay on SatisDetay.SatisId=sa.id
		left join DepoVeAdresler on DepoVeAdresler.id=sa.DepoId
       where sa.Tip='Satis'  and sa.DepoId=@DepoId and sa.Aktif=1 and sa.DurumBelirteci=4 and 
        ISNULL(SatisIsmi,0) like '%{T.SatisIsmi}%'  and ISNULL(Cari.AdSoyad,'') Like '%{T.CariAdSoyad}%' AND    ISNULL(sa.TumToplam,'') like '%{T.TumToplam}%' and
        ISNULL(SatisOgesi,'') like '%{T.SatisOgesi}%' and ISNULL(Malzemeler,'') like '%{T.Malzemeler}%' and ISNULL(Uretme,'') like '%{T.Uretme}%' and ISNULL(sa.DurumBelirteci,'') like '%{T.DurumBelirteci}%' and sa.TeslimSuresi BETWEEN '{T.BaslangıcTarih}' and '{T.SonTarih}'
        group by sa.id,sa.SatisIsmi,sa.CariId,Cari.AdSoyad,sa.TeslimSuresi,sa.DurumBelirteci,sa.DepoId,DepoVeAdresler.Isim)x
        ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ;";

                }
            }



            var ScheludeOpenList = await _db.QueryAsync<SatısList>(sql, param);


            foreach (var item in ScheludeOpenList)
            {

                param.Add("@Listid", item.id);
                param.Add("@SatisId", item.id);
                string sqlsorgu = $@"SELECT ma.id,ma.Isim,ma.StokId,Urunler.Isim as UrunIsmi,ma.BeklenenTarih,ma.OlusturmaTarihi,ma.PlanlananMiktar,ma.ToplamMaliyet,ma.[Durum] FROM Uretim ma  
left join Urunler on Urunler.id=ma.StokId
Where ma.SatisId=@SatisId and ma.Aktif=1 and ma.Durum=3";
                var Manufacturing = await _db.QueryAsync<ManufacturingOrderDetail>(sqlsorgu, param);
                item.MOList = Manufacturing;

            }

            return ScheludeOpenList;
        }

        public async Task<IEnumerable<SalesOrderDetail>> Detail(int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            string sql = $@"select o.id,o.CariId,Cari.AdSoyad,o.DepoId,DepoVeAdresler.Isim as DepoIsmi,o.TeslimSuresi,o.OlusturmaTarihi,o.SatisIsmi,o.FaturaAdresiId,o.KargoAdresiId,
                o.Bilgi,o.DurumBelirteci
                from Satis o
                left join SatinAlmaDetay oi on oi.SatisId=o.id
                left join Cari on Cari.CariKod=o.CariId
	            LEFT join DepoVeAdresler on DepoVeAdresler.id=o.DepoId
                where o.id=@id
                group by o.id,o.CariId,Cari.AdSoyad,o.DepoId,DepoVeAdresler.Isim as DepoIsmi,o.TeslimSuresi,o.OlusturmaTarihi,o.SatisIsmi,o.FaturaAdresiId,o.KargoAdresiId,
                o.Bilgi,o.DurumBelirteci";
            var details = await _db.QueryAsync<SalesOrderDetail>(sql, prm);
            foreach (var item in details)
            {
                DynamicParameters prm1 = new DynamicParameters();
                prm1.Add("@id", id);
                string sqla = $@"Select DepoId from Satis where id=@id";
                var sorgu = await _db.QueryAsync<int>(sqla, prm);
                prm1.Add("@DepoId", sorgu.First());

                string sql1 = $@"
           Select SatisDetay.id as id,SatisDetay.StokId,Urunler.Isim as UrunIsmi,SatisDetay.Miktar,Urunler.Tip,
           SatisDetay.BirimFiyat, SatisDetay.TumTolam, SatisDetay.VergiId, Vergi.VergiIsmi,SatisDetay.VergiDegeri as Rate,SatisDetay.SatisOgesi,SatisDetay.Malzemeler,SatisDetay.Uretme,
		     (SUM(ISNULL(rez.RezerveDeger,0)))- ISNULL(SatisDetay.Miktar,0)+(SUM(ISNULL(Uretim.PlanlananMiktar,0)))as missing
		   from Satis 
        inner join SatisDetay on SatisDetay.SatisId = Satis.id 
		left join Urunler on Urunler.id = SatisDetay.StokId
		left join Tax on Tax.id = SatisDetay.TaxId
		LEFT join Uretim on Uretim.SatisDetayId=SatisDetay.id and Uretim.SatisId=Satis.id and Uretim.Durum!=3 and Uretim.Aktif=1
        LEFT join Rezerve on Rezerve.SatisDetayId=SatisDetay.id and Rezerve.SatisId=Satis.id and Rezerve.StokId=Urunler.id
        LEFT join Rezerve rez on rez.StokId=Urunler.id and rez.Durum=1
        where Satis.id = @id  
		Group by SatisDetay.id,SatisDetay.StokId,Urunler.Name,SatisDetay.Miktar,Urunler.Tip,
           SatisDetay.BirimFiyat, SatisDetay.TumToplam, SatisDetay.TaxId, Tax.TaxName,SatisDetay.TaxValue,
		         SatisDetay.SatisOgesi,SatisDetay.Malzemeler,SatisDetay.Uretme,Rezerve.RezerveDeger";
                var ItemsDetail = await _db.QueryAsync<SatısDetail>(sql1, prm1);
                item.detay = ItemsDetail;
            }
            return details;
        }
        public async Task<IEnumerable<MissingCount>> IngredientsMissingList(IngredientMis T, int CompanyId)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@MamulId", T.MamulId);
            prm.Add("@id", T.id);
            prm.Add("@OrderItemId", T.SatisDetayId);
            prm.Add("@DepoId", T.DepoId);
            var make = await _db.QueryAsync<SalesOrderRezerve>($"select mo.id from Uretim mo where SatisId=@id and SatisDetayId=@OrderItemId and  Aktif=1 and Durum!=3", prm);
            IEnumerable<MissingCount> materialid;
            IEnumerable<MissingCount> list = new List<MissingCount>();
            if (make.Count() == 0)
            {
                string sql = $"select UrunRecetesi.MalzemeId from UrunRecetesi where  UrunRecetesi.MamulId = @MamulId";
                materialid = await _db.QueryAsync<MissingCount>(sql, prm);

                foreach (var item in materialid)
                {
                    prm.Add("@MalzemeId", item.MalzemeId);
                    string sqlb = $@"select UrunRecetesi.MalzemeId,Urunler.Name as MaterialName,
        (Select Rezerve.RezerveDeger from Rezerve where Rezerve.SatisId = @id and Rezerve.StokId= @MalzemeId and SatisDetayId=@OrderItemId) -
        ((Select SatisDetay.Miktar from Satis sa left join SatisDetay on SatisDetay.SatisId = sa.id where sa.id = @id and SatisDetay.id=@OrderItemId) *
        (select UrunRecetesi.Miktar from UrunRecetesi where UrunRecetesi.MalzemeId = @MalzemeId and UrunRecetesi.MamulId = @MamulId))
         AS Kayip
        FROM UrunRecetesi left join Urunler on Urunler.id = UrunRecetesi.MalzemeId where  UrunRecetesi.MamulId = @MamulId and UrunRecetesi.MalzemeId = @MalzemeId";
                    var a = await _db.QueryAsync<MissingCount>(sqlb, prm);
                    list.Append(a.First());

                }
            }
            else
            {
                materialid = await _db.QueryAsync<MissingCount>($@"SELECT UretimDetayId.StokId as MalzemeId from UretimDetayId 
            left join Uretim on Uretim.id=UretimDetayId.UretimId
            where Uretim.SatisId=@id and Uretim.SatisDetayId=@OrderItemId and UretimDetayId.Tip='Malzemeler' and Uretim.Aktif=1 and Uretim.Durum!=3 and UretimDetayId.MalzemeDurum=0
            Group By UretimDetayId.StokId", prm);
                foreach (var liste in materialid)
                {
                    prm.Add("@MalzemeId", liste.MalzemeId);
                    string sqlb = $@"select UrunRecetesi.MalzemeId,Urunler.Name as MaterialName,
             (Select SUM(Rezerve.RezerveDeger) from Rezerve where Rezerve.SatisId = @id and Rezerve.StokId= @MalzemeId and SatisDetayId=@OrderItemId) -
             (Select SUM(UretimDetayId.PlanlananMiktar) from UretimDetayId 
		        LEFT join Uretim on UretimDetayId.UretimId=Uretim.id
		    where Uretim.SatisId=@id and Uretim.SatisDetayId=@OrderItemId 
			and UretimDetayId.Tip='Malzemeler' and UretimDetayId.StokId=@MalzemeId and Uretim.Aktif=1 
			and Uretim.Durum!=3)+
				(select ISNULL(SUM(Miktar),0) from SatinAlma 
                left join SatinAlmaDetay on SatinAlmaDetay.SatinAlmaId = SatinAlma.id and SatinAlma.DepoId=@DepoId
                and SatinAlmaDetay.StokId = @MalzemeId where  DurumBelirteci = 1 and SatinAlma.SatisId=@id and SatinAlma.SatisDetayId=@OrderItemId and SatinAlma.Aktif=1)
                 AS Missing
                 FROM UrunRecetesi left join Urunler on Urunler.id = UrunRecetesi.MalzemeId where UrunRecetesi.MamulId = @MamulId and UrunRecetesi.MalzemeId = @MalzemeId";
                    var a = await _db.QueryAsync<MissingCount>(sqlb, prm);

                    list.Append(a.First());

                }

            }



            return (IEnumerable<SalesOrderDTO.MissingCount>)list;
        }
        public async Task<IEnumerable<SalesOrderSellSomeList>> SalesManufacturingList(int SatisId,int SatisDetayId, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@SatisId", SatisId);
            prm.Add("@SatisDetayId", SatisDetayId);
            string sql = $@"select id,StokId,PlanlananMiktar,DepoId,[Durum],[Name],TeslimSuresi from Uretim ma where ma.SatisId=@SatisId and ma.SatisDetayId=@SatisDetayId and ma.Aktif=1 and  ma.Durum!=3";
            var details = await _db.QueryAsync<SalesOrderSellSomeList>(sql, prm);
            return details;
        }


    }
}
