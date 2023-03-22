using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ItemDTO;
using static DAL.DTO.ProductOperationsBomDTO;
using static DAL.DTO.StockDTO;

namespace DAL.Repositories
{
    public class ItemsRepository : IItemsRepository
    {
        IDbConnection _db;

        public ItemsRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task Delete(ItemsDelete T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Tip", T.Tip);
            prm.Add("@IsActive", false);
            prm.Add("@DateTime", DateTime.Now);
            //Burda Items tablosunda pasife alma yapıyoruz
           await _db.ExecuteAsync($"Update Urunler SET Aktif = @IsActive,SilinmeTarihi=@DateTime where id = @id and Tip = @Tip", prm);
            //burda bom eşlenmesinde olan kayıtları pasife alıyoruz
           await _db.ExecuteAsync($"Update UrunRecetesi SET Aktif = @IsActive,SilinmeTarihi=@DateTime where MalzemeId = @id", prm);
        }

        public async Task<IEnumerable<ListItems>> Detail(int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            var itemDetail = await _db.QueryAsync<ListItems>($"Select ur.id,ur.Tip,ur.Isim,ur.KategoriId ,Kategoriler.Isim as KategoriIsim,ur.OlcuId,Olcu.Isim as OlcuIsim,ur.TedarikciId, Cari.AdSoyad as TedarikciIsmi,ur.StokKodu, ur.VarsayilanFiyat, ur.Bilgi From Urunler ur left join Kategoriler on   Kategoriler.id = ur.KategoriId left join Olcu on  Olcu.id = ur.OlcuId left join Cari on   Cari.CariKod = ur.TedarikciId where ur.id = @id ", prm);


            //İtemin reçete tablosundaki ekli materyallerın toplamını buluyoruz
            #region MaterialCost

            var Costbul =await _db.QueryAsync<costbul>($@"Select urc.id,urc.MalzemeId,urc.Miktar , Urunler.id as itemid, Urunler.VarsayilanFiyat From UrunRecetesi urc
                                                         inner join Urunler on Urunler.id = urc.MalzemeId
                                                         where urc.Aktif = 1 and urc.MamulId = {id}");
            float IngredientsCost = 0;
            if (itemDetail.Count() == 0)
            {
                return itemDetail;
            }
            if (Costbul.Count() == 0)
            {
                itemDetail.First().MalzemeTutarı = 0;
            }
            else
            {
                foreach (var item in Costbul)
                {
                    float price = item.VarsayilanFiyat * item.Miktar;
                    IngredientsCost += price;
                }
                itemDetail.First().MalzemeTutarı = IngredientsCost;
            }

            #endregion
            //Materyal Cost Son
            //InStock Ürünün Stoğunu buluyoruz
            #region InStock
            //var InstockBul =await _db.QueryAsync<StockDTO.Stock>($"Select AllStockQuantity  from Items where CompanyId = {CompanyId} and id = {id} and IsActive = 1");
            //float? InStock;
            //if (InstockBul.Count() == 0)
            //{
            //    itemDetail.First().InStock = 0;
            //}
            //else
            //{
            //    InStock = InstockBul.First().AllStockQuantity;
            //    itemDetail.First().InStock = InStock;
            //}
            #endregion
            //InStockSon
            //OperationCost Itemin Operasyonlarının saatlik ücretlerini hesaplayar operasyon cost u buluyoruz
            #region OperationCost
            var OperationCostBul =await _db.QueryAsync<ProductOperationsBOM>($"Select id From UrunKaynakRecetesi where  StokId = {id}  and Aktif = 1");


            float OperationsCost = 0;


            if (OperationCostBul.Count() == 0)
            {
                itemDetail.First().OperasyonTutarı = 0;
            }
            else
            {
                foreach (var item in OperationCostBul)
                {
                    var a =await _db.QueryAsync<ProductOperationsBOM>($"Select SaatlikUcret,OperasyonZamani From UrunKaynakRecetesi where id = {item.id}  and Aktif = 1");
                    float CostHour = a.First().SaatlikUcret;
                    float time = a.First().OperasyonZamani;

                    OperationsCost += (CostHour / 60 / 60) * time;
                }
                itemDetail.First().OperasyonTutarı = OperationsCost;
            }

            #endregion
            //OperationCostSon
            return itemDetail;
        }

        public async Task<int> Insert(ItemsInsert T, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@Name", T.Isim);
            prm.Add("@CategoryId", T.KategoriId);
            prm.Add("@MeasureId", T.OlcuId);
            prm.Add("@ContactId", T.TedarikciId);
            prm.Add("@VariantCode", T.StokKodu);
            prm.Add("@DefaultPrice", T.VarsayilanFiyat);
            prm.Add("@AllStock", 0);
            prm.Add("@IsActive", true);
            prm.Add("@Info", T.Bilgi);
            prm.Add("@UserId", UserId);
            return await _db.QuerySingleAsync<int>($"Insert into Urunler (Tip, Isim, KategoriId,OlcuId,TedarikciId,StokKodu,VarsayilanFiyat,Bilgi,Aktif,KullaniciId) OUTPUT INSERTED.[id] values (@Tip, @Name, @CategoryId,@MeasureId,@ContactId,@VariantCode,@DefaultPrice,@Info,@IsActive,@UserId)", prm);
        }

        public async Task<IEnumerable<ItemsListele>> ListMaterial(ItemsListele T, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@sayi", KAYITSAYISI);
            prm.Add("@sayfa", SAYFA);
            string sql = $@"DECLARE @@KAYITSAYISI int DECLARE @@SAYFA int SET @@KAYITSAYISI = @sayi SET @@SAYFA = @sayfa
           Select x.* From(
                     Select ur.id,
                     ur.Tip,
                     ur.Isİm,
                     ur.Aktif,
	                     ISNULL(ur.StokKodu,'') as StokKodu,
	                     ISNULL(Kategoriler.Isim,'') as KategoriIsmi,
	                     ISNULL(Cari.AdSoyad,'') as GorunenIsim,
	                     ISNULL(ur.VarsayilanFiyat,0) as VarsayilanFiyat
	                     From Urunler ur
				left join Kategoriler on ur.KategoriId = Kategoriler.id 
				left join Cari on ur.TedarikciId = Cari.CariKod
				 ) x
				where x.Tip = @Tip and x.Aktif = 1
				and x.Isim LIKE '%{T.Isim}%' 
				and x.StokKodu LIKE '%{T.StokKodu}%' 
				and x.KategoriIsmi LIKE '%{T.KategoriIsmi}%' 
				and x.GorunenIsim LIKE '%{T.TedarikciIsim}%'
				and x.VarsayilanFiyat LIKE '%{T.VarsayilanFiyat}%'
               ORDER BY x.id OFFSET @@KAYITSAYISI * (@@SAYFA - 1) ROWS FETCH NEXT @@KAYITSAYISI ROWS ONLY;";
            var list = await _db.QueryAsync<ItemsListele>(sql, prm);
            return list.ToList();
        }

        public async Task<IEnumerable<ItemsListele>> ListProduct(ItemsListele T, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@sayi", KAYITSAYISI);
            prm.Add("@sayfa", SAYFA);

            var urunler = await _db.QueryAsync<ItemsListele>($@"
                        DECLARE @@KAYITSAYISI int DECLARE @@SAYFA int SET @@KAYITSAYISI = @sayi SET @@SAYFA = @sayfa
                        Select item.* From(
                                Select ur.id,ur.Tip,ur.Isim,
                                ISNULL(ur.StokKodu,'') as StokKodu,
                                ISNULL(ur.KategoriId,'') as KategoriId ,
                                ISNULL(cat.Isim,'') as KategoriIsmi,
                                (Cast (ISNULL(ur.VarsayilanFiyat,0) as decimal(15,2))) as VarsayilanFiyat
                                From Urunler ur
                                left join Kategoriler cat on ur.KategoriId = cat.id
                                where ur.Aktif = 1 and ur.Tip = 'Product') item
                                Where item.Isim LIKE '%{T.Isim}%' and Stokkodu LIKE '%{T.StokKodu}%' and item.KategoriIsmi LIKE '%{T.KategoriIsmi}%' and item.VarsayilanFiyat LIKE '%{T.VarsayilanFiyat}%'
                        ORDER BY item.id OFFSET @@KAYITSAYISI * (@@SAYFA - 1) ROWS FETCH NEXT @@KAYITSAYISI ROWS ONLY;
                         ", prm);

            foreach (var item in urunler)
            {
                var x =await Detail(item.id);
                float? materialcost = x.First().MalzemeTutarı;
                float? OperationCost = x.First().OperasyonTutarı;
                var cost = materialcost + OperationCost;
                item.Maliyet = (float)cost;
                float profit = (float)(item.VarsayilanFiyat - cost);
                item.Kar = profit;
                if (item.VarsayilanFiyat == 0)
                {
                    item.Margin = 0;
                }
                else
                {
                    float? margin = (profit / item.VarsayilanFiyat) * 100;
                    item.Margin = (float)margin;
                }
                var timebul = await _db.QueryAsync<ProductOperationsBOMList>($"Select ISNULL(Sum(OperasyonZamani),0) From UrunKaynakRecetesi where  Aktif = 1 and StokId = {item.id}");
                item.UrunZamani = timebul.Count() > 0 ? timebul.First().OperasyonZamani : 0;
            }

            return urunler;
        }

        public async Task<IEnumerable<ItemsListele>> ListSemiProduct(ItemsListele T, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@sayi", KAYITSAYISI);
            prm.Add("@sayfa", SAYFA);

            var urunler = await _db.QueryAsync<ItemsListele>($@"
                        DECLARE @@KAYITSAYISI int DECLARE @@SAYFA int SET @@KAYITSAYISI = @sayi SET @@SAYFA = @sayfa
                        Select item.* From(
                                Select ur.id,ur.Tip,ur.Isim,
                                ISNULL(ur.VarsayilanFiyat,'') as StokKodu,
                                ISNULL(ur.KategoriId,'') as KategoriId ,
                                ISNULL(cat.Isim,'') as KategoriIsmi,
                                (Cast (ISNULL(ur.VarsayilanFiyat,0) as decimal(15,2))) as VarsayilanFiyat
                                From Urunler ur
                                left join Kategoriler cat on ur.KategoriId = cat.id
                                where ur.Aktif = 1 and ur.Tip = 'SemiProduct' ) item
                                Where item.Isim LIKE '%{T.Isim}%' and Stokkodu LIKE '%{T.StokKodu}%' and item.KategoriIsmi LIKE '%{T.KategoriIsmi}%' and item.VarsayilanFiyat LIKE '%{T.VarsayilanFiyat}%'
                        ORDER BY item.id OFFSET @@KAYITSAYISI * (@@SAYFA - 1) ROWS FETCH NEXT @@KAYITSAYISI ROWS ONLY;
                         ", prm);

            foreach (var item in urunler)
            {
                var x = await Detail(item.id);
                float? materialcost = x.First().MalzemeTutarı;
                float? OperationCost = x.First().OperasyonTutarı;
                float? cost = materialcost + OperationCost;
                item.Maliyet = (float)cost;
                float profit = (float)(item.VarsayilanFiyat - cost);
                item.Kar = profit;
                if (item.VarsayilanFiyat == 0)
                {
                    item.Margin = 0;
                }
                else
                {
                    float? margin = (profit / item.VarsayilanFiyat) * 100;
                    item.Margin = (float)margin;
                }
                var timebul =await _db.QueryAsync<ProductOperationsBOMList>($"Select ISNULL(Sum(OperasyonZamani),0) From UrunKaynakRecetesi where  Aktif = 1 and StokId = {item.id}");
                item.UrunZamani = timebul.Count()>0?timebul.First().OperasyonZamani:0;  
            }

            return urunler; throw new NotImplementedException();
        }

        public async Task Update(ItemsUpdate T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Tip", T.Tip);
            prm.Add("@Name", T.Isim);
            prm.Add("@CategoryId", T.KategoriId);
            prm.Add("@MeasureId", T.OlcuId);
            prm.Add("@ContactId", T.TedarikciId);
            prm.Add("@VariantCode", T.StokKodu);
            prm.Add("@DefaultPrice", T.VarsayilanFiyat);
            prm.Add("@Info", T.Bilgi);

          await  _db.ExecuteAsync($"Update Urunler SET  Isim = @Name, KategoriId = @CategoryId ,OlcuId = @MeasureId,TedarikciId = @ContactId ,StokKodu = @VariantCode,VarsayilanFiyat = @DefaultPrice ,Bilgi = @Info where id = @id and Tip = @Tip", prm);
        }
    }
}
