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
        Task<int> Register(int KullanıcıId);
        Task<int> RegisterLegalAddress(int KullanıcıId);
        Task<IEnumerable<LocationsDTO>> List();
        Task<int> Insert(LocationsInsert T, int KullanıcıId);
        Task Update(LocationsDTO T);
        Task Delete(IdControl T);
    }
}
