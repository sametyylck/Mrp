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
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.StockTransferDTO;

namespace DAL.Repositories
{
    public class StockTransferRepository : IStockTransferRepository
    {
        IDbConnection _db;
        ILocationStockRepository _loc;
        private readonly IStockControl _control;

        public StockTransferRepository(IDbConnection db, ILocationStockRepository loc, IStockControl control)
        {
            _db = db;
            _loc = loc;
            _control = control;
        }


        public async Task Delete(IdControl T,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Aktif", false);
            string sqls = $@"select st.id,st.StokId,st.Quantity  from StokAktarimDetay  st
            left join StokAktarim on StokAktarim.id=st.StokAktarimId
            where StokAktarim.id=@id";
            var list = await _db.QueryAsync<StockTransferDetailsItems>(sqls, prm);
            foreach (var item in list)
            {
                prm.Add("@StokId", item.StokId);
                string sqlf = $@"declare @@Origin int,@@Destination int 
            set @@Origin=(Select BaslangicDepo from StokAktarim where id = @id )
            set @@Destination=(Select HedefDepo from StokAktarim where id = @id )
            select (Select VarsayilanFiyat from Urunler where id = @StokId ) as VarsayilanFiyat,
            (Select Tip from Urunler where id = @StokId ) as Tip,(Select @@Origin) as BaslangicDepo,
            (Select @@Destination) as HedefDepo,
            (select id from DepoVeAdresler where DepoId = @@Origin and  StokId = @StokId) as   originvarmi,
            (Select StockCount from DepoVeAdresler where StokId = @StokId and DepoId = @@Origin) as     stockCountOrigin,
          
            (select id from DepoVeAdresler where DepoId = @@Destination and  StokId = @StokId) as   destinationvarmı
            ,(Select StockCount from DepoVeAdresler where StokId = @StokId and DepoId = @@Destination ) as DestinationStockCounts";
                var sorgu = _db.Query<StockMergeSql>(sqlf, prm);

                float? Quantity = item.Miktar;
                var CostPerUnit = sorgu.First().VarsayilanFiyat;
                var Tip = sorgu.First().Tip;
                var value = Quantity * CostPerUnit; //transfer value hesaplama
                prm.Add("@Total", value);
                int Origin = sorgu.First().BaslangicDepo;
                int Destination = sorgu.First().HedefDepo;
                prm.Add("@Destination", Destination);
                prm.Add("@Origin", Origin);
                prm.Add("@AktarimUcreti", value);
                prm.Add("@CostPerUnit", CostPerUnit);


                //Verilen konumlarda bu iteme ait stock değeri var mı kontrol edilir.Yoksa oluşturulur.
                if (sorgu.First().originvarmi == 0)
                {
                  await  _loc.Insert(Tip, item.StokId, Origin);
                }
                float? OriginStockCount = sorgu.First().stockCountOrigin;

                if (sorgu.First().destinationvarmı == 0)
                {
                   await _loc.Insert(Tip, item.StokId,  Destination);
                }

                float? DestinationStockCount = sorgu.First().DestinationStockCounts;

                var NewOriginStock = OriginStockCount + Quantity;
                var NewDestinationStock = DestinationStockCount - Quantity;



                prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
                prm.Add("@NewDestinationStock", NewDestinationStock);





                await _db.ExecuteAsync($"Update StokAktarim set Total=@Total where id=@id ", prm);

                await _db.ExecuteAsync($"Update DepoVeAdresler SET StockCount =@NewOriginStock where DepoId = @Origin and StokId=@StokId ", prm);
                await _db.ExecuteAsync($"Update DepoVeAdresler SET StockCount =@NewDestinationStock where DepoId = @Destination and StokId=@StokId ", prm);


                prm.Add("itemid", item.id);
                await _db.ExecuteAsync($"Delete From StokAktarimDetay  where StokId = @StokId  and id=@itemid and id=@itemid", prm);
            }

            prm.Add("@DateTime", DateTime.Now);
            prm.Add("@User", UserId);
            await _db.ExecuteAsync($"Update StokAktarim Set Aktif=@Aktif,DeleteDate=@DateTime,DeletedUser=@User where id = @id ", prm);
        }

        public async Task DeleteItems(StockTransferDeleteItems T,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@StokAktarimId", T.StokAktarimId);
            prm.Add("@StokId", T.StokId);
            string sqlf = $@"declare @@Origin int,@@Destination int
            set @@Origin=(Select BaslangicDepo from StokAktarim where id = @StokAktarimId )
            set @@Destination=(Select HedefDepo from StokAktarim where id = @StokAktarimId)
            select (Select VarsayilanFiyat from Urunler where id = @StokId ) as VarsayilanFiyat,
            (Select Tip from Urunler where id = @StokId ) as Tip,(Select @@Origin) as BaslangicDepo,
            (Select @@Destination) as HedefDepo,
            (select id from DepoVeAdresler where DepoId = @@Origin and StokId = @StokId) as   originvarmi,
            (Select StockCount from DepoVeAdresler where StokId = @StokId and DepoId = @@Origin ) as     stockCountOrigin,
            (select id from DepoVeAdresler where DepoId = @@Destination and StokId = @StokId) as  destinationvarmı
            ,(Select StockCount from DepoVeAdresler where StokId = @StokId and DepoId = @@Destination ) as DestinationStockCounts,
            (select st.Quantity  from StokAktarimDetay st  where st.id=@id and st.StokId=@StokId )as Quantity";
            var sorgu = await _db.QueryAsync<StockMergeSql>(sqlf, prm);
            float? Quantity = sorgu.First().Miktar;
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

            float? OriginStockCount = sorgu.First().stockCountOrigin;
            float? DestinationStockCount = sorgu.First().DestinationStockCounts;

            var NewOriginStock = OriginStockCount + Quantity;
            var NewDestinationStock = DestinationStockCount - Quantity;



            prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
            prm.Add("@NewDestinationStock", NewDestinationStock);
            await _db.ExecuteAsync($"Update StokAktarim set Total=@Total where id=@StokAktarimId ", prm);




            await _db.ExecuteAsync($"Update DepoVeAdresler SET StockCount =@NewOriginStock where DepoId = @Origin and StokId=@StokId", prm);
            await _db.ExecuteAsync($"Update DepoVeAdresler SET StockCount =@NewDestinationStock where DepoId = @Destination and StokId=@StokId", prm);

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
            prm.Add("Aktif", true);


            return await _db.QuerySingleAsync<int>($"Insert into StokAktarim (AktarimIsmi,AktarmaTarihi,BaslangicDepo,HedefDepo,Bilgi,Aktif) OUTPUT INSERTED.[id] values (@AktarimIsmi,@AktarimTarihi,@BaslangicDepo,@HedefDepo,@Info,@Aktif)", prm);
        }

