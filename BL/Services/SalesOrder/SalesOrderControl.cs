using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.SalesOrderDTO;

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

        public async Task<string> Adress(int id,int? ContactId, int CompanyId)
        {
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
            await _db.ExecuteAsync($"Update Orders Set BillingAddressId=@BillingId ,ShippingAddressId=@ShippingId where CompanyId=@CompanyId and id=@id ", prm);
            return ("true");
        }

        public async Task<string> DeleteItems(SalesDeleteItems T, int CompanyId)
        {
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id From Orders where CompanyId = {CompanyId} and id = {T.OrdersId} and IsActive=1)as OrdersId,
            (Select id From OrdersItem where CompanyId = {CompanyId} and id = {T.id} and OrdersId={T.OrdersId})as id");
            if (list.First().OrdersId==null)
            {
                return ("OrderId bulunamıyor");
            }
            if (list.First().id==null)
            {
                return ("Boyler bir id ve OrderId eslesmesi yok");
            }
            if (list.First().Tip == "Product" || list.First().Tip == "SemiProduct")
            {
                return ("true");
            }
            else
            {
                return ("ItemId,tip hatasi");
            }
        }

        public async Task<string> Insert(SalesOrderDTO.SalesOrder T, int CompanyId)
        {
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Count(*) as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select Tip  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactTip
            ");

            if (T.Tip!="SalesOrder")
            {
                return("Tip değişkeni,tip hatasi");
            }
            if (list.First().ContactTip!="Customer")
            {
                return ("ContactId,tip hatasi");
            }
            if (list.First().LocationId==null)
            {
                return ("Boyle bir Location yok.");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? sell = Locaiton.First().Sell;
            if (sell != true)
            {
                return ("Satis kismina yetkiniz yok");
            }
            else
            {
                return ("true");
            }

        }

        public async Task<string> InsertItem(SalesOrderItem T, int CompanyId)
        {
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id  From Tax where CompanyId = {CompanyId} and id = {T.TaxId})as TaxId,
            (Select id  From Orders where CompanyId = {CompanyId} and id = {T.id} and IsActive=1)as OrdersId,
            (Select id  From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select id  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactId
            ");
            if (list.First().ContactId==null)
            {
                return ("ContactId bulunamadı");
            }
            if (list.First().TaxId==null)
            {
                return ("TaxId bulunamadı");
            }
            if (list.First().OrdersId==null)
            {
                return ("Boyle bir id bulunamadı");
            }
            if (list.First().LocationId==null)
            {
                return ("Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? sell = Locaiton.First().Sell;
            if (sell != true)
            {
                return ("Satis kismina yetkiniz yok");
            }
            if (list.First().Tip == "Product" || list.First().Tip == "SemiProduct")
            {
                return ("true");
            }
            else
            {
                return ("ItemId,tip hatası");
            }
        }

        public async Task<string> Make(ManufacturingOrderDTO.SalesOrderMake T, int CompanyId)
        {
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
             (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id  From Orders where CompanyId = {CompanyId} and id = {T.SalesOrderId} and IsActive=1 and DeliveryId=0)as OrdersId,
            (Select id  From OrdersItem where CompanyId = {CompanyId} and id = {T.SalesOrderItemId} and OrdersId={T.SalesOrderId})as id,
            (Select id  From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select id  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactId
            ");
            if (list.First().ContactId == null)
            {
                return ("ContactId bulunamadı");
            }
            if (list.First().OrdersId==null)
            {
                return ("Boyle bir OrderId bulunamadı");
            }
            if (list.First().id == null)
            {
                return ("Boyle bir id bulunamadı");
            }
            if (list.First().LocationId == null)
            {
                return ("Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? sell = Locaiton.First().Sell;
            if (sell != true)
            {
                return ("Satis kismina yetkiniz yok");
            }
            if (list.First().Tip == "Product" || list.First().Tip == "SemiProduct")
            {
                return ("true");
            }
            else
            {
                return ("ItemId,tip hatası");
            }

        }

        public async Task<string> QuotesDone(SalesOrderDTO.Quotess T, int CompanyId)
        {
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
             (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select Count(*) as varmi From Orders where CompanyId = {CompanyId} and id = {T.id} and IsActive=1)as OrderId,
            (Select Count(*) as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select id  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactId
            ");
            if (list.First().ContactId == null)
            {
                return ("ContactId bulunamadı");
            }
            if (list.First().Tip!="Product" || list.First().Tip!="SemiProduct")
            {
                return ("ItemId,tip hatası");
            }
            if (list.First().id == null)
            {
                return ("Boyle bir id bulunamadı");
            }
            if (list.First().LocationId == null)
            {
                return ("Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? sell = Locaiton.First().Sell;
            if (sell != true)
            {
                return ("Satis kismina yetkiniz yok");
            }
            else
            {
                return ("true");
            }
        }

        public async Task<string> Update(SalesOrderUpdate T, int CompanyId)
        {
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Count(*) as varmi From Orders where CompanyId = {CompanyId} and id = {T.id} and IsActive=1)as OrdersId,
            (Select Count(*) as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select id  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactId
            ");
            if (list.First().ContactId == null)
            {
                return ("ContactId bulunamadı");
            }
            if (list.First().OrdersId == null)
            {
                return ("Boyle bir id bulunamadı");
            }
            if (list.First().LocationId == null)
            {
                return ("Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? sell = Locaiton.First().Sell;
            if (sell != true)
            {
                return ("Satis kismina yetkiniz yok");
            }
            else
            {
                return ("true");
            }
        }

        public async Task<string> UpdateItem(SalesOrderUpdateItems T, int CompanyId)
        {
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id  From Tax where CompanyId = {CompanyId} and id = {T.TaxId})as TaxId,
            (Select id From Orders where CompanyId = {CompanyId} and id = {T.id} and IsActive=1 and DeliveryId=0)as OrdersId,
            (Select id From OrdersItem where CompanyId = {CompanyId} and id = {T.OrderItemId}  and OrdersId={T.id})as id,
            (Select id From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select id  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactId
            ");
            if (list.First().ContactId == null)
            {
                return ("ContactId bulunamadı");
            }
            if (list.First().TaxId == null)
            {
                return ("TaxId bulunamadı");
            }
            if (list.First().OrdersId == null)
            {
                return ("Boyle bir id bulunamadı");
            }
            if (list.First().id == null)
            {
                return ("Boyle bir OrdersItem bulunamadı");
            }
            if (list.First().id == null)
            {
                return ("Boyle bir id bulunamadı");
            }
            if (list.First().LocationId == null)
            {
                return ("Boyle bir Location bulunamadı");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? sell = Locaiton.First().Sell;
            if (sell != true)
            {
                return ("Satis kismina yetkiniz yok");
            }
            if (list.First().Tip == "Product" || list.First().Tip == "SemiProduct")
            {
                return ("true");
            }
            else
            {
                return ("ItemId,tip hatası");
            }

        }
    }
}
