using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface IOperationsRepository
    {
        Task<IEnumerable<OperitaonsDTO>> List(int CompanyId);
            Task<int> Insert(OperationsInsert T, int CompanyId);
            Task Update(OperationsUpdate T, int CompanyId);
             Task Delete(IdControl T, int CompanyId);
    }
}
