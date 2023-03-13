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
using PurchaseBuy = DAL.DTO.PurchaseBuy;

namespace BL.Services.ManufacturingOrder
{
    public class ManufacturingOrderControl : IManufacturingOrderControl
    {
      private readonly  IDbConnection _db;

        public ManufacturingOrderControl(IDbConnection db)
        {
            _db = db;
        }

        public async Task<List<string>> DeleteItems(UretimDeleteItems T, int CompanyId)
        {
            List<string> hatalar = new();
            var LocationVarmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From ManufacturingOrderItems where CompanyId = {CompanyId} and id = {T.id}");
            if (LocationVarmi.Count() == 0)
            {
                string hata = "Böyle Bir ManufacturingOrderItem id Yok";
                hatalar.Add(hata);
            }

            var varmi = await _db.QueryAsync<int>($"Select id  From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.ManufacturingOrderId}");
            if (varmi.Count() == 0)
            {
                string hata = "Böyle Bir id Yok";
                hatalar.Add(hata);
            }  
            string Tip = await _db.QueryFirstAsync<string>($"Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId}");
            if (Tip == "Material" || Tip == "SemiProduct")
            {
                return hatalar;
            }
            else
            {
                string hata = "ItemId,Tip hatası";
                hatalar.Add(hata);
                return hatalar;
            }




        }

        public async Task<List<string>> DoneStock(UretimTamamlama T, int CompanyId)
        {
            List<string> hatalar = new();

            var LocationVarmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.id}");
            if (LocationVarmi.Count() == 0)
            {
                string hata = "Böyle Bir id Yok";
                hatalar.Add(hata);
            }
            return hatalar;

        }

        public async Task<List<string>> IngredientInsert(UretimIngredientsInsert T, int CompanyId)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<ManufacturingOrderItemsIngredientsInsert>($@"select
            (Select id as varmi From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.ManufacturingOrderId} and IsActive=1 and Status!=3)as OrderId,
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId}) as LocationId 
");


            if (list.First().OrderId==null)
            {
                string hata = "Böyle Bir id Yok";
                hatalar.Add(hata);
            }
          
            if (list.First().LocationId==null)
            {
                string hata = "Böyle Bir Lokasyon Yok";
                hatalar.Add(hata);
                
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                string hata = "Yetkiniz yok";
                hatalar.Add(hata);
            }
            string Tip = await _db.QueryFirstAsync<string>($"Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId}");
            if (Tip == "Material" || Tip == "SemiProduct")
            {
                return hatalar;
            }
            else
            {
                string hata = "Make tip hatası";
                hatalar.Add(hata);
                return hatalar;

            }
        }

        public async Task<List<string>> IngredientsUpdate(UretimIngredientsUpdate T, int CompanyId)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<ManufacturingOrderItemsIngredientsUpdate>($@"select
            (Select id as varmi From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.OrderId} and IsActive=1 and Status!=3)as OrderId,
            (Select id From ManufacturingOrderItems where CompanyId = {CompanyId} and id = {T.id} and OrderId={T.OrderId})    as  id,
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId}) as LocationId
                      
