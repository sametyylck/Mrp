using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.TaxDTO;

namespace DAL.Contracts
{
    public interface ITaxRepository
    {
        Task<int> Insert(TaxInsert T, int CompanyId);
        Task<int> Register(int id);
        Task<IEnumerable<TaxClas>> List();
        Task Update(TaxUpdate tax);
        Task Delete(IdControl tax);
    }
}
