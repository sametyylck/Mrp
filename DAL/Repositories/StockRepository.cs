using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockListDTO;

namespace DAL.Repositories
{
    public class StockRepository : IStockRepository
    {
        IDbConnection _db;

        public StockRepository(IDbConnection dbConnection)
        {
            _db = dbConnection;
        }


        public async Task<IEnumerable<StokListResponse>> AllItemsList(StokList T, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@LocationId", T.DepoId);
            string sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI} SET @SAYFA = {SAYFA}
select x.* From(select  ur.id,ur.Isim,ur.Tip,ur.StokKodu,ur.KategoriId,Kategoriler.Isim as KategoriIsmi,ur.TedarikciId,ISNULL(Cari.AdSoyad,'') as Tedarikci,
(select StokAdeti from DepoStoklar where ur.id=DepoStoklar.StokId and DepoStoklar.DepoId=@LocationId )as StokMiktari,ur.VarsayilanFiyat,
(ur.VarsayilanFiyat*(select StokAdeti from DepoStoklar where ur.id=DepoStoklar.StokId and DepoStoklar.DepoId=@LocationId ))as StokDegeri,
ISNULL((Select SUM(RezerveDeger) from Rezerve  where Rezerve.StokId=ur.id and Rezerve.Durum=1 and Rezerve.DepoId=@LocationId),0) AS RezerveMiktari,
(ISNULL((select SUM(Miktar) from SatinAlmaDetay st 
left join SatinAlma on SatinAlma.id=st.SatinAlmaId
where st.StokId=ur.id and SatinAlma.DurumBelirteci=1 and SatinAlma.DepoId=@LocationId ),0)+

ISNULL((select SUM(PlanlananMiktar) from Uretim where
Uretim.StokId=ur.id and Uretim.DepoId=@LocationId and Uretim.Durum!=3),0)) as BeklenenStok,

(select StokAdeti from DepoStoklar where ur.id=DepoStoklar.StokId and DepoStoklar.DepoId=@LocationId )-(ISNULL((Select SUM(RezerveDeger) from Rezerve  where Rezerve.StokId=ur.id and Rezerve.Durum=1 and Rezerve.DepoId=@LocationId),0)) as KullanılabilirStok,
(Select Isim from DepoVeAdresler where id=@LocationId)as DepoIsmi,@LocationId as DepoId

from Urunler ur
left join Kategoriler on Kategoriler.id=ur.KategoriId
left join Cari on Cari.CariKod=ur.TedarikciId
left join Rezerve on Rezerve.StokId=ur.id)x
where ISNULL(x.Isim,'') like '%{T.Isim}%' and ISNULL(x.KategoriIsmi,'') like '%{T.KategoriIsmi}%' and  ISNULL(x.Tedarikci,'') like '%{T.Tedarikci}%' 
and ISNULL(x.StokKodu,'') like '%{T.StokKodu}%' and ISNULL(x.StokDegeri,'') like '%{T.StokDegeri}%' and ISNULL(x.VarsayilanFiyat,'') like '%{T.VarsayilanFiyat}%' 
and ISNULL(x.RezerveMiktari,'') like '%{T.RezerveMiktari}%' and ISNULL(x.BeklenenStok,'') like '%{T.BeklenenStok}%' and ISNULL(x.KullanılabilirStok,'') like '%{T.KullanabilirStok}%'

Group by x.id,x.Isim,x.StokKodu,x.KategoriId,x.DepoIsmi,x.DepoId,x.KategoriIsmi,x.TedarikciId,x.Tedarikci,x.VarsayilanFiyat,x.Tip,x.StokMiktari,x.RezerveMiktari,x.KullanılabilirStok,x.StokDegeri,x.BeklenenStok
ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY  ";
            var list =await _db.QueryAsync<StokListResponse>(sql, prm);
            return list.ToList();
        }


