using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class BomRepository : IBomRepository
    {
        IDbConnection _db;

        public BomRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task Delete(IdControl T)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            string sql = $"Delete From UrunRecetesi where id = @id";
           await _db.ExecuteAsync(sql, param);
        }

        public async Task<int> Insert(BomDTO.BOMInsert T)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@ProductId", T.MamulId);
            param.Add("@MaterialId", T.MalzemeId);
            param.Add("@Quantity", T.Miktar);
            param.Add("@Note", T.Bilgi);
            param.Add("@IsActive", true);
            string sql = $"Insert into UrunRecetesi (MamulId,MalzemeId,Miktar,Bilgi,Aktif) OUTPUT INSERTED.[id] values (@ProductId,@MaterialId,@Quantity,@Note,@IsActive)";
            return await _db.QuerySingleAsync<int>(sql, param);
        }

        public async Task<IEnumerable<BomDTO.ListBOM>> List(int ProductId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@ProductId", ProductId);
            string sql = $"Select ur.id,ur.MamulId,ur.MalzemeId,Urunler.Isim as MalzemeIsmi,(Urunler.VarsayilanFiyat *ur.Miktar) as Tutar,ur.Miktar,ur.Bilgi,ur.Aktif  From UrunRecetesi ur inner join Urunler on ur.MalzemeId = Urunler.id and Urunler.Tip = 'Material' where ur.MamulId = @ProductId  and ur.Aktif=1";
            var list =await _db.QueryAsync<BomDTO.ListBOM>(sql, param);
            return list.ToList();
        }

        public async Task Update(BomDTO.BOMUpdate T)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@MaterialId", T.MalzemeId);
            param.Add("@Quantity", T.Miktar);
            param.Add("@Note", T.Bilgi);

            string sql = $"Update UrunRecetesi SET  MalzemeId = @MaterialId , Miktar = @Quantity , Bilgi = @Note  where id = @id";
            await _db.ExecuteAsync(sql, param);
        }
    }
}
