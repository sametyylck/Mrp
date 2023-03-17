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
    public class ContactsRepository : IContactsRepository
    {

        private readonly IDbConnection _dbConnection;

        public ContactsRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;

        }


        public async Task Delete(ContactsDelete T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@IsActive", false);
            prm.Add("@DateTime", DateTime.Now);
            await _dbConnection.ExecuteAsync($"Update Cari Set Aktif=@IsActive,SilinmeTarihi=@DateTime where CariKod = @id", prm);

        }

        public async Task DeleteAddress(ContactsDelete T, int id, string Tip)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@billing", id);
            prm.Add("@DateTime", DateTime.Now);
            prm.Add("@IsActive", false);
            await _dbConnection.ExecuteAsync($"Update DepoVeAdresler Set Aktif=@IsActive,SilinmeTarihi=@DateTime where id = @billing ", prm);


        }

        public async Task<IEnumerable<ContactsAll>> Details(int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);

            var list = await _dbConnection.QueryAsync<ContactsAll>($@"Select cr.CariKod ,  cr.AdSoyad,cr.VergiDairesi,cr.VergiNumarası,
            cr.Mail,cr.Telefon,cr.CariTipId,cr.FaturaAdresId,cr.KargoAdresId,bil.Ad as FaturaAdresIdAd,
            bil.Soyisim as FaturaAdresIdSoyIsim, bil.SirketIsmi as FaturaAdresIdSirketIsmi,
            bil.Telefon as FaturaAdresIdTelefon,bil.Adres1 as FaturaAdresIdAdres1,
            bil.Adres2 as FaturaAdresIdAdres2, bil.Sehir as FaturaAdresIdSehir,
            bil.Cadde as FaturaAdresIdCadde,bil.PostaKodu as FaturaAdresIdPostaKodu,
            bil.Ulke as FaturaAdresIdUlke,ship.Ad as KargoAdresIdAd,
            ship.Soyisim as KargoAdresIdSoyIsim, ship.SirketIsmi as KargoAdresIdSirketIsmi,
            ship.Telefon as KargoAdresIdTelefon,ship.Adres1 as KargoAdresIdAdres1, 
            ship.Adres2 as KargoAdresIdAdres2, ship.Sehir as KargoAdresIdSehir
            , ship.Cadde as KargoAdresIdCadde,ship.PostaKodu KargoAdresIdPostaKodu, 
            ship.Ulke as KargoAdresIdUlke
            from Cari cr 
            inner join DepoVeAdresler bil on bil.id = cr.FaturaAdresId 
            inner join DepoVeAdresler ship on ship.id = cr.KargoAdresId 
            where cr.CariKod = @id ", prm);
            return list.ToList();
        }

        public async Task<int> Insert(ContactsInsert T, int KullaniciId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CariTipId", T.CariTipId);
            prm.Add("@AdSoyad", T.AdSoyad);
            prm.Add("@VergiDairesi", T.VergiDairesi);
            prm.Add("@Mail", T.Mail);
            prm.Add("@VergiNumarası", T.VergiNumarası);
            prm.Add("@Telefon", T.Telefon);
            prm.Add("@ParaBirimiId", T.ParaBirimiId);
            prm.Add("@CompanyId", KullaniciId);
            prm.Add("@IsActive", true);

            return await _dbConnection.QuerySingleAsync<int>($"Insert into Cari (AdSoyad,VergiDairesi,CariTipId,VergiNumarası, Mail,Telefon,ParaBirimiId,Aktif) OUTPUT INSERTED.[CariKod] values (@AdSoyad,@VergiDairesi,@CariTipId,@VergiNumarası,@Mail, @Telefon,@ParaBirimiId,@IsActive)", prm);

        }

        public async Task<int> InsertAddress(string Tip)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", Tip);
            prm.Add("@IsActive", true);
            return await _dbConnection.QuerySingleAsync<int>($"Insert into DepoVeAdresler (Tip,Aktif)  OUTPUT INSERTED.[id]  values (@Tip,@IsActive)", prm);
        }

        public async Task<IEnumerable<ContactsFilters>> List(ContactsFilters T, int KAYITSAYISI, int SAYFA)
        {
            string sql = $"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI}  SET @SAYFA = {SAYFA}  Select c.CariKod,c.AdSoyad ,c.Mail,c.Telefon,c.Bilgi,c.Aktif,c.VergiDairesi,c.VergiNumarası  from Cari c where c.Aktif=1 and c.VergiDairesi = '{T.VergiDairesi}' and ISNULL(c.AdSoyad,0) LIKE '%{T.AdSoyad}%' and ISNULL(c.Mail,0) LIKE '%{T.Mail}%' and ISNULL(c.Telefon,0) LIKE '%{T.Telefon}%' and ISNULL(c.VergiNumarası,0) LIKE '%{T.VergiNumarası}%' ORDER BY Cari.CariKod OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY; ";

            var list = await _dbConnection.QueryAsync<ContactsFilters>(sql);
            return list.ToList();
        }

        public async Task Update(CariUpdate T)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.CariKod);
            prm.Add("@CariTipId", T.CariTipId);
            prm.Add("@AdSoyad", T.AdSoyad);
            prm.Add("@Mail", T.Mail);
            prm.Add("@VergiNumarası", T.VergiNumarası);
            prm.Add("@VergiDairesi", T.VergiDairesi);
            prm.Add("@ParaBirimiId", T.ParaBirimiId);
            prm.Add("@Telefon", T.Telefon);

            await _dbConnection.ExecuteAsync($"Update Cari SET CariTipId=@CariTipId,AdSoyad = @AdSoyad,Mail=@Mail,Telefon=@Telefon,VergiNumarası=@VergiNumarası,VergiDairesi=@VergiDairesi,ParaBirimiId=@ParaBirimiId where CariKod = @id", prm);

        }

        public async Task UpdateAddress(ContactsUpdateAddress T, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", T.Tip);
            prm.Add("@Adres1", T.Adres1);
            prm.Add("@Adres2", T.Adres2);
            prm.Add("@AdresSehir", T.AdresSehir);
            prm.Add("@AdresCadde", T.AdresCadde);
            prm.Add("@AdresPostaKodu", T.AdresPostaKodu);
            prm.Add("@AdresUlke", T.AdresUlke);
            prm.Add("@AdresAd", T.AdresAd);
            prm.Add("@AdresSoyisim", T.AdresSoyisim);
            prm.Add("@AdresSirket", T.AdresSirket);
            prm.Add("@AdresTelefon", T.AdresTelefon);
            prm.Add("@id", id);


            if (T.Tip == "BillingAddress")
            {
                await _dbConnection.ExecuteAsync($"Update DepoVeAdresler SET Adres1=@Adres1,Adres2=@Adres2,Sehir=@AdresSehir,Cadde=@AdresCadde,PostaKodu=@AdresPostaKodu,Ulke=@AdresUlke,Ad=@AdresAd,Soyisim=@AdresSoyisim,SirketIsmi=@AdresSirket,Telefon=@AdresTelefon where id = @id and Tip=@Tip", prm);
            }
            else if (T.Tip == "ShippingAddress")
            {
                await _dbConnection.ExecuteAsync($"Update DepoVeAdresler SET Adres1=@Adres1,Adres2=@Adres2,Sehir=@AdresSehir,Cadde=@AdresCadde,PostaKodu=@AdresPostaKodu,Ulke=@AdresUlke,Ad=@AdresAd,Soyisim=@AdresSoyisim,SirketIsmi=@AdresSirket,Telefon=@AdresTelefon where id = @id and Tip=@Tip", prm);
            }
        }
    }
}
