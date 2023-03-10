using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;

namespace DAL.Contracts
{
    public interface IKullanıcıRepository
    {
        Task<int> KullanıcıInsert(UserInsert T, int CompanyId, int UserId);
        Task KullanıcıUpdate(UserUpdate T, int CompanyId, int UserId);
        Task KullanıcıDelete(int id, int CompanyId, int UserId);
        Task<IEnumerable<RoleDetay>> KullanıcıList(string? kelime, int CompanyId, int UserId);
        Task<IEnumerable<UserDetail>> KullanıcıDetail(int id, int CompanyId);
        Task<int> RoleInsert(RoleInsert T, int CompanyId, int UserId);
        Task RoleUpdate(RoleUpdate T, int CompanyId, int UserId);
        Task RoleDelete(int id, int CompanyId, int UserId);
        Task<IEnumerable<RoleDetay>> RoleDetail(int id, int CompanyId, int UserId);
        Task<IEnumerable<RoleDetay>> RoleList(string? kelime, int CompanyId, int UserId);
        Task PermisionInsert(PermisionInsert T, int CompanyId, int UserId);
        Task PermisionDelete(PermisionInsert T, int CompanyId, int UserId);

    }
}
