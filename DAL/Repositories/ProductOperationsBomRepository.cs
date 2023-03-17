using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ProductOperationsBomDTO;

namespace DAL.Repositories
{
    public class ProductOperationsBomRepository : IProductOperationsBomRepository
    {
        IDbConnection _db;

        public ProductOperationsBomRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task Delete(IdControl T)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            string sql = $"Delete From UrunKaynakRecetesi where  id = @id";
           await _db.ExecuteAsync(sql, param);
        }

        public async Task<int> Insert(ProductOperationsBOMInsert T, int KullanıcıId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@OperationId", T.OperasyonId);
            param.Add("@ResourceId", T.KaynakId);
            param.Add("@CostHour", T.SaatlikUcret);
            param.Add("@OperationTime", T.OperasyonZamani);
            param.Add("@ItemId", T.StokId);
            param.Add("@IsActive", true);
            param.Add("@KullanıcıId", KullanıcıId);
            string sql = $" Insert into UrunKaynakRecetesi (OperasyonId, KaynakId,SaatlikUcret,OperasyonZamani,StokId,Aktif,KullaniciId) OUTPUT INSERTED.[id]  values (@OperationId, @ResourceId,@CostHour,@OperationTime,@ItemId,@IsActive,@KullanıcıId)";
            return await _db.QuerySingleAsync<int>(sql, param);
        }

        public async Task<IEnumerable<ProductOperationsBOMList>> List(int ItemId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@ItemId", ItemId);
            string sql = $"Select a.id,a.OperasyonId,Operations.Name as OperasyonIsmi,a.KaynakId,Resources.Name as KaynakIsmi,a.SaatlikUcret,a.OperasyonZamani,a.StokId,((a.SaatlikUcret / 60 / 60) * a.OperasyonZamani) as Cost,From UrunKaynakRecetesi a " +
                $"inner join Operasyonlar on a.OperasyonId = Operasyonlar.id " +
                $"inner join Kaynaklar on a.ResourceId = Kaynaklar.id " +
                $"where a.StokId = @ItemId and a.Aktif=1";
            var list = await _db.QueryAsync<ProductOperationsBOMList>(sql, param);
            return list.ToList();
        }

        public async Task Update(ProductOperationsBOMUpdate T)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@OperationId", T.OperasyonId);
            param.Add("@ResourceId", T.KaynakId);
            param.Add("@CostHour", T.SaatlikUcret);
            param.Add("@OperationTime", T.OperasyonZamani);
            string sql = $" Update UrunKaynakRecetesi SET OperasyonId = @OperationId, KaynakId = @ResourceId,SaatlikUcret = @CostHour ,OperasyonZamani = @OperationTime where  id = @id";
            await _db.ExecuteAsync(sql, param);
        }
    }

  
}
 

