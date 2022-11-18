using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.PurchaseOrderDTO;

namespace BL.Services.ManufacturingOrder
{
    public class ManufacturingOrderControl : IManufacturingOrderControl
    {
      private readonly  IDbConnection _db;

        public ManufacturingOrderControl(IDbConnection db)
        {
            _db = db;
        }

        public async Task<string> DeleteItems(ManufacturingDeleteItems T, int CompanyId)
        {
            var LocationVarmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From ManufacturingOrderItems where CompanyId = {CompanyId} and id = {T.id}");
            if (LocationVarmi.Count() == 0)
            {
                return ("Böyle Bir ManufacturingOrderItem id Yok");
            }

            var varmi = await _db.QueryAsync<int>($"Select id  From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.OrdersId}");
            if (varmi.Count() == 0)
            {
                return ("Böyle Bir id Yok");
            }  
            string Tip = await _db.QueryFirstAsync<string>($"Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId}");
            if (Tip == "Material" || Tip == "SemiProduct")
            {
                return ("true");
            }
            else
            {
                return ("ItemId,Tip hatası");
            }




        }

        public async Task<string> DoneStock(ManufacturingStock T, int CompanyId)
        {
            var LocationVarmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.id}");
            if (LocationVarmi.Count() == 0)
            {
                return ("Böyle Bir id Yok");
            }
            if (T.SalesOrderId!=0 && T.SalesOrderItemId!=0)
            {
                var salesid = await _db.QueryAsync<int>($"Select id From Orders where CompanyId = {CompanyId} and id = {T.SalesOrderId} and DeliveryId!=2 and  DeliveryId!=4");
                if (salesid.Count() == 0)
                {
                    return ("SalesOrderid bulunamadı.");
                }
                var varmi = await _db.QueryAsync<int>($"Select id From OrdersItem where CompanyId = {CompanyId} and id = {T.SalesOrderItemId} and OrdersId={T.SalesOrderId}");
                if (varmi.Count() == 0)
                {
                    return ("SalesOrderid ve SalesOrderItemid eşleşme hatalı.");
                }
            }
            return ("true");

        }

        public async Task<string> IngredientInsert(ManufacturingOrderItemDTO.ManufacturingOrderItemsIngredientsInsert T, int CompanyId)
        {
            var list = await _db.QueryAsync<ManufacturingOrderItemsIngredientsInsert>($@"select
            (Select id as varmi From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.OrderId} and IsActive=1 and Status!=3)as OrderId,
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId}) as LocationId,
              (Select id From Orders where CompanyId = {CompanyId} and id = {T.SalesOrderId} and DeliveryId!=2 and  DeliveryId!=4)as SalesOrderId,
            (Select id From OrdersItem where CompanyId = {CompanyId} and id = {T.SalesOrderItemId} and OrdersId={T.SalesOrderId})as SalesOrderItemId  
");


            if (list.First().OrderId==null)
            {
                return ("Böyle Bir id Yok");
            }
          
            if (list.First().LocationId==null)
            {
                return ("Böyle Bir Lokasyon Yok");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                return ("Yetkiniz yok");
            }
            if (T.SalesOrderId != 0 && T.SalesOrderItemId != 0)
            {
               
                if (list.First().SalesOrderId== null)
                {
                    return ("SalesOrderid bulunamadı.");
                }

                if (list.First().SalesOrderItemId == null)
                {
                    return ("SalesOrderid ve SalesOrderItemid hatalı.");
                }
            }
            string Tip = await _db.QueryFirstAsync<string>($"Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId}");
            if (Tip == "Material" || Tip == "SemiProduct")
            {
                return ("true");
            }
            else
            {
                return ("Make tip hatası");
            }
        }

        public async Task<string> IngredientsUpdate(ManufacturingOrderItemDTO.ManufacturingOrderItemsIngredientsUpdate T, int CompanyId)
        {
            var list = await _db.QueryAsync<ManufacturingOrderItemsIngredientsUpdate>($@"select
            (Select id as varmi From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.OrderId} and IsActive=1 and Status!=3)as OrderId,
            (Select id From ManufacturingOrderItems where CompanyId = {CompanyId} and id = {T.id} and OrderId={T.OrderId})    as  id,
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId}) as LocationId,
              (Select id From Orders where CompanyId = {CompanyId} and id = {T.SalesOrderId} and DeliveryId!=2 and  DeliveryId!=4)as SalesOrderId,
            (Select id From OrdersItem where CompanyId = {CompanyId} and id = {T.SalesOrderItemId} and OrdersId={T.SalesOrderId})as SalesOrderItemId              
