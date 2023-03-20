using DAL.Contracts;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;

namespace DAL.Repositories
{
    public class UretimListRepository : IUretimList
    {
        private readonly IDbConnection _db;

        public UretimListRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ManufacturingOrderDetail>> Detail(int CompanyId, int id)
        {
            DynamicParameters prm = new();
            prm.Add("@CompanyId", CompanyId);

            string sql = $@"
                                Select Uretim.id , Uretim.[Isim],Uretim.StokId,Urunler.[Isim] as UrunIsmi,Uretim.BeklenenTarih,Uretim.SatisId,Uretim.SatisDetayId,                                 Uretim.UretimTarihi,Uretim.OlusturmaTarihi,Uretim.PlanlananMiktar,
                                Uretim.DepoId,DepoVeAdresler.Isim,Uretim.[Durum],Uretim.Bilgi
                                From Uretim
                                inner join Urunler on Urunler.id = Uretim.StokId
                                inner join DepoVeAdresler on DepoVeAdresler.id = Uretim.DepoId
                                where Uretim.id = {id}";
            var Detail = await _db.QueryAsync<ManufacturingOrderDetail>(sql);
            foreach (var item in Detail)
            {
                prm.Add("@DepoId", item.DepoId);
                prm.Add("@id", id);

                string sql1 = $@"  select moi.id,moi.Tip,moi.StokId,Urunler.Isim,ISNULL(moi.Bilgi,'')AS Bilgi,moi.PlanlananMiktar,moi.Tutar,moi.MalzemeDurum,
ISNULL(SUM(DISTINCT(case when SatinAlma.DurumBelirteci=1 then SatinAlmaDetay.Miktar else 0 end)),0)-ISNULL(moi.PlanlananMiktar,0)+ISNULL(rez.RezerveDeger,0) as Kayip
from UretimDetay moi
left join Uretim mao on mao.id=moi.UretimId
left join Urunler on Urunler.id=moi.StokId
left join DepoStoklar on DepoStoklar.StokId=moi.StokId and DepoStoklar.DepoId=@DepoId
left join Rezerve rez on rez.UretimId=mao.id and rez.UretimDetayId=moi.id  and rez.Durum=1  and rez.DepoId=@DepoId
left join SatinAlma on SatinAlma.UretimId=mao.id and SatinAlma.UretimDetayId=moi.id 
left join SatinAlmaDetay on SatinAlmaDetay.SatinAlmaId=SatinAlma.id
where mao.id=@id and moi.Tip='Ingredients' and mao.DepoId=@DepoId
Group by moi.id,moi.Tip,moi.StokId,Urunler.Isim,moi.Bilgi,moi.PlanlananMiktar ,moi.Tutar,moi.MalzemeDurum,
 moi.PlanlananMiktar,rez.RezerveDeger,SatinAlma.DurumBelirteci,SatinAlmaDetay.Miktar   
            ";
                var IngredientsDetail = await _db.QueryAsync<ManufacturingOrderItemsIngredientsDetail>(sql1, prm);
                string sql2 = $@"Select moi.id,
                            moi.OperasyonId,Operasyonlar.[Isim] as OperasyonIsmi,
                            moi.KaynakId ,Kaynaklar.[Isim] as KaynakIsmi,
                            moi.PlanlananZaman,moi.SaatlikUcret,
                            Cast(ISNULL(moi.Tutar,0)as decimal(15,2)) as Tutar,
                            moi.[Durum]
                            From UretimDetay moi
                            left join Operasyonlar on Operasyonlar.id = moi.OperasyonId
                            left join Kaynaklar on moi.KaynakId = Kaynaklar.id
                            where Tip='Operasyonlar' and moi.UretimId={id}";
                var OperationDetail = await _db.QueryAsync<ManufacturingOrderItemsOperationDetail>(sql2);
                item.IngredientDetail = IngredientsDetail;
                item.OperationDetail = OperationDetail;

            }

            return Detail.ToList();
        }

