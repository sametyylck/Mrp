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

        public async Task<int> Count(ItemsListele T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@CompanyId", CompanyId);
           var kayitsayisi =await _db.QuerySingleAsync<int>($@"Select COUNT(*) as kayitsayisi FROM Items 
                            left join Categories on Categories.id = Items.CategoryId
                            left join Contacts on Contacts.id = Items.ContactId where
                            Items.CompanyId = @CompanyId
                            and Items.IsActive = 1
                            and Items.Tip = @Tip
                            and Items.Name LIKE '%{T.Name}%'
                            and ISNULL(Items.VariantCode, '') LIKE '%{T.VariantCode}%'
                            and ISNULL(Items.DefaultPrice, 0)LIKE '%{T.DefaultPrice}%'
                            and ISNULL(Categories.Name, '') LIKE '%{T.Category.Name}%'
                            and ISNULL(Contacts.DisplayName, '') LIKE '%{T.Contacts.DisplayName}%'", prm);
            return kayitsayisi;
        }

        public async Task Delete(ItemsDelete T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Tip", T.Tip);
            prm.Add("@IsActive", false);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@DateTime", DateTime.Now);
            //Burda Items tablosunda pasife alma yapıyoruz
           await _db.ExecuteAsync($"Update Items SET IsActive = @IsActive,DeleteDate=@DateTime where id = @id and CompanyId = @CompanyId and Tip = @Tip", prm);
            //burda bom eşlenmesinde olan kayıtları pasife alıyoruz
           await _db.ExecuteAsync($"Update BOM SET IsActive = @IsActive,DeleteDate=@DateTime where MaterialId = @id and CompanyId = @CompanyId", prm);
        }

        public async Task<IEnumerable<ListItems>> Detail(int id, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@CompanyId", CompanyId);
            var itemDetail = await _db.QueryAsync<ListItems>($"Select Items.id,Items.Tip,Items.Name,Items.CategoryId,Categories.Name as CategoryName,Items.MeasureId,Measure.Name as MeasureName,Items.ContactId , Contacts.DisplayName as ContactName,Items.VariantCode, Items.DefaultPrice, Items.Info, Items.CompanyId From Items left join Categories on   Categories.id = Items.CategoryId left join Measure on  Measure.id = Items.MeasureId left join Contacts on   Contacts.id = Items.ContactId where Items.id = @id and Items.CompanyId = @CompanyId", prm);


            //İtemin reçete tablosundaki ekli materyallerın toplamını buluyoruz
            #region MaterialCost

            var Costbul =await _db.QueryAsync<costbul>($@"Select Bom.id,Bom.MaterialId,Bom.Quantity, Items.id as itemid, Items.DefaultPrice From Bom 
                                                         inner join Items on Items.id = Bom.MaterialId
                                                         where Bom.IsActive = 1 and Bom.CompanyId = {CompanyId} and Bom.ProductId = {id}");
            float? IngredientsCost = 0;
            if (itemDetail.Count() == 0)
            {
                return itemDetail;
            }
            if (Costbul.Count() == 0)
            {
                itemDetail.First().IngredientsCost = 0;
            }
            else
            {
                foreach (var item in Costbul)
                {
                    float price = item.DefaultPrice * item.Quantity;
                    IngredientsCost += price;
                }
                itemDetail.First().IngredientsCost = IngredientsCost;
            }

            #endregion
            //Materyal Cost Son
            //InStock Ürünün Stoğunu buluyoruz
            #region InStock
            var InstockBul =await _db.QueryAsync<StockDTO.Stock>($"Select AllStockQuantity  from Items where CompanyId = {CompanyId} and id = {id} and IsActive = 1");
            float? InStock;
            if (InstockBul.Count() == 0)
            {
                itemDetail.First().InStock = 0;
            }
            else
            {
                InStock = InstockBul.First().AllStockQuantity;
                itemDetail.First().InStock = InStock;
            }
            #endregion
            //InStockSon
            //OperationCost Itemin Operasyonlarının saatlik ücretlerini hesaplayar operasyon cost u buluyoruz
            #region OperationCost
            var OperationCostBul =await _db.QueryAsync<ProductOperationsBOM>($"Select id From ProductOperationsBom where CompanyId = {CompanyId} and ItemId = {id}  and IsActive = 1");


            float? OperationsCost = 0;


            if (OperationCostBul.Count() == 0)
            {
                itemDetail.First().OperationsCost = 0;
            }
            else
            {
                foreach (var item in OperationCostBul)
                {
                    var a =await _db.QueryAsync<ProductOperationsBOM>($"Select CostHour,OperationTime From ProductOperationsBom where CompanyId = {CompanyId} and id = {item.id}  and IsActive = 1");
                    float? CostHour = a.First().CostHour;
                    float? time = a.First().OperationTime;

                    OperationsCost += (CostHour / 60 / 60) * time;
                }
                itemDetail.First().OperationsCost = OperationsCost;
            }

            #endregion
            //OperationCostSon
            return itemDetail;
        }

        public async Task<int> Insert(ItemsInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@Name", T.Name);
            prm.Add("@CategoryId", T.CategoryId);
            prm.Add("@MeasureId", T.MeasureId);
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@VariantCode", T.VariantCode);
            prm.Add("@DefaultPrice", T.DefaultPrice);
            prm.Add("@AllStock", 0);
            prm.Add("@IsActive", true);
            prm.Add("@Info", T.Info);
            prm.Add("@CompanyId", CompanyId);
            return await _db.QuerySingleAsync<int>($"Insert into Items (Tip, Name, CategoryId,MeasureId,ContactId,VariantCode,DefaultPrice,Info,IsActive,CompanyId,AllStockQuantity) OUTPUT INSERTED.[id] values (@Tip, @Name, @CategoryId,@MeasureId,@ContactId,@VariantCode,@DefaultPrice,@Info,@IsActive,@CompanyId,@AllStock)", prm);
        }

        public async Task<IEnumerable<ItemsListele>> ListMaterial(int CompanyId, ItemsListele T, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@sayi", KAYITSAYISI);
            prm.Add("@sayfa", SAYFA);
            string sql = $@"DECLARE @@KAYITSAYISI int DECLARE @@SAYFA int SET @@KAYITSAYISI = @sayi SET @@SAYFA = @sayfa
           Select x.* From(
                     Select Items.id,
                     Items.Tip,
                     Items.Name,
                     Items.IsActive,
	                     ISNULL(Items.VariantCode,'') as VariantCode,
	                     ISNULL(Categories.Name,'') as CategoryName,
	                     ISNULL(Contacts.DisplayName,'') as DisplayName,
	                     ISNULL(Items.DefaultPrice,0) as DefaultPrice,
	                     Items.CompanyId
	                     From Items
				left join Categories on Items.CategoryId = Categories.id and Categories.CompanyId = @CompanyId
				left join Contacts on Items.ContactId = Contacts.id and Contacts.CompanyId = @CompanyId
				 ) x
				where x.CompanyId = @CompanyId and x.Tip = @Tip and x.IsActive = 1
				and x.Name LIKE '%{T.Name}%' 
				and x.VariantCode LIKE '%{T.VariantCode}%' 
				and x.CategoryName LIKE '%{T.Category.Name}%' 
				and x.DisplayName LIKE '%{T.Contacts.DisplayName}%'
				and x.DefaultPrice LIKE '%{T.DefaultPrice}%'
               ORDER BY x.id OFFSET @@KAYITSAYISI * (@@SAYFA - 1) ROWS FETCH NEXT @@KAYITSAYISI ROWS ONLY;";
            var list = await _db.QueryAsync<ItemsListele>(sql, prm);
            return list.ToList();
        }

        public async Task<IEnumerable<ItemsListele>> ListProduct(int CompanyId, ItemsListele T, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@sayi", KAYITSAYISI);
            prm.Add("@sayfa", SAYFA);

            var urunler = await _db.QueryAsync<ItemsListele>($@"
                        DECLARE @@KAYITSAYISI int DECLARE @@SAYFA int SET @@KAYITSAYISI = @sayi SET @@SAYFA = @sayfa
                        Select item.* From(
                                Select Items.id,Items.Tip,Items.[Name],
                                ISNULL(Items.VariantCode,'') as VariantCode,
                                ISNULL(Items.CategoryId,'') as CategoryId ,
                                ISNULL(cat.[Name],'') as CategoryName,
                                (Cast (ISNULL(Items.DefaultPrice,0) as decimal(15,2))) as DefaultPrice
                                From Items 
                                left join Categories cat on Items.CategoryId = cat.id
                                where Items.IsActive = 1 and Items.Tip = 'Product' and Items.CompanyId = @CompanyId) item
                                Where item.Name LIKE '%{T.Name}%' and VariantCode LIKE '%{T.VariantCode}%' and item.CategoryName LIKE '%{T.Category.Name}%' and item.DefaultPrice LIKE '%{T.DefaultPrice}%'
                        ORDER BY item.id OFFSET @@KAYITSAYISI * (@@SAYFA - 1) ROWS FETCH NEXT @@KAYITSAYISI ROWS ONLY;
                         ", prm);

            foreach (var item in urunler)
            {
                var x =await Detail(item.id, CompanyId);
                float? materialcost = x.First().IngredientsCost;
                float? OperationCost = x.First().OperationsCost;
                var cost = materialcost + OperationCost;
                item.Cost = (float)cost;
                float profit = (float)(item.DefaultPrice - cost);
                item.Profit = profit;
                if (item.DefaultPrice == 0)
                {
                    item.Margin = 0;
                }
                else
                {
                    float? margin = (profit / item.DefaultPrice) * 100;
                    item.Margin = (float)margin;
                }
                List<int> timebul =await _db.QueryFirstAsync($"Select ISNULL(Sum(OperationTime),0) From ProductOperationsBom where CompanyId = {CompanyId} and IsActive = 1 and ItemId = {item.id}");
                int time = timebul[0];
                item.ProductTime = time.ToString();
            }

            return urunler;
        }

        public async Task<IEnumerable<ItemsListele>> ListSemiProduct(int CompanyId, ItemsListele T, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@sayi", KAYITSAYISI);
            prm.Add("@sayfa", SAYFA);

            var urunler = await _db.QueryAsync<ItemsListele>($@"
                        DECLARE @@KAYITSAYISI int DECLARE @@SAYFA int SET @@KAYITSAYISI = @sayi SET @@SAYFA = @sayfa
                        Select item.* From(
                                Select Items.id,Items.Tip,Items.[Name],
                                ISNULL(Items.VariantCode,'') as VariantCode,
                                ISNULL(Items.CategoryId,'') as CategoryId ,
                                ISNULL(cat.[Name],'') as CategoryName,
                                (Cast (ISNULL(Items.DefaultPrice,0) as decimal(15,2))) as DefaultPrice
                                From Items 
                                left join Categories cat on Items.CategoryId = cat.id
                                where Items.IsActive = 1 and Items.Tip = 'SemiProduct' and Items.CompanyId = @CompanyId) item
                                Where item.Name LIKE '%{T.Name}%' and VariantCode LIKE '%{T.VariantCode}%' and item.CategoryName LIKE '%{T.Category.Name}%' and item.DefaultPrice LIKE '%{T.DefaultPrice}%'
                        ORDER BY item.id OFFSET @@KAYITSAYISI * (@@SAYFA - 1) ROWS FETCH NEXT @@KAYITSAYISI ROWS ONLY;
                         ", prm);

            foreach (var item in urunler)
            {
                var x = await Detail(item.id, CompanyId);
                float? materialcost = x.First().IngredientsCost;
                float? OperationCost = x.First().OperationsCost;
                float? cost = materialcost + OperationCost;
                item.Cost = (float)cost;
                float profit = (float)(item.DefaultPrice - cost);
                item.Profit = profit;
                if (item.DefaultPrice == 0)
                {
                    item.Margin = 0;
                }
                else
                {
                    float? margin = (profit / item.DefaultPrice) * 100;
                    item.Margin = (float)margin;
                }
                List<int> timebul =( await _db.QueryAsync<int>($"Select ISNULL(Sum(OperationTime),0) From ProductOperationsBom where CompanyId = {CompanyId} and IsActive = 1 and ItemId = {item.id}")).ToList();
                int time = timebul[0];
                item.ProductTime = time.ToString();
            }

            return urunler; throw new NotImplementedException();
        }

        public async Task Update(ItemsUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Tip", T.Tip);
            prm.Add("@Name", T.Name);
            prm.Add("@CategoryId", T.CategoryId);
            prm.Add("@MeasureId", T.MeasureId);
            prm.Add("@ContactId", T.ContactId);
            prm.Add("@VariantCode", T.VariantCode);
            prm.Add("@DefaultPrice", T.DefaultPrice);
            prm.Add("@Info", T.Info);
            prm.Add("@CompanyId", CompanyId);

          await  _db.ExecuteAsync($"Update Items SET  Name = @Name, CategoryId = @CategoryId ,MeasureId = @MeasureId,ContactId = @ContactId ,VariantCode = @VariantCode,DefaultPrice = @DefaultPrice ,Info = @Info where id = @id and CompanyId = @CompanyId and Tip = @Tip", prm);
        }
    }
}
