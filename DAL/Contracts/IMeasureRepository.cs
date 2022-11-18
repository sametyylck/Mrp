using DAL.DTO;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface IMeasureRepository
    {
        Task Register(int id);
        Task<IEnumerable<MeasureDTO>> List(int CompanyId);
        Task<int> Insert(MeasureInsert T, int CompanyId);
        Task Update(MeasureUpdate T, int CompanyId);
        Task Delete(IdControl T, int CompanyId);
    }
}
