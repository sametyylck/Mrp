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
    public class OperationsRepository : IOperationsRepository
    {
        private readonly IDbConnection _db;
    
        public OperationsRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task Delete(IdControl T, int CompanyId)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@CompanyId", CompanyId);
            //Burada OperationBom eşleşme tablosundaki operasyon id si silinen operasyon olan kayıtları soft delete ediyoruz
           await _db.ExecuteAsync($"Update ProductOperationsBom SET IsActive = 0 where OperationId = @id and CompanyId = @CompanyId", prm);
            //Burada Normal Operasyon Kaydını Siliyoruz
           await _db.ExecuteAsync($"Update Operations SET IsActive = 0 where id = @id and CompanyId = @CompanyId", prm);
        }

        public async Task<int> Insert(OperationsInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Name", T.Name);
            prm.Add("@IsActive", true);
            prm.Add("@CompanyId", CompanyId);
            return await _db.QuerySingleAsync<int>($"Insert into Operations (Name,IsActive,CompanyId) OUTPUT INSERTED.[id] values (@Name,@IsActive,@CompanyId)", prm);
        }

        public async Task<IEnumerable<OperitaonsDTO>> List(int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            var list =await _db.QueryAsync<OperitaonsDTO>($"Select * From Operations where CompanyId = @CompanyId and IsActive = 1", prm);
            return list.ToList();
        }

        public async Task Update(OperationsUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Name", T.Name);
            prm.Add("@CompanyId", CompanyId);
           await _db.ExecuteAsync($"Update Operations SET Name = @Name where id = @id  and CompanyId = @CompanyId", prm);
        }
    }
}
