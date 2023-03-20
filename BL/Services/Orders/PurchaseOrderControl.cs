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


       public async Task<List<string>>  Delete(Delete T)
        {
            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@Tip", T.Tip);
            string sql = $"Select id From SatinAlma where  id  = @id";
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

       public async Task<List<string>> DeleteItem(DeleteItems T)
        {
            List<string> hatalar = new();

            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select Tip  From Urunler where id={T.StokId})as Tip,
            (Select id as varmi From SatinAlma where  id = {T.SatinAlmaId} and Aktif=1)as SatinAlmaId,
            (Select id From SatinAlmaDetay where  id = {T.id} and SatinAlmaId={T.SatinAlmaId})as id");

         
            if (list.First().SatinAlmaId == null)
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
                return hatalar;
            }
           
        }

       public async Task<List<string>> Insert(PurchaseOrderInsert T)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl2>($@"select
            (Select id  From Olcu where  id = {T.OlcuId})as OlcuId,
            (Select Tip  From Urunler where  id={T.StokId})as Tip,
            (Select id  From Vergi where  id = {T.VergiId})as VergiId,
            (Select  id as UretimId From Uretim where id = {T.UretimId} and Aktif=1 and Durum!=3)as UretimId,
            (Select id From UretimDetay where  id = {T.UretimDetayId} and UretimId={T.UretimId})as id,
             (select id From Satis where  id = {T.SatisId} and DurumBelirteci!=2 and  DurumBelirteci!=4)as SatisId, 
            (Select id From SatisDetay where  id = {T.SatisDetayId} and SatisId={T.SatisId})as SatisDetayId,
            (Select id as DepoId From DepoVeAdresler where  id = {T.DepoId})as DepoId
            ");

            if (T.UretimId!=0 && T.UretimDetayId !=0)
            {
                if (list.First().UretimId == null)
                {
                    hatalar.Add("Böyle Bir id Yok");
                }
                if (list.First().id == null)
                {
                    hatalar.Add("id ve OrderId eşleşmiyor.İki id'nin ilişkisi yok.");
                }
            }

            if (T.SatisId != 0 && T.SatisDetayId != 0)
            {
                if (list.First().SatisId == null)
                {
                    hatalar.Add("SalesOrderid bulunamadı.");
                }

                if (list.First().SatisDetayId == null)
                {
                    hatalar.Add("SalesOrderid ve SalesOrderItemid eşleşmesi hatalı.");
                }
            }
            if (T.Tip != "PurchaseOrder")
            {
                hatalar.Add("Tip değişkeni,tip hatasi");
            }
            if (list.First().DepoId==null)
            {
                hatalar.Add("Böyle Bir Lokasyon Yok");
            }
            var Locaiton = await _db.QueryFirstAsync<bool>($"select Uretim from DepoVeAdresler where  id={T.DepoId} ");
            if (Locaiton != true)
            {
                hatalar.Add("Uretim kismina yetkiniz yok");

            }
            if (list.First().OlcuId==null)
            {
                hatalar.Add("MeasureId bulunamadı");
            }
            if (list.First().VergiId== null)
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

        public async Task<List<string>> InsertItem(PurchaseOrderInsertItem T)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id  From Olcu where  id = {T.OlcuId})as OlcuId,
            (Select Tip  From Urunler where id={T.StokId})as Tip,
            (Select id  From Vergi where  id = {T.VergiId})as VergiId,
            (Select id as varmi From SatinAlma where  id = {T.SatinAlmaId} and Aktif=1 and DurumBelirteci=1)as id");



            if (list.First().OlcuId==null)
            {
                hatalar.Add("MeasureId bulunamadı");

            }
            if (list.First().Tip != "Material")
            {
                if (list.First().Tip != "SemiProduct")
                {
                    hatalar.Add("ItemId tip hatası");

                }

            }
            if (list.First().id == null)
            {
                hatalar.Add("Böyle Bir OrdersId Yok");
            }
            if (list.First().VergiId == null)
            {
                hatalar.Add("TaxId bulunamadi");
                return hatalar;

            }
            else
            {
                return hatalar;
            }
        }

       public async Task<List<string>> Update(PurchaseOrderUpdate T)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id as varmi From DepoVeAdresler where  id = {T.DepoId})as DepoId
        ");
            if (list.First().DepoId==null)
            {
                hatalar.Add("Böyle Bir Lokasyon Yok");
            }
            List<LocationsDTO> Locaiton = (await _db.QueryAsync<LocationsDTO>($"select Satis,Uretim,SatinAlma from DepoVeAdresler where  id={T.DepoId} ")).ToList();
            bool? make = Locaiton.First().Uretim;
            if (make != true)
            {
                hatalar.Add("Uretim kismina yetkiniz yok");
                return hatalar;

            }
            else
            {
                return hatalar;
            }
        }

      public async Task<List<string>> UpdatePurchaseItem(PurchaseItem T)
        {
            List<string> hatalar = new();
            var list = await _db.QueryAsync<PurchaseItemControl>($@"select
            (Select id  From Olcu where  id = {T.OlcuId})as OlcuId,
            (Select Tip  From Urunler where  id={T.StokId})as Tip,
            (Select id  From Vergi where  id = {T.VergiId})as VergiId,
            (Select id as varmi From SatinAlma where  id = {T.SatinAlmaId} and Aktif=1 and DurumBelirteci=1)as SatinAlmaId,
            (Select id as varmi From SatinAlmaDetay where  id ={T.id} and SatinAlmaId={T.SatinAlmaId})as id");
            if (list.First().OlcuId ==null)
            {
                hatalar.Add("MeasureId bulunamadı");
            }
            if (list.First().Tip != "Material")
            {
                if (list.First().Tip != "SemiProduct")
                {
                    hatalar.Add("ItemId tip hatası");
                }
         
            }
            if (list.First().VergiId == null)
            {
                hatalar.Add("TaxId bulunamadi");
            }
            if (list.First().SatinAlmaId == null)
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
