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
    public class BomRepository : IBomRepository
    {
        IDbConnection _db;

        public BomRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task Delete(IdControl T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@CompanyId", CompanyId);
            string sql = $"Delete From BOM where CompanyId = @CompanyId and id = @id";
           await _db.ExecuteAsync(sql, param);
        }

        public async Task<int> Insert(BomDTO.BOMInsert T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@ProductId", T.ProductId);
            param.Add("@MaterialId", T.MaterialId);
            param.Add("@Quantity", T.Quantity);
            param.Add("@Note", T.Note);
            param.Add("@IsActive", true);
            param.Add("@CompanyId", CompanyId);
            string sql = $"Insert into BOM (ProductId,MaterialId,Quantity,Note,IsActive,CompanyId) OUTPUT INSERTED.[id] values (@ProductId,@MaterialId,@Quantity,@Note,@IsActive,@CompanyId)";
            return await _db.QuerySingleAsync<int>(sql, param);
        }

        public async Task<IEnumerable<BomDTO.ListBOM>> List(int ProductId, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@ProductId", ProductId);
            param.Add("@CompanyId", CompanyId);
            string sql = $"Select Bom.id,Bom.ProductId,Bom.MaterialId,Items.Name as MaterialName,(Items.DefaultPrice *Bom.Quantity) as StockCost,Bom.Quantity,Bom.Note,Bom.IsActive From BOM inner join Items on Bom.MaterialId = Items.id and Items.Tip = 'Material' where Bom.ProductId = @ProductId  and BOM.CompanyId = @CompanyId AND Bom.IsActive=1";
            var list =await _db.QueryAsync<BomDTO.ListBOM>(sql, param);
            return list.ToList();
        }

        public async Task Update(BomDTO.BOMUpdate T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@MaterialId", T.MaterialId);
            param.Add("@Quantity", T.Quantity);
            param.Add("@Note", T.Note);

            param.Add("@CompanyId", CompanyId);
            string sql = $"Update BOM SET  MaterialId = @MaterialId , Quantity = @Quantity , Note = @Note  where CompanyId = @CompanyId and id = @id";
            await _db.ExecuteAsync(sql, param);
        }
    }
}
