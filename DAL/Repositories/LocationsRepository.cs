using DAL.Contracts;
using DAL.DTO;
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
    public class LocationsRepository : ILocationsRepository
    {
        IDbConnection _dbConnection;
        public LocationsRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task Delete(IdControl T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Tip", "SettingsLocation");
            prm.Add("@IsActive", false);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@DateTime", DateTime.Now);
           await _dbConnection.ExecuteAsync($"Update Locations SET IsActive = @IsActive,DeleteDate=@DateTime where id = @id and CompanyId = @CompanyId and Tip=@Tip", prm);
        }

        public async Task<int> Insert(LocationsInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "SettingsLocation");
            prm.Add("@LocationName", T.LocationName);
            prm.Add("@AddressLine1", T.AddressLine1);
            prm.Add("@AddressLine2", T.AddressLine2);
            prm.Add("@CityTown", T.CityTown);
            prm.Add("@StateRegion", T.StateRegion);
            prm.Add("@ZipPostalCode", T.ZipPostalCode);
            prm.Add("@Country", T.Country);
            prm.Add("@LegalName", T.LegalName);
            prm.Add("@Sell", T.Sell);
            prm.Add("@Make", T.Make);
            prm.Add("@IsActive", true);
            prm.Add("@Buy", T.Buy);
            prm.Add("@CompanyId", CompanyId);
            return await _dbConnection.QuerySingleAsync<int>($"Insert into Locations (LocationName,LegalName,Sell,Make,Buy,AddressLine1,AddressLine2,Tip,CityTown,StateRegion,ZipPostalCode,Country,IsActive,CompanyId) OUTPUT INSERTED.[id] values (@LocationName,@LegalName,@Sell,@Make,@Buy,@AddressLine1,@AddressLine2,@Tip,@CityTown,@StateRegion,@ZipPostalCode, @Country,@IsActive,@CompanyId)", prm);
        }

        public async Task<IEnumerable<LocationsDTO>> List(int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@Tip", "SettingsLocation");
            var list = await _dbConnection.QueryAsync<LocationsDTO>($"Select id,LocationName,LegalName,Sell,Make,Buy,AddressLine1,AddressLine2,Tip,CityTown,StateRegion,ZipPostalCode,Country  from Locations where CompanyId = @CompanyId and Tip=@Tip and IsActive=1", prm);
            return list.ToList();
        }

        public async Task<int> Register(int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "SettingsLocation");
            prm.Add("@LocationName", "Ana Konum");
            prm.Add("@LegalName", "Ana Konum");
            prm.Add("@Sell", 1);
            prm.Add("@Make", 1);
            prm.Add("@Buy", 1);
            prm.Add("@IsActive", true);
            prm.Add("@CompanyId", id);
            return await _dbConnection.QuerySingleAsync<int>($"Insert into Locations (Tip,LocationName,LegalName,Sell,Make,Buy,IsActive,CompanyId) OUTPUT INSERTED.[id] values (@Tip,@LocationName,@LegalName,@Sell,@Make,@Buy,@IsActive,@CompanyId) ", prm);
        }

        public async Task<int> RegisterLegalAddress(int id)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@Tip", "LegalAddress");
            param.Add("@CompanyId", id);
            param.Add("@IsActive", true);
            return await _dbConnection.QuerySingleAsync<int>($"Insert into Locations (Tip,IsActive,CompanyId) OUTPUT INSERTED.[id] values (@Tip,@IsActive,@CompanyId) ", param);
        }

        public async Task Update(LocationsDTO T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "SettingsLocation");
            prm.Add("@id", T.id);
            prm.Add("@LocationName", T.LocationName);
            prm.Add("@AddressLine1", T.AddressLine1);
            prm.Add("@AddressLine2", T.AddressLine2);
            prm.Add("@CityTown", T.CityTown);
            prm.Add("@StateRegion", T.StateRegion);
            prm.Add("@ZipPostalCode", T.ZipPostalCode);
            prm.Add("@Country", T.Country);
            prm.Add("@LegalName", T.LegalName);
            prm.Add("@Sell", T.Sell);
            prm.Add("@Make", T.Make);
            prm.Add("@Buy", T.Buy);
            prm.Add("@CompanyId", CompanyId);
            await _dbConnection.ExecuteAsync($"Update Locations SET LocationName=@LocationName,LegalName=@LegalName,Sell=@Sell,Make=@Make,Buy=@Buy,AddressLine1=@AddressLine1,AddressLine2=@AddressLine2,CityTown=@CityTown,StateRegion=@StateRegion,ZipPostalCode=@ZipPostalCode,Country=@Country where id = @id  and CompanyId = @CompanyId and Tip=@Tip", prm);
        }
    }
}
