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

        public async Task<List<string>> Update(GeneralSettingsDTO.GeneralDefaultSettings T)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<GeneralSettingsDTO.GeneralDefaultSettings>($@"select
             (Select id  From ParaBirimleri where id={T.ParaBirimiId})as CurrencyId,
            (Select Count(*) as varmi From Vergi where  id = {T.VarsayilanSatisVergi})as VarsayilanSatisVergi,
            (Select id  From Vergi where id = {T.VarsayilanSatinAlimVergi})as VarsayilanSatinAlimVergi,
            (Select id  From Locations where  id = {T.VarsayilanUretimDepo})as VarsayilanUretimDepo,
            (Select id  From Locations where  id = {T.VarsayilanSatinAlimDepo})as VarsayilanSatinAlimDepo,
            (Select id  From Locations where  id = {T.VarsayilanSatisDepo})as VarsayilanSatisDepo
            ");
            if (list.First().VarsayilanUretimDepo == null)
            {
                hatalar.Add("DefaultManufacturingLocationId,Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where id={T.VarsayilanUretimDepo} ")).ToList();
            bool? make = Locaiton.First().Uretim;
            if (make != true)
            {
                hatalar.Add("Make kismina yetkiniz yok");
            }
            if (list.First().VarsayilanSatinAlimDepo == null)
            {
                hatalar.Add("DefaultPurchaseLocationId,Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> purchaselocation = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where  id={T.VarsayilanSatinAlimDepo} ")).ToList();
            bool? buy = purchaselocation.First().SatinAlma;
            if (buy != true)
            {
                hatalar.Add("Buy kismina yetkiniz yok");
            }
            if (list.First().VarsayilanSatisDepo == null)
            {
                hatalar.Add("DefaultSalesLocationId,Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> saleslocation = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where  id={T.VarsayilanSatisDepo} ")).ToList();
            bool? sell = saleslocation.First().Satis;
            if (sell != true)
            {
                hatalar.Add("Make kismina yetkiniz yok");
            }
            if (list.First().VarsayilanSatinAlim==null)
            {
                hatalar.Add("DefaultTaxPurchaseOrderId, id bulunamadı");
              }
            if (list.First().VarsayilanSatinAlimVergi == null)
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
