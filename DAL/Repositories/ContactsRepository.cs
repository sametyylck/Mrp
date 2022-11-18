using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.ContactDTO.ContactsList;

namespace DAL.Repositories
{
    public class ContactsRepository:IContactsRepository
    {
      
        private readonly IDbConnection _dbConnection;

        public ContactsRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
       
        }

        public async Task<int> Count(ContactsFilters T, int CompanyId)
        {
            var kayitsayisi =await _dbConnection.QuerySingleAsync<int>($"Select COUNT(*) as kayitsayisi from Contacts where Contacts.CompanyId = {CompanyId} and Contacts.IsActive=1  and Contacts.Tip = '{T.Tip}' and ISNULL(Contacts.DisplayName,0) LIKE '%{T.DisplayName}%' and ISNULL(Contacts.Mail,0) LIKE '%{T.Mail}%' and ISNULL(Contacts.Phone,0) LIKE '%{T.Phone}%' and ISNULL(Contacts.Comment,0) LIKE '%{T.Comment}%'");
            return kayitsayisi;
        }

        public async Task Delete(ContactsDelete T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Tip", T.Tip);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@IsActive", false);
            prm.Add("@DateTime", DateTime.Now);
          await  _dbConnection.ExecuteAsync($"Update Contacts Set IsActive=@IsActive,DeleteDate=@DateTime where id = @id and CompanyId = @CompanyId and Tip=@Tip", prm);
           
        }

        public async Task DeleteAddress(ContactsDelete T, int CompanyId, int id, string Tip)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@billing", id);
            prm.Add("@DateTime", DateTime.Now);
            prm.Add("@IsActive", false);
            if (Tip== "BillingAddress")
             await  _dbConnection.ExecuteAsync($"Update Locations Set IsActive=@IsActive,DeleteDate=@DateTime where id = @billing and CompanyId = @CompanyId", prm);
            else if (Tip== "ShippingAddress")
               await _dbConnection.ExecuteAsync($"Update Locations Set IsActive=@IsActive,DeleteDate=@DateTime where id = @billing and CompanyId = @CompanyId", prm);

        }

        public async Task<IEnumerable<ContactsAll>> Details(int id, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@CompanyId", CompanyId);

            var list = await _dbConnection.QueryAsync<ContactsAll>($"Select Contacts.id as CustomerId,Contacts.Tip,  Contacts.FirstName,Contacts.LastName,Contacts.CompanyName,Contacts.DisplayName,Contacts.Mail,Contacts.Phone,Contacts.Comment,Contacts.BillingLocationId,Contacts.ShippingLocationId,bil.FirstName as BillingFirstName, bil.LastName as BillingLastName, bil.CompanyName as BillingCompanyName, bil.Phone as BillingPhone,bil.AddressLine1 as BillingAddressLine1, bil.AddressLine2 as BillingAddressline2, bil.CityTown as BillingCityTown, bil.StateRegion as BillingStateRegion,bil.ZipPostalCode as BillingZipPostal, bil.Country as BillingCountry,ship.FirstName as ShippingFirstName, ship.LastName as ShippingLastName, ship.CompanyName as ShippingCompanyName, ship.Phone as ShippingPhone,ship.AddressLine1 as ShippingAddressLine1, ship.AddressLine2 as ShippingAddressline2, ship.CityTown as ShippingCityTown, ship.StateRegion as ShippingStateRegion,ship.ZipPostalCode ShippingZipPostal, ship.Country as ShippingCountry,Contacts.CompanyId from Contacts inner join Locations bil on bil.id = Contacts.BillingLocationId inner join Locations ship on ship.id = Contacts.ShippingLocationId where Contacts.id = @id and Contacts.CompanyId=@CompanyId", prm);
            return list.ToList();
        }

        public async Task<int> Insert(ContactsInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@FirstName", T.FirstName);
            prm.Add("@LastName", T.LastName);
            prm.Add("@CompanyName", T.CompanyName);
            prm.Add("@DisplayName", T.DisplayName);
            prm.Add("@Mail", T.Mail);
            prm.Add("@Comment", T.Comment);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@IsActive", true);
            if (T.Tip == "Customer")
            {
                //Ekleme işlemi yapılırken id i çekiyoruz
                return await _dbConnection.QuerySingleAsync<int>($"Insert into Contacts (FirstName,LastName,Tip,CompanyName,DisplayName, CompanyId,IsActive) OUTPUT INSERTED.[id] values (@FirstName,@LastName,@Tip,@CompanyName, @DisplayName,@CompanyId,@IsActive)", prm);
            }
            else if (T.Tip == "Supplier")
            {
                //Ekleme işlemi yapılırken id i çekiyoruz
                return await _dbConnection.QuerySingleAsync<int>($"Insert into Contacts (DisplayName,Mail,Tip,Comment,CompanyId,IsActive) OUTPUT INSERTED.[id] values (@DisplayName,@Mail,@Tip, @Comment,@CompanyId,@IsActive)", prm);
            }
            return 1;
        }

        public async Task<int> InsertAddress( int CompanyId,string Tip)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", Tip);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@IsActive", true);
            return await _dbConnection.QuerySingleAsync<int>($"Insert into Locations (Tip, CompanyId,IsActive)  OUTPUT INSERTED.[id]  values (@Tip,@CompanyId,@IsActive)", prm);
        }

        public async Task<IEnumerable<ContactsFilters>> List(ContactsFilters T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            string sql = $"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}  Select Contacts.id,Contacts.Tip,Contacts.DisplayName,Contacts.Mail,Contacts.Phone,Contacts.Comment,Contacts.IsActive from Contacts where Contacts.CompanyId = {CompanyId} and Contacts.IsActive=1 and Contacts.Tip = '{T.Tip}' and ISNULL(Contacts.DisplayName,0) LIKE '%{T.DisplayName}%' and ISNULL(Contacts.Mail,0) LIKE '%{T.Mail}%' and ISNULL(Contacts.Phone,0) LIKE '%{T.Phone}%' and ISNULL(Contacts.Comment,0) LIKE '%{T.Comment}%' ORDER BY Contacts.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";

            var list = await _dbConnection.QueryAsync<ContactsFilters>(sql);
            return list.ToList();
        }

        public async Task Update(ContactsList T, int CompanyId,int id)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@FirstName", T.FirstName);
            prm.Add("@LastName", T.LastName);
            prm.Add("@CompanyName", T.CompanyName);
            prm.Add("@DisplayName", T.DisplayName);
            prm.Add("@Mail", T.Mail);
            prm.Add("@Phone", T.Phone);
            prm.Add("@Comment", T.Comment);
            prm.Add("@CompanyId", CompanyId);
            if (T.Tip == "Customer")
            {
              await  _dbConnection.ExecuteAsync($"Update Contacts SET FirstName = @FirstName,LastName=@LastName,CompanyName=@CompanyName,DisplayName=@DisplayName,Mail=@Mail,Phone=@Phone,Comment=@Comment where id = @id  and CompanyId = @CompanyId", prm);
            }
            else if (T.Tip == "Supplier")
            {
             await _dbConnection.ExecuteAsync($"Update Contacts SET DisplayName=@DisplayName,Mail=@Mail,Comment=@Comment where id = @id  and CompanyId = @CompanyId", prm);
            }
        }

        public async Task UpdateAddress(ContactsUpdateAddress T, int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@AddressLine1", T.AddressLine1);
            prm.Add("@AddressLine2", T.AddressLine2);
            prm.Add("@CityTown", T.AddressCityTown);
            prm.Add("@StateRegion", T.AddressStateRegion);
            prm.Add("@ZipPostalCode", T.AddressZipPostal);
            prm.Add("@Country", T.AddressCountry);
            prm.Add("@FirstName", T.AddressFirstName);
            prm.Add("@LastName", T.AddressLastName);
            prm.Add("@CompanyName", T.AddressCompany);
            prm.Add("@Phone", T.AddressPhone);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", id);


            if (T.Tip == "BillingAddress")
            {
              await  _dbConnection.ExecuteAsync($"Update Locations SET FirstName=@FirstName,LastName=@LastName,CompanyName=@CompanyName,Phone=@Phone,AddressLine1=@AddressLine1,AddressLine2=@AddressLine2,CityTown=@CityTown,StateRegion=@StateRegion,ZipPostalCode=@ZipPostalCode,Country=@Country where id = @id  and CompanyId = @CompanyId and Tip=@Tip", prm);
            }
            else if (T.Tip == "ShippingAddress")
            {
              await  _dbConnection.ExecuteAsync($"Update Locations SET FirstName=@FirstName,LastName=@LastName,CompanyName=@CompanyName,Phone=@Phone,AddressLine1=@AddressLine1,AddressLine2=@AddressLine2,CityTown=@CityTown,StateRegion=@StateRegion,ZipPostalCode=@ZipPostalCode,Country=@Country where id = @id  and CompanyId = @CompanyId and Tip=@Tip", prm);
            }
        }
    }
}
