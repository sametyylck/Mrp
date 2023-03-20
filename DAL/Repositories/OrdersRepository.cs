using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.GeneralSettingsDTO;
using static DAL.DTO.ItemDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.TaxDTO;

namespace DAL.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        IDbConnection _db;
        private readonly IUretimRepository _uretim;
        public OrdersRepository(IDbConnection db, IUretimRepository uretim)
        {
            _db = db;
            _uretim = uretim;
        }

        public async Task Delete(List<Delete> A,int user)
        {
            foreach (var T in A)
            {
                DynamicParameters prm = new DynamicParameters();
                prm.Add("@id", T.id);
                prm.Add("@Tip", T.Tip);
                prm.Add("@IsActive", false);
                prm.Add("@DateTime", DateTime.Now);
                prm.Add("@User", user);

                string sql = $"select SatinAlma.UretimId,SatinAlma.UretimDetayId,SatinAlmaDetay.id,StokId from SatinAlmaDetay left join SatinAlma on SatinAlma.id=SatinAlmaDetay.SatinAlmaId where  SatinAlma.Tip='PurchaseOrder' and SatinAlma.id=@id";
                var idcontrol = await _db.QueryAsync<PurchaseOrder>(sql, prm);
                foreach (var item in idcontrol)
                {
                    DeleteItems B = new DeleteItems();
                    B.id = item.id;
                    B.StokId = (int)item.StokId;
                    B.SatinAlmaId = T.id;
                    await DeleteItems(B);
                }
                var manuorderid = idcontrol.First().UretimId;
                var manuorderitemid = idcontrol.First().UretimDetayId;

                string sql1 = $@"Select m.DepoId,mi.StokId,mi.PlananlananMiktar from Uretim m 
                left join UretimDetay mi on mi.UretimId=m.id
                where m.id={manuorderid} and mi.id={manuorderitemid}";
                var manufacturing = await _db.QueryAsync<PurchaseOrder>(sql1, prm);
                await _db.ExecuteAsync($"Delete from SatinAlma where id = @id and Tip=@Tip", prm);
                foreach (var item in manufacturing)
                {
                    UretimIngredientsUpdate clas = new();
                    clas.id = manuorderitemid;
                    clas.UretimId = manuorderid;
                    clas.Miktar = item.Miktar;
                    clas.DepoId=item.DepoId;
                    clas.StokId = item.StokId;
                    await _uretim.IngredientsUpdate(clas);
                }



            }
          
        }

        public async Task DeleteItems(DeleteItems T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@OrdersId", T.SatinAlmaId);
            prm.Add("@StokId", T.StokId);
            await _db.ExecuteAsync($"Delete From SatinAlmaDetay  where StokId = @StokId and id=@id and SatinAlmaId=@OrdersId", prm);
        }

        public async Task<IEnumerable<PurchaseDetails>> Details(int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);

            var list = await _db.QueryAsync<PurchaseDetails>($@"Select SatinAlma.id,SatinAlma.Tip,SatinAlma.SubeId,Subeler.SubeIsmi,SatinAlma.TedarikciId,Cari.AdSoyad as SupplierName,SatinAlma.BeklenenTarih,SatinAlma.OlusturmaTarihi,
            SatinAlma.SatinAlmaIsmi, SatinAlma.DepoId,DepoVeAdresler.Isim, SatinAlma.Bilgi,SatinAlma.DurumBelirteci 
            From SatinAlma 
            left join Subeler on Subeler.id=SatinAlma.SubeId
            left join Cari on Cari.CariKod = SatinAlma.TedarikciId 
            left join DepoVeAdresler on DepoVeAdresler.id=SatinAlma.DepoId 
            left join SatinAlmaDetay on SatinAlmaDetay.SatinAlmaId = SatinAlma.id 
            where SatinAlma.id = @id 
            Group By SatinAlma.id, SatinAlma.Tip, SatinAlma.TedarikciId, Cari.AdSoyad, SatinAlma.BeklenenTarih, SatinAlma.OlusturmaTarihi,
            SatinAlma.SatinAlmaIsmi, SatinAlma.DepoId, SatinAlma.Bilgi,DepoVeAdresler.Isim,SatinAlma.DurumBelirteci", prm);
            foreach (var item in list)
            {
                var list2 = await _db.QueryAsync<PurchaseOrdersItemDetails>(@$"  Select SatinAlmaDetay.id as id,SatinAlmaDetay.StokId ,Urunler.Isim as UrunIsmi, SatinAlmaDetay.Miktar, SatinAlmaDetay.BirimFiyat,
                SatinAlmaDetay.VergiId, Vergi.VergiIsim, SatinAlmaDetay.VergiDegeri, SatinAlmaDetay.TumToplam, SatinAlmaDetay.ToplamTutar, SatinAlmaDetay.VergiMiktari ,
                SatinAlmaDetay.SatinAlmaId, SatinAlmaDetay.OlcuId, Olcu.Isim as OlcuIsmi from SatinAlmaDetay
                left join Urunler on Urunler.id = SatinAlmaDetay.StokId  
                left    join Vergi on Vergi.id = SatinAlmaDetay.VergiId
                left  join Olcu on Olcu.id = SatinAlmaDetay.OlcuId 
                where  SatinAlmaDetay.SatinAlmaId = @id", prm);
                item.detay = list2;
            }
            return list;
        }

        public async Task<int> Insert(PurchaseOrderInsert T,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "PurchaseOrder");
            prm.Add("@ContactId", T.TedarikciId);
            prm.Add("@ExpectedDate", T.BeklenenTarih);
            prm.Add("@CreateDate", T.OlusturmaTarihi);
            prm.Add("@OrderName", T.SatinAlmaIsmi);
            prm.Add("@SatisId", T.SatisId);
            prm.Add("@UretimId", T.UretimId);
            prm.Add("@UretimDetayId", T.UretimDetayId);
            prm.Add("@SatisDetayId", T.SatisDetayId);
            prm.Add("@Info", T.Bilgi);
            prm.Add("@DeliveryId", 1);
            prm.Add("@DepoId", T.DepoId);
            prm.Add("@IsActive", true);
            prm.Add("@KullaniciId", UserId);


            return await _db.QuerySingleAsync<int>($"Insert into SatinAlma (Tip,SatisId,SatisDetayId,UretimId,UretimDetayId,TedarikciId,BeklenenTarih,OlusturmaTarihi,SatinAlmaIsmi,DepoId,DurumBelirteci,Bilgi,Aktif,KullaniciId) OUTPUT INSERTED.[id] values (@Tip,@SatisId,@SatisDetayId,@UretimId,@UretimDetayId,@ContactId,@ExpectedDate,@CreateDate,@OrderName,@DepoId,@DeliveryId,@Info,@IsActive,@KullaniciId)", prm);
        }

        public async Task<int> InsertPurchaseItem(PurchaseOrderInsertItem T, int OrdersId)
        {
            var prm = new DynamicParameters();

            prm.Add("StokId", T.StokId);

            var Rate = await _db.QueryFirstAsync<TaxClas>($"select VergiDegeri from Vergi where id =@id", new { id = T.VergiId });
            float TaxRate = Rate.VergiDegeri;

            var DefaultPricee = await _db.QueryFirstAsync<Items>($"select VarsayilanFiyat from Urunler where id =@StokId", new { StokId = T.StokId });
            var PriceUnit = DefaultPricee.VarsayilanFiyat;
            var TotalPrice = (T.Miktar * PriceUnit); //adet*fiyat
            float? PlusTax = (TotalPrice * TaxRate) / 100; //tax fiyatı hesaplama
            var TotalAll = TotalPrice + PlusTax; //toplam fiyat hesaplama  
            prm.Add("@Miktar", T.Miktar);
            prm.Add("@BirimFiyat", PriceUnit);
            prm.Add("@VergiId", T.VergiId);
            prm.Add("@VergiDegeri", TaxRate);
            prm.Add("@SatinAlmaId", OrdersId);
            prm.Add("@VergiMiktari", PlusTax);
            prm.Add("@ToplamTutar", TotalPrice);
            prm.Add("@TumToplam", TotalAll);

            return await _db.QuerySingleAsync<int>($"Insert into SatinAlmaDetay (StokId,Miktar,BirimFiyat,VergiId,VergiDegeri,SatinAlmaId,ToplamTutar,VergiMiktari,TumToplam) OUTPUT INSERTED.[id] values (@StokId,@Miktar,@BirimFiyat,@VergiId,@VergiDegeri,@SatinAlmaId,@ToplamTutar,@VergiMiktari,@TumToplam)", prm);

        }

        public async Task Update(PurchaseOrderUpdate T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "PurchaseOrder");
            prm.Add("@id", T.id);
            prm.Add("@ContactId", T.TedarikciId);
            prm.Add("@ExpectedDate", T.BeklenenTarih);
            prm.Add("@OrderName", T.SatinAlmaIsmi);
            prm.Add("@Info", T.Bilgi);
            prm.Add("@DepoId", T.DepoId);
            prm.Add("@TotalAll", T.TumTutar);

            await _db.ExecuteAsync($"Update SatinAlma SET TedarikciId = @ContactId,TumTutar=@TotalAll,BeklenenTarih=@ExpectedDate,SatinAlmaIsmi=@OrderName,DepoId=@DepoId,Bilgi=@Info where id=@id", prm);
        }

        public async Task UpdatePurchaseItem(PurchaseItem T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@VergiId", T.VergiId);
            prm.Add("@StokId", T.StokId);

            var Rate = _db.Query<TaxClas>($"(select VergiDegeri from Vergi where id =@VergiId)", prm);
            float TaxRate = Rate.First().VergiDegeri;

            prm.Add("@PricePerUnit", T.BirimFiyat);

            var TotalPrice = (T.Miktar * T.BirimFiyat); //adet*fiyat
            float? PlusTax = (TotalPrice * TaxRate) / 100; //tax fiyatı hesaplama
            var TotalAll = TotalPrice + PlusTax; //toplam fiyat hesaplama  

            prm.Add("@PlusTax", PlusTax);
            prm.Add("@Miktar", T.Miktar);
            prm.Add("@VergiId", T.VergiId);
            prm.Add("@TaxValue", TaxRate);
            prm.Add("@OrdersId", T.SatinAlmaId);
            prm.Add("@TotalPrice", TotalPrice);
            prm.Add("@TotalAll", TotalAll);
            prm.Add("@MeasureId", T.OlcuId);

            await _db.ExecuteAsync($"Update SatinAlmaDetay SET ToplamTutar = @TotalPrice,StokId=@StokId,TumToplam=@TotalAll,OlcuId=@MeasureId,VergiMiktari=@PlusTax,VergiDegeri=@TaxValue,VergiId=@VergiId,BirimFiyat=@PricePerUnit,Miktar=@Miktar where SatinAlmaId = @OrdersId and id=@id", prm);
        }
    }
}
