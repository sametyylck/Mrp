using DAL.DTO;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface ICompanyRepository
    {
        Task<int> Register(CompanyRegisterDTO T);
        Task<int> RoleInsert(int CompanyId);
        Task UserRegister(User T, int id, int RoleId);
        Task<IEnumerable<CompanyClas>> List(int CompanyId);
        Task<int> Insert(CompanyInsert T, int CompanyId);
        Task UpdateCompany(CompanyUpdateCompany T, int CompanyId);
        Task Update(CompanyUpdate T, int CompanyId);
        Task Delete(IdControl T, int CompanyId,int User);

    }
}
