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
    public class MeasureRepository : IMeasureRepository
    {
        IDbConnection _connection;

        public MeasureRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task Delete(IdControl T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@CompanyId", CompanyId);
            //Burda Silinen Measure ye ait Item ların MeasureId lerini Null Yapıyoruz
            await _connection.ExecuteAsync($"Update Items Set MeasureId = null where MeasureId = @id and CompanyId = @CompanyId", prm);
            //Burda Normal Measure Kaydını Siliyoruz
           await _connection.ExecuteAsync($"Delete From Measure where id = @id and CompanyId = @CompanyId", prm);
        }

        public async Task<int> Insert(MeasureInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Name", T.Name);
            prm.Add("@CompanyId", CompanyId);
            return await _connection.QuerySingleAsync<int>($"Insert into Measure (Name, CompanyId) OUTPUT INSERTED.[id] values (@Name, @CompanyId)", prm);
        }

        public async Task<IEnumerable<MeasureDTO>> List(int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            var list = await _connection.QueryAsync<MeasureDTO>($"Select * From Measure where CompanyId = @CompanyId", prm);
            return list.ToList();
        }

        public async Task Register(int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Name", "Mt");
            prm.Add("@CompanyId", id);
          await _connection.QuerySingleOrDefaultAsync($"Insert into Measure (Name,CompanyId) values  (@Name,@CompanyId)", prm);
        }

        public async Task Update(MeasureUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Name", T.Name);
            prm.Add("@CompanyId", CompanyId);
          await _connection.ExecuteAsync($"Update Measure SET Name = @Name where id = @id  and CompanyId = @CompanyId", prm);
        }
    }
}
