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
    public class CurrencyRepository : ICurrencyRepository
    {
        IDbConnection _db;

        public CurrencyRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CurrencyDTO>> List()
        {
            string sql = $"Select * From Currency";
            var list = await _db.QueryAsync<CurrencyDTO>(sql);
            return list.ToList();
        }
    }
}
