using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.DTO;
using static DAL.DTO.PurchaseOrderDTO;

namespace BL.Services.GeneralDefaultSettings
{
    public class GeneralDefaultSettingsControl : IGeneralDefaultSettingsControl
    {
        private readonly IDbConnection _db;

        public GeneralDefaultSettingsControl(IDbConnection db)
        {
            _db = db;
        }

        public async Task<string> Update(GeneralSettingsDTO.GeneralDefaultSettings T, int CompanyId)
        {
            var list = await _db.QueryAsync<GeneralSettingsDTO.GeneralDefaultSettings>($@"select
             (Select id  From Currency where id={T.CurrencyId})as CurrencyId,
            (Select Count(*) as varmi From Tax where CompanyId = {CompanyId} and id = {T.DefaultTaxSalesOrderId})as DefaultTaxSalesOrderId,
            (Select id  From Tax where CompanyId = {CompanyId} and id = {T.DefaultTaxPurchaseOrderId})as DefaultTaxPurchaseOrderId,
            (Select id  From Locations where CompanyId = {CompanyId} and id = {T.DefaultManufacturingLocationId})as DefaultManufacturingLocationId,
            (Select id  From Locations where CompanyId = {CompanyId} and id = {T.DefaultPurchaseLocationId})as DefaultPurchaseLocationId,
            (Select id  From Locations where CompanyId = {CompanyId} and id = {T.DefaultSalesLocationId})as DefaultSalesLocationId
            ");
            if (list.First().DefaultManufacturingLocationId == null)
            {
                return ("DefaultManufacturingLocationId,Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.DefaultManufacturingLocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                return ("Make kismina yetkiniz yok");
            }
            if (list.First().DefaultPurchaseLocationId == null)
            {
                return ("DefaultPurchaseLocationId,Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> purchaselocation = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.DefaultPurchaseLocationId} ")).ToList();
            bool? buy = purchaselocation.First().Buy;
            if (buy != true)
            {
                return ("Buy kismina yetkiniz yok");
            }
            if (list.First().DefaultSalesLocationId == null)
            {
                return ("DefaultSalesLocationId,Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> saleslocation = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.DefaultSalesLocationId} ")).ToList();
            bool? sell = saleslocation.First().Sell;
            if (sell != true)
            {
                return ("Make kismina yetkiniz yok");
            }
            if (list.First().DefaultTaxPurchaseOrderId==null)
            {
                return ("DefaultTaxPurchaseOrderId, id bulunamadı");
              }
            if (list.First().DefaultTaxSalesOrderId == null)
            {
                return ("DefaultTaxSalesOrderId, id bulunamadı");
            }
            else
            {
                return ("true");
            }
        }
    }
}
