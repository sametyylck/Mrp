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

namespace DAL.Repositories
{
    public class GeneralDefaultSettingsRepository : IGeneralDefaultRepository
    {
        IDbConnection _db;

        public GeneralDefaultSettingsRepository(IDbConnection db)
        {
            _db = db;
        }

      

        public async Task Register(int id,int taxid,int locationid)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CurrencyId", 1);
            prm.Add("@DefaultSalesOrder", 14);
            prm.Add("@DefaultPurchaseOrder", 14);
            prm.Add("@DefaultTaxSalesOrderId", taxid);
            prm.Add("@DefaultTaxPurchaseOrderId", taxid);
            prm.Add("@DefaultSalesLocationId", locationid);
            prm.Add("@DefaultPurchaseLocationId", locationid);
            prm.Add("@DefaultManufacturingLocationId", locationid);
            prm.Add("@KullaniciId", id);
          await  _db.ExecuteAsync($"Insert into GenelAyarlar (ParaBirimiId,VarsayilanSatis,VarsayilanSatinAlim,VarsayilanSatisVergi,VarsayilanSatinAlimVergi,VarsayilanSatisDepo,VarsayilanSatinAlimDepo,VarsayilanUretimDepo,KullaniciId) values (@CurrencyId,@DefaultSalesOrder,@DefaultPurchaseOrder,@DefaultTaxSalesOrderId,@DefaultTaxPurchaseOrderId,@DefaultSalesLocationId,@DefaultPurchaseLocationId,@DefaultManufacturingLocationId,@KullaniciId) ",
                prm);
        }

        public async Task Update(GeneralSettingsDTO.GeneralDefaultSettings T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@CurrencyId", T.ParaBirimiId);
            prm.Add("@DefaultSalesOrder", T.VarsayilanSatis);
            prm.Add("@DefaultPurchaseOrder", T.VarsayilanSatinAlim);
            prm.Add("@DefaultTaxSalesOrderId", T.VarsayilanSatinAlimVergi);
            prm.Add("@DefaultTaxPurchaseOrderId", T.VarsayilanSatinAlim);
            prm.Add("@DefaultSalesLocationId", T.VarsayilanSatisDepo);
            prm.Add("@DefaultPurchaseLocationId", T.VarsayilanSatinAlimDepo);
            prm.Add("@DefaultManufacturingLocationId", T.VarsayilanUretimDepo);
           await _db.ExecuteAsync($"Update GenelAyarlar SET ParaBirimiId = @CurrencyId ,VarsayilanSatis = @DefaultSalesOrder , VarsayilanSatinAlim = @DefaultPurchaseOrder , VarsayilanSatisVergi = @DefaultTaxSalesOrderId , VarsayilanSatinAlimVergi = @DefaultTaxPurchaseOrderId , VarsayilanSatisDepo = @DefaultSalesLocationId , VarsayilanSatinAlimDepo = @DefaultPurchaseLocationId , VarsayilanUretimDepo = @DefaultManufacturingLocationId where id = @id", prm);
        }


      public async Task<IEnumerable<GeneralSettingsDTO.DefaultSettingList>> List(int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
         
            var list = await _db.QueryAsync<GeneralSettingsDTO.DefaultSettingList>($"select ds.id,ds.ParaBirimiId, c.Isim as ParaBirimiIsmi,ds.VarsayilanSatis , ds.VarsayilanSatinAlim ,ds.VarsayilanSatisVergi, ts.VergiIsim SatisVergiIsmi, ts.VergiDegeri SatisVergiDegeri,ds.VarsayilanSatinAlimVergi SatinAlimVergiId, ts.VergiIsim SatinAlimVergiIsmi, ts.VergiDegeri SatinAlimVergiDegeri,ds.VarsayilanSatisDepo SatisDepoId, ls.Isim SatisDepoIsim,ds.VarsayilanSatinAlimDepo SatinAlimDepoId, lp.Isim SatinAlimDepoIsim,ds.VarsayilanUretimDepo UretimDepoId, lm.Isim UretimDepoIsim from GenelAyarlar ds inner join ParaBirimleri c on ParaBirimiId = c.id inner join Vergi ts on VarsayilanSatisVergi = ts.id inner join Vergi tp on VarsayilanSatinAlimVergi = tp.id inner join DepoVeAdresler ls on VarsayilanSatisDepo = ls.id inner join DepoVeAdresler lp on VarsayilanSatinAlimDepo = lp.id inner join DepoVeAdresler lm on VarsayilanUretimDepo = lm.id", prm);
            return list.ToList();
        }
    }
}
