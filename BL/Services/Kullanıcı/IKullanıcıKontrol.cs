using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services.Kullanıcı
{
    public interface IKullanıcıKontrol
    {
        Task<List<string>> KullanıcıInsertKontrol(string mail,int RoleId,int CompanyId);
        Task<List<string>> KullanıcıUpdateKontrol(int id, string mail,int RoleId,int CompanyId);
        Task<List<string>> RoleDelete(int id,int CompanyId);
        Task<List<string>> PermisionKontrol(List<int> id, int RoleId, int CompanyId);
        Task<List<string>> RoleInsert(RoleInsert T, int CompanyId);
        Task<List<string>> KullanıcıDelete(int id, int CompanyId);
    }
}
