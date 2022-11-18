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

        public async Task Delete(IdControl T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@CompanyId", CompanyId);
            string sql = $"Delete From ProductOperationsBom where CompanyId = @CompanyId and id = @id";
           await _db.ExecuteAsync(sql, param);
        }

        public async Task<int> Insert(ProductOperationsBOMInsert T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@OperationId", T.OperationId);
            param.Add("@ResourceId", T.ResourceId);
            param.Add("@CostHour", T.CostHour);
            param.Add("@OperationTime", T.OperationTime);
            param.Add("@ItemId", T.ItemId);
            param.Add("@IsActive", true);
            param.Add("@CompanyId", CompanyId);
            string sql = $" Insert into ProductOperationsBOM (OperationId, ResourceId,CostHour,OperationTime,ItemId,IsActive,CompanyId) OUTPUT INSERTED.[id]  values (@OperationId, @ResourceId,@CostHour,@OperationTime,@ItemId,@IsActive,@CompanyId)";
            return await _db.QuerySingleAsync<int>(sql, param);
        }

        public async Task<IEnumerable<ProductOperationsBOMList>> List(int CompanyId, int ItemId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@ItemId", ItemId);
            string sql = $"Select a.id,a.IsActive,a.OperationId,Operations.Name as OperationName,a.ResourceId,Resources.Name as ResourcesName,a.CostHour,a.OperationTime,a.ItemId,a.ItemId,((a.CostHour / 60 / 60) * a.OperationTime) as Cost,a.CompanyId From ProductOperationsBom a " +
                $"inner join Operations on a.OperationId = Operations.id and Operations.CompanyId = @CompanyId " +
                $"inner join Resources on a.ResourceId = Resources.id and Resources.CompanyId = @CompanyId " +
                $"where a.CompanyId = @CompanyId and a.ItemId = @ItemId and a.IsActive=1";
            var list = await _db.QueryAsync<ProductOperationsBOMList>(sql, param);
            return list.ToList();
        }

        public async Task Update(ProductOperationsBOMUpdate T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@OperationId", T.OperationId);
            param.Add("@ResourceId", T.ResourceId);
            param.Add("@CostHour", T.CostHour);
            param.Add("@OperationTime", T.OperationTime);
            param.Add("@CompanyId", CompanyId);
            string sql = $" Update ProductOperationsBOM SET OperationId = @OperationId, ResourceId = @ResourceId,CostHour = @CostHour ,OperationTime = @OperationTime where CompanyId = @CompanyId and id = @id";
            await _db.ExecuteAsync(sql, param);
        }
    }

  
}
 

