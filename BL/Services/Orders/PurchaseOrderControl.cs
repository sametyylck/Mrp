using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ContactDTO;
using static DAL.DTO.PurchaseOrderDTO;

namespace BL.Services.Orders
{
    public class PurchaseOrderControl : IPurchaseOrderControl
    {
        private readonly IDbConnection _db;

        public PurchaseOrderControl(IDbConnection db)
        {
            _db = db;
        }


       public async Task<List<string>>  Delete(Delete T, int CompanyId)
        {
            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Tip", T.Tip);
            prm.Add("@CompanyId", CompanyId);
            string sql = $"Select id From Orders where CompanyId = @CompanyId and id  = @id";
            var idcontrol =await _db.QueryAsync<Contacts>(sql, prm);
            if (idcontrol.Count() == 0)
            {
                hatalar.Add("id bulunamadı");
            }

            if (T.Tip == "PurchaseOrder")
            {
                return hatalar;
            }
            else
            {
                hatalar.Add("Tip degiskeni,tip hatasi");
                return hatalar;


            }

        }

       public async Task<List<string>> DeleteItem(DeleteItems T, int CompanyId)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id as varmi From Orders where CompanyId = {CompanyId} and id = {T.OrdersId} and IsActive=1)as OrdersId,
            (Select id From OrdersItem where CompanyId = {CompanyId} and id = {T.id} and OrdersId={T.OrdersId})as id");

         
            if (list.First().OrdersId == null)
            {
                hatalar.Add("OrdersId bulunamadi");
            }
            if (list.First().id== null)
            {
                hatalar.Add("Böyle Bir id ve  OrdersId eslesmesi Yok");

            }
            if (list.First().Tip == "Material")
            {
                return hatalar;
            }
            else
            {
                hatalar.Add("Make tip hatası");

                return hatalar;
            }
           
        }

       public async Task<List<string>> Insert(PurchaseOrderInsert T, int CompanyId)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id  From Measure where CompanyId = {CompanyId} and id = {T.MeasureId})as MeasureId,
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id  From Tax where CompanyId = {CompanyId} and id = {T.TaxId})as TaxId,
            (Select  id as OrdersId From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.ManufacturingOrderId} and IsActive=1 and Status!=3)as OrdersId,
            (Select id From ManufacturingOrderItems where CompanyId = {CompanyId} and id = {T.ManufacturingOrderItemId} and OrderId={T.ManufacturingOrderId})as id,
             (select id From SalesOrder where CompanyId = {CompanyId} and id = {T.SalesOrderId} and DeliveryId!=2 and  DeliveryId!=4)as SalesOrderId, 
            (Select id From SalesOrderItem where CompanyId = {CompanyId} and id = {T.SalesOrderItemId} and SalesOrderId={T.SalesOrderId})as SalesOrderItemId,
            (Select id as LocationId From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select Tip  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactTip
            ");

            if (T.ManufacturingOrderId!=0 && T.ManufacturingOrderItemId !=0)
            {
                if (list.First().OrdersId == null)
                {
                    hatalar.Add("Böyle Bir id Yok");
                }
                if (list.First().id == null)
                {
                    hatalar.Add("id ve OrderId eşleşmiyor.İki id'nin ilişkisi yok.");
                }
            }

            if (T.SalesOrderId != 0 && T.SalesOrderItemId != 0)
            {
                if (list.First().SalesOrderId == null)
                {
                    hatalar.Add("SalesOrderid bulunamadı.");
                }

                if (list.First().SalesOrderItemId == null)
                {
                    hatalar.Add("SalesOrderid ve SalesOrderItemid eşleşmesi hatalı.");
                }
            }
            if (T.Tip != "PurchaseOrder")
            {
                hatalar.Add("Tip değişkeni,tip hatasi");
            }
            if (list.First().LocationId==null)
            {
                hatalar.Add("Böyle Bir Lokasyon Yok");
            }
            var Locaiton = await _db.QueryFirstAsync<bool>($"select Make from Locations where CompanyId={CompanyId} and id={T.LocationId} ");
            if (Locaiton != true)
            {
                hatalar.Add("Uretim kismina yetkiniz yok");

            }
            if (list.First().ContactTip != "Supplier")
            {
                hatalar.Add("ContactId,Böyle Bir Tedarikci Yok");

            }
            if (list.First().MeasureId==null)
            {
                hatalar.Add("MeasureId bulunamadı");
            }
            if (list.First().TaxId== null)
            {
                hatalar.Add("TaxId bulunamadi");
            }
            if (list.First().Tip== "Material" || list.First().Tip == "SemiProduct")
            {
                return hatalar;

            }
            else
            {
                hatalar.Add("ItemId,tip hatası");
                return hatalar;
            }
        

        }

        public async Task<List<string>> InsertItem(PurchaseOrderInsertItem T, int CompanyId)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id  From Measure where CompanyId = {CompanyId} and id = {T.MeasureId})as MeasureId,
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id  From Tax where CompanyId = {CompanyId} and id = {T.TaxId})as TaxId,
            (Select id as varmi From Orders where CompanyId = {CompanyId} and id = {T.OrderId} and IsActive=1 and DeliveryId=1)as OrdersId");



            if (list.First().MeasureId==null)
            {
                hatalar.Add("MeasureId bulunamadı");

            }
            if (list.First().Tip != "Material")
            {
                hatalar.Add("ItemId tip hatası");

            }
            if (list.First().OrdersId== null)
            {
                hatalar.Add("Böyle Bir OrdersId Yok");
            }
            if (list.First().TaxId == null)
            {
                hatalar.Add("TaxId bulunamadi");
                return hatalar;

            }
            else
            {
                return hatalar;
            }
        }

       public async Task<List<string>> Update(PurchaseOrderUpdate T, int CompanyId)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select Tip  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as Tip
        ");
            if (list.First().LocationId==null)
            {
                hatalar.Add("Böyle Bir Lokasyon Yok");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                hatalar.Add("Uretim kismina yetkiniz yok");
            }
            if (list.First().Tip != "Supplier")
            {
                hatalar.Add("ContactId,Böyle Bir Tedarikci Yok");
                return hatalar;

            }
            else
            {
                return hatalar;
            }
        }

      public async Task<List<string>> UpdatePurchaseItem(PurchaseItem T, int CompanyId)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id  From Measure where CompanyId = {CompanyId} and id = {T.MeasureId})as MeasureId,
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id  From Tax where CompanyId = {CompanyId} and id = {T.TaxId})as TaxId,
            (Select id as varmi From Orders where CompanyId = {CompanyId} and id = {T.OrdersId} and IsActive=1 and DeliveryId=1)as OrdersId,
            (Select id as varmi From OrdersItem where CompanyId = {CompanyId} and id ={T.id} and OrdersId={T.OrdersId})as id");
            if (list.First().MeasureId==null)
            {
                hatalar.Add("MeasureId bulunamadı");
            }
            if (list.First().Tip != "Material")
            {
                hatalar.Add("ItemId tip hatası");
            }
            if (list.First().TaxId == null)
            {
                hatalar.Add("TaxId bulunamadi");
            }
            if (list.First().OrdersId == null)
            {
                hatalar.Add("Böyle Bir OrdersId Yok");
            }
            if (list.First().id==null)
            {

                hatalar.Add("Böyle Bir eslesme yok.Id ve OrdersId Yok");
                return hatalar;
            }
            else
            {
                return hatalar;
            }
        }

    }
}