        public  async Task<int> InsertStockTransferItem(StockTransferInsertItem T, int? id,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();

            prm.Add("@StokId", T.StokId);
            prm.Add("@Quantity", T.Miktar);
            prm.Add("@StokAktarimId", id);
            //VarsayilanFiyat,Tip,BaslangicDepo,HedefDepo getiriliyor.
            string sqlf = $@"declare @@BaslangicDepo int,@@HedefDepo int 
            set @@BaslangicDepo=(Select BaslangicDepo from StokAktarim where id = @StokAktarimId)
            set @@HedefDepo=(Select HedefDepo from StokAktarim where id = @StokAktarimId )
            select (Select VarsayilanFiyat from Urunler where id = @StokId) as VarsayilanFiyat,
            (Select Tip from Urunler where id = @StokId ) as Tip,(Select @@BaslangicDepo) as BaslangicDepo,
            (Select @@HedefDepo) as HedefDepo,
            (select id from DepoStoklar where DepoId = @@BaslangicDepo and  StokId = @StokId) as   originvarmi,
            (Select StokAdeti from DepoStoklar where StokId = @StokId and DepoId = @@BaslangicDepo) as   stockCountOrigin,
            (select id from DepoStoklar where DepoId = @@HedefDepo and StokId = @StokId) as   destinationvarmı ,
			(Select StokAdeti from DepoStoklar where StokId = @StokId and DepoId = @@HedefDepo ) as DestinationStockCounts             ";
            var sorgu = await _db.QueryAsync<StockMergeSql>(sqlf, prm);

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

            float? OriginStockCount = sorgu.First().stockCountOrigin;
            float? DestinationStockCount = sorgu.First().DestinationStockCounts;
            
            var NewOriginStock = OriginStockCount - T.Miktar;
            var NewDestinationStock = DestinationStockCount + T.Miktar;

            prm.Add("@NewOriginStock", NewOriginStock); //Yeni count değerini tabloya güncelleştiriyoruz.
            prm.Add("@NewDestinationStock", NewDestinationStock);
            await _db.ExecuteAsync($"Update StokAktarim set Toplam=@Total where id=@StokAktarimId", prm);

         

            await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewOriginStock where DepoId = @Origin and StokId=@StokId ", prm);
            await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewDestinationStock where DepoId = @Destination and StokId=@StokId", prm);
       
            return await _db.QuerySingleAsync<int>($"Insert into StokAktarimDetay (StokId,BirimFiyat,Miktar,AktarimUcreti,StokAktarimId) OUTPUT INSERTED.[id]  values (@StokId,@CostPerUnit,@Quantity,@AktarimUcreti,@StokAktarimId)", prm);
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
            var quantity = T.Miktar - deger.First();
           


            //VarsayilanFiyat,Tip,BaslangicDepo,HedefDepo getiriliyor.Gelen id ile originvarmı,destinationvarmı kontrol edilir.Eğer var ise StockCount değerleri çekilir.
            string sqlf = $@"declare @@Origin int,@@Destination int 
            set @@Origin=(Select BaslangicDepo from StokAktarim where id = @StokAktarimId )
            set @@Destination=(Select HedefDepo from StokAktarim where id = @StokAktarimId)
            select
            (Select Tip from Urunler where id = @StokId ) as Tip,(Select @@Origin) as BaslangicDepo,
            (Select @@Destination) as HedefDepo,
            (select AktarimIsmi from StokAktarim where id = @StokAktarimId ) as AktarimIsmi
  ";
            var sorgu = await _db.QueryAsync<StockTransferDetails>(sqlf, prm);
            int id = T.StokAktarimId; ;
            if (T.Miktar != deger.First())
            {
                StockTransferInsert insert = new();
                StockTransferInsertItem item = new();
                insert.StokId = T.StokId;
                insert.AktarimIsmi = sorgu.First().AktarimIsmi;
                if (deger.First()<T.Miktar)
                {
                    insert.BaslangicDepo = sorgu.First().BaslangicDepo;
                    insert.HedefDepo = sorgu.First().HedefDepo;
                    item.Miktar = T.Miktar - deger.First();
                }
                else
                {
                    insert.BaslangicDepo = sorgu.First().HedefDepo;
                    insert.HedefDepo = sorgu.First().BaslangicDepo;
                    item.Miktar = deger.First()-T.Miktar;
                }
                insert.AktarmaTarihi = DateTime.Now;
               id= await Insert(insert);
                item.StokId= T.StokId;
                item.StokAktarimId = id;
                await InsertStockTransferItem(item,id, UserId);
            }

            return id;
          
        }
    }
}
