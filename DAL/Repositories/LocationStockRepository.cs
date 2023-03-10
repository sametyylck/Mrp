using DAL.Contracts;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class LocationStockRepository : ILocationStockRepository
    {
        IDbConnection _db;

        public LocationStockRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task Delete(int LocationStockId,  int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", LocationStockId);
            param.Add("@IsActive", false);
            param.Add("@CompanyId", CompanyId);
           await _db.ExecuteAsync($"Update LocationStock SET IsActive = @IsActive where CompanyId = @CompanyId and id = @id", param);
        }

        public async Task<int> Insert(string Tip, int? ItemId, int CompanyId, int? LocationId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", Tip);
            prm.Add("@LocationId", LocationId);
            prm.Add("@StockCount", 0);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ItemId", ItemId);
            prm.Add("@IsActive", true);
            int id= await _db.QuerySingleAsync<int>($"Insert into LocationStock (Tip,LocationId,ItemId,StockCount,CompanyId,IsActive)  OUTPUT INSERTED.[id] values (@Tip,@LocationId,@ItemId,@StockCount,@CompanyId,@IsActive)", prm);
            return id;
        }
    }
}
