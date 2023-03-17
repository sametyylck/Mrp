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
        Task<int> RoleInsert();
        Task<int> UserRegister(User T,int RoleId);
        Task<int> Insert(CompanyInsert T, int UserId);
        Task Update(CompanyUpdate T, int UserId);
        Task Delete(IdControl T, int User);

    }
}
