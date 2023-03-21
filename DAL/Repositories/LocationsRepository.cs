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

        public async Task Delete(IdControl T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Tip", "SettingsLocation");
            prm.Add("@Aktif", false);
            prm.Add("@DateTime", DateTime.Now);
           await _dbConnection.ExecuteAsync($"Update DepoVeAdresler SET Aktif = @Aktif,DeleteDate=@DateTime where id = @id and Tip=@Tip", prm);
        }

        public async Task<int> Insert(LocationsInsert T, int KullanıcıId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "SettingsLocation");
            prm.Add("@LocationName", T.Isim);
            prm.Add("@AddressLine1", T.Adres1);
            prm.Add("@AddressLine2", T.Adres2);
            prm.Add("@CityTown", T.Sehir);
            prm.Add("@StateRegion", T.Cadde);
            prm.Add("@ZipPostalCode", T.PostaKodu);
            prm.Add("@Country", T.Ulke);
            prm.Add("@LegalName", T.GercekIsim);
            prm.Add("@Sell", T.Satis);
            prm.Add("@Make", T.Uretim);
            prm.Add("@Aktif", true);
            prm.Add("@Buy", T.SatinAlma);
            prm.Add("@KullanıcıId", KullanıcıId);
            return await _dbConnection.QuerySingleAsync<int>($"Insert into DepoVeAdresler (Isim,GercekIsim,Satis,Uretim,SatinAlma,Adres1,Adres2,Tip,Sehir,Cadde,PostaKodu,Ulke,Aktif,KullaniciId) OUTPUT INSERTED.[id] values (@LocationName,@LegalName,@Sell,@Make,@Buy,@AddressLine1,@AddressLine2,@Tip,@CityTown,@StateRegion,@ZipPostalCode, @Country,@Aktif,@KullanıcıId)", prm);
        }

        public async Task<IEnumerable<LocationsDTO>> List()
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "SettingsLocation");
            var list = await _dbConnection.QueryAsync<LocationsDTO>($"Select id,Isim ,GercekIsim ,Satis a,Uretim ,SatinAlma ,Adres1 ,Adres2 ,Tip,Sehir ,Cadde a,PostaKodu ,Ulke from DepoVeAdresler where Tip=@Tip and Aktif=1", prm);
            return list.ToList();
        }

        public async Task<int> Register(int KullanıcıId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "SettingsLocation");
            prm.Add("@LocationName", "Ana Konum");
            prm.Add("@LegalName", "Ana Konum");
            prm.Add("@Sell", 1);
            prm.Add("@Make", 1);
            prm.Add("@Buy", 1);
            prm.Add("@Aktif", true);
            prm.Add("@KullanıcıId", KullanıcıId);


            return await _dbConnection.QuerySingleAsync<int>($"Insert into DepoVeAdresler (Tip,Isim,GercekIsim,Satis,Uretim,SatinAlma,Aktif,KullaniciId) OUTPUT INSERTED.[id] values (@Tip,@LocationName,@LegalName,@Sell,@Make,@Buy,@Aktif,@KullanıcıId) ", prm);
        }

        public async Task<int> RegisterLegalAddress(int KullanıcıId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@Tip", "LegalAddress");
            param.Add("@KullanıcıId", KullanıcıId);
            param.Add("@Aktif", true);
            return await _dbConnection.QuerySingleAsync<int>($"Insert into DepoVeAdresler (Tip,Aktif,KullaniciId) OUTPUT INSERTED.[id] values (@Tip,@Aktif,@KullanıcıId) ", param);
        }

        public async Task Update(LocationsDTO T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "SettingsLocation");
            prm.Add("@id", T.id);
            prm.Add("@LocationName", T.Isim);
            prm.Add("@AddressLine1", T.Adres1);
            prm.Add("@AddressLine2", T.Adres2);
            prm.Add("@CityTown", T.Sehir);
            prm.Add("@StateRegion", T.Cadde);
            prm.Add("@ZipPostalCode", T.PostaKodu);
            prm.Add("@Country", T.Ulke);
            prm.Add("@LegalName", T.GercekIsim);
            prm.Add("@Sell", T.Satis);
            prm.Add("@Make", T.Uretim);
            prm.Add("@Buy", T.SatinAlma);
            await _dbConnection.ExecuteAsync($"Update DepoVeAdresler SET Isim=@LocationName,GercekIsim=@LegalName,Satis=@Sell,Uretim=@Make,SatinAlma=@Buy,Adres1=@AddressLine1,Adres2=@AddressLine2,Sehir=@CityTown,Cadde=@StateRegion,PostaKodu=@ZipPostalCode,Ulke=@Country where id = @id and Tip=@Tip", prm);
        }
    }
}
