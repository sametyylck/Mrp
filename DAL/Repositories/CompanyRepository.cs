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

        public async Task Delete(IdControl T,int User)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Tip", "LegalAddress");
            prm.Add("@User", User);
            prm.Add("IsActive", false);
            prm.Add("@DateTime", DateTime.Now);
            //"LegalAddress")
             await _db.ExecuteAsync($"Update DepoVeAdresler Set Aktif=@IsActive,SilinmeTarihi=@DateTime,@SilenKullanici=@User where id = @id and Tip=@Tip", prm);
        }

        public async Task<int> Insert(CompanyInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@AddressLine1", T.Adres1);
            prm.Add("@AddressLine2", T.Adres2);
            prm.Add("@Tip", "LegalAddress");
            prm.Add("@CityTown", T.Sehir);
            prm.Add("@StateRegion", T.Cadde);
            prm.Add("@ZipPostalCode", T.PostaKodu);
            prm.Add("@Country", T.Ulke);
            prm.Add("IsActive", true);

            return await _db.QuerySingleAsync<int>($"Insert into DepoVeAdresler (Adres1,Adres2,Tip,Sehir,Cadde,PostaKodu,Ulke,Aktif) OUTPUT INSERTED.[id] values (@AddressLine1,@AddressLine2,@Tip,@CityTown,@StateRegion,@ZipPostalCode,@Country,@IsActive)", prm);
        }
        public async Task<int> RoleInsert()
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@RoleName", "Admin");
            prm.Add("@Varsayilan", true);
            prm.Add("@PermisionId", 1);

            int id = await _db.QuerySingleAsync<int>($"Insert into Role (Varsayilan,RoleIsmi) OUTPUT INSERTED.[id] values (@Varsayilan,@RoleName)", prm);
            prm.Add("@RoleId",id);

            await _db.QuerySingleAsync<int>($"Insert into Izinler (IzinId,RoleId) OUTPUT INSERTED.[id] values (@PermisionId,@RoleId)", prm);
            return id;
        }


        public async Task Update(CompanyUpdate T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "LegalAddress");
            prm.Add("@AddressLine1", T.Adres1);
            prm.Add("@AddressLine2", T.Adres2);
            prm.Add("@CityTown", T.Sehir);
            prm.Add("@StateRegion", T.Cadde);
            prm.Add("@ZipPostalCode", T.PostaKodu);
            prm.Add("@Country", T.Ulke);
            prm.Add("@id", T.id);

            //legal adresss
           await _db.ExecuteAsync($"Update DepoVeAdresler SET Adres1=@AddressLine1,Adres2=@AddressLine2,Sehir=@CityTown,Cadde=@StateRegion,PostaKodu=@ZipPostalCode,Ulke=@Country where id = @id ", prm);
        }


        public  async Task<int> UserRegister(User T,int RoleId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Mail", T.Mail);
            prm.Add("@Password", T.Sifre);
            prm.Add("@FirstName", T.Ad);
            prm.Add("@LastName", T.Soyisim);
            prm.Add("@PhoneNumber", T.Telefon);
            prm.Add("@RoleId", RoleId);

            var sql = @"Insert into Kullanıcılar (RoleId,Mail,Sifre,Ad,Soyisim,Telefon) OUTPUT INSERTED.[id] values  (@RoleId,@Mail,@Password,@FirstName,@LastName,@PhoneNumber)";
           int userid= await _db.QuerySingleAsync<int>(sql, prm);
            prm.Add("KullanıcıId", userid);

            var sql1 = @"Update Role set KullaniciId=@KullanıcıId where id=@RoleId";
            await _db.ExecuteAsync(sql1, prm);
            var sql2 = @"Update Izinler set KullaniciId=@KullanıcıId where RoleId=@RoleId";
            await _db.ExecuteAsync(sql2, prm);
            return userid;

        }


    }
}