        public async Task<IEnumerable<StokListResponse>> MaterialList(StokList T, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@LocationId", T.DepoId);
            string sql = @$"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI} SET @SAYFA = {SAYFA}
select x.* From(select  ur.id,ur.Isim,ur.Tip,ur.StokKodu,ur.KategoriId,Kategoriler.Isim as KategoriIsmi,ur.TedarikciId,ISNULL(Cari.AdSoyad,'') as Tedarikci,
(select StokAdeti from DepoStoklar where ur.id=DepoStoklar.StokId and DepoStoklar.DepoId=@LocationId )as StokMiktari,ur.VarsayilanFiyat,
(ur.VarsayilanFiyat*(select StokAdeti from DepoStoklar where ur.id=DepoStoklar.StokId and DepoStoklar.DepoId=@LocationId ))as StokDegeri,
ISNULL((Select SUM(RezerveDeger) from Rezerve  where Rezerve.StokId=ur.id and Rezerve.Durum=1 and Rezerve.DepoId=@LocationId),0) AS RezerveMiktari,
ISNULL((select SUM(Miktar) from SatinAlmaDetay st 
left join SatinAlma on SatinAlma.id=st.SatinAlmaId
where st.StokId=ur.id and SatinAlma.DurumBelirteci=1 and SatinAlma.DepoId=@LocationId ),0) as BeklenenStok,
(select StokAdeti from DepoStoklar where ur.id=DepoStoklar.StokId and DepoStoklar.DepoId=@LocationId )-(ISNULL((Select SUM(RezerveDeger) from Rezerve  where Rezerve.StokId=ur.id and Rezerve.Durum=1 and Rezerve.DepoId=@LocationId),0)) as KullanılabilirStok ,
(Select Isim from DepoVeAdresler where id=@LocationId)as DepoIsmi,@LocationId as DepoId

from Urunler ur
left join Kategoriler on Kategoriler.id=ur.KategoriId
left join Cari on Cari.CariKod=ur.TedarikciId
left join Rezerve on Rezerve.StokId=ur.id)x
where x.Tip='Material' and  ISNULL(x.Isim,'') like '%{T.Isim}%' and ISNULL(x.KategoriIsmi,'') like '%{T.KategoriIsmi}%' and  ISNULL(x.Tedarikci,'') like '%{T.Tedarikci}%' 
and ISNULL(x.StokKodu,'') like '%{T.StokKodu}%' and ISNULL(x.StokDegeri,'') like '%{T.StokDegeri}%' and ISNULL(x.VarsayilanFiyat,'') like '%{T.VarsayilanFiyat}%' 
and ISNULL(x.RezerveMiktari,'') like '%{T.RezerveMiktari}%' and ISNULL(x.BeklenenStok,'') like '%{T.BeklenenStok}%' and ISNULL(x.KullanılabilirStok,'') like '%{T.KullanabilirStok}%'

Group by x.id,x.Isim,x.StokKodu,x.KategoriId,x.DepoIsmi,x.DepoId,x.KategoriIsmi,x.TedarikciId,x.Tedarikci,x.VarsayilanFiyat,x.Tip,x.StokMiktari,x.RezerveMiktari,x.KullanılabilirStok,x.StokDegeri,x.BeklenenStok
ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ";
            var list =await _db.QueryAsync<StokListResponse>(sql, prm);
            return list.ToList();
        }


        public async Task<IEnumerable<StokListResponse>> ProductList(StokList T, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@LocationId", T.DepoId);

            string sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI} SET @SAYFA = {SAYFA}
select x.* From(select  ur.id,ur.Isim,ur.Tip,ur.StokKodu,ur.KategoriId,Kategoriler.Isim as KategoriIsmi,ur.TedarikciId,ISNULL(Cari.AdSoyad,'') as Tedarikci,
(select StokAdeti from DepoStoklar where ur.id=DepoStoklar.StokId and DepoStoklar.DepoId=@LocationId )as StokMiktari,ur.VarsayilanFiyat,
(ur.VarsayilanFiyat*(select StokAdeti from DepoStoklar where ur.id=DepoStoklar.StokId and DepoStoklar.DepoId=@LocationId ))as StokDegeri,
ISNULL((Select SUM(RezerveDeger) from Rezerve  where Rezerve.StokId=ur.id and Rezerve.Durum=1 and Rezerve.DepoId=@LocationId),0) AS RezerveMiktari,
ISNULL((select SUM(PlanlananMiktar) from Uretim where
Uretim.StokId=ur.id and Uretim.DepoId=@LocationId and Uretim.Durum!=3),0) as BeklenenStok,
(select StokAdeti from DepoStoklar where ur.id=DepoStoklar.StokId and DepoStoklar.DepoId=@LocationId )-(ISNULL((Select SUM(RezerveDeger) from Rezerve  where Rezerve.StokId=ur.id and Rezerve.Durum=1 and Rezerve.DepoId=@LocationId),0)) as KullanılabilirStok ,
(Select Isim from DepoVeAdresler where id=@LocationId)as DepoIsmi,@LocationId as DepoId

from Urunler ur
left join Kategoriler on Kategoriler.id=ur.KategoriId
left join Cari on Cari.CariKod=ur.TedarikciId
left join Rezerve on Rezerve.StokId=ur.id)x
where x.Tip='Product' and  ISNULL(x.Isim,'') like '%{T.Isim}%' and ISNULL(x.KategoriIsmi,'') like '%{T.KategoriIsmi}%' and  ISNULL(x.Tedarikci,'') like '%{T.Tedarikci}%' 
and ISNULL(x.StokKodu,'') like '%{T.StokKodu}%' and ISNULL(x.StokDegeri,'') like '%{T.StokDegeri}%' and ISNULL(x.VarsayilanFiyat,'') like '%{T.VarsayilanFiyat}%' 
and ISNULL(x.RezerveMiktari,'') like '%{T.RezerveMiktari}%' and ISNULL(x.BeklenenStok,'') like '%{T.BeklenenStok}%' and ISNULL(x.KullanılabilirStok,'') like '%{T.KullanabilirStok}%'

Group by x.id,x.Isim,x.StokKodu,x.KategoriId,x.DepoIsmi,x.DepoId,x.KategoriIsmi,x.TedarikciId,x.Tedarikci,x.VarsayilanFiyat,x.Tip,x.StokMiktari,x.RezerveMiktari,x.KullanılabilirStok,x.StokDegeri,x.BeklenenStok
ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY  ";
            var list =await _db.QueryAsync<StokListResponse>(sql, prm);
      
