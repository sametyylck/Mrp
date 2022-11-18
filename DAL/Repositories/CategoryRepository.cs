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

        public async Task Delete(IdControl T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@CompanyId", CompanyId);
            //Silinen Categorye ait ürünlerin CategoryId Kolonunu null olarak gücelliyoruz
            _db.Execute($"Update Items SET CategoryId = null where CategoryId = @id and CompanyId = @CompanyId", prm);
            //Normal Category Kaydını Siliyoruz
            await _db.ExecuteAsync($"Delete From Categories where id = @id and CompanyId = @CompanyId", prm);
        }

        public async Task<int> Insert(CategoryDTO.CategoryInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Name", T.Name);
            prm.Add("@CompanyId", CompanyId);
            int id = await _db.QuerySingleAsync<int>($"Insert into Categories (Name, CompanyId) OUTPUT INSERTED.[id] values (@Name, @CompanyId)", prm);
            return id;
        }

        public async Task<IEnumerable<CategoryDTO.CategoryClass>> List(int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            var list = await _db.QueryAsync<CategoryDTO.CategoryClass>($"Select * From Categories where CompanyId = @CompanyId", prm);
            return list.ToList();
        }

        public async Task Update(CategoryDTO.CategoryUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Name", T.Name);
            prm.Add("@CompanyId", CompanyId);
            await _db.ExecuteAsync($"Update Categories SET Name = @Name where id = @id and CompanyId = @CompanyId", prm);
        }
    }
}
