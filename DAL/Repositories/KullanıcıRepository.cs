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

        public async Task<IEnumerable<UserDetail>> KullanıcıDetail(int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@UserId", id);
           var list= await _db.QueryAsync<UserDetail>(@$"Select us.id,us.Ad,us.Soyisim,us.Telefon,us.Mail,us.Sifre,us.RoleId,Role.RoleIsmi from Kullanıcılar us
            left join Role on Role.id=RoleId where us.id=@UserId", prm);

            foreach (var item in list)
            {
                prm.Add("@RoleId", item.RoleId);
                var izinler = await _db.QueryAsync<PermisionDetay>(@$"select per.id,per.RoleId,per.IzinId,IzinlerList.IzinIsmi as PermisionName from Izinler per 
                left join PermisionList on PermisionList.id=per.PermisionId
                where per.RoleId=@RoleId", prm);
                item.Permision = izinler;
            }
            return list;
        }

        public async Task<int> KullanıcıInsert(UserInsert T, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Mail", T.Mail);
            prm.Add("@Password", T.Password);
            prm.Add("@FirstName", T.FirstName);
            prm.Add("@LastName", T.LastName);
            prm.Add("@PhoneNumber", T.PhoneNumber);
            prm.Add("@RoleId", T.RoleId);
            prm.Add("@ParentId", UserId);

            var sql = @"Insert into Kullanıcılar (ParentId,RoleId,Mail,Sifre,Ad,Soyisim,Telefon) OUTPUT INSERTED.[id] values  (@ParentId,@RoleId,@Mail,@Password,@FirstName,@LastName,@PhoneNumber)";
            int userid = await _db.QuerySingleAsync<int>(sql, prm);

            return userid;
        }
        public async Task KullanıcıUpdate(UserUpdate T,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Mail", T.Mail);
            prm.Add("@Password", T.Password);
            prm.Add("@FirstName", T.FirstName);
            prm.Add("@LastName", T.LastName);
            prm.Add("@PhoneNumber", T.PhoneNumber);
            prm.Add("@RoleId", T.RoleId);
            var sql = @"Update Users set RoleId=@RoleId ,Mail=@Mail,Sifre=@Password,Ad=@FirstName,Soyisim=@LastName,Telefon=@PhoneNumber where id=@id)";
            await _db.ExecuteAsync(sql, prm);
        }
        public async Task KullanıcıDelete(int id, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@UserId", UserId);
            prm.Add("@id", id);

            await _db.ExecuteAsync($"Delete from Kullanıcılar where id=@id and  ParentId is not null)", prm);
        }
        public async Task<IEnumerable<RoleDetay>> KullanıcıList(string? kelime, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@UserId", UserId);

            var list = await _db.QueryAsync<RoleDetay>(@$"Select id,RoleIsmi as RoleName from Kullanıcılar us where  us.Ad Like '%{kelime}%' 
            or us.Soyisim Like '%{kelime}%' or us.Telefon Like '%{kelime}%' or us.Mail Like '%{kelime}%'", prm);
            return list;
        }

        public async Task<int> RoleInsert(RoleInsert T,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@RoleName", T.RoleName);
            prm.Add("@UserId", UserId);
            prm.Add("@Varsayilan", false);


            var sql = @"Insert into Role (Varsayilan,RoleIsmi,KullaniciId) OUTPUT INSERTED.[id] values  (@Varsayilan,@RoleName,@UserId)";
            int roleid = await _db.QuerySingleAsync<int>(sql, prm);
            foreach (var item in T.PermisionId)
            {
                prm.Add("@PermisionId", item);
                prm.Add("@RoleId", roleid);

                await _db.QuerySingleAsync<int>($"Insert into Izinler (IzinId,RoleId,KullaniciId) OUTPUT INSERTED.[id] values (@PermisionId,@RoleId,@UserId)", prm);
            }
            return roleid;
        }
        public async Task RoleUpdate(RoleUpdate T,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@RoleName", T.RoleName);
            prm.Add("@UserId", UserId);
            prm.Add("@id", T.id);

            var sql = @"Update Role set  RoleIsmi=@RoleName where id=@id";
            await _db.ExecuteAsync(sql, prm);
        }
        public async Task RoleDelete(int id, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@UserId", UserId);
            prm.Add("@id", id);

            await _db.ExecuteAsync($"Delete from Role where id=@id ", prm);
        }
        public async Task<IEnumerable<RoleDetay>> RoleDetail(int id,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@UserId", UserId);
            prm.Add("@RoleId", id);

            var list = await _db.QueryAsync<RoleDetay>(@$"Select * from Role where id=@RoleId ", prm);
            foreach (var item in list)
            {
                var izinler = await _db.QueryAsync<PermisionDetay>(@$"select per.id,per.IzinId,PermisionList.IzinIsmi from Izinler per 
                left join IzinlerList on IzinlerList.id=per.IzinId
                where per.RoleId=@RoleId", prm);
                item.Permision = izinler;
            }
            return list;
           

        }
        public async Task<IEnumerable<RoleDetay>> RoleList(string? kelime,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@UserId", UserId);

            var list = await _db.QueryAsync<RoleDetay>(@$"Select * from Role where  Role.RoleIsmi Like '%{kelime}%'", prm);
            return list;
        }

        public async Task PermisionInsert(PermisionInsert T, int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@RoleId", T.RoleId);
            prm.Add("@UserId", UserId);
            foreach (var item in T.PermisionId)
            {
                prm.Add("@PermisionId", item);
                await _db.QuerySingleAsync<int>($"Insert into Izinler (IzinId,RoleId,KullaniciId) OUTPUT INSERTED.[id] values (@PermisionId,@RoleId,@UserId)", prm);
            }
        }
        public async Task PermisionDelete(PermisionInsert T,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@RoleId", T.RoleId);
            prm.Add("@UserId", UserId);
            foreach (var item in T.PermisionId)
            {
                prm.Add("@PermisionId", item);
                await _db.ExecuteAsync($"Delete from Izinler where id=@PermisionId and RoleId=@RoleId", prm);
            }
        }

    }
}
