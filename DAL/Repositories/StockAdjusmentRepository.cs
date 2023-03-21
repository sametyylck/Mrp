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
using static DAL.DTO.StockAdjusmentDTO;

namespace DAL.Repositories
{
    public class StockAdjusmentRepository : IStockAdjusmentRepository
    {
        IDbConnection _db;
        ILocationStockRepository _locationStockRepository;
        IItemsRepository _itemsRepository;
        private readonly IStockControl _control;

        public StockAdjusmentRepository(IDbConnection db, ILocationStockRepository locationStockRepository, IItemsRepository itemsRepository, IStockControl control)
        {
            _db = db;
            _locationStockRepository = locationStockRepository;
            _itemsRepository = itemsRepository;
            _control = control;
        }


        public async Task Delete(IdControl T,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Aktif", false);

            string sqls = $@"select st.id,st.Miktar,st.StokId  from StokDuzenlemeDetay st
            left join StokDuzenleme on StokDuzenleme.id = st.StokDuzenlemeId
            where StokDuzenleme.id = @id";
            var sqlsorgu = await _db.QueryAsync<LocaVarmı>(sqls, prm);//
            foreach (var item in sqlsorgu)
            {
                prm.Add("@StokId", item.StokId);
                prm.Add("@itemid", item.id);
                string sql = $@"declare @@locationId int 
            set @@locationId=(Select DepoId From StokDuzenleme where id = @id)
            select 
            (select @@locationId)as StockId,
            (Select StokAdeti from DepoStoklar where StokId = @StokId   and DepoId = @@locationId )as StokAdeti,
			(Select Tip from Urunler where id=@StokId) as Tip,
            (Select id from DepoStoklar where StokId = @StokId and DepoId = @@locationId) as DepoStokId
            ";
                var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sql, prm);//
                float? adjusment = item.Miktari;


                float? stockCount = sorgu.First().StokAdeti;
                float? NewStockCount = stockCount - adjusment;
                var stocklocationId = sorgu.First().DepoStokId;


                prm.Add("@stocklocationId", stocklocationId);
                prm.Add("@NewStockCount", NewStockCount); //Yeni count değerini tabloya güncelleştiriyoruz.
               await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewStockCount where id = @stocklocationId", prm);

                prm.Add("itemid", item.id);
               await _db.ExecuteAsync($"Delete From StokDuzenlemeDetay  where StokId = @StokId and id=@itemid and StockAdjusmentId=@id", prm);
            }
            prm.Add("@DateTime", DateTime.Now);
            prm.Add("@User", UserId);
            await _db.ExecuteAsync($"Update StokDuzenleme Set Aktif=@Aktif,SilinmeTarihi=@DateTime,SilenKullanıcı=@User where id = @i ", prm);
        }

        public async Task DeleteItems(StockAdjusmentItemDelete T, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@StockAdjusmentId", T.StokDuzenelemeId);
            prm.Add("@StokId", T.StokId);
            string sql = $@"declare @@locationId int 
            set @@locationId=(Select DepoId From StokDuzenleme where id = @StockAdjusmentId)
            select 
            (select @@locationId) as StockId,
            (Select StokAdeti from DepoStoklar where StokId = @StokId  and DepoId = @@locationId)as StokAdeti,
			(Select Tip from Urunler where id=@StokId) as Tip,
            (Select id from DepoStoklar where StokId = @StokId and DepoId = @@locationId) as DepoStokId,
            (select s.Miktar from StokDuzenlemeDetay s
            left join StokDuzenleme on s.StokDuzenlemeId=s.id
            where s.StokDuzenlemeId=@StockAdjusmentId and s.id=@id)as Miktar";
            var sorgu =await _db.QueryAsync<StockAdjusmentStockUpdate>(sql, prm);//
            float? adjusment = sorgu.First().Miktar;

            float? stockCount = sorgu.First().StokAdeti;
            float? NewStockCount = stockCount - adjusment;
            var stocklocationId = sorgu.First().DepoStokId;


            prm.Add("@stocklocationId", stocklocationId);
            prm.Add("@NewStockCount", NewStockCount); //Yeni count değerini tabloya güncelleştiriyoruz.
           await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewStockCount where id = @stocklocationId  ", prm);

            await _db.ExecuteAsync($"Delete From StockAdjusmentItems  where StokId = @StokId  id=@id and StockAdjusmentId=@StockAdjusmentId", prm);
        }

