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

        public async Task Delete(IdControl T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            //Burda Silinen Measure ye ait Item ların MeasureId lerini Null Yapıyoruz
            await _connection.ExecuteAsync($"Update Urunler Set OlcuId = null where OlcuId = @id", prm);
            //Burda Normal Measure Kaydını Siliyoruz
           await _connection.ExecuteAsync($"Delete From Olcu where id = @id", prm);
        }

        public async Task<int> Insert(MeasureInsert T, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Name", T.Isim);
            prm.Add("@UserId", UserId);
            return await _connection.QuerySingleAsync<int>($"Insert into Olcu (Isim, KullaniciId) OUTPUT INSERTED.[id] values (@Name, @UserId)", prm);
        }

        public async Task<IEnumerable<MeasureDTO>> List()
        {
            DynamicParameters prm = new DynamicParameters();
            var list = await _connection.QueryAsync<MeasureDTO>($"Select id,Isim From Olcu", prm);
            return list.ToList();
        }

        public async Task Register(int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Name", "Mt");
            prm.Add("@UserId", UserId);
          await _connection.QuerySingleOrDefaultAsync($"Insert into Olcu (Isim,KullaniciId) values  (@Name,@UserId)", prm);
        }

        public async Task Update(MeasureUpdate T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Name", T.Isim);
            prm.Add("@CompanyId");
          await _connection.ExecuteAsync($"Update Olcu SET Isim = @Name where id = @id", prm);
        }
    }
}
