using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class ResourceRepository : IResourceRepository
    {
        IDbConnection _db;

        public ResourceRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task Delete(IdControl T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@IsActive", false);
            prm.Add("@CompanyId", CompanyId);
           await _db.ExecuteAsync($"Update  Resources Set IsActive=@IsActive where id = @id and CompanyId = @CompanyId", prm);
        }

        public async Task<int> Insert(ResourcesInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Name", T.Name);
            prm.Add("@DefaultCostHour", T.DefaultCostHour);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@IsActive", true);
            return await _db.QuerySingleAsync<int>($"Insert into Resources (Name, DefaultCostHour,IsActive,CompanyId) OUTPUT INSERTED.[id] values (@Name, @DefaultCostHour,@IsActive, @CompanyId)", prm);
        }

        public async Task<IEnumerable<ResourcesDTO>> List(int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            var list =await _db.QueryAsync<ResourcesDTO>($"Select * From Resources where CompanyId = @CompanyId and IsActive=1", prm);
            return list.ToList();
        }

        public async Task Update(ResourcesUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Name", T.Name);
            prm.Add("@DefaultCostHour", T.DefaultCostHour);
            prm.Add("@CompanyId", CompanyId);
           await _db.ExecuteAsync($"Update Resources SET Name = @Name , DefaultCostHour = @DefaultCostHour where id = @id  and CompanyId = @CompanyId", prm);
        }
    }
}