        public async Task<IEnumerable<ManufacturingOrderItemsIngredientsDetail>> IngredientsDetail(int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", id);
            var Location = await _db.QueryAsync<int>($"Select DepoId From Uretim where CompanyId = {CompanyId} and id = {id}");
            prm.Add("@DepoId", Location.First());
            string sql = $@"  select moi.id,moi.Tip,moi.StokId,Urunler.Isim,ISNULL(Bilgi,'')AS Bilgi,moi.PlanlananMiktar,moi.Tutar,moi.MalzemeDurum,
            (ISNULL(DepoStoklar.StokAdeti,0)-ISNULL(SUM(DISTINCT(Rezerve.RezerveDeger)),0))+(ISNULL(rez.RezerveDeger,0))-(ISNULL(moi.PlanlananMiktar,0))+ISNULL(SUM(DISTINCT(case when SatinAlma.DurumBelirteci=1 then SatinAlmaDetay.Miktar else 0 end)),0)AS missing
            from UretimDetay moi
            left join Uretim mao on mao.id=moi.UretimId
            left join Urunler on Urunler.id=moi.StokId
            left join DepoStoklar on DepoStoklar.StokId=moi.StokId and DepoStoklar.DepoId=@DepoId
            left join SatinAlmaDetay on SatinAlmaDetay.StokId=moi.StokId 
            right join SatinAlma on SatinAlma.id=SatinAlmaDetay.SatinAlmaId and SatinAlma.UretimId=mao.id 
            left join Rezerve on Rezerve.StokId=Urunler.id  and Rezerve.Durum=1  and Rezerve.DepoId=@DepoId
			 left join Rezerve rez on rez.UretimId=mao.id and rez.UretimDetayId=moi.id  and rez.Durum=1  and rez.DepoId=@DepoId
            where mao.id=@id and moi.Tip='Ingredients' and mao.DepoId=@DepoId  and mao.Durum!=3
            Group by moi.id,moi.Tip,moi.StokId,Urunler.Isim,moi.Notes,moi.PlanlananMiktar ,moi.Tutar,moi.MalzemeDurum,
            moi.PlanlananMiktar,DepoStoklar.StokAdeti,rez.RezerveDeger,SatinAlma.DurumBelirteci,SatinAlmaDetay.Miktar   
            ";
            var IngredientsDetail = await _db.QueryAsync<ManufacturingOrderItemsIngredientsDetail>(sql, prm);

            return IngredientsDetail.ToList();
        }

        public async Task<IEnumerable<ManufacturingOrderItemsOperationDetail>> OperationDetail(int CompanyId, int id)
        {
            string sql = $@"Select moi.id,
                            moi.OperasyonId,Operasyonlar.[Isim] as OperasyonIsmi,
                            moi.KaynakId ,Kaynaklar.[Isim] as KaynakIsmi,
                            moi.PlanlananZaman,moi.SaatlikUcret,
                            Cast(ISNULL(moi.Tutar,0)as decimal(15,2)) as Tutar,
                            moi.[Durum]
                            From UretimDetay moi
                            left join Operasyonlar on Operasyonlar.id = moi.OperasyonId
                            left join Kaynaklar on moi.KaynakId = Kaynaklar.id
                            where Tip='Operasyonlar' and moi.UretimId={id} ";
            var OperationDetail = await _db.QueryAsync<ManufacturingOrderItemsOperationDetail>(sql);
            return OperationDetail.ToList();
        }

