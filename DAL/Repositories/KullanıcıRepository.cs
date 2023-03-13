using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ItemDTO;

namespace DAL.Repositories
{
    public class KullanıcıRepository : IKullanıcıRepository
    {
        private readonly IDbConnection _db;
        public KullanıcıRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<UserDetail>> KullanıcıDetail(int id, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@UserId", id);
           var list= await _db.QueryAsync<UserDetail>(@$"Select us.id,us.FirstName,us.LastName,us.PhoneNumber,us.Mail,us.Password,us.RoleId,Role.RoleName from Users us
            left join Role on Role.id=RoleId where us.id=@UserId and us.CompanyId=@CompanyId ", prm);

            foreach (var item in list)
            {
                prm.Add("@RoleId", item.RoleId);
                var izinler = await _db.QueryAsync<PermisionDetay>(@$"select per.id,per.RoleId,per.PermisionId,PermisionList.PermisionName from Permision per 
                left join PermisionList on PermisionList.id=per.PermisionId
                where per.RoleId=@RoleId and per.CompanyId=@CompanyId", prm);
                item.Permision = izinler;
            }
            return list;
        }

        public async Task<int> KullanıcıInsert(UserInsert T, int CompanyId, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Mail", T.Mail);
            prm.Add("@Password", T.Password);
            prm.Add("@FirstName", T.FirstName);
            prm.Add("@LastName", T.LastName);
            prm.Add("@PhoneNumber", T.PhoneNumber);
            prm.Add("@RoleId", T.RoleId);
            prm.Add("@ParentId", UserId);
            prm.Add("@CompanyId", CompanyId);

            var sql = @"Insert into Users (ParentId,RoleId,Mail,Password,FirstName,LastName,PhoneNumber,CompanyId) OUTPUT INSERTED.[id] values  (@ParentId,@RoleId,@Mail,@Password,@FirstName,@LastName,@PhoneNumber,@CompanyId)";
            int userid = await _db.QuerySingleAsync<int>(sql, prm);

            return userid;
        }
        public async Task KullanıcıUpdate(UserUpdate T, int CompanyId, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Mail", T.Mail);
            prm.Add("@Password", T.Password);
            prm.Add("@FirstName", T.FirstName);
            prm.Add("@LastName", T.LastName);
            prm.Add("@PhoneNumber", T.PhoneNumber);
            prm.Add("@RoleId", T.RoleId);
            prm.Add("@CompanyId", CompanyId);
            var sql = @"Update Users set RoleId=@RoleId ,Mail=@Mail,Password=@Password,FirstName=@FirstName,LastName=@LastName,PhoneNumber=@PhoneNumber where CompanyId=@CompanyId and id=@id)";
            await _db.ExecuteAsync(sql, prm);
        }
        public async Task KullanıcıDelete(int id, int CompanyId, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@UserId", UserId);
            prm.Add("@id", id);

            await _db.ExecuteAsync($"Delete from Users where id=@id and CompanyId=@CompanyId and ParentId is not null)", prm);
        }
        public async Task<IEnumerable<RoleDetay>> KullanıcıList(string? kelime, int CompanyId, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@UserId", UserId);

            var list = await _db.QueryAsync<RoleDetay>(@$"Select * from Users us where us.CompanyId=@CompanyId and us.FirstName Like '%{kelime}%' 
            or us.LastName Like '%{kelime}%' or us.PhoneNumber Like '%{kelime}%' or us.Mail Like '%{kelime}%'", prm);
            return list;
        }

        public async Task<int> RoleInsert(RoleInsert T, int CompanyId, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@RoleName", T.RoleName);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@UserId", UserId);
            prm.Add("@Varsayilan", false);


            var sql = @"Insert into Role (Varsayilan,RoleName,CompanyId,UserId) OUTPUT INSERTED.[id] values  (@Varsayilan,@RoleName,@CompanyId,@UserId)";
            int roleid = await _db.QuerySingleAsync<int>(sql, prm);
            foreach (var item in T.PermisionId)
            {
                prm.Add("@PermisionId", item);
                prm.Add("@RoleId", roleid);

                await _db.QuerySingleAsync<int>($"Insert into Permision (PermisionId,RoleId,CompanyId,UserId) OUTPUT INSERTED.[id] values (@PermisionId,@RoleId,@CompanyId,@UserId)", prm);
            }
            return roleid;
        }
        public async Task RoleUpdate(RoleUpdate T, int CompanyId, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@RoleName", T.RoleName);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@UserId", UserId);
            prm.Add("@id", T.id);

            var sql = @"Update Role set  RoleName=@RoleName where @CompanyId=@CompanyId and id=@id";
            await _db.ExecuteAsync(sql, prm);
        }
        public async Task RoleDelete(int id, int CompanyId, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@UserId", UserId);
            prm.Add("@id", id);

            await _db.ExecuteAsync($"Delete from Role where id=@id and CompanyId=@CompanyId", prm);
        }
        public async Task<IEnumerable<RoleDetay>> RoleDetail(int id, int CompanyId, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@UserId", UserId);
            prm.Add("@RoleId", id);

            var list = await _db.QueryAsync<RoleDetay>(@$"Select * from Role where id=@RoleId and CompanyId=@CompanyId", prm);
            foreach (var item in list)
            {
                var izinler = await _db.QueryAsync<PermisionDetay>(@$"select per.id,per.PermisionId,PermisionList.PermisionName from Permision per 
                left join PermisionList on PermisionList.id=per.PermisionId
                where per.RoleId=@RoleId and per.CompanyId=@CompanyId", prm);
                item.Permision = izinler;
            }
            return list;
           

        }
        public async Task<IEnumerable<RoleDetay>> RoleList(string? kelime, int CompanyId, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@UserId", UserId);

            var list = await _db.QueryAsync<RoleDetay>(@$"Select * from Role where CompanyId=@CompanyId and Role.RoleName Like '%{kelime}%'", prm);
            return list;
        }

        public async Task PermisionInsert(PermisionInsert T, int CompanyId, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@RoleId", T.RoleId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@UserId", UserId);
            foreach (var item in T.PermisionId)
            {
                prm.Add("@PermisionId", item);
                await _db.QuerySingleAsync<int>($"Insert into Permision (PermisionId,RoleId,CompanyId,UserId) OUTPUT INSERTED.[id] values (@PermisionId,@RoleId,@CompanyId,@UserId)", prm);
            }
        }
        public async Task PermisionDelete(PermisionInsert T, int CompanyId, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@RoleId", T.RoleId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@UserId", UserId);
            foreach (var item in T.PermisionId)
            {
                prm.Add("@PermisionId", item);
                await _db.ExecuteAsync($"Delete from Permision where id=@PermisionId and RoleId=@RoleId and CompanyId=@CompanyId", prm);
            }
        }

    }
}
