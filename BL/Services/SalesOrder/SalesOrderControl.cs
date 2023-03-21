using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockListDTO;

namespace BL.Services.SalesOrder
{
    public class SalesOrderControl : ISalesOrderControl
    {
        private readonly IDbConnection _db;
        private readonly ISatısRepository _salesOrder;


        public SalesOrderControl(IDbConnection db, ISatısRepository salesOrder)
        {
            _db = db;
            _salesOrder = salesOrder;
        }

        public async Task<List<string>> Adress(int id,int? ContactId)
        {
            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@ContactId", ContactId);
            string sqlh = $@"select FaturaAdresId,KargoAdresId from Cari where  CariKod=@ContactId";
            var locationS = await _db.QueryAsync<SalesOrderItemResponse>(sqlh, prm);
            prm.Add("@BillingId", locationS.First().FaturaAdresId);
            prm.Add("@ShippingId", locationS.First().KargoAdresId);
            string sqla = $@"select * from DepoVeAdresler where  id=@BillingId";
            var billingveri = await _db.QueryAsync<SalesOrderCloneAddress>(sqla, prm);
            string sqlb = $@"select * from DepoVeAdresler where id=@ShippingId";
            var shippingveri = await _db.QueryAsync<SalesOrderCloneAddress>(sqlb, prm);
            SalesOrderCloneAddress A = new SalesOrderCloneAddress();
            foreach (var item in billingveri)
            {
                A.FirstName = item.FirstName;
                A.LastName = item.LastName;
                A.CompanyName = item.CompanyName;
                A.Phone = item.Phone;
                A.AddressLine2 = item.AddressLine2;
                A.AddressLine1 = item.AddressLine1;
                A.CityTown = item.CityTown;
                A.StateRegion = item.StateRegion;
                A.ZipPostal = item.ZipPostal;
                A.Country = item.Country;
                A.Tip = "BillingAddress";

            }
            int billingid = await _salesOrder.InsertAddress(A, A.ContactsId);

            foreach (var item in shippingveri)
            {
                A.FirstName = item.FirstName;
                A.LastName = item.LastName;
                A.CompanyName = item.CompanyName;
                A.Phone = item.Phone;
                A.AddressLine2 = item.AddressLine2;
                A.AddressLine1 = item.AddressLine1;
                A.CityTown = item.CityTown;
                A.StateRegion = item.StateRegion;
                A.ZipPostal = item.ZipPostal;
                A.Country = item.Country;
                A.Tip = "ShippingAddress";
            }
            int shipping = await _salesOrder.InsertAddress(A, A.ContactsId);
            prm.Add("@BillingId", billingid);
            prm.Add("@ShippingId", shipping);
            await _db.ExecuteAsync($"Update Satis Set FaturaAdresiId=@BillingId ,KargoAdresiId=@ShippingId where id=@id ", prm);
            return hatalar;
        }

        public async Task<List<string>> DeleteItems(SatısDeleteItems T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Tip  From Urunler where  id={T.StokId})as Tip,
            (Select id From Satis where  id = {T.OrdersId} and Aktif=1)as SatisId,
            (Select id From SatisDetay where id = {T.id} and SatisId={T.OrdersId})as id");
            if (list.First().SatisId==null)
            {
                string hata = "OrderId bulunamıyor";
                hatalar.Add(hata);
            }
            if (list.First().id==null)
            {
                string hata = "Boyler bir id ve OrderId eslesmesi yok";
                hatalar.Add(hata);
            }
            if (list.First().Tip == "Product" || list.First().Tip == "SemiProduct")
            {
                return hatalar;
            }
            else
            {
                string hata = "ItemId,tip hatasi";
                hatalar.Add(hata);
                return hatalar;
            }
        }

        public async Task<List<string>> Insert(SatısDTO T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Count(*) as varmi From DepoVeAdresler where  id = {T.DepoId})as DepoId
            ");

            if (T.Tip!="Satis")
            {
                hatalar.Add("Tip değişkeni,tip hatasi");
            }
            if (list.First().DepoId ==null)
            {
                hatalar.Add("Boyle bir Location yok");

            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where  id={T.DepoId} ")).ToList();
            bool? sell = Locaiton.First().Satis;
            if (sell != true)
            {
                hatalar.Add("Satis kismina yetkiniz yok");
                return hatalar;


            }
            else
            {
                return hatalar;
            }

        }

        public async Task<List<string>> InsertItem(SatısInsertItem T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Tip  From Urunler where id={T.StokId})as Tip,
            (Select id  From Vergi where  id = {T.VergiId})as VergiId,
            (Select id  From Satis where  id = {T.SatisId} and Aktif=1)as SatisId,
            (Select id  From DepoVeAdresler where  id = {T.DepoId})as DepoId,
            (Select CariKod  From Cari where  CariKod = {T.CariId})as CariId
            ");
            if (list.First().CariId == null)
            {
                hatalar.Add("ContactId bulunamadı");
            }
            if (list.First().VergiId == null)
            {
                hatalar.Add("TaxId bulunamadı");

            }
            if (list.First().SatisId == null)
            {
                hatalar.Add("Boyle bir id bulunamadı");

            }
            if (list.First().DepoId == null)
            {
                hatalar.Add("Boyle bir Location bulunamadı");

            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where id={T.DepoId} ")).ToList();
            bool? sell = Locaiton.First().Satis;
             if (sell != true)
            {
                hatalar.Add("Satis kismina yetkiniz yok");

            }
            if (list.First().Tip == "Product" || list.First().Tip == "SemiProduct")
            {

                return hatalar;
            }
            else
            {
                hatalar.Add("ItemId,tip hatası");
                return hatalar;
            }
        }

