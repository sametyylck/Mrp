using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ProductOperationsBomDTO;

namespace BL.Services.IdControl
{
    public class IDControl : IIDControl
    {
        private readonly IDbConnection _db;

        public IDControl(IDbConnection db)
        {
            _db = db;
        }

        public async Task<List<string>> GetControl(string tabloadi, int id, int companyid)
        {
            List<string> hatalar = new();
            DynamicParameters param = new DynamicParameters();
            param.Add("@Tablo", tabloadi);
            param.Add("id", id);
            param.Add("@CompanyId", companyid);
            if (tabloadi=="Company")
            {
                string sql = $"Select id from {tabloadi} where id=@id";
                var kontrol = await _db.QueryAsync<int>(sql, param);
                if (kontrol.Count() == 0)
                {
                    hatalar.Add("Boyle bir id yok.");
                    return hatalar;

                }
                else
                {
                    return hatalar;
                }
            }
            else
            {
                string sql = $"Select id from {tabloadi} where id=@id and CompanyId = @CompanyId";
                var kontrol = await _db.QueryAsync<int>(sql, param);
                if (kontrol.Count() == 0)
                {
                    hatalar.Add("Boyle bir id yok.");
                    return hatalar;

                }
                else
                {
                    return hatalar;

                }
            }
   
           
        }
    }
}
