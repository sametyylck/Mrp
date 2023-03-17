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
           await _db.ExecuteAsync($"Update  Kaynaklar Set Aktif=@IsActive where id = @id", prm);
        }

        public async Task<int> Insert(ResourcesInsert T, int KullaniciId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Name", T.Isim);
            prm.Add("@DefaultCostHour", T.VarsayilanSaatlikUcret);
            prm.Add("@KullaniciId", KullaniciId);
            prm.Add("@Aktif", true);
            return await _db.QuerySingleAsync<int>($"Insert into Kaynaklar (Isim, VarsayilanSaatlikUcret,Aktif,KullaniciId) OUTPUT INSERTED.[id] values (@Name, @DefaultCostHour,@Aktif, @KullaniciId)", prm);
        }

        public async Task<IEnumerable<ResourcesDTO>> List(int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            var list =await _db.QueryAsync<ResourcesDTO>($"Select id,Isim ,VarsayilanSaatlikUcret  From Kaynaklar where Aktif=1", prm);
            return list.ToList();
        }

        public async Task Update(ResourcesUpdate T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Name", T.Isim);
            prm.Add("@DefaultCostHour", T.VarsayilanSaatlikUcret);
           await _db.ExecuteAsync($"Update Kaynaklar SET Isim = @Name , VarsayilanSaatlikUcret = @DefaultCostHour where id = @id", prm);
        }
    }
}
