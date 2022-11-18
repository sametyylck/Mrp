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


       public async Task<string>  Delete(Delete T, int CompanyId)
        {

            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Tip", T.Tip);
            prm.Add("@CompanyId", CompanyId);
            string sql = $"Select id From Orders where CompanyId = @CompanyId and id  = @id";
            var idcontrol =await _db.QueryAsync<Contacts>(sql, prm);
            if (idcontrol.Count() == 0)
            {
                return("id bulunamadı"); ;
            }

            if (T.Tip == "PurchaseOrder")
            {
                return("true");
            }
            else
            {
                return("Tip degiskeni,tip hatasi");
            }
        
        }

       public async Task<string> DeleteItem(DeleteItems T, int CompanyId)
        {

            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id as varmi From Orders where CompanyId = {CompanyId} and id = {T.OrdersId} and IsActive=1)as OrdersId,
            (Select id From OrdersItem where CompanyId = {CompanyId} and id = {T.id} and OrdersId={T.OrdersId})as id");

         
            if (list.First().OrdersId == null)
            {
                return("OrdersId bulunamadi");
            }
            if (list.First().id== null)
            {
                return ("Böyle Bir id ve  OrdersId eslesmesi Yok");
            }
            if (list.First().Tip == "Material")
            {
                return ("true");
            }
            else
            {
                return ("Make tip hatası");
            }
           
        }

       public async Task<string> Insert(PurchaseOrderInsert T, int CompanyId)
        {

            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id  From Measure where CompanyId = {CompanyId} and id = {T.MeasureId})as MeasureId,
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id  From Tax where CompanyId = {CompanyId} and id = {T.TaxId})as TaxId,
            (Select  id as varmi From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.ManufacturingOrderId} and IsActive=1 and Status!=3)as OrderId,
            (Select id From ManufacturingOrderItems where CompanyId = {CompanyId} and id = {T.ManufacturingOrderItemId} and OrderId={T.ManufacturingOrderId})as id,
             (select id From Orders where CompanyId = {CompanyId} and id = {T.SalesOrderId} and DeliveryId!=2 and  DeliveryId!=4)as SalesOrderId, 
            (Select id From OrdersItem where CompanyId = {CompanyId} and id = {T.SalesOrderItemId} and OrdersId={T.SalesOrderId})as SalesOrderItemId,
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select Tip  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as ContactTip
            ");

            if (T.ManufacturingOrderId!=0 && T.ManufacturingOrderItemId !=0)
            {
                if (list.First().OrdersId == null)
                {
                    return ("Böyle Bir id Yok");
                }
                if (list.First().id == null)
                {
                    return ("id ve OrderId eşleşmiyor.İki id'nin ilişkisi yok.");
                }
            }

            if (T.SalesOrderId != 0 && T.SalesOrderItemId != 0)
            {
                if (list.First().SalesOrderId == null)
                {
                    return ("SalesOrderid bulunamadı.");
                }

                if (list.First().SalesOrderItemId == null)
                {
                    return ("SalesOrderid ve SalesOrderItemid eşleşmesi hatalı.");
                }
            }
            if (T.Tip != "PurchaseOrder")
            {
                return ("Tip değişkeni,tip hatasi");
            }
            if (list.First().LocationId==null)
            {
                return ("Böyle Bir Lokasyon Yok");
            }
            var Locaiton = await _db.QueryFirstAsync<bool>($"select Make from Locations where CompanyId={CompanyId} and id={T.LocationId} ");
            if (Locaiton != true)
            {
                return ("Uretim kismina yetkiniz yok");
            }
            if (list.First().ContactTip != "Supplier")
            {
                return ("ContactId,Böyle Bir Tedarikci Yok");
            }
            if (list.First().MeasureId==null)
            {
                return ("MeasureId bulunamadı");
            }
            if (list.First().TaxId== null)
            {
                return ("TaxId bulunamadi");
            }
            if (list.First().Tip!= "Material")
            {
                return ("ItemId,tip hatası");

            }
            else
            {
                return ("true");
            }


        }

        public async Task<string> InsertItem(PurchaseOrderInsertItem T, int CompanyId)
        {
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id  From Measure where CompanyId = {CompanyId} and id = {T.MeasureId})as MeasureId,
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id  From Tax where CompanyId = {CompanyId} and id = {T.TaxId})as TaxId,
            (Select id as varmi From Orders where CompanyId = {CompanyId} and id = {T.OrderId} and IsActive=1 and DeliveryId=1)as OrdersId");



            if (list.First().MeasureId==null)
            {
                return ("MeasureId bulunamadı");
            }
            if (list.First().Tip != "Material")
            {
                return ("ItemId tip hatası");
            }
            if (list.First().OrdersId== null)
            {
                return ("Böyle Bir OrdersId Yok");
            }
            if (list.First().TaxId == null)
            {
                return ("TaxId bulunamadi");
            }
            else
            {
                return ("true");
            }
        }

       public async Task<string> Update(PurchaseOrderUpdate T, int CompanyId)
        {
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId})as LocationId,
            (Select Tip  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId})as Tip
        ");
            if (list.First().LocationId==null)
            {
                return ("Böyle Bir Lokasyon Yok");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                return ("Uretim kismina yetkiniz yok");
            }
            if (list.First().Tip != "Supplier")
            {
                return ("ContactId,Böyle Bir Tedarikci Yok");
            }
            else
            {
                return ("true");
            }
        }

      public async Task<string> UpdatePurchaseItem(PurchaseItem T, int CompanyId)
        {
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id  From Measure where CompanyId = {CompanyId} and id = {T.MeasureId})as MeasureId,
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id  From Tax where CompanyId = {CompanyId} and id = {T.TaxId})as TaxId,
            (Select id as varmi From Orders where CompanyId = {CompanyId} and id = {T.OrdersId} and IsActive=1 and DeliveryId=1)as OrdersId,
            (Select id as varmi From OrdersItem where CompanyId = {CompanyId} and id ={T.id} and OrdersId={T.OrdersId})as id");
            if (list.First().MeasureId==null)
            {
                return ("MeasureId bulunamadı");
            }
            if (list.First().Tip != "Material")
            {
                return ("ItemId tip hatası");
            }
            if (list.First().TaxId == null)
            {
                return ("TaxId bulunamadi");
            }
            if (list.First().OrdersId == null)
            {
                return ("Böyle Bir OrdersId Yok");
            }
            if (list.First().id==null)
            {
                return ("Böyle Bir eslesme yok.Id ve OrdersId Yok");
            }
            else
            {
                return ("true");
            }
        }
    }
}
