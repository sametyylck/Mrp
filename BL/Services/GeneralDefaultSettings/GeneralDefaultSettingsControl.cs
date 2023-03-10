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

        public async Task<List<string>> Update(GeneralSettingsDTO.GeneralDefaultSettings T, int CompanyId)
        {
            List<string> hatalar = new();
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
                hatalar.Add("DefaultManufacturingLocationId,Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.DefaultManufacturingLocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                hatalar.Add("Make kismina yetkiniz yok");
            }
            if (list.First().DefaultPurchaseLocationId == null)
            {
                hatalar.Add("DefaultPurchaseLocationId,Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> purchaselocation = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.DefaultPurchaseLocationId} ")).ToList();
            bool? buy = purchaselocation.First().Buy;
            if (buy != true)
            {
                hatalar.Add("Buy kismina yetkiniz yok");
            }
            if (list.First().DefaultSalesLocationId == null)
            {
                hatalar.Add("DefaultSalesLocationId,Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> saleslocation = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.DefaultSalesLocationId} ")).ToList();
            bool? sell = saleslocation.First().Sell;
            if (sell != true)
            {
                hatalar.Add("Make kismina yetkiniz yok");
            }
            if (list.First().DefaultTaxPurchaseOrderId==null)
            {
                hatalar.Add("DefaultTaxPurchaseOrderId, id bulunamadı");
              }
            if (list.First().DefaultTaxSalesOrderId == null)
            {
                hatalar.Add("DefaultTaxSalesOrderId, id bulunamadı");
                return hatalar;

            }
            else
            {
                return hatalar;
            }
        }
    }
}
