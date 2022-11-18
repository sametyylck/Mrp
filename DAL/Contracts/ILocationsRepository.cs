using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface ILocationsRepository
    {
        Task<int> Register(int id);
        Task<int> RegisterLegalAddress(int id);
        Task<IEnumerable<LocationsDTO>> List(int CompanyId);
        Task<int> Insert(LocationsInsert T, int CompanyId);
        Task Update(LocationsDTO T, int CompanyId);
        Task Delete(IdControl T, int CompanyId);
    }
}
