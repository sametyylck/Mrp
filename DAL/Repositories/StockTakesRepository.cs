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
using static DAL.DTO.StockAdjusmentDTO;
using static DAL.DTO.StockTakesDTO;

namespace DAL.Repositories
{
    public class StockTakesRepository : IStockTakesRepository
    {
        IDbConnection _db;
        IStockAdjusmentRepository _adjusment;
        private readonly IStockControl _control;


        public StockTakesRepository(IDbConnection db, IStockAdjusmentRepository adjusment, IStockControl control)
        {
            _db = db;
            _adjusment = adjusment;
            _control = control;
        }

        public async Task Delete(IdControl T, int CompanyId, int User)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@User", User);
            prm.Add("@Date", DateTime.Now);

            prm.Add("@Aktif", false);
            await _db.ExecuteAsync($"Update StokSayim Set Aktif=@Aktif,SilinmeTarihi=@Date,SilenKullanici=@User where id = @id",prm);
            await _db.ExecuteAsync($"delete from StokSayimDetay where StokSayimId=@id", prm);

        }

        public async Task DeleteItems(StockTakeDelete T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@StokId", T.StokId);
            await _db.ExecuteAsync($"Delete from StokSayimDetay where  StokId=@StokId and id = @id  ", prm);
        }

        public async Task<IEnumerable<StockTakes>> Detail(int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            string sql = $@"select * from StokSayim where id=@id";
            var details = await _db.QueryAsync<StockTakes>(sql, prm);
            return details.ToList();
        }

        public async Task<int> Insert(StockTakesInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();

            prm.Add("@Isim", T.Isim);
            prm.Add("@BaslangıcTarihi", T.OlusturmaTarihi);
            prm.Add("@Sebeb", T.Sebeb);
            prm.Add("@Info", T.Bilgi);
            prm.Add("@DepoId", T.DepoId);

        
            prm.Add("@Aktif", true);
            prm.Add("@Durum", 0);

            return await _db.QuerySingleAsync<int>($"Insert into StokSayim (Isim,OlusturmaTarihi,DepoId,Sebeb,Bilgi,Aktif,Durum) OUTPUT INSERTED.[id] values (@Isim,@BaslangıcTarihi,@DepoId,@Sebeb,@Info,@Aktif,@Durum)", prm);
        }

        public async Task<int> InsertItem(List<StockTakeInsertItems> T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();

            foreach (var item in T)
            {
                prm.Add("@StokSayimId", item.StokSayimId);
                prm.Add("@StokId", item.StokId);
                prm.Add("@Notes", item.Bilgi);
                await _db.QuerySingleAsync<int>($"Insert into StokSayimDetay(StokId,Bilgi,StokSayimId) OUTPUT INSERTED.[id] values (@StokId,@Notes,@StokSayimId)", prm);
            }
            return 1;
        }

        public async Task<IEnumerable<StockTakeItems>> ItemDetail(int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            string sql = $@"select StokSayimDetay.id,StokId,Kategoriler.Isim,StokSayimDetay.Bilgi,StokSayimId,SayilanMiktar,EksikMiktar from StokSayimDetay
			left join Urunler on Urunler.id=StokId
			left join Kategoriler on Kategoriler.id=Urunler.KategoriId
            where StokSayimDetay.StokSayimId=@id ";
            var ItemsDetail = await _db.QueryAsync<StockTakeItems>(sql, prm);
            return ItemsDetail.ToList();
        }