        public async Task<IEnumerable<StockAdjusmentClas>> Detail(int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);

            var list =await _db.QueryAsync<StockAdjusmentClas>($"Select x.* From (Select StokDuzenleme.id, StokDuzenleme.Isim, StokDuzenleme.Tarih, StokDuzenleme.Sebeb, StokDuzenleme.DepoId, loc.LocationName as LocationName,StokDuzenleme.Bilgi from StokDuzenleme left join Locations loc on loc.id = StokDuzenleme.DepoId) x where x.id = @id", prm);

            foreach (var item in list)
            {

                string sqla = $@"Select StockAdjusmentItems.id,StockAdjusmentItems.StokId,Urunler.Isim as ItemName,StockAdjusmentItems.Adjusment,
            StockAdjusmentItems.CostPerUnit,StockAdjusmentItems.AdjusmentValue,
            StockAdjusmentItems.StockAdjusmentId,l.StokAdeti as InStock 
            from StockAdjusmentItems 
            left join Urunler on Urunler.id = StockAdjusmentItems.StokId
            left join StokDuzenleme on StokDuzenleme.id = StockAdjusmentItems.StockAdjusmentId 
            left join Locations on Locations.id = StokDuzenleme.DepoId 
            left join DepoStoklar l on l.StokId = Urunler.id
            and l.DepoId = StokDuzenleme.DepoId
            
            where  StockAdjusmentItems.StockAdjusmentId = @id 
            Group By StockAdjusmentItems.id,StockAdjusmentItems.StokId,
            Urunler.Isim,StockAdjusmentItems.Adjusment,StockAdjusmentItems.CostPerUnit,
            StockAdjusmentItems.AdjusmentValue, StockAdjusmentItems.StockAdjusmentId,StokAdeti";
                var list2 = await _db.QueryAsync<StockAdjusmentItems>(sqla, prm);
                item.detay = list2;
            }
            return list.ToList();
        }

        public async Task<int> Insert(StockAdjusmentInsert T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Isim", T.Isim);
            prm.Add("@Sebeb", T.Sebeb);
            prm.Add("@Tarih", T.Tarih);
            prm.Add("@DepoId", T.DepoId);
            prm.Add("@Bilgi", T.Bilgi);
            prm.Add("@StokSayimId", T.StokSayimId);
            prm.Add("@Aktif", true);
            int id= await _db.QuerySingleAsync<int>($"Insert into StokDuzenleme (Isim,StokSayimId,Sebeb,Tarih,DepoId,Bilgi,Aktif) OUTPUT INSERTED.[id] values (@Isim,@StokSayimId,@Sebeb,@Tarih,@DepoId,@Bilgi,@Aktif)", prm);
            //var model = new StockAdjusmentAll
            //{
            //    id = id,
            //    Isim = T.Isim,
            //    Tarih = T.Tarih,
            //    Sebeb = T.Sebeb,
            //    DepoId = T.DepoId,
            //    Bilgi = T.Bilgi,
            //    StokSayimId = T.StokSayimId,
            //    StokId = T.StokId,

            //};
            return id;
        }

        public async Task<int> InsertItem(StockAdjusmentInsertItem T, int StockAdjusmentId,int user)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@StokId", T.StokId);
            prm.Add("@DepoId", T.DepoId);

            string sqlh = $@" 
            select 
			(Select StokAdeti from DepoStoklar where StokId = @StokId   and DepoId = @DepoId) as DepoStoklar,
            (Select id from DepoStoklar where StokId = @StokId   and DepoId = @DepoId) as DepoStokId,
            (Select Tip from Urunler where id=@StokId)as Tip";
            var locationStockVarmı =await _db.QueryAsync<LocaVarmı>(sqlh, prm);
            var tip1 = locationStockVarmı.First().Tip;
            if (locationStockVarmı.First().DepoStokId == 0)
            {
             await   _locationStockRepository.Insert(tip1, T.StokId,T.DepoId);
            }


            string sqlv = $@"select Tip,VarsayilanFiyat from Urunler where id=@StokId";
            var itembul =await _db.QueryAsync<LocaVarmı>(sqlv, prm);
            var tip = itembul.First().Tip;
            var defaultprice = itembul.First().VarsayilanFiyat;

            var items =await _itemsRepository.Detail(T.StokId);
            var IngredientCost = items.First().MalzemeTutarı;
            float? operioncost = items.First().OperasyonTutarı;
            float? AdjusmentValue = 0;
            if (tip == "Material")
            {
                var CostPerUnit = defaultprice;
                AdjusmentValue = T.BirimFiyat * T.Miktar;
            }
            else
            {
                float? CostPerUnit = IngredientCost + operioncost;
               
                AdjusmentValue = CostPerUnit * T.Miktar;
            }

            var Total = AdjusmentValue;
            prm.Add("@Adjusment", T.Miktar);
            prm.Add("@CostPerUnit", T.BirimFiyat);
            prm.Add("@StockAdjusmentId", StockAdjusmentId);
            prm.Add("@AdjusmentValue", AdjusmentValue);
            prm.Add("@Total", Total);



            string sql = $@"declare @@locationId int 
            set @@locationId=(Select DepoId From StokDuzenleme where id = @StockAdjusmentId)
            select 
            (Select StokAdeti from DepoStoklar where StokId = @StokId   and DepoId = @@locationId)as StokAdeti,
			(Select Tip from Urunler where id=@StokId ) as Tip,
            (Select id from DepoStoklar where StokId = @StokId and DepoId = @@locationId)   as    DepoStokId";
            var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sql, prm);//
            await _db.ExecuteAsync($"Update StokDuzenleme set Toplam=@Total where id=@StockAdjusmentId", prm);


            float? stockCount = sorgu.First().StokAdeti;
            float? NewStockCount = stockCount + T.Miktar;
            var stocklocationId = sorgu.First().DepoStokId;
            if (stocklocationId == 0)
            {
              await  _locationStockRepository.Insert(tip, T.StokId,  T.DepoId);
            }

            prm.Add("@stocklocationId", stocklocationId);
           
            prm.Add("@NewStockCount", NewStockCount); //Yeni count değerini tabloya güncelleştiriyoruz.
            _db.Execute($"Update DepoStoklar SET StokAdeti =@NewStockCount where id = @stocklocationId", prm);

            int id= await _db.QuerySingleAsync<int>($"Insert into StokDuzenlemeDetay (StokId,Miktar,BirimFiyat,StokDuzenlemeId,Toplam) OUTPUT INSERTED.[id] values (@StokId,@Adjusment,@CostPerUnit,@StockAdjusmentId,@AdjusmentValue)", prm);
            return id;
        }

        public async Task<IEnumerable<StockAdjusmentList>> List(StockAdjusmentList T, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();

            string sql = @$"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}
Select StokDuzenleme.id,StokDuzenleme.Isim,DepoVeAdresler.Isim as DepoIsim ,StokDuzenleme.Sebeb,StokDuzenleme.Toplam,StokDuzenleme.Tarih
from StokDuzenleme left join DepoVeAdresler on DepoVeAdresler.id = StokDuzenleme.DepoId 
where StokDuzenleme.Aktif = 1 and  ISNULL(StokDuzenleme.Sebeb,0) LIKE '%{T.Sebeb}%'and 
ISNULL(StokDuzenleme.Toplam,0) LIKE '%{T.Toplam}%' and 
ISNULL(StokDuzenleme.Isim,0) LIKE '%{T.Isim}%' and 
ISNULL(DepoVeAdresler.Isim,0) LIKE '%{T.DepoIsmi}%' 
ORDER BY StokDuzenleme.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY ";

            var list =await _db.QueryAsync<StockAdjusmentList>(sql);
            return list.ToList();
        }

        public async Task Update(StockAdjusmentUpdate T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Isim", T.Isim);
            prm.Add("@Sebeb", T.Sebeb);
            prm.Add("@Tarih", T.Tarih);
            prm.Add("@DepoId", T.DepoId);
            prm.Add("@Bilgi", T.Bilgi);

           await _db.ExecuteAsync($"Update StokDuzenleme SET Isim = @Isim,Sebeb=@Sebeb,Tarih=@Tarih,DepoId=@DepoId,Bilgi=@Bilgi where id=@id", prm);
        }

        public async Task UpdateStockAdjusmentItem(StockAdjusmentUpdateItems T,int UserId)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@StokId", T.StokId);
            prm.Add("@StockAdjusmentId", T.StokDuzenlemeId);
            string sqlv = $@"select Tip,VarsayilanFiyat from Urunler where id=@StokId";
            var itembul =await _db.QueryAsync<LocaVarmı>(sqlv, prm);
            var tip = itembul.First().Tip;
            var defaultprice = itembul.First().VarsayilanFiyat;


            prm.Add("@id", T.id);
            string sql1 = $@"Select Miktar from StokDuzenlemeDetay where id=@id";
            var sorgu2 =await _db.QueryAsync<float>(sql1, prm);
            float adjusment = sorgu2.First();

            var items =await _itemsRepository.Detail(T.StokId);
            var IngredientCost = items.First().MalzemeTutarı;
            float? operioncost = items.First().OperasyonTutarı;
            float? CostPerUnit = IngredientCost + operioncost;
            float? AdjusmentValue = 0;
            if (tip == "Material")
            {
               
                    AdjusmentValue = T.BirimFiyat * T.Miktar;
            }
            else
            {
                if (T.BirimFiyat == CostPerUnit)
                {
                    T.BirimFiyat = (float)CostPerUnit;
                    AdjusmentValue = (float)CostPerUnit * T.Miktar;
                }
                else
                {
                    AdjusmentValue = T.BirimFiyat * T.Miktar;
                }

            }


            prm.Add("@Adjusment", T.Miktar);
            prm.Add("@CostPerUnit", T.BirimFiyat);
            prm.Add("@StockAdjusmentId", T.StokDuzenlemeId);
            prm.Add("@AdjusmentValue", AdjusmentValue);

          await  _db.ExecuteAsync($"Update StokDuzenlemeDetay SET StokId=@StokId,BirimFiyat=@CostPerUnit,Toplam=@AdjusmentValue,Miktar=@Adjusment where StokDuzenlemeId = @StockAdjusmentId and id=@id", prm);


            if (T.Miktar == adjusment)
            {
                return;
            }
            else
            {
                T.Miktar = T.Miktar - adjusment;
                string sql = $@"declare @@locationId int 
            set @@locationId=(Select DepoId From StokDuzenleme where id = @StockAdjusmentId)
            select 
            (select @@locationId) as StockId,
            (Select StokAdeti from DepoStoklar where StokId = @StokId   and DepoId = @@locationId)as StokAdeti,
            (Select id from DepoStoklar where StokId = @StokId and DepoId = @@locationId) AS DepoStokId";
                var sorgu =await _db.QueryAsync<StockAdjusmentStockUpdate>(sql, prm);//
                int DepoId = sorgu.First().StokId;


                float? stockCount = sorgu.First().StokAdeti;
                float? NewStockCount = stockCount + T.Miktar;
                var stocklocationId = sorgu.First().DepoStokId;

                prm.Add("@stocklocationId", stocklocationId);

                prm.Add("@NewStockCount", NewStockCount); //Yeni count değerini tabloya güncelleştiriyoruz.
               await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewStockCount where id = @stocklocationId", prm);



            }
        }
    }
}
