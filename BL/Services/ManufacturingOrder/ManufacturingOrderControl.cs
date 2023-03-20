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

        public async Task<List<string>> DeleteItems(UretimDeleteItems T)
        {
            List<string> hatalar = new();
            var LocationVarmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From UretimDetay where  id = {T.id}");
            if (LocationVarmi.Count() == 0)
            {
                string hata = "Böyle Bir UretimDetay id Yok";
                hatalar.Add(hata);
            }

            var varmi = await _db.QueryAsync<int>($"Select id  From Uretim where  id = {T.UretimId}");
            if (varmi.Count() == 0)
            {
                string hata = "Böyle Bir id Yok";
                hatalar.Add(hata);
            }  
            string Tip = await _db.QueryFirstAsync<string>($"Select Tip  From Urunler where id={T.StokId}");
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

        public async Task<List<string>> DoneStock(UretimTamamlama T)
        {
            List<string> hatalar = new();

            var LocationVarmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From Uretim where  id = {T.id}");
            if (LocationVarmi.Count() == 0)
            {
                string hata = "Böyle Bir id Yok";
                hatalar.Add(hata);
            }
            return hatalar;

        }

        public async Task<List<string>> IngredientInsert(UretimIngredientsInsert T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<ManufacturingOrderItemsIngredientsInsert>($@"select
            (Select id as varmi From Uretim where  id = {T.UretimId} and Aktif=1 and Durum!=3)as UretimId,
            (Select id as varmi From DepoVeAdresler where id = {T.DepoId}) as DepoId 
");


            if (list.First().UretimId==null)
            {
                string hata = "Böyle Bir id Yok";
                hatalar.Add(hata);
            }
          
            if (list.First().DepoId==null)
            {
                string hata = "Böyle Bir Lokasyon Yok";
                hatalar.Add(hata);
                
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where id={T.DepoId} ")).ToList();
            bool? make = Locaiton.First().Uretim;
            if (make != true)
            {
                string hata = "Yetkiniz yok";
                hatalar.Add(hata);
            }
            string Tip = await _db.QueryFirstAsync<string>($"Select Tip  From Urunler where  id={T.StokId}");
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

        public async Task<List<string>> IngredientsUpdate(UretimIngredientsUpdate T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<ManufacturingOrderItemsIngredientsUpdate>($@"select
            (Select id as varmi From Uretim where  id = {T.UretimId} and Aktif=1 and Durum!=3)as UretimId,
            (Select id From UretimDetay where id = {T.id} and UretimId={T.UretimId})    as  id,
            (Select id as varmi From DepoVeAdresler where id = {T.DepoId}) as DepoId
                      
");


            if (list.First().UretimId== null)
            {
                string hata = "Böyle Bir OrderId Yok";
                hatalar.Add(hata);
            }

            if (list.First().id== null)
            {
                string hata = "id ve OrderId eşleşmiyor.İki id'nin ilişkisi yok.";
                hatalar.Add(hata);
            }
           
            if (list.First().DepoId==null)
            {
                string hata = "Böyle Bir Lokasyon Yok";
                hatalar.Add(hata);
            }

            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where  id={T.DepoId} ")).ToList();
            bool? make = Locaiton.First().Uretim;
            if (make != true)
            {
                string hata = "Yetkiniz yok";
                hatalar.Add(hata);
            }
            string Tip = await _db.QueryFirstAsync<string>($"Select Tip  From Urunler where  id={T.StokId}");
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

        public async Task<List<string>> Insert(UretimDTO T)
        {
            List<string> hatalar = new();

            //eklenen locationStock Varmi Kontrol Ediyoruz
            var list = await _db.QueryAsync<ManufacturingPurchaseOrder>($@"select
            (Select id as varmi From DepoVeAdresler where id = {T.DepoId}) as DepoId,
            (Select Tip  From Urunler where id={T.StokId})as Tip    
            ");


            if (list.First().DepoId ==null)
            {
                hatalar.Add("Böyle Bir Lokasyon Yok");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where  id={T.DepoId} ")).ToList();
            bool? make = Locaiton.First().Uretim;
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

        public async Task<List<string>> OperationsInsert(UretimOperationsInsert T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<ManufacturingOrderItemsOperationsUpdate>($@"select
            (Select id as varmi From Uretim where  id = {T.UretimId} and Aktif=1 and Durum!=3)as UretimId,
            (Select id From Kaynaklar where  id = {T.KaynakId} and Aktif=1)    as  KaynakId,
            (Select id From Operasyonlar where id = {T.OperasyonId} and Aktif=1) as OperasyonId");

            if (list.First().UretimId==null)
            {
                string hata = "OrderId,Böyle Bir id Yok";
                hatalar.Add(hata);
            }
   
            if (list.First().KaynakId == null)
            {
                string hata = "ResourceId bulunamıyor.";
                hatalar.Add(hata);
              
            }
            if (list.First().OperasyonId == null)
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

        public async Task<List<string>> OperationUpdate(UretimOperationsUpdate T)
        {
            List<string> hatalar = new();


            var list = await _db.QueryAsync<ManufacturingOrderItemsOperationsUpdate>($@"select
            (Select id From UretimDetay where id = {T.id} and UretimId={T.UretimId})as id,
            (Select id as varmi From Uretim where  id = {T.UretimId} and Aktif=1 and Durum!=3)as UretimId,
            (Select id From Kaynaklar where  id = {T.KaynakId} and Aktif=1)    as  KaynakId,
            (Select id From Operasyonlar where  id = {T.OperasyonId} and Aktif=1) as OperasyonId");

            if (list.First().UretimId == null)
            {
                string hata = "Böyle Bir Orderid Yok";
                hatalar.Add(hata);
            }
            if (list.First().id == null)
            {
                string hata = "id ve OrderId eşleşmiyor.İki id'nin ilişkisi yok.";
                hatalar.Add(hata);
            }
            if (list.First().KaynakId == null)
            {
                string hata = "ResourceId bulunamıyor...";
                hatalar.Add(hata);
              
            }
            if (list.First().OperasyonId == null)
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

        public async Task<List<string>> PurchaseOrder(PurchaseBuy T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<ManufacturingPurchaseOrder>($@"select
            (Select id From Uretim where  id = {T.UretimId} and Aktif=1 and Durum!=3)as UretimId,
            (Select id From UretimDetay where id = {T.UretimDetayId} and UretimId={T.UretimId})    as  UretimDetayId,
            (Select id as varmi From DepoVeAdresler where id = {T.DepoId}) as DepoId,
            (Select Tip  From Urunler where  id={T.StokId})as Tip
            ");


            if (list.First().UretimId==null)
            {
                string hata = "Böyle Bir id Yok";
                hatalar.Add(hata);
            }
            if (list.First().UretimDetayId == 0)
            {
                string hata = "id ve OrderId eşleşmiyor.İki id'nin ilişkisi yok.";
                hatalar.Add(hata);
            }
          
            if (list.First().DepoId==null)
            {
                string hata = "Böyle Bir Lokasyon Yok";
                hatalar.Add(hata);
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where  id={T.DepoId} ")).ToList();
            bool? make = Locaiton.First().Uretim;
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
            return hatalar;

        }

        public async Task<List<string>> Update(UretimUpdate T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<ManufacturingPurchaseOrder>($@"select
            (Select id From Uretim where id = {T.id} and Aktif=1 and Durum!=3)as UretimId,
            (Select id as varmi From DepoVeAdresler where  id = {T.DepoId}) as DepoId,
            (Select Tip  From Urunler where  id={T.StokId})as Tip

            ");

            if (list.First().UretimId == null)
            {
                string hata = "Böyle Bir id Yok";
                hatalar.Add(hata);
            }
            if (list.First().DepoId==null)
            {
                string hata = "Böyle Bir Lokasyon Yok";
                hatalar.Add(hata);
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where  id={T.DepoId} ")).ToList();
            bool? make = Locaiton.First().Uretim;
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

        public async Task<List<string>> DeleteKontrol(List<UretimDeleteKontrol> T)
        {

            List<string> hatalar = new();
            foreach (var item in T)
            {
                var list = await _db.QueryAsync<UretimDeleteKontrolClas>($@"select * from Uretim where id={item.id}");
                if (list.First().SatisId != null)
                {
                    string hata = @$"{list.First().Isim} isimli üretim bir satişa bağlı. ";
                    hatalar.Add(hata);
                }
            }
            return hatalar;
          

        }


    }
}
