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

        public async Task<string> Insert(BomDTO.BOMInsert T, int CompanyId)
        {


            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@MaterialId", T.MaterialId);//materyal id yi alıp aşağıda sorgulatıyoruz
            string sql = $"Select Tip From Items where CompanyId = @CompanyId and id  = @MaterialId";
            var Materialtipdogrumukontrol = await _db.QueryAsync<ItemDTO.Items>(sql, param);
            if (Materialtipdogrumukontrol.Count() == 0)
            {
                return ("Materialid tipi Material degil.");
            }
            string? Materialtip = Materialtipdogrumukontrol.First().Tip;


            //burda eşlenmesi istenen Product Id nin tipi Product mu diye kontrol ediyoruz.
            DynamicParameters param1 = new DynamicParameters();
            param1.Add("@CompanyId", CompanyId);
            param1.Add("@ProductId", T.ProductId);//Product id yi alıp aşağıda sorgulatıyoruz
            string? ProductTip;
            if (T.Tip == "SemiProduct")
            {
                string sql1 = $"Select Tip From Items where CompanyId = @CompanyId and id = @ProductId and Tip='SemiProduct' and Tip!='Product'";
                var ProductTipDogrumuKontrol = await _db.QueryAsync<ItemDTO.Items>(sql1, param1);
                if (ProductTipDogrumuKontrol.Count() == 0)
                {
                    return ("SemiPrdouct'a Product Eklenmez.");
                }
                //Burda eşlemek istenilen material eklenmek istediği producta eklimi diye kontrol ediyoruz.
                ProductTip = ProductTipDogrumuKontrol.First().Tip;
                if (T.Tip != ProductTip)
                {
                    return ("ProductId Tip hatası");

                }
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@CompanyId", CompanyId);
                param2.Add("@ProductId", T.ProductId);
                param2.Add("@MaterialId", T.MaterialId);
                string sql2 = $"Select COUNT(*) as Eklimi From BOM where CompanyId = @CompanyId and ProductId = @ProductId and MaterialId = @MaterialId";
                var EklimiKontrol = await _db.QueryAsync<int>(sql2, param2);
                int eklimi = EklimiKontrol.First();

                if (eklimi == 0)
                {
                    if (Materialtip != null || ProductTip != null)
                    {
                        if (Materialtip == "Material" && ProductTip == "SemiProduct")
                        {

                            return ("true");
                        }
                    }
                    return ("MaterialTip veya Tip hatalı ");
                }
                else
                {
                    return ("Boyle bir tarif mevcut.");
                }

            }
            else if (T.Tip == "Product")
            {
                string sql1 = $"Select Tip From Items where CompanyId = @CompanyId and id = @ProductId";
                var ProductTipDogrumuKontrol = await _db.QueryAsync<ItemDTO.Items>(sql1, param1);
                if (ProductTipDogrumuKontrol.Count() == 0)
                {
                    return ("ProductId Tip hatası");
                }
                ProductTip = ProductTipDogrumuKontrol.First().Tip;
                if (T.Tip!=ProductTip)
                {
                    return ("ProductId Tip hatası");

                }
                //Burda eşlemek istenilen material eklenmek istediği producta eklimi diye kontrol ediyoruz.
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@CompanyId", CompanyId);
                param2.Add("@ProductId", T.ProductId);
                param2.Add("@MaterialId", T.MaterialId);
                string sql2 = $"Select COUNT(*) as Eklimi From BOM where CompanyId = @CompanyId and ProductId = @ProductId and MaterialId = @MaterialId";
                var EklimiKontrol = await _db.QueryAsync<int>(sql2, param2);
                int eklimi = EklimiKontrol.First();


                if (eklimi == 0)
                {
                    if (Materialtip != null || ProductTip != null)
                    {
                        if (Materialtip == "Material" && ProductTip == "Product")
                        {

                            return ("true");
                        }
                    }
                    return ("MatrialId veya ProductId Tip hatası.");
                }
                return ("boyle bir tarif mevcut.");


            }
            return ("Tip degiskeni hatası");
        }

        public async Task<string> Update(BomDTO.BOMUpdate T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@ProductId", T.ProductId);

            param.Add("@MaterialId", T.MaterialId);//materyal id yi alıp aşağıda sorgulatıyoruz
            param.Add("@id", T.id);
            string sqlquery = $"Select id from Bom where id=@id and CompanyId = @CompanyId";
            var kontrol = await _db.QueryAsync(sqlquery, param);
            if (kontrol.Count() == 0)
            {
                return ("Boyle bir id yok.");
            }
     
            string sql = $"Select Tip From Items where CompanyId = @CompanyId and id  = @MaterialId";
            var Materialtipdogrumukontrol = await _db.QueryAsync<ItemDTO.Items>(sql, param);
            if (Materialtipdogrumukontrol.Count() == 0)
            {
                return ("MaterialId tip Material degil");
            }
            string? Materialtip = Materialtipdogrumukontrol.First().Tip;

            if (T.Tip == "SemiProduct")
            {
                string sql1 = $"Select Tip From Items where CompanyId = @CompanyId and id = @ProductId and Tip='SemiProduct' and Tip!='Product'";
                var ProductTipDogrumuKontrol = await _db.QueryAsync<ItemDTO.Items>(sql1, param);
                if (ProductTipDogrumuKontrol.Count() == 0)
                {
                    return ("SemiPrdouct'a Product Eklenmez.");
                }
         
                string? ProductTip = ProductTipDogrumuKontrol.First().Tip;
                if (T.Tip != ProductTip)
                {
                    return ("SemiProduct,productId Tip hatası");

                }
                if (Materialtip != null || ProductTip != null)
                {
                    if (Materialtip == "Material" && ProductTip == "SemiProduct")
                    {

                        return ("true");
                    }
                }
                return ("MaterialTip veya Tip hatalı ");


            }
            else if (T.Tip == "Product")
            {
                //burda eşlenmesi istenen Product Id nin tipi Product mu diye kontrol ediyoruz.
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@CompanyId", CompanyId);
                param1.Add("@id", T.id);
                param1.Add("@ProductId", T.ProductId);//Product id yi alıp aşağıda sorgulatıyoruz
                string sql1 = $"Select Tip From Items where CompanyId = @CompanyId and id = @ProductId";
                var ProductTipDogrumuKontrol = await _db.QueryAsync<ItemDTO.Items>(sql1, param1);
                if (ProductTipDogrumuKontrol.Count() == 0)
                {
                    return ("ProductId tip hatası ");
                }
                string? ProductTip = ProductTipDogrumuKontrol.First().Tip;
                if (T.Tip != ProductTip)
                {
                    return ("Product,productId Tip hatası");

                }
                if (Materialtip != null || ProductTip != null)
                {
                    if (Materialtip == "Material" && ProductTip == "Product")
                    {
                        return ("true");
                    }
                    return ("MaterialId veya ProductId Tip hatası");
                }
                return ("Tip null olamaz");
            }
            else
            {
                return ("Tip degiskeni yanlıs");
            }




        }
    }
}