");


            if (list.First().OrderId== null)
            {
                return ("Böyle Bir OrderId Yok");
            }

            if (list.First().id== null)
            {
                return ("id ve OrderId eşleşmiyor.İki id'nin ilişkisi yok.");
            }
           
            if (list.First().LocationId==null)
            {
                return ("Böyle Bir Lokasyon Yok");
            }

            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                return ("Yetkiniz yok");
            }
            if (T.SalesOrderId != 0 && T.SalesOrderItemId != 0)
            {
                if (list.First().SalesOrderId == null)
                {
                    return ("SalesOrderid bulunamadı.");
                }

                if (list.First().SalesOrderItemId == null)
                {
                    return ("SalesOrderid ve SalesOrderItemid hatalı.");
                }
            }
            string Tip = await _db.QueryFirstAsync<string>($"Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId}");
            if (Tip == "Material" || Tip == "SemiProduct")
            {
                return ("true");
            }
            else
            {
                return ("Make tip hatası");
            }
        }

        public async Task<string> Insert(ManufacturingOrderDTO.ManufacturingOrderA T, int CompanyId)
        {
            //eklenen locationStock Varmi Kontrol Ediyoruz
            var list = await _db.QueryAsync<ManufacturingPurchaseOrder>($@"select
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId}) as LocationId,
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id From Orders where CompanyId = {CompanyId} and id = {T.SalesOrderId} and DeliveryId!=2 and  DeliveryId!=4)as SalesOrderId,
            (Select id From OrdersItem where CompanyId = {CompanyId} and id = {T.SalesOrderItemId} and OrdersId={T.SalesOrderId})as SalesOrderItemId
            ");


            if (list.First().LocationId==null)
            {
                return("Böyle Bir Lokasyon Yok");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                return("Uretim kismina yetkiniz yok");
            }
            if (T.SalesOrderId != 0 && T.SalesOrderItemId != 0)
            {
              
                if (list.First().SalesOrderId == null)
                {
                    return ("SalesOrderid bulunamadı.");
                }
                if (list.First().SalesOrderItemId== null)
                {
                    return ("SalesOrderid ve SalesOrderItemid hatalı.Eslesme yok");
                }
            }
            string Tip = list.First().Tip;
            if (Tip== "Product" || Tip=="SemiProduct")
            {
                return ("true");
            }
            else
            {
                return ("Make tip hatası");
            }
        
         
           


        }

        public async Task<string> OperationsInsert(ManufacturingOrderItemDTO.ManufacturingOrderItemsOperationsInsert T, int CompanyId)
        {
            var list = await _db.QueryAsync<ManufacturingOrderItemsOperationsUpdate>($@"select
            (Select id as varmi From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.OrderId} and IsActive=1 and Status!=3)as OrderId,
            (Select id From Resources where CompanyId = {CompanyId} and id = {T.ResourceId} and IsActive=1)    as  ResourceId,
            (Select id From Operations where CompanyId = {CompanyId} and id = {T.OperationId} and IsActive=1) as OperationId");

            if (list.First().OrderId==null)
            {
                return ("OrderId,Böyle Bir id Yok");
            }
   
            if (list.First().ResourceId== null)
            {
                return ("ResourceId bulunamıyor.");
            }
            if (list.First().OperationId == null)
            {
                return ("OperationId bulunamıyor.");
            }
            else
            {
                return ("true");
            }
        }

        public async Task<string> OperationUpdate(ManufacturingOrderItemDTO.ManufacturingOrderItemsOperationsUpdate T, int CompanyId)
        {

            var list = await _db.QueryAsync<ManufacturingOrderItemsOperationsUpdate>($@"select
            (Select id From ManufacturingOrderItems where CompanyId = {CompanyId} and id = {T.id} and OrderId={T.OrderId})as id,
            (Select id as varmi From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.OrderId} and IsActive=1 and Status!=3)as OrderId,
            (Select id From Resources where CompanyId = {CompanyId} and id = {T.ResourceId} and IsActive=1)    as  ResourceId,
            (Select id From Operations where CompanyId = {CompanyId} and id = {T.OperationId} and IsActive=1) as OperationId");

            if (list.First().OrderId == null)
            {
                return ("Böyle Bir Orderid Yok");
            }
            if (list.First().id == null)
            {
                return ("id ve OrderId eşleşmiyor.İki id'nin ilişkisi yok.");
            }
            if (list.First().ResourceId== null)
            {
                return ("ResourceId bulunamıyor...");
            }
            if (list.First().OperationId == null)
            {
                return ("OperationId bulunamıyor...");
            }
            else
            {
                return ("true");
            }
        }

        public async Task<string> PurchaseOrder(ManufacturingOrderItemDTO.ManufacturingPurchaseOrder T, int CompanyId)
        {
            var list = await _db.QueryAsync<ManufacturingPurchaseOrder>($@"select
            (Select id From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.ManufacturingOrderId} and IsActive=1 and Status!=3)as ManufacturingOrderId,
            (Select id From ManufacturingOrderItems where CompanyId = {CompanyId} and id = {T.ManufacturingOrderItemId} and OrderId={T.ManufacturingOrderId})    as  ManufacturingOrderItemId,
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId}) as LocationId,
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip,
            (Select id From Orders where CompanyId = {CompanyId} and id = {T.SalesOrderId} and DeliveryId!=2 and  DeliveryId!=4)as SalesOrderId,
            (Select id From OrdersItem where CompanyId = {CompanyId} and id = {T.SalesOrderItemId} and OrdersId={T.SalesOrderId})as SalesOrderItemId
            ");


            if (list.First().ManufacturingOrderId==null)
            {
                return ("Böyle Bir id Yok");
            }
            if (list.First().ManufacturingOrderItemId == 0)
            {
                return ("id ve OrderId eşleşmiyor.İki id'nin ilişkisi yok.");
            }
          
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
            if (list.First().Tip != "Material")
            {
                return ("ItemId,tip hatasi");
            }
            if (T.Tip != "PurchaseOrder")
            {
                return ("Tip değişkeni,tip hatasi");
            }
            string contact = await _db.QueryFirstAsync<string>($"Select Tip  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId}");
            if (contact!="Supplier")
            {
                return ("ContactId,Böyle Bir Tedarikci Yok");
            }
            if (T.SalesOrderId != 0 && T.SalesOrderItemId != 0)
            {
                var salesid = await _db.QueryAsync<int>($"");
                if (list.First().SalesOrderId == null)                {
                    return ("SalesOrderid bulunamadı.");
                }

                if (list.First().SalesOrderItemId == null)
                {
                    return ("SalesOrderid ve SalesOrderItemid eşleşmesi hatalı.");
                }
            }
   

            return ("true");

        }

        public async Task<string> Update(ManufacturingOrderDTO.ManufacturingOrderUpdate T, int CompanyId)
        {
            var list = await _db.QueryAsync<ManufacturingPurchaseOrder>($@"select
            (Select id From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.id} and IsActive=1 and Status!=3)as ManufacturingOrderId,
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId}) as LocationId,
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip

            ");

            if (list.First().ManufacturingOrderId == null)
            {
                return("Böyle Bir id Yok");
            }
            if (list.First().LocationId==null)
            {
                return ("Böyle Bir Lokasyon Yok");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                return("Yetkiniz yok");
            }
            string Tip = list.First().Tip;
            if (Tip == "Product" || Tip == "SemiProduct")
            {
                return ("true");
            }
            else
            {
                return ("Make tip hatası");
            }


        }

    }
}
