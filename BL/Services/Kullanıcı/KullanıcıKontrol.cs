using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services.Kullanıcı
{
    public class KullanıcıKontrol : IKullanıcıKontrol
    {
        private readonly IDbConnection _db;

        public KullanıcıKontrol(IDbConnection db)
        {
            _db = db;
        }

        public async Task<List<string>> KullanıcıInsertKontrol(string mail,int RoleId,int CompanyId)
        {
            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@Mail", mail);
            prm.Add("@RoleId", RoleId);

            var list = await _db.QueryAsync<UserUpdate>(@$"			select
            (Select Count(id) from Role where id=@RoleId and CompanyId=2028) as RoleId,
			(Select Count(id) from Users where Mail='@mail') as id", prm);
            if (list.First().RoleId==0)
            {
                hatalar.Add("Boyle bir Rol bulunamadi");
            }
            if (list.First().id!=0)
            {
                hatalar.Add("Boyle bir mail sisteme kayıtlıdır.");
            }
            return hatalar;
        }

        public async Task<List<string>> KullanıcıUpdateKontrol(int id,string mail,int RoleId, int CompanyId)

        {
            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@Mail", mail);
            prm.Add("@RoleId", RoleId);
            prm.Add("@id", id);

            var list = await _db.QueryAsync<UserUpdate>(@$"select
            (Select Count(id) from Role where id=@RoleId and CompanyId=@CompanyId) as RoleId,
			(Select Count(id) from Users where Mail='@mail') as id,
            (Select Mail from Users where id=@id) as Mail", prm);
           
            if (list.First().RoleId == 0)
            {
                hatalar.Add("Boyle bir Rol bulunamadi");
            }
            if (list.First().Mail!=mail)
            {
                if (list.First().id != 0)
                {
                    hatalar.Add("Boyle bir mail sisteme kayıtlıdır.");
                }
            }
          
            return hatalar;
        }
        public async Task<List<string>> KullanıcıDelete(int id, int CompanyId)
        {

            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@RoleId", id);
            var userid = await _db.QueryFirstAsync<int>(@$"select Count(id) from Users where Company=@Company and id=@id", prm);
            if (userid==0)
            {
                hatalar.Add("Id bulunamadı");

            }
            var adminid = await _db.QueryFirstAsync<int>(@$"select Count(Parentid) from Users where Company=@Company and id=@id", prm);
            if (adminid==1)
            {
                hatalar.Add("Admin silinemez");
                return hatalar;
            }

            return hatalar;
        }


        public async Task<List<string>> PermisionKontrol(List<int> id, int RoleId, int CompanyId)
        {
            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@RoleId", RoleId);

            var list = await _db.QueryAsync<UserUpdate>(@$"select
            (Select Count(id) from Role where id=@RoleId and CompanyId=@CompanyId) as RoleId", prm);
            foreach (var item in id)
            {
                prm.Add("@id", item);

                var list1 = await _db.QueryAsync<UserUpdate>(@$"select
                (Select Count(id) from PermisionList where id=@id) as id", prm);
                if (list1.First().id == 0)
                {
                    hatalar.Add($"Boyle bir {item}'idli Izin bulunamadi");
                }
            }
            if (list.First().RoleId == 0)
            {
                hatalar.Add("Boyle bir Rol bulunamadi");
            }

            return hatalar;
        }

        public async Task<List<string>> RoleInsert(RoleInsert T, int CompanyId)
        {
            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            foreach (var item in T.PermisionId)
            {
                prm.Add("@id", item);
                var list = await _db.QueryAsync<UserUpdate>(@$"select
            (Select Count(id) from PermisionList where id=@id) as id", prm);
                if (list.First().id == 0)
                {
                    hatalar.Add($"Boyle bir {item}'idli Izin bulunamadi");
                }
               
            }
            return hatalar;
        }

        public async Task<List<string>> RoleDelete(int id, int CompanyId)
        {

            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@RoleId", id);
            var adminid = await _db.QueryFirstAsync<int>(@$"select id from Role where CompanyId=@CompanyId and Varsayilan=1", prm);
            if (adminid==id)
            {
                hatalar.Add("Admin rolü silinemez");
                return hatalar;
            }
            var list = await _db.QueryAsync<UserUpdate>(@$"select
            (Select Count(id) from Role where id=@RoleId and CompanyId=@CompanyId) as RoleId", prm);

            var user = await _db.QueryAsync<UserUpdate>(@$"Select Count(id) as id from Users where RoleId=@RoleId and CompanyId=@CompanyId", prm);

            if (list.First().RoleId == 0)
            {
                hatalar.Add("Boyle bir Rol bulunamadi");
            }
            if (user.First().id==0)
            {
                hatalar.Add("Bu rol bir kullanıcılara bağlı.");

            }
            return hatalar;
        }
    }
}