        public async Task StockTakesDone(StockTakesDone T, int CompanyId, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Durum", T.Durum);

            string sql = $@"select Durum from StokSayim where  id={T.id}";
            var Durum = await _db.QueryFirstAsync<int>(sql);

            string sql2 = $@"select * from StokSayimDetay where StokSayimId={T.id}";
            var itemlist = await _db.QueryAsync<StockTakeInsertItems>(sql2);

            int eskiStatus = Durum;
            var DepoVeAdresler = await _db.QueryFirstAsync<int>($"select id from DepoVeAdresler where Tip='SettingsLocation'");
            prm.Add("@DepoId", DepoVeAdresler);

            if (eskiStatus == 0)
            {
                if (T.Durum == 1)
                {

                    await _db.ExecuteAsync($"Update StokSayim Set Durum={T.Durum} where id = {T.id} ");
                }
            }
            else if (eskiStatus == 1)
            {
                if (T.Durum == 2)
                {

                    var degerler = await _db.QueryAsync<StockTakeItems>($@"select * from StokSayimDetay where  StokSayimId={T.id}");
                    foreach (var item in degerler)
                    {
                        var stokdeger = await _control.Count(item.StokId, DepoVeAdresler);
                        var EksikMiktar = item.SayilanMiktar - stokdeger;
                        string sqlquery = $@"Update StokSayimDetay Set EksikMiktar={EksikMiktar}  where id = {item.id} ";
                        await _db.ExecuteAsync(sqlquery);
                    }



                    await _db.ExecuteAsync($"Update StokSayim Set Durum={T.Durum} where id = {T.id}  ");

                }
                else if (T.Durum == 3)
                {
                    prm.Add("@id", T.id);
                    prm.Add("@Durum", T.Durum);
                    string sqlsorgu = $@"select * from StokSayimDetay where StokSayimDetay.StokSayimId={T.id}";
                    var degerler = await _db.QueryAsync<StockTakeItems>(sqlsorgu, prm);
                    StockAdjusmentInsert stockAdjusmentAll = new StockAdjusmentInsert();
                    stockAdjusmentAll.Tarih = DateTime.Now;
                    stockAdjusmentAll.Isim = "ST";
                    stockAdjusmentAll.Sebeb = "STK";
                    stockAdjusmentAll.DepoId = DepoVeAdresler;
                    stockAdjusmentAll.StokSayimId = T.id;
                    int id = await _adjusment.Insert(stockAdjusmentAll);
                    foreach (var item in degerler)
                    {
                        var stokdeger = await _control.Count(item.StokId, DepoVeAdresler);

                        var Discrepancys = item.SayilanMiktar - stokdeger;
                        _db.Execute($"Update StokSayimDetay Set EksikMiktar={Discrepancys} where id = {item.id}  ");

                        StockAdjusmentInsertItem A = new StockAdjusmentInsertItem();
                        A.StokId = item.StokId;
                        A.DepoId = DepoVeAdresler;
                        A.Miktar = (float)item.EksikMiktar;

                        await _adjusment.InsertItem(A, id, UserId);



                    }
                    await _db.QueryAsync($"Update StokSayim Set Durum={T.Durum} where id = {T.id}  ");

                }
                else
                {
                    await _db.ExecuteAsync($"Update StokSayim Set Durum={T.Durum} where id = {T.id} ");
                }

            }
            else if (eskiStatus == 2)
            {
                if (T.Durum == 3)
                {
                    prm.Add("@id", T.id);
                    prm.Add("@Durum", T.Durum);
                    await _db.ExecuteAsync($"Update StokSayim Set Durum={T.Durum} where id = {T.id}  ");
                    string sqlsorgu = $@"  select StokId,EksikMiktar,Min(EksikMiktar) as [Control] from StokSayimDetay where StokSayimDetay.StokSayimId={T.id}  Group by StokId,EksikMiktar";
                    var degerler = await _db.QueryAsync<StockTakeItems>(sqlsorgu);
                    string mindiscrepanyc = $@"    select Min(EksikMiktar) as EksikMiktar  from StokSayimDetay where StokSayimDetay.StokSayimId={T.id} ";
                    var discrepancy = await _db.QueryAsync<StockTakeItems>(mindiscrepanyc);

                    if (discrepancy.First().EksikMiktar != null)
                    {
                        StockAdjusmentInsert stockAdjusmentAll = new StockAdjusmentInsert();
                        stockAdjusmentAll.Tarih = DateTime.Now;
                        stockAdjusmentAll.Isim = "ST";
                        stockAdjusmentAll.Sebeb = "STK";
                        stockAdjusmentAll.DepoId = DepoVeAdresler;
                        stockAdjusmentAll.StokSayimId = T.id;


                        int id = await _adjusment.Insert(stockAdjusmentAll);
                        foreach (var item in degerler)
                        {

                            StockAdjusmentInsertItem A = new StockAdjusmentInsertItem();
                            A.StokId = item.StokId;
                            A.DepoId = DepoVeAdresler;
                            A.Miktar = (float)item.EksikMiktar;
                            await _adjusment.InsertItem(A, id, UserId);


                        }
                    }

                    else
                    {
                        await _db.ExecuteAsync($"Update StokSayim Set Durum={T.Durum} where id = {T.id} ");
                    }

                }
                else
                {
                    await _db.ExecuteAsync($"Update StokSayim Set Durum={T.Durum} where id = {T.id}  ");
                }

            }
            else if (eskiStatus == 3)
            {
                if (T.Durum == 3)
                {
                    await _db.ExecuteAsync($"Update StokSayim Set Durum={T.Durum} where id = {T.id} ");
                }
                else
                {
                    string sqlsorgu = $@"select id from StokDuzenleme where StokSayimId={T.id} ";
                    var adjusmentid = await _db.QueryAsync(sqlsorgu);
                    if (adjusmentid.First() != 0)
                    {
                        IdControl delete = new IdControl();
                        delete.id = adjusmentid.First();
                        await _adjusment.Delete(delete,UserId);
                    }

                }

            }
        }

