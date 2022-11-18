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
        Task<IEnumerable<TaxClas>> List(int CompanyId);
        Task Update(TaxUpdate tax,int CompanyId);
        Task Delete(IdControl tax,int CompanyId);
    }
}
