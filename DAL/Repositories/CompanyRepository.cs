using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using Dapper;
using System.Data;

namespace DAL.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly IDbConnection _db;

        public CompanyRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task Delete(IdControl T, int CompanyId,int User)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Tip", "LegalAddress");
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@User", User);
            prm.Add("IsActive", false);
            prm.Add("@DateTime", DateTime.Now);
            //"LegalAddress")
             await _db.ExecuteAsync($"Update Locations Set IsActive=@IsActive,DeleteDate=@DateTime,@DeletedUser=@User where id = @id and CompanyId = @CompanyId and Tip=@Tip", prm);
        }

        public async Task<int> Insert(CompanyInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@AddressLine1", T.AddressLine1);
            prm.Add("@AddressLine2", T.AddressLine2);
            prm.Add("@Tip", "LegalAddress");
            prm.Add("@CityTown", T.CityTown);
            prm.Add("@StateRegion", T.StateRegion);
            prm.Add("@ZipPostalCode", T.ZipPostalCode);
            prm.Add("@Country", T.Country);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("IsActive", true);

            return await _db.QuerySingleAsync<int>($"Insert into Locations (AddressLine1,AddressLine2,Tip,CityTown,StateRegion,ZipPostalCode,Country,CompanyId,IsActive) OUTPUT INSERTED.[id] values (@AddressLine1,@AddressLine2,@Tip,@CityTown,@StateRegion,@ZipPostalCode,@Country,@CompanyId,@IsActive)", prm);
        }
        public async Task<int> RoleInsert(int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@RoleName", "Admin");
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@Varsayilan", true);
            prm.Add("@PermisionId", 1);

            int id = await _db.QuerySingleAsync<int>($"Insert into Role (Varsayilan,RoleName,CompanyId) OUTPUT INSERTED.[id] values (@Varsayilan,@RoleName,@CompanyId)", prm);
            prm.Add("@RoleId",id);

            await _db.QuerySingleAsync<int>($"Insert into Permision (PermisionId,RoleId,CompanyId) OUTPUT INSERTED.[id] values (@PermisionId,@RoleId,@CompanyId)", prm);
            return id;
        }

        public async Task<IEnumerable<CompanyClas>> List(int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@Tip", "LegalAddress");
            var list =await _db.QueryAsync<CompanyClas>($"Select Company.id,Company.LocationId, Locations.AddressLine1,Locations.AddressLine2,Locations.Tip,Locations.CityTown,Locations.StateRegion,Locations.ZipPostalCode,Locations.Country,Locations.IsActive,Company.LegalName, Company.DisplayName from Locations inner join Company ON Locations.id = Company.LocationId where CompanyId = @CompanyId and IsActive=1 and Tip=@Tip", prm);
            return list.ToList();
        }

        public async Task<int> Register(CompanyRegisterDTO T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("DisplayName", T.DisplayName, DbType.String);
            prm.Add("LegalName", T.LegalName,DbType.String);
            var sql = @"Insert into Company (DisplayName,LegalName) OUTPUT INSERTED.[id]  values  (@DisplayName,@LegalName)";
            return await _db.QuerySingleAsync<int>(sql, prm);
        }

        public async Task Update(CompanyUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "LegalAddress");
            prm.Add("@AddressLine1", T.AddressLine1);
            prm.Add("@AddressLine2", T.AddressLine2);
            prm.Add("@CityTown", T.CityTown);
            prm.Add("@StateRegion", T.StateRegion);
            prm.Add("@ZipPostalCode", T.ZipPostalCode);
            prm.Add("@Country", T.Country);
            prm.Add("@id", T.id);
            prm.Add("@CompanyId", CompanyId);

            //legal adresss
           await _db.ExecuteAsync($"Update Locations SET AddressLine1=@AddressLine1,AddressLine2=@AddressLine2,CityTown=@CityTown,StateRegion=@StateRegion,ZipPostalCode=@ZipPostalCode,Country=@Country where id = @id  and CompanyId = @CompanyId", prm);
        }

        public async Task UpdateCompany(CompanyUpdateCompany T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();

            prm.Add("@LegalName", T.LegalName);
            prm.Add("@DisplayName", T.DisplayName);
            prm.Add("@id", CompanyId);

            //legal adresss
           await _db.ExecuteAsync($"Update Company SET DisplayName=@DisplayName, LegalName=@LegalName where id = @id", prm);
        }

        public  async Task UserRegister(User T, int id,int RoleId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Mail", T.Mail);
            prm.Add("@Password", T.Password);
            prm.Add("@PasswordSalt", T.PasswordSalt);
            prm.Add("@PasswordHash", T.PasswordHash);
            prm.Add("@FirstName", T.FirstName);
            prm.Add("@LastName", T.LastName);
            prm.Add("@PhoneNumber", T.PhoneNumber);
            prm.Add("@RoleId", RoleId);
            prm.Add("@CompanyId", id);

            var sql = @"Insert into Users (RoleId,Mail,Password,FirstName,LastName,PhoneNumber,CompanyId,PasswordSalt,PasswordHash) OUTPUT INSERTED.[id] values  (@RoleId,@Mail,@Password,@FirstName,@LastName,@PhoneNumber,@CompanyId,@PasswordSalt,@PasswordHash)";
           int userid= await _db.QuerySingleAsync<int>(sql, prm);
            prm.Add("KullanıcıId", userid);

            var sql1 = @"Update Role set UserId=@KullanıcıId where id=@RoleId and CompanyId=@CompanyId";
            await _db.ExecuteAsync(sql1, prm);
            var sql2 = @"Update Permision set UserId=@KullanıcıId where RoleId=@RoleId and CompanyId=@CompanyId";
            await _db.ExecuteAsync(sql2, prm);

        }

       
    }
}
