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

        public async Task Delete(IdControl tax)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", tax.id);
           await _db.ExecuteAsync($"Delete From Vergi where id = @id", prm);
        }

        public Task<int> Insert(TaxInsert T,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Rate", T.VergiDegeri);
            prm.Add("@TaxName", T.VergiIsim);
            prm.Add("@UserId", UserId);

            return _db.QuerySingleAsync<int>($"Insert into Vergi (VergiDegeri, VergiIsim,KullaniciId) OUTPUT INSERTED.[id] values (@Rate, @TaxName,@UserId)", prm);
        }

        public async Task<IEnumerable<TaxClas>> List()
        {
            DynamicParameters prm = new DynamicParameters();
            var list =await _db.QueryAsync<TaxClas>($"Select id,VergiDegeri ,VergiIsim  From Vergi", prm);
            return  list.ToList();
        }

        public async Task<int> Register(int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@VergiDegeri", 18);
            prm.Add("@VergiIsim", "KDV");
            prm.Add("@CompanyId", id);
            string sql = @"Insert into Vergi (VergiDegeri, VergiIsim) OUTPUT INSERTED.[id] values (@VergiDegeri, @VergiIsim)";
            return await _db.QuerySingleAsync<int>(sql,prm);
        }

        public async Task Update(TaxUpdate T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Rate", T.VergiDegeri);
            prm.Add("@TaxName", T.VergiIsim);
           await _db.ExecuteAsync($"Update Vergi SET VergiDegeri = @Rate , VergiIsim = @TaxName where id = @id", prm);
        }
    }
}
