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
        private readonly ISalesOrderRepository _salesOrder;


        public SalesOrderControl(IDbConnection db, ISalesOrderRepository salesOrder)
        {
            _db = db;
            _salesOrder = salesOrder;
        }

        public async Task<List<string>> Adress(int id,int? ContactId, int CompanyId)
        {
            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", id);
            prm.Add("@ContactId", ContactId);
            prm.Add("@CompanyId", CompanyId);
            string sqlh = $@"select BillingLocationId,ShippingLocationId from Contacts where CompanyId=@CompanyId and id=@ContactId";
            var locationS = await _db.QueryAsync<SalesOrderItemResponse>(sqlh, prm);
            prm.Add("@BillingId", locationS.First().BillingLocationId);
            prm.Add("@ShippingId", locationS.First().ShippingLocationId);
            string sqla = $@"select * from Locations where CompanyId=@CompanyId and id=@BillingId";
            var billingveri = await _db.QueryAsync<SalesOrderCloneAddress>(sqla, prm);
            string sqlb = $@"select * from Locations where CompanyId=@CompanyId and id=@ShippingId";
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
            int billingid = await _salesOrder.InsertAddress(A, CompanyId, A.ContactsId);

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
            int shipping = await _salesOrder.InsertAddress(A, CompanyId, A.ContactsId);
            prm.Add("@BillingId", billingid);
            prm.Add("@ShippingId", shipping);
            await _db.ExecuteAsync($"Update SalesOrder Set BillingAddressId=@BillingId ,ShippingAddressId=@ShippingId where CompanyId=@CompanyId and id=@id ", prm);
            return hatalar;
        }

        public async Task<List<string>> DeleteItems(SatısDeleteItems T, int CompanyId)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id From SalesOrder where CompanyId = {CompanyId} and id = {T.OrdersId} and IsActive=1)as OrdersId,
            (Select id From SalesOrderItem where CompanyId = {CompanyId} and id = {T.id} and SalesOrderId={T.OrdersId})as id");
            if (list.First().OrdersId==null)
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

        public async Task<List<string>> Insert(SatısDTO T, int CompanyId)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Count(*) as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select Tip  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactTip
            ");

            if (T.Tip!="SalesOrder")
            {
                hatalar.Add("Tip değişkeni,tip hatasi");
            }
            if (list.First().ContactTip!="Customer")
            {
                hatalar.Add("ContactId,tip hatasi");
            }
            if (list.First().LocationId==null)
            {
                hatalar.Add("Boyle bir Location yok");

            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? sell = Locaiton.First().Sell;
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

        public async Task<List<string>> InsertItem(SatısInsertItem T, int CompanyId)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id  From Tax where CompanyId = {CompanyId} and id = {T.TaxId})as TaxId,
            (Select id  From SalesOrder where CompanyId = {CompanyId} and id = {T.SalesOrderId} and IsActive=1)as OrdersId,
            (Select id  From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select id  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactId
            ");
            if (list.First().ContactId == null)
            {
                hatalar.Add("ContactId bulunamadı");
            }
            if (list.First().TaxId == null)
            {
                hatalar.Add("TaxId bulunamadı");

            }
            if (list.First().OrdersId == null)
            {
                hatalar.Add("Boyle bir id bulunamadı");

            }
            if (list.First().LocationId == null)
            {
                hatalar.Add("Boyle bir Location bulunamadı");

            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? sell = Locaiton.First().Sell;
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

        public async Task<List<string>> Make(SalesOrderMake T, int CompanyId)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
             (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id  From SalesOrder where CompanyId = {CompanyId} and id = {T.SalesOrderId} and IsActive=1 and DeliveryId=0)as OrdersId,
            (Select id  From SalesOrderItem where CompanyId = {CompanyId} and id = {T.SalesOrderItemId} and SalesOrderId={T.SalesOrderId})as id,
            (Select id  From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select id  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactId
            ");
            if (list.First().ContactId == null)
            {
                hatalar.Add("ContactId bulunamadı");
            }
            if (list.First().OrdersId == null)
            {
                hatalar.Add("Boyle bir OrderId bulunamadı");
            }
            if (list.First().id == null)
            {
                hatalar.Add("Boyle bir id bulunamadı");
            }
            if (list.First().LocationId == null)
            {
                hatalar.Add("Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? sell = Locaiton.First().Sell;
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

        public async Task<List<string>> QuotesDone(QuotesDone T, int CompanyId)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Count(*) as varmi From SalesOrder where CompanyId = {CompanyId} and id = {T.id} and IsActive=1)as OrderId,
            (Select Count(*) as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select id  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactId
            ");
            if (list.First().ContactId == null)
            {
                hatalar.Add("ContactId bulunamadı");
            }
            if (list.First().id == null)
            {
                hatalar.Add("Boyle bir id bulunamadı");

            }
            if (list.First().LocationId == null)
            {
                hatalar.Add("Boyle bir Location bulunamadı");

            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? sell = Locaiton.First().Sell;
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

        public async Task<List<string>> Update(SalesOrderUpdate T, int CompanyId)
        {
            List<string> hatalar= new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Count(*) as varmi From SalesOrder where CompanyId = {CompanyId} and id = {T.id} and IsActive=1)as OrdersId,
            (Select Count(*) as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select id  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactId
            ");
            if (list.First().ContactId == null)
            {
                hatalar.Add("ContactId bulunamadı");
            }
            if (list.First().OrdersId == null)
            {
                hatalar.Add("Boyle bir id bulunamadı");

            }
            if (list.First().LocationId == null)
            {
                hatalar.Add("Boyle bir Location bulunamadı");

            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? sell = Locaiton.First().Sell;
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

        public async Task<List<string>> UpdateItem(SatısUpdateItems T, int CompanyId)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id  From Tax where CompanyId = {CompanyId} and id = {T.TaxId})as TaxId,
            (Select id From SalesOrder where CompanyId = {CompanyId} and id = {T.id} and IsActive=1 and DeliveryId=0)as OrdersId,
            (Select id From SalesOrderItem where CompanyId = {CompanyId} and id = {T.id}  and SalesOrderId={T.SalesOrderId})as id,
            (Select id From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select id  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactId
            ");
            if (list.First().ContactId == null)
            {
                hatalar.Add("ContactId bulunamadı");
            }
            if (list.First().TaxId == null)
            {
                hatalar.Add("TaxId bulunamadı");

            }
            if (list.First().OrdersId == null)
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
            if (list.First().LocationId == null)
            {
                hatalar.Add("Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? sell = Locaiton.First().Sell;
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
