using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.TaxDTO;

namespace DAL.Repositories
{
    public class TaxRepository : ITaxRepository
    {
        IDbConnection _db;

        public TaxRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task Delete(IdControl tax, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", tax.id);
            prm.Add("@CompanyId", CompanyId);
           await _db.ExecuteAsync($"Delete From Tax where id = @id and CompanyId = @CompanyId", prm);
        }

        public Task<int> Insert(TaxInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Rate", T.Rate);
            prm.Add("@TaxName", T.TaxName);
            prm.Add("@CompanyId", CompanyId);
            return _db.QuerySingleAsync<int>($"Insert into Tax (Rate, TaxName, CompanyId) OUTPUT INSERTED.[id] values (@Rate, @TaxName, @CompanyId)", prm);
        }

        public async Task<IEnumerable<TaxClas>> List(int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            var list =await _db.QueryAsync<TaxClas>($"Select * From Tax where CompanyId = @CompanyId", prm);
            return  list.ToList();
        }

        public async Task<int> Register(int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Rate", 18);
            prm.Add("@TaxName", "KDV");
            prm.Add("@CompanyId", id);
            string sql = @"Insert into Tax (Rate, TaxName, CompanyId) OUTPUT INSERTED.[id] values (@Rate, @TaxName, @CompanyId)";
            return await _db.QuerySingleAsync<int>(sql,prm);
        }

        public async Task Update(TaxUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Rate", T.Rate);
            prm.Add("@TaxName", T.TaxName);
            prm.Add("@CompanyId", CompanyId);
           await _db.ExecuteAsync($"Update Tax SET Rate = @Rate , TaxName = @TaxName where id = @id  and CompanyId = @CompanyId", prm);
        }
    }
}