            return list.ToList();
        }

        public async Task<IEnumerable<StokListResponse>> SemiProductList(StokList T, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@location", T.DepoId);

            string sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI} SET @SAYFA = {SAYFA}
            select x.* From(select  ur.id,ur.Isim,ur.Tip,ur.StokKodu,ur.KategoriId,Kategoriler.Isim as KategoriIsmi,ur.TedarikciId,ISNULL(Cari.AdSoyad,'') as Tedarikci,
            (select StokAdeti from DepoStoklar where ur.id=DepoStoklar.StokId and DepoStoklar.DepoId=@LocationId )as StokMiktari,ur.VarsayilanFiyat,
            (ur.VarsayilanFiyat*(select StokAdeti from DepoStoklar where ur.id=DepoStoklar.StokId and DepoStoklar.DepoId=@LocationId ))as StokDegeri,
            ISNULL((Select SUM(RezerveDeger) from Rezerve  where Rezerve.StokId=ur.id and Rezerve.Durum=1 and Rezerve.DepoId=@LocationId),0) AS RezerveMiktari,
            (ISNULL((select SUM(Miktar) from SatinAlmaDetay st 
            left join SatinAlma on SatinAlma.id=st.SatinAlmaId
            where st.StokId=ur.id and SatinAlma.DurumBelirteci=1 and SatinAlma.DepoId=@LocationId ),0)+

            ISNULL((select SUM(PlanlananMiktar) from Uretim where
            Uretim.StokId=ur.id and Uretim.DepoId=@LocationId and Uretim.Durum!=3),0)) as BeklenenStok,
            (select StokAdeti from DepoStoklar where ur.id=DepoStoklar.StokId and DepoStoklar.DepoId=@LocationId )-(ISNULL((Select SUM(RezerveDeger) from Rezerve  where Rezerve.StokId=ur.id and Rezerve.Durum=1 and Rezerve.DepoId=@LocationId),0)) as KullanılabilirStok,
            (Select Isim from DepoVeAdresler where id=@LocationId)as DepoIsmi,4 as DepoId

            from Urunler ur
            left join Kategoriler on Kategoriler.id=ur.KategoriId
            left join Cari on Cari.CariKod=ur.TedarikciId
            left join Rezerve on Rezerve.StokId=ur.id)x
            where x.Tip='SemiProduct' and  ISNULL(x.Isim,'') like '%{T.Isim}%' and ISNULL(x.KategoriIsmi,'') like '%{T.KategoriIsmi}%' and  ISNULL(x.Tedarikci,'') like '%{T.Tedarikci}%' 
            and ISNULL(x.StokKodu,'') like '%{T.StokKodu}%' and ISNULL(x.StokDegeri,'') like '%{T.StokDegeri}%' and ISNULL(x.VarsayilanFiyat,'') like '%{T.VarsayilanFiyat}%' 
            and ISNULL(x.RezerveMiktari,'') like '%{T.RezerveMiktari}%' and ISNULL(x.BeklenenStok,'') like '%{T.BeklenenStok}%' and ISNULL(x.KullanılabilirStok,'') like '%{T.KullanabilirStok}%'

            Group by x.id,x.Isim,x.StokKodu,x.KategoriId,x.DepoIsmi,x.DepoId,x.KategoriIsmi,x.TedarikciId,x.Tedarikci,x.VarsayilanFiyat,x.Tip,x.StokMiktari,x.RezerveMiktari,x.KullanılabilirStok,x.StokDegeri,x.BeklenenStok
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ";
            var list = await _db.QueryAsync<StokListResponse>(sql, prm);

            return list.ToList();
        }
    }
}