        public async Task<IEnumerable<StockTakeList>> StockTakesList(StockTakeList T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters param = new DynamicParameters();

            string sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI} SET @SAYFA = {SAYFA}
            select x.* from (
            select StokSayim.id,Isim,StokSayim.Sebeb,StokSayim.DepoId,DepoVeAdresler.Isim as DepoIsmi,BaslangıcTarihi,BitisTarihi,StokDuzenlemeId,StokDuzenleme.Isim as StokDuzenlemeIsmi,Durum from StokSayim
            left join DepoVeAdresler on DepoVeAdresler.id=StokSayim.DepoId
			left join StockAdjusment on StokDuzenleme.id=StokSayim.StokDuzenlemeId
            where StokSayim.Aktif=1 and StokSayim.Durum!=3 AND ISNULL(Isim,'') like '%{T.Isim}%' 
			and ISNULL(StokSayim.Sebeb,'') Like '%{T.Sebeb}%' AND    ISNULL(DepoIsmi,'') like '%{T.DepoIsmi}%' and
        ISNULL(Isim,'') like '%{T.Isim}%' and ISNULL(StokSayim.Durum,'') like '%{T.Durum}%'
		Group by Isim,StokSayim.Sebeb,StokSayim.DepoId,StokSayim.id,DepoVeAdresler.Isim,BaslangıcTarihi,BitisTarihi,StokDuzenlemeId,StokDuzenleme.Isim,Durum)x
		  ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY";
            var List = await _db.QueryAsync<StockTakeList>(sql, param);

            return List.ToList();
        }

        public async Task Update(StockTakesUpdate T, int id, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Isim", T.Isim);
            prm.Add("@BaslangıcTarihi", T.OlusturmaTarihi);
            prm.Add("@StartedDate", T.BaslangıcTarihi);
            prm.Add("@BitisTarihi", T.BitisTarihi);
            prm.Add("@Sebeb", T.Sebeb);
            prm.Add("@Info", T.Bilgi);
            string sql = $@"select Durum from StokSayim where id=@id ";
            var Durum = await _db.QueryFirstAsync<int>(sql, prm);
            if (Durum == 0)
            {
                await _db.ExecuteAsync($"Update StokSayim set Bilgi=@Info,Sebeb=@Sebeb,BaslangıcTarihi=@BaslangıcTarihi,Isim=@Isim where id=@id ", prm);

            }
            else if (Durum == 1)
            {
                await _db.ExecuteAsync($"Update StokSayim set Bilgi=@Info,Sebeb=@Sebeb,Isim=@Isim,BaslangıcTarihi=@StartedDate where  id=@id", prm);
            }
            else if (Durum == 2)
            {
                await _db.ExecuteAsync($"Update StokSayim set Bilgi=@Info,Sebeb=@Sebeb,Isim=@Isim,BaslangıcTarihi=@StartedDate,BitisTarihi=@BitisTarihi where id=@id", prm);
            }
            else if (Durum == 3)
            {
                await _db.ExecuteAsync($"Update StokSayim set Bilgi=@Info,Sebeb=@Sebeb,OlusturmaTarihi=@CreadtedDate,Isim=@Isim,BaslangıcTarihi=@StartedDate,BitisTarihi=@BitisTarihi where  StokSayimId=@id", prm);
            }
        }
        public async Task UpdateItems(StockTakesUpdateItems T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.StokSayimId);
            prm.Add("@StockTakesItemId", T.StokSayimDetayId);
            prm.Add("@SayilanMiktar", T.SayilanMiktar);
            prm.Add("@Note", T.Bilgi);

            string sql = $@"select Durum from StokSayim where  id=@id ";
            var Durum = await _db.QueryFirstAsync<int>(sql, prm);
            if (Durum == 0)
            {
                await _db.ExecuteAsync($"Update StokSayimDetay set Bilgi=@Note where StokSayimId=@id and id=@StockTakesItemId", prm);

            }
            else if (Durum == 1)
            {
                await _db.ExecuteAsync($"Update StokSayimDetay set Bilgi=@Note,SayilanMiktar=@SayilanMiktar where  StokSayimId=@id and id=@StockTakesItemId", prm);
            }
        }
    }
}
