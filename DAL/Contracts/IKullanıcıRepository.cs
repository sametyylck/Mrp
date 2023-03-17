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
        Task<int> KullanıcıInsert(UserInsert T, int UserId);
        Task KullanıcıUpdate(UserUpdate T, int UserId);
        Task KullanıcıDelete(int id, int UserId);
        Task<IEnumerable<RoleDetay>> KullanıcıList(string? kelime,  int UserId);
        Task<IEnumerable<UserDetail>> KullanıcıDetail(int id);
        Task<int> RoleInsert(RoleInsert T, int UserId);
        Task RoleUpdate(RoleUpdate T, int UserId);
        Task RoleDelete(int id, int UserId);
        Task<IEnumerable<RoleDetay>> RoleDetail(int id, int UserId);
        Task<IEnumerable<RoleDetay>> RoleList(string? kelime,int UserId);
        Task PermisionInsert(PermisionInsert T,int UserId);
        Task PermisionDelete(PermisionInsert T,int UserId);

    }
}
