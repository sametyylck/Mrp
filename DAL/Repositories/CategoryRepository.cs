using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.BomDTO;

namespace DAL.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbConnection _db;
        public CategoryRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task Delete(IdControl T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            //Silinen Categorye ait ürünlerin CategoryId Kolonunu null olarak gücelliyoruz
            _db.Execute($"Update Urunler SET KategoriId = null where KategoriId = @id", prm);
            //Normal Category Kaydını Siliyoruz
            await _db.ExecuteAsync($"Delete From Kategoriler where id = @id", prm);
        }

        public async Task<int> Insert(CategoryDTO.CategoryInsert T, int KullaniciId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Name", T.Isim);
            prm.Add("@KullaniciId", KullaniciId);
            int id = await _db.QuerySingleAsync<int>($"Insert into Kategoriler (Isim, KullaniciId) OUTPUT INSERTED.[id] values (@Name, @KullaniciId)", prm);
            return id;
        }

        public async Task<IEnumerable<CategoryDTO.CategoryClass>> List()
        {
            DynamicParameters prm = new DynamicParameters();
            var list = await _db.QueryAsync<CategoryDTO.CategoryClass>($"Select id,Isim From Kategoriler", prm);
            return list.ToList();
        }

        public async Task Update(CategoryDTO.CategoryUpdate T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Name", T.Isim);
            await _db.ExecuteAsync($"Update Kategoriler SET Isim = @Name where id = @id", prm);
        }
    }
}