        public async Task<List<string>> Make(SalesOrderMake T)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
             (Select Tip  From Urunler where  id={T.StokId})as Tip,
            (Select id  From Satis where  id = {T.SatisId} and Aktif=1 and DurumBelirteci=0)as SatisId,
            (Select id  From SatisDetay where  id = {T.SatisDetayId} and SatisId={T.SatisId})as id,
            (Select id  From DepoVeAdresler where id = {T.DepoId})as DepoId,
            (Select CariKod  From Cari where CariKod = {T.CariId})as CariId
            ");
            if (list.First().CariId == null)
            {
                hatalar.Add("ContactId bulunamadı");
            }
            if (list.First().SatisId == null)
            {
                hatalar.Add("Boyle bir OrderId bulunamadı");
            }
            if (list.First().id == null)
            {
                hatalar.Add("Boyle bir id bulunamadı");
            }
            if (list.First().DepoId == null)
            {
                hatalar.Add("Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where id={T.DepoId} ")).ToList();
            bool? sell = Locaiton.First().Satis;
            if (sell != true)
            {
                hatalar.Add("Satis kismina yetkiniz yok");
            }
            if (list.First().Tip == "Product" || list.First().Tip == "SemiProduct")
            {

                return hatalar;
            }
            else
            {
                hatalar.Add("ItemId,tip hatası");

                return hatalar;
            }

        }

        public async Task<List<string>> QuotesDone(QuotesDone T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Count(*) as varmi From Satis where  id = {T.id} and IsActive=1)as id,
            (Select Count(*) as varmi From DepoVeAdresler where id = {T.DepoId})as DepoId,
            (Select CariKod  From Cari where  CariKod = {T.CariId})as CariId
            ");
            if (list.First().CariId == null)
            {
                hatalar.Add("ContactId bulunamadı");
            }
            if (list.First().id == null)
            {
                hatalar.Add("Boyle bir id bulunamadı");

            }
            if (list.First().DepoId == null)
            {
                hatalar.Add("Boyle bir Location bulunamadı");

            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where id={T.DepoId} ")).ToList();
            bool? sell = Locaiton.First().Satis;
            if (sell != true)
            {
                hatalar.Add("Satis kismina yetkiniz yok");
                return hatalar;

            }
            else
            {
                return hatalar;
            }
        }

        public async Task<List<string>> Update(SalesOrderUpdate T)
        {
            List<string> hatalar= new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Count(*) as varmi From Satis where id = {T.id} and Aktif=1)as SatisId,
            (Select Count(*) as varmi From DepoVeAdresler where  id = {T.DepoId})as DepoId,
            (Select CariKod  From Cari where  CariKod = {T.CariId})as CariId
            ");
            if (list.First().CariId == null)
            {
                hatalar.Add("ContactId bulunamadı");
            }
            if (list.First().SatisId == null)
            {
                hatalar.Add("Boyle bir id bulunamadı");

            }
            if (list.First().DepoId == null)
            {
                hatalar.Add("Boyle bir Location bulunamadı");

            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where id={T.DepoId} ")).ToList();
            bool? sell = Locaiton.First().Satis;
            if (sell != true)
            {
                hatalar.Add("Satis kismina yetkiniz yok");
                return hatalar;

            }
            else
            {
                return hatalar;
            }
        }

        public async Task<List<string>> UpdateItem(SatısUpdateItems T)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Tip  From Urunler where  id={T.StokId})as Tip,
            (Select id  From Vergi where  id = {T.VergiId})as VergiId,
            (Select id From Satis where  id = {T.SatisId} and Aktif=1 and DurumBelirteci=0)as SatisId,
            (Select id From SatisDetay where  id = {T.id}  and SatisId={T.SatisId})as id,
            (Select id From DepoVeAdresler where  id = {T.DepoId})as DepoId,
            (Select CariKod  From Cari where  CariKod = {T.CariId})as CariId
            ");
            if (list.First().CariId == null)
            {
                hatalar.Add("ContactId bulunamadı");
            }
            if (list.First().VergiId == null)
            {
                hatalar.Add("TaxId bulunamadı");

            }
            if (list.First().SatisId == null)
            {
                hatalar.Add("Boyle bir id bulunamadı");

            }
            if (list.First().id == null)
            {
                hatalar.Add("Boyle bir OrdersItem bulunamadı");

            }
            if (list.First().id == null)
            {
                hatalar.Add("Boyle bir id bulunamadı");

            }
            if (list.First().DepoId == null)
            {
                hatalar.Add("Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where  id={T.DepoId} ")).ToList();
            bool? sell = Locaiton.First().Satis;
            if (sell != true)
            {
                hatalar.Add("Satis kismina yetkiniz yok");
            }
            if (list.First().Tip == "Product" || list.First().Tip == "SemiProduct")
            {
                return hatalar;
            }
            else
            {
                hatalar.Add("ItemId,tip hatası");
                return hatalar;
            }

        }

   
    }
}
