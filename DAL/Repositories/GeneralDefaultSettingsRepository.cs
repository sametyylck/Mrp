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
            prm.Add("@CompanyId", id);
          await  _db.ExecuteAsync($"Insert into GeneralDefaultSettings (CurrencyId,DefaultSalesOrder,DefaultPurchaseOrder,DefaultTaxSalesOrderId,DefaultTaxPurchaseOrderId,DefaultSalesLocationId,DefaultPurchaseLocationId,DefaultManufacturingLocationId,CompanyId) values (@CurrencyId,@DefaultSalesOrder,@DefaultPurchaseOrder,@DefaultTaxSalesOrderId,@DefaultTaxPurchaseOrderId,@DefaultSalesLocationId,@DefaultPurchaseLocationId,@DefaultManufacturingLocationId,@CompanyId) ",
                prm);
        }

        public async Task Update(GeneralSettingsDTO.GeneralDefaultSettings T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@CurrencyId", T.CurrencyId);
            prm.Add("@DefaultSalesOrder", T.DefaultSalesOrder);
            prm.Add("@DefaultPurchaseOrder", T.DefaultPurchaseOrder);
            prm.Add("@DefaultTaxSalesOrderId", T.DefaultTaxSalesOrderId);
            prm.Add("@DefaultTaxPurchaseOrderId", T.DefaultTaxPurchaseOrderId);
            prm.Add("@DefaultSalesLocationId", T.DefaultSalesLocationId);
            prm.Add("@DefaultPurchaseLocationId", T.DefaultPurchaseLocationId);
            prm.Add("@DefaultManufacturingLocationId", T.DefaultManufacturingLocationId);
            prm.Add("@CompanyId", CompanyId);

           await _db.ExecuteAsync($"Update GeneralDefaultSettings SET CurrencyId = @CurrencyId ,DefaultSalesOrder = @DefaultSalesOrder , DefaultPurchaseOrder = @DefaultPurchaseOrder , DefaultTaxSalesOrderId = @DefaultTaxSalesOrderId , DefaultTaxPurchaseOrderId = @DefaultTaxPurchaseOrderId , DefaultSalesLocationId = @DefaultSalesLocationId , DefaultPurchaseLocationId = @DefaultPurchaseLocationId , DefaultManufacturingLocationId = @DefaultManufacturingLocationId where id = @id and CompanyId = @CompanyId ", prm);
        }

      public async Task<IEnumerable<GeneralSettingsDTO.DefaultSettingList>> List(int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();

            prm.Add("@CompanyId", CompanyId);
         
            var list = await _db.QueryAsync<GeneralSettingsDTO.DefaultSettingList>($"select ds.id,ds.CurrencyId, c.Name CurrencyName,ds.DefaultSalesOrder SalesOrderDate, ds.DefaultPurchaseOrder PurchaseOrderDate,ds.DefaultTaxSalesOrderId TaxSalesOrderId, ts.TaxName TaxSalesOrderName, ts.Rate TaxSalesRate,ds.DefaultTaxPurchaseOrderId TaxPurchaseOrderId, ts.TaxName TaxPurchaseOrderName, ts.Rate TaxPurchaseRate,ds.DefaultSalesLocationId SalesLocationId, ls.LocationName SalesLocationName,ds.DefaultPurchaseLocationId PurchaseLocationId, lp.LocationName PurchaseLocationName,ds.DefaultManufacturingLocationId ManufacturingLocationId, lm.LocationName ManufacturingLocationName, ds.CompanyId from GeneralDefaultSettings ds inner join Currency c on CurrencyId = c.id inner join Tax ts on DefaultTaxSalesOrderId = ts.id inner join Tax tp on DefaultTaxPurchaseOrderId = tp.id inner join Locations ls on DefaultSalesLocationId = ls.id inner join Locations lp on DefaultPurchaseLocationId = lp.id inner join Locations lm on DefaultManufacturingLocationId = lm.id where ds.CompanyId = @CompanyId ", prm);
            return list.ToList();
        }
    }
}
