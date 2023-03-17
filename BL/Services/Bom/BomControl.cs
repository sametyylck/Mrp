using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services.Bom
{
    public class BomControl : IBomControl
    {
        private readonly IDbConnection _db;

        public BomControl(IDbConnection db)
        {
            _db = db;
        }

        public async Task<List<string>> Insert(BomDTO.BOMInsert T)
        {

            List<string> hatalar = new();
            DynamicParameters param = new DynamicParameters();
            param.Add("@ProductId", T.MamulId);

            param.Add("@MaterialId", T.MalzemeId);//materyal id yi alıp aşağıda sorgulatıyoruz
            string sql = $"Select Tip From Urunler where id  = @MaterialId";
            var Materialtipdogrumukontrol = await _db.QueryAsync<ItemDTO.Items>(sql, param);

            if (Materialtipdogrumukontrol.Count()==0)
            {
                hatalar.Add("Materialid bulunamadı.");

            }
            if (Materialtipdogrumukontrol.First().Tip!= "Material")
            {

                if (Materialtipdogrumukontrol.First().Tip != "SemiProduct")
                {

                    hatalar.Add("Materialid tipi dogru degil.");
                }

            }
           
            string? Materialtip = Materialtipdogrumukontrol.First().Tip;

   

            //burda eşlenmesi istenen Product Id nin tipi Product mu diye kontrol ediyoruz.
            DynamicParameters param1 = new DynamicParameters();
            param1.Add("@ProductId", T.MamulId);//Product id yi alıp aşağıda sorgulatıyoruz


            string? ProductTip;
            if (T.Tip == "SemiProduct")
            {
                string sql1 = $"Select Tip,VarsayilanFiyat From Urunler where  id = @ProductId and Tip='SemiProduct' and Tip!='Product'";
                var ProductTipDogrumuKontrol = await _db.QueryAsync<ItemDTO.Items>(sql1, param1);
                if (ProductTipDogrumuKontrol.Count() == 0)
                {
                    hatalar.Add("ProductId bulunamadı.");

                }
                //Burda eşlemek istenilen material eklenmek istediği producta eklimi diye kontrol ediyoruz.
                ProductTip = ProductTipDogrumuKontrol.First().Tip;
                if (T.Tip != ProductTip)
                {
                    hatalar.Add("ProductId Tip hatası");

                }   
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@ProductId", T.MamulId);
                param2.Add("@MaterialId", T.MalzemeId);
                string sql2 = $"Select COUNT(*) as Eklimi From UrunRecetesi where  MamulId = @ProductId and MalzemeId = @MaterialId";
                var EklimiKontrol = await _db.QueryAsync<int>(sql2, param2);
                int eklimi = EklimiKontrol.First();

                if (eklimi == 0)
                {
                    if (Materialtip != null || ProductTip != null)
                    {
                        if (Materialtip == "Material" && ProductTip == "SemiProduct")
                        {

                            return hatalar;
                        }
                    }
                    hatalar.Add("MaterialTip veya Tip hatalı");
                }
                else
                {
                    hatalar.Add("Boyle bir tarif mevcut.");

                }

            }
            else if (T.Tip == "Product")
            {
                string sql1 = $"Select Tip,VarsayilanFiyat From Urunler where id = @ProductId";
                var ProductTipDogrumuKontrol = await _db.QueryAsync<ItemDTO.Items>(sql1, param1);
                if (ProductTipDogrumuKontrol.Count() == 0)
                {
                    hatalar.Add("ProductId bulunamadı.");

                }
                ProductTip = ProductTipDogrumuKontrol.First().Tip;
                if (T.Tip!=ProductTip)
                {
                    hatalar.Add("ProductId Tip hatası");

                }
           
                //Burda eşlemek istenilen material eklenmek istediği producta eklimi diye kontrol ediyoruz.
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@ProductId", T.MamulId);
                param2.Add("@MaterialId", T.MalzemeId);
                string sql2 = $"Select COUNT(*) as Eklimi From UrunRecetesi where  MamulId = @ProductId and MalzemeId = @MaterialId";
                var EklimiKontrol = await _db.QueryAsync<int>(sql2, param2);
                int eklimi = EklimiKontrol.First();


                if (eklimi == 0)
                {
                    if (Materialtip != null || ProductTip != null)
                    {
                        if (Materialtip == "Material" || Materialtip=="SemiProduct" && ProductTip == "Product")
                        {

                            return hatalar;
                        }
                    }
                    hatalar.Add("MatrialId veya ProductId Tip hatası.");
                }
                hatalar.Add("boyle bir tarif mevcut.");


            }
            hatalar.Add("Tip degiskeni hatası");
            return hatalar;
        }

        public async Task<List<string>> Update(BomDTO.BOMUpdate T)
        {
            List<string> hatalar = new(); 
            DynamicParameters param = new DynamicParameters();
            param.Add("@ProductId", T.MamulId);

            param.Add("@MaterialId", T.MalzemeId);//materyal id yi alıp aşağıda sorgulatıyoruz
            param.Add("@id", T.id);
            string sqlquery = $"Select id from UrunRecetesi where id=@id";
            var kontrol = await _db.QueryAsync(sqlquery, param);
            if (kontrol.Count() == 0)
            {
                hatalar.Add("Boyle bir id yok.");
            }
     
            string sql = $"Select Tip From Urunler where  id  = @MaterialId";
            var Materialtipdogrumukontrol = await _db.QueryAsync<ItemDTO.Items>(sql, param);
            if (Materialtipdogrumukontrol.Count() == 0)
            {
                hatalar.Add("MaterialId tip Material degil");

            }
            string? Materialtip = Materialtipdogrumukontrol.First().Tip;

            if (T.Tip == "SemiProduct")
            {
                string sql1 = $"Select Tip,VarsayilanFiyat From Urunler where  id = @ProductId and Tip='SemiProduct' and Tip!='Product'";
                var ProductTipDogrumuKontrol = await _db.QueryAsync<ItemDTO.Items>(sql1, param);
                if (ProductTipDogrumuKontrol.Count() == 0)
                {
                    hatalar.Add("SemiPrdouct'a Product Eklenmez.");
                }
         
                string? ProductTip = ProductTipDogrumuKontrol.First().Tip;
                if (T.Tip != ProductTip)
                {
                    hatalar.Add("SemiProduct,productId Tip hatası");

                }
                if (Materialtip != null || ProductTip != null)
                {
                    if (Materialtip == "Material" && ProductTip == "SemiProduct")
                    {

                        return hatalar;
                    }
                }
                hatalar.Add("MaterialTip veya Tip hatalı ");
                return hatalar;

            }
            else if (T.Tip == "Product")
            {
                //burda eşlenmesi istenen Product Id nin tipi Product mu diye kontrol ediyoruz.
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@id", T.id);
                param1.Add("@ProductId", T.MamulId);//Product id yi alıp aşağıda sorgulatıyoruz
                string sql1 = $"Select Tip,VarsayilanFiyat From Urunler where id = @ProductId";
                var ProductTipDogrumuKontrol = await _db.QueryAsync<ItemDTO.Items>(sql1, param1);
                if (ProductTipDogrumuKontrol.Count() == 0)
                {
                    hatalar.Add("ProductId tip hatası ");
                }
                string? ProductTip = ProductTipDogrumuKontrol.First().Tip;
                if (T.Tip != ProductTip)
                {
                    hatalar.Add("Product,productId Tip hatası");

                }
                if (Materialtip != null || ProductTip != null)
                {
                    if (Materialtip == "Material" && ProductTip == "Product")
                    {
                        return hatalar;
                    }
                    hatalar.Add("MaterialId veya ProductId Tip hatası");
                }
                hatalar.Add("Tip null olamaz");
                return hatalar;

            }
            else
            {
                hatalar.Add("Tip degiskeni yanlıs");
                return hatalar;

            }




        }
    }
}
