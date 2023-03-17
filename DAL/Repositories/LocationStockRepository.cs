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

        public async Task Delete(int StokId,int LocationStockId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", LocationStockId);
            param.Add("@IsActive", false);
            param.Add("@StokId", StokId);

            await _db.ExecuteAsync($"Update DepoStoklar SET Aktif = @IsActive where  StokId=@StokId", param);
        }

        public async Task<int> Insert(string Tip, int? ItemId,int? LocationId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", Tip);
            prm.Add("@LocationId", LocationId);
            prm.Add("@StockCount", 0);
            prm.Add("@ItemId", ItemId);
            prm.Add("@IsActive", true);
            int id= await _db.QuerySingleAsync<int>($"Insert into DepoStoklar (Tip,DepoId,StokId,StokAdeti,Aktif)  OUTPUT INSERTED.[id] values (@Tip,@LocationId,@ItemId,@StockCount,@IsActive)", prm);
            return id;
        }
    }
}