");


            if (list.First().OrderId== null)
            {
                string hata = "Böyle Bir OrderId Yok";
                hatalar.Add(hata);
            }

            if (list.First().id== null)
            {
                string hata = "id ve OrderId eşleşmiyor.İki id'nin ilişkisi yok.";
                hatalar.Add(hata);
            }
           
            if (list.First().LocationId==null)
            {
                string hata = "Böyle Bir Lokasyon Yok";
                hatalar.Add(hata);
            }

            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                string hata = "Yetkiniz yok";
                hatalar.Add(hata);
            }
            string Tip = await _db.QueryFirstAsync<string>($"Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId}");
            if (Tip == "Material" || Tip == "SemiProduct")
            {

                return hatalar;
            }
            else
            {
                string hata = "Make tip hatası";
                hatalar.Add(hata);
                return hatalar;

            }
        }

        public async Task<List<string>> Insert(UretimDTO T, int CompanyId)
        {
            List<string> hatalar = new();

            //eklenen locationStock Varmi Kontrol Ediyoruz
            var list = await _db.QueryAsync<ManufacturingPurchaseOrder>($@"select
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId}) as LocationId,
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip    
            ");


            if (list.First().LocationId==null)
            {
                hatalar.Add("Böyle Bir Lokasyon Yok");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                string hata = "Uretim kismina yetkiniz yok";
                hatalar.Add(hata);
            }
            string Tip = list.First().Tip;
            if (Tip== "Product" || Tip=="SemiProduct")
            {

                return hatalar;
            }
            else
            {
                string hata = "Make tip hatası";
                hatalar.Add(hata);
                return hatalar;
            }
        
         
           


        }

        public async Task<List<string>> OperationsInsert(UretimOperationsInsert T, int CompanyId)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<ManufacturingOrderItemsOperationsUpdate>($@"select
            (Select id as varmi From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.ManufacturingOrderId} and IsActive=1 and Status!=3)as OrderId,
            (Select id From Resources where CompanyId = {CompanyId} and id = {T.ResourceId} and IsActive=1)    as  ResourceId,
            (Select id From Operations where CompanyId = {CompanyId} and id = {T.OperationId} and IsActive=1) as OperationId");

            if (list.First().OrderId==null)
            {
                string hata = "OrderId,Böyle Bir id Yok";
                hatalar.Add(hata);
            }
   
            if (list.First().ResourceId== null)
            {
                string hata = "ResourceId bulunamıyor.";
                hatalar.Add(hata);
              
            }
            if (list.First().OperationId == null)
            {
                string hata = "OperationId bulunamıyor.";
                hatalar.Add(hata);
                return hatalar;

            }
            else
            {

                return hatalar;
            }
        }

        public async Task<List<string>> OperationUpdate(UretimOperationsUpdate T, int CompanyId)
        {
            List<string> hatalar = new();


            var list = await _db.QueryAsync<ManufacturingOrderItemsOperationsUpdate>($@"select
            (Select id From ManufacturingOrderItems where CompanyId = {CompanyId} and id = {T.id} and OrderId={T.OrderId})as id,
            (Select id as varmi From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.OrderId} and IsActive=1 and Status!=3)as OrderId,
            (Select id From Resources where CompanyId = {CompanyId} and id = {T.ResourceId} and IsActive=1)    as  ResourceId,
            (Select id From Operations where CompanyId = {CompanyId} and id = {T.OperationId} and IsActive=1) as OperationId");

            if (list.First().OrderId == null)
            {
                string hata = "Böyle Bir Orderid Yok";
                hatalar.Add(hata);
            }
            if (list.First().id == null)
            {
                string hata = "id ve OrderId eşleşmiyor.İki id'nin ilişkisi yok.";
                hatalar.Add(hata);
            }
            if (list.First().ResourceId== null)
            {
                string hata = "ResourceId bulunamıyor...";
                hatalar.Add(hata);
              
            }
            if (list.First().OperationId == null)
            {
                string hata = "OperationId bulunamıyor..";
                hatalar.Add(hata);
                return hatalar;

            }
            else
            {
                return hatalar;
            }
        }

        public async Task<List<string>> PurchaseOrder(PurchaseBuy T, int CompanyId)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<ManufacturingPurchaseOrder>($@"select
            (Select id From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.ManufacturingOrderId} and IsActive=1 and Status!=3)as ManufacturingOrderId,
            (Select id From ManufacturingOrderItems where CompanyId = {CompanyId} and id = {T.ManufacturingOrderItemId} and OrderId={T.ManufacturingOrderId})    as  ManufacturingOrderItemId,
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId}) as LocationId,
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip
            ");


            if (list.First().ManufacturingOrderId==null)
            {
                string hata = "Böyle Bir id Yok";
                hatalar.Add(hata);
            }
            if (list.First().ManufacturingOrderItemId == 0)
            {
                string hata = "id ve OrderId eşleşmiyor.İki id'nin ilişkisi yok.";
                hatalar.Add(hata);
            }
          
            if (list.First().LocationId==null)
            {
                string hata = "Böyle Bir Lokasyon Yok";
                hatalar.Add(hata);
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                string hata = "Uretim kismina yetkiniz yok";
                hatalar.Add(hata);
            }
            if (list.First().Tip != "Material")
            {
                string hata = "ItemId,tip hatasi";
                hatalar.Add(hata);
            }
            if (T.Tip != "PurchaseOrder")
            {
                string hata = "Tip değişkeni,tip hatasi";
                hatalar.Add(hata);
            }
            string contact = await _db.QueryFirstAsync<string>($"Select Tip  From Contacts where CompanyId = {CompanyId} and id = {T.ContactId}");
            if (contact!="Supplier")
            {
                string hata = "ContactId,Böyle Bir Tedarikci Yok";
                hatalar.Add(hata);
            }


            return hatalar;

        }

        public async Task<List<string>> Update(UretimUpdate T, int CompanyId)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<ManufacturingPurchaseOrder>($@"select
            (Select id From ManufacturingOrder where CompanyId = {CompanyId} and id = {T.id} and IsActive=1 and Status!=3)as ManufacturingOrderId,
            (Select id as varmi From Locations where CompanyId = {CompanyId} and id = {T.LocationId}) as LocationId,
            (Select Tip  From Items where CompanyId = {CompanyId} and id={T.ItemId})as Tip

            ");

            if (list.First().ManufacturingOrderId == null)
            {
                string hata = "Böyle Bir id Yok";
                hatalar.Add(hata);
            }
            if (list.First().LocationId==null)
            {
                string hata = "Böyle Bir Lokasyon Yok";
                hatalar.Add(hata);
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Sell,Make,Buy from Locations where CompanyId={CompanyId} and id={T.LocationId} ")).ToList();
            bool? make = Locaiton.First().Make;
            if (make != true)
            {
                string hata = "Yetkiniz yok";
                hatalar.Add(hata);
            }
            string Tip = list.First().Tip;
            if (Tip == "Product" || Tip == "SemiProduct")
            {
                return hatalar;
            }
            else
            {
                string hata = "Make tip hatası";
                hatalar.Add(hata);
                return hatalar;

            }


        }

        public async Task<List<string>> DeleteKontrol(List<UretimDeleteKontrol> T , int CompanyId)
        {

            List<string> hatalar = new();
            foreach (var item in T)
            {
                var list = await _db.QueryAsync<UretimDeleteKontrolClas>($@"select * from ManufacturingOrder where id={item.id} and CompanyId={CompanyId}");
                if (list.First().SalesOrderId != null)
                {
                    string hata = @$"{list.First().Name} isimli üretim bir satişa bağlı. ";
                    hatalar.Add(hata);
                }
            }
            return hatalar;
          

        }


    }
}
