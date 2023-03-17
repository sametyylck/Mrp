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
           await _db.ExecuteAsync($"Update UrunKaynakRecetesi SET Aktif = 0 where OperasyonId = @id", prm);
            //Burada Normal Operasyon Kaydını Siliyoruz
           await _db.ExecuteAsync($"Update Operasyonlar SET Aktif = 0 where id = @id", prm);
        }

        public async Task<int> Insert(OperationsInsert T, int KullaniciId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Name", T.Isim);
            prm.Add("@IsActive", true);
            prm.Add("@KullaniciId", KullaniciId);
            return await _db.QuerySingleAsync<int>($"Insert into Operasyonlar (Isim,Aktif,KullaniciId) OUTPUT INSERTED.[id] values (@Name,@IsActive,@KullaniciId)", prm);
        }

        public async Task<IEnumerable<OperitaonsDTO>> List(int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            var list =await _db.QueryAsync<OperitaonsDTO>($"Select id,Isim From Operasyonlar where  Aktif = 1", prm);
            return list.ToList();
        }

        public async Task Update(OperationsUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Name", T.Isim);
            prm.Add("@CompanyId", CompanyId);
           await _db.ExecuteAsync($"Update Operasyonlar SET Isim = @Name where id = @id", prm);
        }
    }
}
