using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface IResourceRepository
    {
        Task<IEnumerable<ResourcesDTO>> List(int CompanyId);
        Task<int> Insert(ResourcesInsert T, int CompanyId);
        Task Update(ResourcesUpdate T, int CompanyId);
        Task Delete(IdControl T, int CompanyId);
    }
}