        public async Task<IEnumerable<ManufacturingOrderDoneList>> ScheludeDoneList(ManufacturingOrderDoneListArama T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("location", T.DepoId);
            string sql = string.Empty;
            if (KAYITSAYISI == null)
            {
                KAYITSAYISI = 10;
            }
            if (SAYFA == null)
            {
                SAYFA = 1;
            }

            if (T.BaslangıcTarih == null || T.SonTarih == null)
            {

                if (T.DepoId == null || T.DepoId == 0)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            Uretim.id
            , Uretim.[Isim], Uretim.StokId,
			Urunler.[Isim] AS UrunIsmi,Uretim.DepoId,DepoVeAdresler.Isim ,Uretim.BeklenenTarih,
            Kategoriler.[Isim] as  KategoriIsmi,ISNULL(Uretim.CariId,0) as CariId,
			Uretim.[Durum],
            ISNULL(Uretim.PlanlananMiktar,0)as PlanlananMiktar,
            SUM(ISNULL(UretimDetay.PlanlananZaman,0))as PlanlananZaman,
			ISNULL(Uretim.MalzemeFiyati,0)as MalzemeFiyati,ISNULL(Uretim.OperasyonFiyati,0) as OperasyonFiyati
			,ISNULL(Uretim.ToplamMaliyet,0)as ToplamMaliyet,Uretim.TamamlamaTarihi
            from Uretim
            left join UretimDetay on Uretim.id= UretimDetay.UretimId 
            left join DepoVeAdresler on DepoVeAdresler.id=Uretim.DepoId
            left join Cari on Cari.CariKod=Uretim.CariId
            inner join Urunler on Urunler.id=Uretim.StokId
            inner join Kategoriler on Kategoriler.id=Urunler.KategoriId 
            where  Uretim.Durum=3 and  Uretim.Aktif=1 and 
			(UretimDetay.Tip = 'Operasyonlar' or  UretimDetay.Tip is null or   UretimDetay.Tip='Ingredients')
            Group By Uretim.id, Uretim.[Isim],  Uretim.StokId,
			Uretim.CariId,Uretim.DepoId,Uretim.TamamlamaTarihi,
			Uretim.MalzemeFiyati,Uretim.OperasyonFiyati,Uretim.ToplamMaliyet,
            DepoVeAdresler.Isim , Urunler.[Isim],Kategoriler.[Isim],Uretim.PlanlananMiktar,Uretim.[Durum],Uretim.BeklenenTarih) x
            where ISNULL(PlanlananZaman,0) like '%{T.PlanlananZaman}%' AND  ISNULL(Isim,'') Like '%{T.Isim}%' AND    ISNULL     (Customer,'') like '%{T.CariAdSoyad}%' and
            ISNULL(ItemName,'') like '%{T.UrunIsmi}%' and ISNULL(KategoriIsmi,'') like '%{T.KategoriIsmi}%' and ISNULL(PlanlananMiktar,'') like '%{T.PlanlananMiktar}%' and
             ISNULL(MalzemeFiyati,'') like '%{T.MalzemeFiyati}%' and    ISNULL(OperasyonFiyati,'') like '%{T.OperasyonFiyati}%'
			 and    ISNULL(ToplamMaliyet,'') like '%{T.ToplamTutar}%'
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            Uretim.id, Uretim.[Isim], Uretim.StokId,
			Urunler.[Isim] AS ItemName,Uretim.DepoId,DepoVeAdresler.Isim ,Uretim.BeklenenTarih,
            Kategoriler.[Isim] as  KategoriIsmi,ISNULL(Uretim.CariId,0) as CariId,
			ISNULL(Cari.DisplayName,'') AS Customer,Uretim.[Durum],
            ISNULL(Uretim.PlanlananMiktar,0)as PlanlananMiktar,
            SUM(ISNULL(UretimDetay.PlanlananZaman,0))as PlanlananZaman,
			ISNULL(Uretim.MalzemeFiyati,0)as MalzemeFiyati,ISNULL(Uretim.OperasyonFiyati,0) as OperasyonFiyati
			,ISNULL(Uretim.ToplamMaliyet,0)as ToplamMaliyet,Uretim.TamamlamaTarihi
            from Uretim
            left join UretimDetay on Uretim.id= UretimDetay.OrderId 
            left join DepoVeAdresler on DepoVeAdresler.id=Uretim.DepoId
            left join Cari on Cari.CariKod=Uretim.CariId
            inner join Urunler on Urunler.id=Uretim.StokId
            inner join Kategoriler on Kategoriler.id=Urunler.KategoriId 
            whereUretim.DepoId=@location and  Uretim.Durum=3 and  Uretim.Aktif=1 and 
			(UretimDetay.Tip = 'Operasyonlar' or    UretimDetay.Tip is null or   UretimDetay.Tip='Ingredients')
            Group By Uretim.id, Uretim.[Isim],  Uretim.StokId,
			Uretim.CariId,Cari.DisplayName,Uretim.DepoId,Uretim.TamamlamaTarihi,
			Uretim.MalzemeFiyati,Uretim.OperasyonFiyati,Uretim.ToplamMaliyet,
            DepoVeAdresler.Isim , Urunler.[Isim],Kategoriler.[Isim],Uretim.PlanlananMiktar,Uretim.[Durum],Uretim.BeklenenTarih) x
			 where ISNULL(PlanlananZaman,0) like '%{T.PlanlananZaman}%' AND  ISNULL(Isim,'') Like '%{T.Isim}%' AND    ISNULL     (Customer,'') like '%{T.CariAdSoyad}%' and
            ISNULL(ItemName,'') like '%{T.UrunIsmi}%' and ISNULL(KategoriIsmi,'') like '%{T.KategoriIsmi}%' and ISNULL(PlanlananMiktar,'') like '%{T.PlanlananMiktar}%' and
             ISNULL(MalzemeFiyati,'') like '%{T.MalzemeFiyati}%' and    ISNULL(OperasyonFiyati,'') like '%{T.OperasyonFiyati}%'
			 and    ISNULL(ToplamMaliyet,'') like '%{T.ToplamTutar}%'
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
                }
                var ScheludeOpenDoneList = await _db.QueryAsync<ManufacturingOrderDoneList>(sql, param);
                return ScheludeOpenDoneList.ToList();

            }
            else
            {
                var ilkgun = T.BaslangıcTarih.ToString();
                var songun = T.SonTarih.ToString();
                if (T.DepoId == null || T.DepoId == 0)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            Uretim.id
            , Uretim.[Isim], Uretim.StokId,
			Urunler.[Isim] AS ItemName,Uretim.DepoId,DepoVeAdresler.Isim ,Uretim.BeklenenTarih,
            Kategoriler.[Isim] as  KategoriIsmi,ISNULL(Uretim.CariId,0) as CariId,
			ISNULL(Cari.DisplayName,'') AS Customer,Uretim.[Durum],
            ISNULL(Uretim.PlanlananMiktar,0)as PlanlananMiktar,
            SUM(ISNULL(UretimDetay.PlanlananZaman,0))as PlanlananZaman,
			ISNULL(Uretim.MalzemeFiyati,0)as MalzemeFiyati,ISNULL(Uretim.OperasyonFiyati,0) as OperasyonFiyati
			,ISNULL(Uretim.ToplamMaliyet,0)as ToplamMaliyet,Uretim.TamamlamaTarihi
            from Uretim
            left join UretimDetay on Uretim.id= UretimDetay.OrderId 
            left join DepoVeAdresler on DepoVeAdresler.id=Uretim.DepoId
            left join Cari on Cari.CariKod=Uretim.CariId
            inner join Urunler on Urunler.id=Uretim.StokId
            inner join Kategoriler on Kategoriler.id=Urunler.KategoriId 
            where  Uretim.Durum=3 and  Uretim.Aktif=1 and 
			(UretimDetay.Tip = 'Operasyonlar' or  UretimDetay.Tip is null or   UretimDetay.Tip='Ingredients')
            Group By Uretim.id, Uretim.[Isim],  Uretim.StokId,
			Uretim.CariId,Cari.DisplayName,Uretim.DepoId,Uretim.TamamlamaTarihi,
			Uretim.MalzemeFiyati,Uretim.OperasyonFiyati,Uretim.ToplamMaliyet,
            DepoVeAdresler.Isim , Urunler.[Isim],Kategoriler.[Isim],Uretim.PlanlananMiktar,Uretim.[Durum],Uretim.BeklenenTarih) x
            where ISNULL(PlanlananZaman,0) like '%{T.PlanlananZaman}%' AND  ISNULL(Isim,'') Like '%{T.Isim}%' AND    ISNULL     (Customer,'') like '%{T.CariAdSoyad}%' and
            ISNULL(ItemName,'') like '%{T.UrunIsmi}%' and ISNULL(KategoriIsmi,'') like '%{T.KategoriIsmi}%' and ISNULL(PlanlananMiktar,'') like '%{T.PlanlananMiktar}%' and
             ISNULL(MalzemeFiyati,'') like '%{T.MalzemeFiyati}%' and    ISNULL(OperasyonFiyati,'') like '%{T.OperasyonFiyati}%'
			 and    ISNULL(ToplamMaliyet,'') like '%{T.ToplamTutar}%' and x.TamamlamaTarihi BETWEEN '{ilkgun}' and '{songun}'
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            Uretim.id, Uretim.[Isim], Uretim.StokId,
			Urunler.[Isim] AS ItemName,Uretim.DepoId,DepoVeAdresler.Isim ,Uretim.BeklenenTarih,
            Kategoriler.[Isim] as  KategoriIsmi,ISNULL(Uretim.CariId,0) as CariId,
			ISNULL(Cari.DisplayName,'') AS Customer,Uretim.[Durum],
            ISNULL(Uretim.PlanlananMiktar,0)as PlanlananMiktar,
            SUM(ISNULL(UretimDetay.PlanlananZaman,0))as PlanlananZaman,
			ISNULL(Uretim.MalzemeFiyati,0)as MalzemeFiyati,ISNULL(Uretim.OperasyonFiyati,0) as OperasyonFiyati
			,ISNULL(Uretim.ToplamMaliyet,0)as ToplamMaliyet,Uretim.TamamlamaTarihi
            from Uretim
            left join UretimDetay on Uretim.id= UretimDetay.OrderId 
            left join DepoVeAdresler on DepoVeAdresler.id=Uretim.DepoId
            left join Cari on Cari.CariKod=Uretim.CariId
            inner join Urunler on Urunler.id=Uretim.StokId
            inner join Kategoriler on Kategoriler.id=Urunler.KategoriId 
            where Uretim.DepoId=@location and  Uretim.Durum=3 and  Uretim.Aktif=1 and 
			(UretimDetay.Tip = 'Operasyonlar' or    UretimDetay.Tip is null or   UretimDetay.Tip='Ingredients')
            Group By Uretim.id, Uretim.[Isim],  Uretim.StokId,
			Uretim.CariId,Cari.DisplayName,Uretim.DepoId,Uretim.TamamlamaTarihi,
			Uretim.MalzemeFiyati,Uretim.OperasyonFiyati,Uretim.ToplamMaliyet,
            DepoVeAdresler.Isim , Urunler.[Isim],Kategoriler.[Isim],Uretim.PlanlananMiktar,Uretim.[Durum],Uretim.BeklenenTarih) x
			 where ISNULL(PlanlananZaman,0) like '%{T.PlanlananZaman}%' AND  ISNULL(Isim,'') Like '%{T.Isim}%' AND    ISNULL     (Customer,'') like '%{T.CariAdSoyad}%' and
            ISNULL(ItemName,'') like '%{T.UrunIsmi}%' and ISNULL(KategoriIsmi,'') like '%{T.KategoriIsmi}%' and ISNULL(PlanlananMiktar,'') like '%{T.PlanlananMiktar}%' and
             ISNULL(MalzemeFiyati,'') like '%{T.MalzemeFiyati}%' and    ISNULL(OperasyonFiyati,'') like '%{T.OperasyonFiyati}%'
			 and    ISNULL(ToplamMaliyet,'') like '%{T.ToplamTutar}%' and x.TamamlamaTarihi BETWEEN '{ilkgun}' and '{songun}'
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
                }
                var ScheludeOpenDoneList = await _db.QueryAsync<ManufacturingOrderDoneList>(sql, param);
                return ScheludeOpenDoneList.ToList();

            }



        }


        public async Task<IEnumerable<ManufacturingOrderList>> ScheludeOpenList(ManufacturingOrderListArama T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("location", T.DepoId);
            string sql = string.Empty;

            if (KAYITSAYISI == null)
            {
                KAYITSAYISI = 10;
            }
            if (SAYFA == null)
            {
                SAYFA = 1;
            }

            if (T.BaslangıcTarih == null || T.SonTarih == null)
            {
                if (T.DepoId == null || T.DepoId == 0)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            Uretim.id,Uretim.BeklenenTarih, Uretim.[Isim], Uretim.StokId, Urunler.[Isim] AS            ItemName,Uretim.DepoId,DepoVeAdresler.Isim ,
            ISNULL(Kategoriler.[Isim],'') as  KategoriIsmi,ISNULL(Uretim.CariId,0) as CariId,ISNULL     (Cari.DisplayName,'')    AS Customer,Uretim.[Durum],
            ISNULL(Uretim.PlanlananMiktar,0)as PlanlananMiktar,
            SUM(ISNULL(UretimDetay.PlanlananZaman,0))as PlanlananZaman,
            ISNULL(Uretim.ProductionDeadline,'') as ProductDeadline,
            ISNULL(min(UretimDetay.MalzemeDurum),0) as MalzemeDurum
            from Uretim
            left join UretimDetay on Uretim.id= UretimDetay.OrderId 
            left join DepoVeAdresler on DepoVeAdresler.id=Uretim.DepoId
            left join Cari on Cari.CariKod=Uretim.CariId
            left join Urunler on Urunler.id=Uretim.StokId
            left join Kategoriler on Kategoriler.id=Urunler.KategoriId 
            where  Uretim.Aktif=1 and   Uretim.Durum!=3 and (UretimDetay.Tip = 'Operasyonlar' or    UretimDetay.Tip is null or   UretimDetay.Tip='Ingredients')
            Group By Uretim.id, Uretim.[Isim], Uretim.StokId,Uretim.CariId,Cari.DisplayName,Uretim.DepoId,
            DepoVeAdresler.Isim ,Uretim.BeklenenTarih,
            Urunler.[Isim],Kategoriler.            [Isim],Uretim.PlanlananMiktar,Uretim.ProductionDeadline,Uretim.[Durum]) x
            where ISNULL(PlanlananZaman,0) like '%{T.PlanlananZaman}%' AND ISNULL(MalzemeDurum,0) like '%{T.MalzemeDurumu}%' and ISNULL(Isim,'') Like '%{T.Isim}%' AND    ISNULL     (Customer,'') like '%{T.CariAdSoyAd}%' and
            ISNULL(ItemName,'') like '%{T.UrunIsmi}%' and ISNULL(KategoriIsmi,'') like '%{T.KategoriIsmi}%' and ISNULL(PlanlananMiktar,'') like '%{T.PlanlananMiktar}%' 
            and ISNULL(Durum,'') like '%{T.Durum}%' 
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;
                        ";
                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            Uretim.id,Uretim.BeklenenTarih, Uretim.[Isim], Uretim.StokId, Urunler.[Isim] AS            ItemName,Uretim.DepoId,DepoVeAdresler.Isim ,
            ISNULL(Kategoriler.[Isim],'') as  KategoriIsmi,ISNULL(Uretim.CariId,0) as CariId,ISNULL     (Cari.DisplayName,'')    AS Customer,Uretim.[Durum],
            ISNULL(Uretim.PlanlananMiktar,0)as PlanlananMiktar,
            SUM(ISNULL(UretimDetay.PlanlananZaman,0))as PlanlananZaman,
            ISNULL(Uretim.ProductionDeadline,'') as ProductDeadline,
            ISNULL(min(UretimDetay.MalzemeDurum),0) as MalzemeDurum
            from Uretim
            left join UretimDetay on Uretim.id= UretimDetay.OrderId 
            left join DepoVeAdresler on DepoVeAdresler.id=Uretim.DepoId
            left join Cari on Cari.CariKod=Uretim.CariId
            left join Urunler on Urunler.id=Uretim.StokId
            left join Kategoriler on Kategoriler.id=Urunler.KategoriId 
            where  Uretim.Aktif=1 and Uretim.DepoId=@location and       Uretim.Durum!=3 and (UretimDetay.Tip = 'Operasyonlar' or    UretimDetay.Tip is null or   UretimDetay.Tip='Ingredients')
            Group By Uretim.id, Uretim.[Isim],          Uretim.StokId,Uretim.CariId,Cari.DisplayName,Uretim.DepoId,
            DepoVeAdresler.Isim ,Uretim.BeklenenTarih,
            Urunler.[Isim],Kategoriler.            [Isim],Uretim.PlanlananMiktar,Uretim.ProductionDeadline,Uretim.[Durum]) x
            where ISNULL(PlanlananZaman,0) like '%{T.PlanlananZaman}%' AND ISNULL(MalzemeDurum,0) like '%{T.MalzemeDurumu}%' and ISNULL(Isim,'') Like '%{T.Isim}%' AND    ISNULL     (Customer,'') like '%{T.CariAdSoyAd}%' and
            ISNULL(ItemName,'') like '%{T.UrunIsmi}%' and ISNULL(KategoriIsmi,'') like '%{T.KategoriIsmi}%' and ISNULL(PlanlananMiktar,'') like '%{T.PlanlananMiktar}%' and
            ISNULL(Durum,'') like '%{T.Durum}%' 
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;
                        ";
                }
            }
            else
            {
                var ilkgun = T.BaslangıcTarih.ToString();
                var songun = T.SonTarih.ToString();
                if (T.DepoId == null || T.DepoId == 0)
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            Uretim.id,Uretim.BeklenenTarih, Uretim.[Isim], Uretim.StokId, Urunler.[Isim] AS            ItemName,Uretim.DepoId,DepoVeAdresler.Isim ,
            ISNULL(Kategoriler.[Isim],'') as  KategoriIsmi,ISNULL(Uretim.CariId,0) as CariId,ISNULL     (Cari.DisplayName,'')    AS Customer,Uretim.[Durum],
            ISNULL(Uretim.PlanlananMiktar,0)as PlanlananMiktar,
            SUM(ISNULL(UretimDetay.PlanlananZaman,0))as PlanlananZaman,
            ISNULL(Uretim.ProductionDeadline,'') as ProductDeadline,
            ISNULL(min(UretimDetay.MalzemeDurum),0) as MalzemeDurum
            from Uretim
            left join UretimDetay on Uretim.id= UretimDetay.OrderId 
            left join DepoVeAdresler on DepoVeAdresler.id=Uretim.DepoId
            left join Cari on Cari.CariKod=Uretim.CariId
            left join Urunler on Urunler.id=Uretim.StokId
            left join Kategoriler on Kategoriler.id=Urunler.KategoriId 
            where  Uretim.Aktif=1 and   Uretim.Durum!=3 and (UretimDetay.Tip = 'Operasyonlar' or    UretimDetay.Tip is null or   UretimDetay.Tip='Ingredients')
            Group By Uretim.id, Uretim.[Isim], Uretim.StokId,Uretim.CariId,Cari.DisplayName,Uretim.DepoId,
            DepoVeAdresler.Isim ,Uretim.BeklenenTarih,
            Urunler.[Isim],Kategoriler.            [Isim],Uretim.PlanlananMiktar,Uretim.ProductionDeadline,Uretim.[Durum]) x
            where ISNULL(PlanlananZaman,0) like '%{T.PlanlananZaman}%' AND ISNULL(MalzemeDurum,0) like '%{T.MalzemeDurumu}%' and ISNULL(Isim,'') Like '%{T.Isim}%' AND    ISNULL     (Customer,'') like '%{T.CariAdSoyAd}%' and
            ISNULL(ItemName,'') like '%{T.UrunIsmi}%' and ISNULL(KategoriIsmi,'') like '%{T.KategoriIsmi}%' and ISNULL(PlanlananMiktar,'') like '%{T.PlanlananMiktar}%' 
            and ISNULL(Durum,'') like '%{T.Durum}%'  and x.BeklenenTarih BETWEEN '{ilkgun}' and '{songun}'
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;
                        ";
                }
                else
                {
                    sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
            select x.* from (Select
            Uretim.id,Uretim.BeklenenTarih, Uretim.[Isim], Uretim.StokId, Urunler.[Isim] AS            ItemName,Uretim.DepoId,DepoVeAdresler.Isim ,
            ISNULL(Kategoriler.[Isim],'') as  KategoriIsmi,ISNULL(Uretim.CariId,0) as CariId,ISNULL     (Cari.DisplayName,'')    AS Customer,Uretim.[Durum],
            ISNULL(Uretim.PlanlananMiktar,0)as PlanlananMiktar,
            SUM(ISNULL(UretimDetay.PlanlananZaman,0))as PlanlananZaman,
            ISNULL(Uretim.ProductionDeadline,'') as ProductDeadline,
            ISNULL(min(UretimDetay.MalzemeDurum),0) as MalzemeDurum
            from Uretim
            left join UretimDetay on Uretim.id= UretimDetay.OrderId 
            left join DepoVeAdresler on DepoVeAdresler.id=Uretim.DepoId
            left join Cari on Cari.CariKod=Uretim.CariId
            left join Urunler on Urunler.id=Uretim.StokId
            left join Kategoriler on Kategoriler.id=Urunler.KategoriId 
            where  Uretim.Aktif=1 and Uretim.DepoId=@location and       Uretim.Durum!=3 and (UretimDetay.Tip = 'Operasyonlar' or    UretimDetay.Tip is null or   UretimDetay.Tip='Ingredients')
            Group By Uretim.id, Uretim.[Isim],          Uretim.StokId,Uretim.CariId,Cari.DisplayName,Uretim.DepoId,
            DepoVeAdresler.Isim ,Uretim.BeklenenTarih,
            Urunler.[Isim],Kategoriler.            [Isim],Uretim.PlanlananMiktar,Uretim.ProductionDeadline,Uretim.[Durum]) x
            where ISNULL(PlanlananZaman,0) like '%{T.PlanlananZaman}%' AND ISNULL(MalzemeDurum,0) like '%{T.MalzemeDurumu}%' and ISNULL(Isim,'') Like '%{T.Isim}%' AND    ISNULL     (Customer,'') like '%{T.CariAdSoyAd}%' and
            ISNULL(ItemName,'') like '%{T.UrunIsmi}%' and ISNULL(KategoriIsmi,'') like '%{T.KategoriIsmi}%' and ISNULL(PlanlananMiktar,'') like '%{T.PlanlananMiktar}%' and
            ISNULL(Durum,'') like '%{T.Durum}%'   and x.BeklenenTarih BETWEEN '{ilkgun}' and '{songun}'
            ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;
                        ";
                }
            }


            var ScheludeOpenList = await _db.QueryAsync<ManufacturingOrderList>(sql, param);



            return ScheludeOpenList.ToList();
        }


        public async Task<IEnumerable<ManufacturingTask>> TaskDoneList(ManufacturingTaskArama T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@DepoId", T.DepoId);
            string sql = string.Empty;
            if (T.DepoId == null)
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
             select x.* from(
        select UretimDetay.id as id,Uretim.id as UretimId ,UretimDetay.KaynakId,Kaynaklar.[Isim]as ResourcesName,Uretim.[Isim]as OrderName,Uretim.ProductionDeadline,Uretim.StokId,UretimDetay.CompletedDate, 
        Urunler.[Isim]as ItemName,Uretim.PlanlananMiktar,UretimDetay.OperasyonId,Operasyonlar.[Isim]as OperationName,UretimDetay.PlanlananZaman,UretimDetay.[Durum] from UretimDetay
        left join Uretim on Uretim.id=UretimDetay.OrderId
        left join Urunler on Urunler.id=Uretim.StokId
        left join Kaynaklar on Kaynaklar.id=UretimDetay.KaynakId
        left join Operasyonlar on Operasyonlar.id=UretimDetay.OperasyonId 
        where Uretim.id=UretimDetay.UretimId and UretimDetay.Durum=3  and 
        ISNULL(PlanlananZaman,0) like '%{T.PlanlananZaman}%'  and ISNULL(Kaynaklar.Isim,'') Like '%{T.KaynakIsmi}%' AND    ISNULL(Uretim.Isim,'') like '%{T.UretimIsmi}%' and
        ISNULL(Urunler.Isim,'') like '%{T.UrunIsmi}%' and ISNULL(Uretim.PlanlananMiktar,'') like '%{T.PlanlananMiktar}%' and ISNULL(Operasyonlar.Isim,'') like '%{T.OperasyonIsmi}%' 
       
        Group By Uretim.id,UretimDetay.id,UretimDetay.KaynakId,Kaynaklar.[Isim],Uretim.[Isim],Uretim.ProductionDeadline,Uretim.StokId,UretimDetay.CompletedDate,
        Urunler.[Isim],Uretim.PlanlananMiktar,UretimDetay.OperasyonId,Operasyonlar.[Isim],UretimDetay.PlanlananZaman,UretimDetay.[Durum])x
        ORDER BY x.KaynakId OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
            }
            else
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
             select x.* from(
        select UretimDetay.id as id,Uretim.id as UretimId ,UretimDetay.KaynakId,Kaynaklar.[Isim]as ResourcesName,Uretim.[Isim]as OrderName,Uretim.ProductionDeadline,Uretim.StokId,UretimDetay.CompletedDate, 
        Urunler.[Isim]as ItemName,Uretim.PlanlananMiktar,UretimDetay.OperasyonId,Operasyonlar.[Isim]as OperationName,UretimDetay.PlanlananZaman,UretimDetay.[Durum] from UretimDetay
        left join Uretim on Uretim.id=UretimDetay.OrderId
        left join Urunler on Urunler.id=Uretim.StokId
        left join Kaynaklar on Kaynaklar.id=UretimDetay.KaynakId
        left join Operasyonlar on Operasyonlar.id=UretimDetay.OperasyonId 
        where UretimDetay.CompanyId=@CompanyId and Uretim.id=UretimDetay.OrderId and UretimDetay.Durum=3  and Uretim.DepoId=@DepoId AND
        ISNULL(PlanlananZaman,0) like '%{T.PlanlananZaman}%'  and ISNULL(Kaynaklar.Isim,'') Like '%{T.KaynakIsmi}%' AND    ISNULL(Uretim.Isim,'') like '%{T.UretimIsmi}%' and
        ISNULL(Urunler.Isim,'') like '%{T.UrunIsmi}%' and ISNULL(Uretim.PlanlananMiktar,'') like '%{T.PlanlananMiktar}%' and ISNULL(Operasyonlar.Isim,'') like '%{T.OperasyonIsmi}%' 
       
        Group By Uretim.id,UretimDetay.id,UretimDetay.KaynakId,Kaynaklar.[Isim],Uretim.[Isim],Uretim.ProductionDeadline,Uretim.StokId,UretimDetay.CompletedDate,
        Urunler.[Isim],Uretim.PlanlananMiktar,UretimDetay.OperasyonId,Operasyonlar.[Isim],UretimDetay.PlanlananZaman,UretimDetay.[Durum])x
        ORDER BY x.KaynakId OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
            }


            var TaskDoneList = await _db.QueryAsync<ManufacturingTask>(sql, param);



            return TaskDoneList;
        }


        public async Task<IEnumerable<ManufacturingTask>> TaskOpenList(ManufacturingTaskArama T, int CompanyId, int? KAYITSAYISI, int? SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@DepoId", T.DepoId);
            string sql = string.Empty;
            if (T.DepoId == null)
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
             select x.* from(
        select UretimDetay.id as id,Uretim.id as UretimId,UretimDetay.KaynakId,Kaynaklar.[Isim]as ResourcesName,Uretim.[Isim]as OrderName,Uretim.ProductionDeadline,Uretim.StokId,
        Urunler.[Isim]as ItemName,Uretim.PlanlananMiktar,UretimDetay.OperasyonId,Operasyonlar.[Isim]as OperationName,UretimDetay.PlanlananZaman,UretimDetay.[Durum] from UretimDetay
        left join Uretim on Uretim.id=UretimDetay.OrderId
        left join Urunler on Urunler.id=Uretim.StokId
        left join Kaynaklar on Kaynaklar.id=UretimDetay.KaynakId
        left join Operasyonlar on Operasyonlar.id=UretimDetay.OperasyonId 
        where UretimDetay.CompanyId=@CompanyId and Uretim.id=UretimDetay.OrderId and UretimDetay.Durum!=3  and 
        ISNULL(PlanlananZaman,0) like '%{T.PlanlananZaman}%'  and ISNULL(Kaynaklar.Isim,'') Like '%{T.KaynakIsmi}%' AND    ISNULL(Uretim.Isim,'') like '%{T.UretimIsmi}%' and
        ISNULL(Urunler.Isim,'') like '%{T.UrunIsmi}%' and ISNULL(Uretim.PlanlananMiktar,'') like '%{T.PlanlananMiktar}%' and ISNULL(Operasyonlar.Isim,'') like '%{T.OperasyonIsmi}%' 
        Group By Uretim.id,UretimDetay.KaynakId,Kaynaklar.[Isim],Uretim.[Isim],Uretim.ProductionDeadline,Uretim.StokId,UretimDetay.id,
        Urunler.[Isim],Uretim.PlanlananMiktar,UretimDetay.OperasyonId,Operasyonlar.[Isim],UretimDetay.PlanlananZaman,UretimDetay.[Durum])x
        ORDER BY x.KaynakId OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
            }
            else
            {
                sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
             select x.* from(
        select UretimDetay.id as id,Uretim.id as UretimId,UretimDetay.KaynakId,Kaynaklar.[Isim]as ResourcesName,Uretim.[Isim]as OrderName,Uretim.ProductionDeadline,Uretim.StokId,
        Urunler.[Isim]as ItemName,Uretim.PlanlananMiktar,UretimDetay.OperasyonId,Operasyonlar.[Isim]as OperationName,UretimDetay.PlanlananZaman,UretimDetay.[Durum] from UretimDetay
        left join Uretim on Uretim.id=UretimDetay.OrderId
        left join Urunler on Urunler.id=Uretim.StokId
        left join Kaynaklar on Kaynaklar.id=UretimDetay.KaynakId
        left join Operasyonlar on Operasyonlar.id=UretimDetay.OperasyonId 
        where UretimDetay.CompanyId=@CompanyId and Uretim.id=UretimDetay.OrderId and UretimDetay.Durum!=3  and Uretim.DepoId=@DepoId AND
        ISNULL(PlanlananZaman,0) like '%{T.PlanlananZaman}%'  and ISNULL(Kaynaklar.Isim,'') Like '%{T.KaynakIsmi}%' AND    ISNULL(Uretim.Isim,'') like '%{T.UretimIsmi}%' and
        ISNULL(Urunler.Isim,'') like '%{T.UrunIsmi}%' and ISNULL(Uretim.PlanlananMiktar,'') like '%{T.PlanlananMiktar}%' and ISNULL(Operasyonlar.Isim,'') like '%{T.OperasyonIsmi}%' 
        Group By Uretim.id,UretimDetay.KaynakId,Kaynaklar.[Isim],Uretim.[Isim],Uretim.ProductionDeadline,Uretim.StokId,UretimDetay.id,
        Urunler.[Isim],Uretim.PlanlananMiktar,UretimDetay.OperasyonId,Operasyonlar.[Isim],UretimDetay.PlanlananZaman,UretimDetay.[Durum])x
        ORDER BY x.KaynakId OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY;";
            }

            var ScheludeOpenList = await _db.QueryAsync<ManufacturingTask>(sql, param);



            return ScheludeOpenList;
        }

    }
}
