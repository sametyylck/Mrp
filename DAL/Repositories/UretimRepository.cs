using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using DAL.StockControl;
using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.BomDTO;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.ManufacturingOrderItemDTO;
using static DAL.DTO.ProductOperationsBomDTO;
using static DAL.DTO.PurchaseOrderDTO;
using static DAL.DTO.SalesOrderDTO;
using static DAL.DTO.StockAdjusmentDTO;

namespace DAL.Repositories
{
    public class UretimRepository : IUretimRepository
    {
        private readonly IDbConnection _db;
        private readonly IStockControl _control;


        public UretimRepository(IDbConnection db, IStockControl control)
        {
            _db = db;
            _control = control;
        }

        public async Task<int> Insert(UretimDTO T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);
            param.Add("@Isim", T.Isim);
            param.Add("@StokId", T.StokId);
            if (T.PlananlananMiktar == 0)
            {
                param.Add("@PlanlananMiktar", 1);//ilk insert edilirken 1 değerini veriyoruz daha update kısmından kullanıcı kendisi ayarlayabilecek.
            }
            else
            {
                param.Add("@PlanlananMiktar", T.PlananlananMiktar);
            }

            param.Add("@UretimTarihi", DateTime.Now);
            param.Add("@OlusturmaTarihi", T.OlusturmTarihi);
            param.Add("@DepoId", T.DepoId);
            param.Add("@Bilgi", T.Bilgi);
            param.Add("@Durum", 0);
            param.Add("@Aktif", true);
            param.Add("@Private", T.Ozel);
            param.Add("@Tip", T.Tip);
            param.Add("@ParentId", T.ParentId);

            param.Add("@BeklenenTarih", T.BeklenenTarih);
            string sql = string.Empty;
            if (T.Ozel == true)
            {
                sql = $@"Insert into Uretim (ParentId,Tip,Ozel,Isim,Aktif,StokId,PlanlananMiktar,BeklenenTarih,UretimTarihi,OlusturmaTarihi,DepoId,Bilgi,Durum)  OUTPUT INSERTED.[id] values (@ParentId,@Tip,@Private,@Isim,@Aktif,@StokId,@PlanlananMiktar,@BeklenenTarih,@UretimTarihi,@OlusturmaTarihi,@DepoId,@Bilgi,@Durum)";
            }
            else
            {
                param.Add("@Private", false);
                sql = $@"Insert into Uretim (ParentId,Tip,Ozel,Isim,Aktif,StokId,PlanlananMiktar,BeklenenTarih,UretimTarihi,OlusturmaTarihi,DepoId,Bilgi,Durum)  OUTPUT INSERTED.[id] values (@ParentId,@Tip,@Private,@Isim,@Aktif,@StokId,@PlanlananMiktar,@BeklenenTarih,@UretimTarihi,@OlusturmaTarihi,@DepoId,@Bilgi,@Durum)";
            }


            int id = await _db.QuerySingleAsync<int>(sql, param);

            return id;
        }

        //Insert ile birlikte kullanılır.
        public async Task InsertOrderItems(int id, int? StokId, int DepoId, float? adet, int? SatisId,int? SatisDetayId)
        {

            int? ProductId = StokId;
            var BomList = await _db.QueryAsync<BOM>($"Select * From UrunRecetesi where MamulId = {ProductId} and Aktif = 1");

            foreach (var item in BomList)
            {

                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Ingredients");
                param.Add("@OrderId", id);
                param.Add("@StokId", item.MalzemeId);
                param.Add("@Bilgi", item.Bilgi);
                param.Add("@SatisId", SatisId);
                param.Add("@SatisDetayId", SatisDetayId);

                param.Add("@DepoId", DepoId);
                // materyalin VarsayilanFiyat,stockıd,locatinstock,locationstockId,RezerveCount
                // Bul
                List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($@" select (select ISNULL(VarsayilanFiyat, 0) From Urunler where id = @StokId)as  VarsayilanFiyat,(Select ISNULL(id, 0) from DepoStoklar where StokId=@StokId and DepoId = @DepoId )   as DepoStokId,(select Tip from Urunler where id=@StokId)as Tip", param)).ToList();

                param.Add("@PlanlananMiktar", item.Miktar * adet);
                float VarsayilanFiyat = sorgu.First().VarsayilanFiyat;
                param.Add("@Tutar", VarsayilanFiyat * item.Miktar * adet);
                //Avaibility Hesapla Stoktaki miktar işlemi gerçekleştirmeye yetiyormu kontrol et


                string sqlb = $@"select ISNULL(SUM(Miktar),0) from SatinAlma 
                left join SatinAlmaDetay on SatinAlmaDetay.SatinAlmaId = SatinAlma.id  and SatinAlmaDetay.StokId = @StokId where  DurumBelirteci = 1 and SatinAlma.SatisId is null and SatinAlma.UretimId is null and SatinAlma.Aktif=1 and SatinAlma.Ozel=0";
                var expected = await _db.QueryFirstAsync<int>(sqlb, param);
                param.Add("@LocationStockId",
                sorgu.First().DepoStokId);
                int rezerveid = 0;

                int? rezerve = await _control.Count(item.MalzemeId, DepoId);
                rezerve = rezerve >= 0 ? rezerve : 0;

                if (SatisDetayId!=0)
                {
                    string sqld = $@"select id,ISNULL(RezerveDeger,0) as RezerveDeger from Rezerve where SatisId=@SatisId and SatisDetayId=@SatisDetayId and UretimId is null and UretimDetayId is null and StokId=@StokId and DepoId=@DepoId and Durum=1";
                    var rezerv = await _db.QueryAsync<LocaVarmı>(sqld, param);

                    float? rezervestockCount = 0;
                    if (rezerv.Count() == 0)
                    {

                        param.Add("@RezervTip", sorgu.First().Tip);
                        param.Add("@RezerveDeger", 0);
                        param.Add("@Durum", 1);
                        param.Add("@LocationStockCount", rezerve);
                        param.Add("@ContactId", sorgu.First().CariKod);

                        rezervestockCount = 0;
                       rezerveid= await _db.QuerySingleAsync<int>($"Insert into Rezerve (SatisId,SatisDetayId,Tip,StokId,RezerveDeger,DepoId,Durum) OUTPUT INSERTED.[id] values (@SatisId,@SatisDetayId,@RezervTip,@StokId,@RezerveDeger,@DepoId,@Durum)", param);
                        param.Add("@Rezerveid", rezerveid);

                    }
                    else
                    {
                        rezerveid = rezerv.First().id;
                        param.Add("@Rezerveid", rezerveid);

                        rezervestockCount = rezerv.First().RezerveDeger;
                    }
                    if (rezervestockCount >= item.Miktar * adet)
                    {
                        param.Add("@MalzemeDurum", 2);

                        var newStock = rezervestockCount - (item.Miktar * adet);
                        param.Add("@RezerveCount", item.Miktar * adet);
                        param.Add("@LocationStockCount", rezerve + newStock);
                        await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount,UretimId=@OrderId where SatisId=@SatisId and SatisDetayId=@SatisDetayId and DepoId=@DepoId and StokId=@StokId and id=@Rezerveid ", param);

                    }
                    else
                    {
                        float? newQuantity = (item.Miktar * adet) - rezervestockCount;
                        if (rezerve >= newQuantity)
                        {

                            var newStockCount = rezerve - newQuantity;
                            param.Add("@LocationStockCount", newStockCount);
                            var newrezervecount = rezervestockCount + newQuantity;
                            param.Add("@RezerveCount", newrezervecount);
                            param.Add("@MalzemeDurum", 2);

                        }
                        else
                        {

                            param.Add("@RezerveCount", rezerve + rezervestockCount);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                            param.Add("@LocationStockCount", 0);
                            param.Add("@MalzemeDurum", 0);

                        }
                        param.Add("@id", id);
                        await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount,UretimId=@OrderId where  SatisId=@SatisId and SatisDetayId=@SatisDetayId and DepoId=@DepoId and StokId=@StokId and id=@Rezerveid  ", param);


                    }
                }

                else if (rezerve > 0)
                {


                    if (rezerve >= item.Miktar * adet)//Stok sayısı istenilenden büyük ise rezerve sayısı adet olur
                    {
                        param.Add("@RezerveCount", item.Miktar * adet);
                        param.Add("@MalzemeDurum", 2);
                        param.Add("@LocationStockCount", rezerve - item.Miktar * adet);

                    }
                    else
                    {

                        param.Add("@RezerveCount", rezerve);//Stok sayısı adetten kücük ise rezer sayısı Stok adeti kadar olur.
                        param.Add("@MalzemeDurum", 0);
                        param.Add("@LocationStockCount", 0);

                    }
                    param.Add("@Durum", 1);
                    param.Add("@id", id);
                    await _db.ExecuteAsync($"Insert into Rezerve  (Tip,UretimId,StokId,RezerveDeger,DepoId,Durum) values   (@Tip,@id,@StokId,@RezerveCount,@DepoId,@Durum)", param);

                }

                else if ((rezerve - (item.Miktar * adet)) * (-2) <= expected && expected != 0)
                {
                    param.Add("@id", id);

                    param.Add("@RezerveCount", rezerve);
                    param.Add("@MalzemeDurum", 1);
                    param.Add("@Durum", 1);

                    await _db.ExecuteAsync($"Insert into Rezerve  (Tip,UretimId,StokId,RezerveDeger,DepoId,Durum) values   (@Tip,@id,@StokId,@RezerveCount,@DepoId,@Durum)", param);
                }
                else
                {
                    param.Add("@id", id);

                    param.Add("@RezerveCount", rezerve);
                    param.Add("@MalzemeDurum", 0);
                    param.Add("@Durum", 1);

                    await _db.ExecuteAsync($"Insert into Rezerve  (Tip,UretimId,StokId,RezerveDeger,DepoId,Durum) values   (@Tip,@id,@StokId,@RezerveCount,@DepoId,@Durum)", param);
                }



                string sql = $@"Insert into UretimDetay (Tip,UretimId,StokId,Bilgi,PlanlananMiktar,Tutar,MalzemeDurum) values (@Tip,@OrderId,@StokId,@Bilgi,@PlanlananMiktar,@Tutar,@MalzemeDurum)";
                await _db.ExecuteAsync(sql, param);
                string sqlf = $@"(select UretimDetay.id from UretimDetay where StokId=@StokId and UretimId=@OrderId)";
                var ManuItemId = await _db.QueryFirstAsync<int>(sqlf, param);
                param.Add("@UretimDetayId", ManuItemId);
                if (SatisId != 0)
                {
                    await _db.ExecuteAsync($"Update Rezerve set UretimDetayId=@UretimDetayId where  SatisId=@SatisId and SatisDetayId=@SatisDetayId and DepoId=@DepoId and UretimId=@OrderId and StokId=@StokId and id=@Rezerveid  ", param);
                }
                else
                {
                    await _db.ExecuteAsync($"Update Rezerve set UretimDetayId=@UretimDetayId where  UretimId=@OrderId and DepoId=@DepoId and StokId=@StokId ", param);
                }

            }


            //Eklenen Ordera ait ıtemin Operation Bomlarını buluyoruz
            var OperationList = await _db.QueryAsync<ProductOperationsBomDTO.ProductOperationsBOM>($"Select * From UrunKaynakRecetesi where  StokId = {ProductId}");

            foreach (var item in OperationList)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Operations");
                param.Add("@OrderId", id);
                param.Add("@OperasyonId", item.OperasyonId);
                param.Add("@KaynakId", item.KaynakId);
                param.Add("@PlanlananZaman ", item.OperasyonZamani * adet);
                param.Add("@Durum", 0);
                param.Add("@SaatlikUcret", item.SaatlikUcret);
                param.Add("@Tutar", (item.SaatlikUcret / 60 / 60) * item.OperasyonZamani * adet);

                string sql = $@"Insert into UretimDetay (Tip,UretimId,OperasyonId,KaynakId,PlanlananZaman,Durum,SaatlikUcret,Tutar) values (@Tip,@OrderId,@OperasyonId,@KaynakId,@PlanlananZaman,@Durum,@SaatlikUcret,@Tutar)";
                await _db.ExecuteAsync(sql, param);
            }
        }
        public async Task Update(UretimUpdate T)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@Isim", T.Isim);
            param.Add("@StokId", T.StokId);
            param.Add("@UretimTarihi", DateTime.Now);
            param.Add("@OlusturmaTarihi", T.OlusturmaTarihi);
            param.Add("@DepoId", T.DepoId);
            param.Add("@MalzemeFiyati", T.MalzemeTutarı);
            param.Add("@OperasyonFiyati", T.OperasyonTutarı);
            param.Add("@TotalCost", T.ToplamTutar);
            param.Add("@Bilgi", T.Bilgi);
            param.Add("@Durum", 0);
            param.Add("@PlanlananMiktar", T.PlanlananMiktar);
            param.Add("@BeklenenTarih", T.BeklenenTarih);
            string sqlv = $@"Select ISNULL(PlanlananMiktar,0)as PlanlananMiktar ,StokId,DepoId from  Uretim where id=@id and Aktif=1 and Durum!=3";
            var deger = await _db.QueryAsync<ManufacturingOrderA>(sqlv, param);
            T.eskiPlanned = (float)deger.First().PlanlananMiktar;
            T.eskiLocation = deger.First().DepoId;
            if (T.StokId != deger.First().StokId && deger.First().StokId != null && T.StokId != null)
            {
                string sqlsorgu = $@"Select * from UretimDetay where  and UretimDetay.UretimId=@id";
                var ManuItems = await _db.QueryAsync<ManufacturingOrderResponse>(sqlsorgu, param);
                foreach (var item in ManuItems)
                {
                    UretimDeleteItems A = new UretimDeleteItems();

                    A.UretimId = T.id;
                    A.id = item.id;
                    if (item.Tip == "Ingredients")
                    {
                        A.StokId = (int)item.StokId;
                    }
                    await DeleteItems(A);
                }
                param.Add("@StokId", T.StokId);
                string sql = $@"Update Uretim Set Isim=@Isim,MalzemeFiyati=@MalzemeFiyati,OperasyonFiyati=@OperasyonFiyati,ToplamMaliyet=@TotalCost,StokId=@StokId,UretimTarihi=@UretimTarihi,BeklenenTarih=@BeklenenTarih,PlanlananMiktar=@PlanlananMiktar,OlusturmaTarihi=@OlusturmaTarihi,DepoId=@DepoId,Bilgi=@Bilgi,Durum=@Durum where  and id=@id";
                await _db.ExecuteAsync(sql, param);
                await InsertOrderItems(T.id, T.StokId, T.DepoId, T.PlanlananMiktar,0,0);





            }

            else
            {
                if (T.DepoId != T.eskiLocation)
                {
                    var rezervedegerler = await _db.QueryAsync<Manufacturing>($"select * from Rezerve where UretimId={T.id} and Durum=1");
                    foreach (var item in rezervedegerler)
                    {
                        param.Add("@ItemsId", item.StokId);
                        await _db.ExecuteAsync($"Delete from Rezerve where UretimId=@id and  StokId=@ItemsId", param);
                    }
                }

                string sql = $@"Update Uretim Set Isim=@Isim,MalzemeFiyati=@MalzemeFiyati,OperasyonFiyati=@OperasyonFiyati,ToplamMaliyet=@TotalCost,StokId=@StokId,UretimTarihi=@UretimTarihi,BeklenenTarih=@BeklenenTarih,PlanlananMiktar=@PlanlananMiktar,OlusturmaTarihi=@OlusturmaTarihi,DepoId=@DepoId,Bilgi=@Bilgi,Durum=@Durum where id=@id";
                await _db.ExecuteAsync(sql, param);
            }
        }
        public async Task DeleteItems(UretimDeleteItems T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@UretimId", T.UretimId);
            prm.Add("@StokId", T.StokId);

            if (T.StokId != 0)
            {
                var sorgu = await _db.QueryAsync<ItemKontrol>($"select id from Rezerve where UretimId = @UretimId  and StokId = @StokId and Durum=1 and UretimDetayId=@id", prm);
                await _db.ExecuteAsync($"Update SatinAlma set UretimId=NULL,UretimDetayId=NULL where UretimId=@UretimId  and UretimDetayId=@id and SatinAlma.Aktif=1 and SatinAlma.DurumBelirteci=1", prm);
                await _db.ExecuteAsync($"Delete From UretimDetay  where StokId = @StokId and id=@id and UretimId=@UretimId", prm);

                int? rezervid = sorgu.First().id;
                prm.Add("@RezerveId", rezervid);

                await _db.ExecuteAsync($"Delete From Rezerve where id=@RezerveId ", prm);
            }
            else
            {
                await _db.ExecuteAsync($"Delete From UretimDetay  where  and id=@id and UretimId=@UretimId", prm);

            }

        }
        public async Task UpdateOrderItems(int id, int DepoId, float adetbul, float eski)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@DepoId", DepoId);
            prm.Add("@id", id);

            //Eklenen Ordera ait ıtemin  Bomlarını buluyoruz
            var BomList = await _db.QueryAsync<BOM>($"Select id,ISNULL(StokId,0) as MalzemeId,ISNULL(PlanlananMiktar,0) as PlanlananMiktar,ISNULL(Bilgi,'') as Bilgi from UretimDetay where  UretimDetay.UretimId={id} and Tip='Ingredients'");

            foreach (var item in BomList)
            {

                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Ingredients");
                param.Add("@UretimDetayId", item.id);
                param.Add("@OrderId", id);
                param.Add("@StokId", item.MalzemeId);
                param.Add("@Bilgi", item.Bilgi);
                if (adetbul == eski)
                {

                    param.Add("@PlanlananMiktar", item.Miktar);
                }
                else if (adetbul > eski)
                {
                    float anadeger = item.Miktar / eski;
                    float yenideger = adetbul - eski;
                    var artışdegeri = yenideger * anadeger;
                    item.Miktar = item.Miktar + artışdegeri;
                    param.Add("@PlanlananMiktar", item.Miktar);
                }
                else
                {
                    var yenideger = item.Miktar / eski;
                    var degerler = eski - adetbul;
                    item.Miktar = item.Miktar - (yenideger * degerler);
                    param.Add("@PlanlananMiktar", item.Miktar);
                }


                param.Add("@DepoId", DepoId);
                string sql = $@"Update UretimDetay Set PlanlananMiktar=@PlanlananMiktar where UretimId=@OrderId and StokId=@StokId and id=@UretimDetayId  ";
                await _db.ExecuteAsync(sql, param);
                // materyalin VarsayilanFiyat
                // Bul
                string sqlb = $@"select ISNULL(SUM(Miktar),0) from SatinAlma 
                left join SatinAlmaDetay on SatinAlmaDetay.SatinAlmaId = SatinAlma.id and SatinAlma.DepoId=@DepoId
                and SatinAlmaDetay.StokId = @StokId where      DurumBelirteci = 1 and SatinAlma.UretimId={id} and SatinAlma.Aktif=1";
                var expected = await _db.QueryFirstAsync<float>(sqlb, param);

                List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select   (Select ISNULL(id, 0) from DepoStoklar where StokId = @StokId and DepoId = @DepoId  )   as DepoStokId, (select ISNULL(VarsayilanFiyat, 0) From Urunler where  id = @StokId)as  VarsayilanFiyat", param)).ToList();
                float VarsayilanFiyat = sorgu.First().VarsayilanFiyat;
                param.Add("@Tutar", VarsayilanFiyat * item.Miktar);
                param.Add("@LocationStockId", sorgu.First().DepoStokId);


                float DepoStoklar = await _control.Count(item.MalzemeId, DepoId);
                DepoStoklar = DepoStoklar >= 0 ? DepoStoklar : 0;


                var Count = await _db.QueryAsync<int>($"Select ISNULL(Rezerve.RezerveDeger,0)as Count from Rezerve where  StokId=@StokId and UretimId=@OrderId and UretimDetayId=@UretimDetayId and Rezerve.Durum=1 and Rezerve.DepoId=@DepoId", param);
                float? RezerveCounts = 0;
                float? deger;
                if (Count.Count() == 0)
                {
                    if (DepoStoklar >= item.Miktar)
                    {
                        deger = item.Miktar;
                    }

                    else
                    {
                        deger = DepoStoklar;

                    }
                    DynamicParameters prm2 = new DynamicParameters();
                    prm2.Add("@Durum", 1);
                    prm2.Add("@LocationStockCount", DepoStoklar);
                    prm2.Add("@Tip", "Ingredients");
                    prm2.Add("@OrderId", id);
                    prm2.Add("@UretimDetayId", item.id);
                    prm2.Add("@RezerveCount", deger);
                    prm2.Add("@StokId", item.MalzemeId);
                    prm2.Add("@DepoId", DepoId);

                    await _db.ExecuteAsync($"Insert into Rezerve  (Tip,UretimId,UretimDetayId,StokId,RezerveDeger,DepoId,Durum) values   (@Tip,@OrderId,@UretimDetayId,@StokId,@RezerveCount,@DepoId,@Durum)", prm2);



                }
                else
                {
                    RezerveCounts = Count.First();
                }





                if (item.Miktar <= RezerveCounts && item.Miktar>0)
                {
                    param.Add("@RezerveCount", item.Miktar);   
                    param.Add("@MalzemeDurum", 2);
                }
                else if (DepoStoklar >= item.Miktar - RezerveCounts && item.Miktar>0)
                {
                    param.Add("@RezerveCount", item.Miktar);
                    param.Add("@MalzemeDurum", 2);
                }

                else if (item.Miktar - RezerveCounts <= expected && expected > 0)
                {
                    param.Add("@MalzemeDurum", 1);
                }
                else
                {

                    param.Add("@RezerveCount", DepoStoklar + RezerveCounts);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                    param.Add("@MalzemeDurum", 0);
                }

                await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where  UretimId=@OrderId  and UretimDetayId=@UretimDetayId and StokId=@StokId and Rezerve.DepoId=@DepoId", param);

                string sqlw = $@"Update UretimDetay Set Tip=@Tip,StokId=@StokId,Bilgi=@Bilgi,PlanlananMiktar=@PlanlananMiktar,Tutar=@Tutar,MalzemeDurum=@MalzemeDurum where  UretimId=@OrderId and StokId=@StokId and id=@UretimDetayId  ";
                await _db.ExecuteAsync(sqlw, param);

            }


            //Eklenen Ordera ait ıtemin Operation Bomlarını buluyoruz
            var OperationList = await _db.QueryAsync<ProductOperationsBOM>($"Select ISNULL(id,0)As id,ISNULL(OperasyonId,0) as OperasyonId,ISNULL(KaynakId,0)as KaynakId,ISNULL(SaatlikUcret,0)as SaatlikUcret,ISNULL(PlanlananZaman,0)as PlanlananZaman  from UretimDetay where  UretimDetay.UretimId = {id} and Tip = 'Operations'");

            foreach (var item in OperationList)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@Tip", "Operations");
                param.Add("@OrderId", id);
                param.Add("@OperasyonId", item.OperasyonId);
                param.Add("@KaynakId", item.KaynakId);
                if (adetbul == eski)
                {
                    param.Add("@PlanlananMiktar", item.OperasyonZamani);
                }
                else
                {
                    var saatlik = item.OperasyonZamani / eski;
                    item.OperasyonZamani = saatlik * adetbul;

                }

                param.Add("@PlanlananZaman ", item.OperasyonZamani);
                param.Add("@Tutar", (item.SaatlikUcret / 60 / 60) * item.OperasyonZamani);

                string sql = $@"Update UretimDetay Set Tip=@Tip,UretimId=@OrderId,OperasyonId=@OperasyonId,KaynakId=@KaynakId,PlanlananZaman=@PlanlananZaman,Tutar=@Tutar where UretimId=@OrderId and OperasyonId=@OperasyonId and KaynakId=KaynakId  ";
                await _db.ExecuteAsync(sql, param);
            }
        }
        public async Task<int> IngredientsInsert(UretimIngredientsInsert T)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@Tip", "Ingredients");
            param.Add("@OrderId", T.UretimId);
            param.Add("@StokId", T.StokId);
            param.Add("@Bilgi", T.Bilgi);
            param.Add("@DepoId", T.DepoId);
            param.Add("@PlanlananMiktar", T.Miktar);
            int rezerveid = 0;
            //param.Add("@Tutar", T.Tutar);
            //param.Add("@MalzemeDurum", T.MalzemeDurum);
            List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select  (Select ISNULL(id, 0) from DepoStoklar where StokId = @StokId  and DepoId = (Select DepoId From Uretim where  id = @OrderId)) as DepoStokId, (select ISNULL(VarsayilanFiyat, 0) From Urunler where id = @StokId)as  VarsayilanFiyat", param)).ToList();

            param.Add("@LocationStockId", sorgu.First().DepoStokId);

            //yeni costu buluyoruz.
            float VarsayilanFiyat = sorgu.First().VarsayilanFiyat;
            var newCost = T.Miktar * VarsayilanFiyat;
            param.Add("@Tutar", newCost);

            float? rezerve = await _control.Count(T.StokId, T.DepoId);
            rezerve = rezerve >= 0 ? rezerve : 0;


            if (rezerve >= 0)
            {

                if (rezerve >= T.Miktar)//Stok sayısı istesnilenden büyük ise rezerve sayısı adet olur
                {
                    param.Add("@RezerveCount", T.Miktar);
                    var newStockCount = rezerve - (T.Miktar);
                    param.Add("@LocationStockCount", newStockCount);
                    param.Add("@MalzemeDurum", 2);

                }
                else
                {

                    param.Add("@RezerveCount", rezerve);//Stok sayısı adetten kücük ise rezer sayısı Stokadeti kadar olur.
                    param.Add("@LocationStockCount", 0);
                    param.Add("@MalzemeDurum", 0);
                }
                param.Add("@Durum", 1);

                rezerveid = await _db.QuerySingleAsync<int>($"Insert into Rezerve  (Tip,UretimId,StokId,RezerveDeger,DepoId,Durum) OUTPUT INSERTED.[id] values (@Tip,@OrderId,@StokId,@RezerveCount,@DepoId,@Durum)", param);


            }
            else
            {
                param.Add("@MalzemeDurum", 0);
                rezerveid = await _db.QuerySingleAsync<int>($"Insert into Rezerve  (Tip,UretimId,StokId,RezerveDeger,DepoId,Durum) OUTPUT INSERTED.[id] values (@Tip,@OrderId,@StokId,@RezerveCount,@DepoId,@Durum)", param);
            }

            string sql = $@"Insert into UretimDetay (Tip,UretimId,StokId,Bilgi,PlanlananMiktar,Tutar,MalzemeDurum) OUTPUT INSERTED.[id] values (@Tip,@OrderId,@StokId,@Bilgi,@PlanlananMiktar,@Tutar,@MalzemeDurum)";
            int ManuId = await _db.QuerySingleAsync<int>(sql, param);
            param.Add("@UretimDetayId", ManuId);



            await _db.ExecuteAsync($"Update Rezerve set UretimDetayId=@UretimDetayId where id={rezerveid}", param);
            return ManuId;
        }
        public async Task<int> OperationsInsert(UretimOperationsInsert T)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@Tip", "Operations");
            prm.Add("@KaynakId", T.KaynakId);
            prm.Add("@OperasyonId", T.OperasyonId);
            prm.Add("@PlanlananZaman", T.PlanlananZaman);
            prm.Add("@OrderId", T.UretimId);
            prm.Add("@Durum", 0);

            return await _db.QuerySingleAsync<int>($"Insert into UretimDetay (Durum,Tip,UretimId,KaynakId, OperasyonId,PlanlananZaman) OUTPUT INSERTED.[id] values (@Durum,@Tip,@OrderId,@KaynakId, @OperasyonId,@PlanlananZaman)", prm);
        }
        public async Task OperationsUpdate(UretimOperationsUpdate T)
        {
            var Costbul = await _db.QueryAsync<float>($"Select CAST(({T.SaatlikUcret}*{T.PlanlananZaman})/60/60 as decimal(15,4)) ");
            float cost = Costbul.First();
            var newcost = Convert.ToDecimal(cost);
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@OrderId", T.UretimId);
            param.Add("@OperasyonId", T.OperasyonId);
            param.Add("@KaynakId", T.KaynakId);
            param.Add("@PlanlananZaman", T.PlanlananZaman);
            param.Add("@Durum", T.Durum);
            param.Add("SaatlikUcret", T.SaatlikUcret);
            param.Add("@Tutar", newcost);
            string sql = $@"Update UretimDetay SET UretimId = @OrderId, OperasyonId = @OperasyonId, KaynakId = @KaynakId, PlanlananZaman = @PlanlananZaman, Durum = @Durum,Tutar = @Tutar,SaatlikUcret=@SaatlikUcret
                            where   id = @id and UretimId = @OrderId";
            await _db.ExecuteAsync(sql, param);
        }
        public async Task IngredientsUpdate(UretimIngredientsUpdate T)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@OrderId", T.UretimId);
            param.Add("@StokId", T.StokId);
            param.Add("@Tip", " Ingredients");
            param.Add("@DepoId", T.DepoId);
            param.Add("@PlanlananMiktar", T.Miktar);
            string sqlu = $@"Update UretimDetay SET  PlanlananMiktar = @PlanlananMiktar where  id = @id and UretimId = @OrderId";
            await _db.ExecuteAsync(sqlu, param);

            List<LocaVarmı> sorgu = (await _db.QueryAsync<LocaVarmı>($"   select (Select ISNULL(id, 0) from DepoStoklar where StokId = @StokId  and DepoId = (Select DepoId From Uretim where  id = @OrderId))   as DepoStokId, (select ISNULL(VarsayilanFiyat, 0) From Urunler where  id = @StokId)as  VarsayilanFiyat", param)).ToList();

            param.Add("@LocationStockId", sorgu.First().DepoStokId);

            float VarsayilanFiyat = sorgu.First().VarsayilanFiyat;
            var newCost = T.Miktar * VarsayilanFiyat;

            float? rezerve = await _control.Count(T.StokId, T.DepoId);
            rezerve = rezerve >= 0 ? rezerve : 0;

            string sqld = $@"select ISNULL(RezerveDeger,0) from Rezerve where UretimId=@OrderId and StokId=@StokId and DepoId=@DepoId and UretimDetayId=@id and Durum=1";
            var rezervestockCount = await _db.QueryAsync<float>(sqld, param);
            string sqlb = $@"select ISNULL(SUM(Miktar),0) from SatinAlma 
                left join SatinAlmaDetay on SatinAlmaDetay.SatinAlmaId = SatinAlma.id and SatinAlma.DepoId=@DepoId
                and SatinAlmaDetay.StokId = @StokId where  DurumBelirteci = 1 and SatinAlma.UretimId={T.UretimId} and SatinAlma.Aktif=1 ";
            var expectedsorgu = await _db.QueryAsync<float>(sqlb, param);
            float expected = expectedsorgu.First();

            float rezervecount = 0;
            if (rezervestockCount.Count() == 0)
            {
                rezervecount = 0;
            }
            else
            {
                rezervecount = rezervestockCount.First();
            }






            if (rezervecount >= T.Miktar)
            {
                param.Add("@MalzemeDurum", 2);
                var newStock = rezervecount - T.Miktar;
                param.Add("@LocationStockCount", rezerve + newStock);
                param.Add("@RezerveCount", T.Miktar);
                param.Add("@Durum", 1);
                await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where UretimId=@OrderId and DepoId=@DepoId and UretimDetayId=@id and StokId=@StokId  and Durum=1 ", param);

            }

            else if (rezervecount < T.Miktar)
            {
                float? newQuantity = T.Miktar - rezervecount;
                if (rezerve >= newQuantity)
                {

                    var newStockCount = rezerve - newQuantity;
                    param.Add("@LocationStockCount", newStockCount);
                    var newrezervecount = rezervecount + newQuantity;
                    param.Add("@RezerveCount", newrezervecount);
                    param.Add("@MalzemeDurum", 2);
                }
                else if ((T.Miktar - rezervecount <= expected && expected > 0))
                {
                    param.Add("@MalzemeDurum", 1);
                    param.Add("@RezerveCount", rezervecount);
                }
                else
                {
                    param.Add("@MalzemeDurum", 0);
                    param.Add("@RezerveCount", rezerve + rezervecount);//Stok sayısı adetten kücük ise rezer sayısıStokadeti kadar olur.
                    param.Add("@LocationStockCount", 0);

                }
                param.Add("@Durum", 1);
                await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where UretimId=@OrderId and DepoId=@DepoId and UretimDetayId=@id and StokId=@StokId  and Durum=1 ", param);

            }
            else if ((T.Miktar - rezervecount <= expected && expected > 0))
            {
                param.Add("@MalzemeDurum", 1);
            }
            param.Add("@id", T.id);
            param.Add("@OrderId", T.UretimId);
            param.Add("@StokId", T.StokId);
            param.Add("@Bilgi", T.Bilgi);
            param.Add("@PlanlananMiktar", T.Miktar);
            param.Add("@Tutar", newCost);
            string sql = $@"Update UretimDetay SET StokId = @StokId , Bilgi = @Bilgi , PlanlananMiktar = @PlanlananMiktar, Tutar = @Tutar,MalzemeDurum = @MalzemeDurum
                            where  id = @id and UretimId = @OrderId";
            await _db.ExecuteAsync(sql, param);


        }
        public async Task Delete(List<UretimDeleteKontrol> T, int UserId)
        {
            foreach (var A in T)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@id", A.id);
                param.Add("@DateTime", DateTime.Now);
                param.Add("@User", UserId);
                param.Add("@Aktif", false);
                List<bool> IsActived = (await _db.QueryAsync<bool>($"select Aktif from Uretim where id=@id ", param)).ToList();
                if (IsActived[0] == false)
                {

                }
                else
                {
                    List<int> ItemsCount = (await _db.QueryAsync<int>($"select id from Rezerve where UretimId=@id and Durum=1", param)).ToList();
                    var detay = await _db.QueryAsync<UretimIngredientsUpdate>($"select * from UretimDetay where UretimId=@id ", param);

                    var order = await _db.QueryAsync<OperationsUpdate>($"select SatinAlma.id from SatinAlma where  SatisId=@id and Aktif=1", param);
                    foreach (var item in order)
                    {
                        param.Add("@orderid", item.id);

                        await _db.ExecuteAsync($"Update SatinAlma Set SatisId=NULL , SatisDetayId=NULL  where id = @orderid", param);

                    }
                    foreach (var item in detay)
                    {
                        await _db.ExecuteAsync($"Delete from  UretimDetay where id={item.id}");

                    }
                    await _db.QueryAsync($"Delete from Uretim where id = @id ", param);
                    foreach (var item in ItemsCount)
                    {
                        param.Add("@RezerveId", item);
                        await _db.ExecuteAsync($"Delete from  Rezerve where  id=@RezerveId", param);

                    }

                }
            }
           
        }
        public async Task DoneStock(UretimTamamlama T , int UserId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            var BomList = await _db.QueryAsync<DoneStock>($@"Select moi.id,moi.StokId,moi.PlanlananMiktar,moi.Tip,Rezerve.id as RezerveId,Uretim.Durum,Uretim.DepoId,Uretim.SatisId,Uretim.SatisDetayId,Uretim.ParentId     from UretimDetay moi 
            left join Uretim on Uretim.id=moi.UretimId 
            left join Rezerve on Rezerve.UretimDetayId=moi.id 
            where moi. and moi.UretimId=@id and Uretim.Aktif=1", param);
            param.Add("@Durum", T.Durum);
            int eskiStatus = BomList.First().Durum;
            if (T.Durum == 3 && eskiStatus != 3)
            {
                foreach (var item in BomList)
                {
                    if (item.Tip == "Ingredients")
                    {
                        param.Add("@StokId", item.StokId);
                        param.Add("@DepoId", item.DepoId);

                        string sqla = $@"select 
                         (Select ISNULL(StokAdeti,0) from DepoStoklar where StokId = @StokId
                         and DepoId = (Select ISNULL(DepoId,0) From Uretim where id = @id)
                         and )as StokAdeti,
                         (Select ISNULL(id,0) from DepoStoklar where StokId=@StokId
                         
                         and DepoId = (Select ISNULL(DepoId,0) From Uretim where  id =@id)
                         and )   as    DepoStokId";
                        param.Add("@OrderItemId", item.id);
                        var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, param);//
                        float? stockQuantity = sorgu.First().Miktar;


                        float? stockCount = sorgu.First().StokAdeti;
                        float? NewStockCount = stockCount - item.PlanlananMiktar;
                        var stocklocationId = sorgu.First().DepoStokId;
                        param.Add("@ManufacturingOrderItemsId", item.id);
                        param.Add("@stocklocationId", stocklocationId);
                        param.Add("@NewStockCount", NewStockCount);


                        //Yeni count değerini tabloya güncelleştiriyoruz.
                        await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewStockCount where id = @stocklocationId ", param);
                        param.Add("@MalzemeDurum", 3);
                        await _db.ExecuteAsync($"Update UretimDetay Set MalzemeDurum=@MalzemeDurum where id=@OrderItemId", param);
                        param.Add("@Statu", 4);
                        param.Add("@RezerveId", item.RezerveId);
                        _db.Execute($"Update Rezerve Set Durum=@Statu where id=@RezerveId ", param);

                    }
                    else
                    {
                        param.Add("@ManuId", item.id);
                        param.Add("@Durum", 3);
                        await _db.ExecuteAsync($"Update UretimDetay Set Durum=@Durum where id=@ManuId", param);

                    }

                }

                string sqlc = $@"select  
                 (Select ISNULL(StokAdeti,0) from DepoStoklar where StokId = (select Uretim.StokId from Uretim where  id=@id)
                 and DepoId = (Select ISNULL(DepoId,0) From Uretim where id =  @id))as StokAdeti ,(Select Uretim.PlanlananMiktar from Uretim where  and id=@id)as PlanlananMiktar,
                 (Select ISNULL(id,0) from DepoStoklar where StokId = (select Uretim.StokId from Uretim
                 where id=@id)
                 and DepoId = (Select ISNULL(DepoId,0) From Uretim where id =  @id) ) as DepoStokId,
                (Select Uretim.StokId from Uretim where  id=@id)as StokId ";
                var sorgu4 = await _db.QueryAsync<StockAdjusmentSql>(sqlc, param);
                var ManufacturingQuantity = sorgu4.First().PlanlananMiktar;
                param.Add("@StokId", sorgu4.First().StokId);

                if (BomList.First().SatisId!=0)
                {
                    var stokcontrol = await _control.Count(sorgu4.First().StokId, BomList.First().DepoId);
                    stokcontrol = stokcontrol >= 0 ? stokcontrol : 0;

                    DynamicParameters prm = new();
                    prm.Add("@SatisId", BomList.First().SatisId);
                    prm.Add("@SatisDetayId", BomList.First().SatisDetayId);
                    prm.Add("@StokId", sorgu4.First().StokId);
                    prm.Add("@StokId", sorgu4.First().StokId);
                    prm.Add("@DepoId", BomList.First().DepoId);

                    string sqld = $@"select id,ISNULL(RezerveDeger,0) as RezerveDeger from Rezerve where SatisId=@SatisId AND SatisDetayId=@SatisDetayId and StokId=@StokId and DepoId=@DepoId and Durum=1";
                    var rezervestockCount = await _db.QueryAsync<LocaVarmı>(sqld, prm);
                    float? rezervecount = rezervestockCount.First().RezerveDeger;
                    int rezerveid = rezervestockCount.First().id;
                    prm.Add("@rezerveid", rezerveid);

                    float missing;
                    string missingsorgu = $@"
                         Select 
                         ((select Miktar from SatisDetay where id=@SatisDetayId and SatisId=@SatisId )-ISNULL((Rezerve.RezerveDeger),0))as Missing
                        
                         from SatinAlma
				    	 LEFT join Rezerve on Rezerve.SatisId=@SatisId and Rezerve.SatisDetayId=@SatisDetayId  and Rezerve.StokId=@StokId
                         where SatinAlma.Aktif=1
                         Group by Rezerve.RezerveCount";
                    var missingcount = await _db.QueryAsync<LocaVarmı>(missingsorgu, prm);
                    if (missingcount.Count() == 0)
                    {
                        missing = 0;
                    }
                    else
                    {
                        missing = missingcount.First().Kayıp;
                    }
                    if (missing<=rezervecount)
                    {
                        prm.Add("@RezerveCount", missing);

                        prm.Add("@SalesItem", 3);
                        prm.Add("@Production", 4);
                        prm.Add("@Ingredients", 3);

                        await _db.ExecuteAsync($"Update SatisDetay set MalzemeDurum=@Ingredients where SatisId=@SatisId and id=@SatisDetayId and StokId=@StokId ", prm);
                    }
                   else if (missing<=ManufacturingQuantity+rezervecount)
                    {
                        prm.Add("@RezerveCount", missing);

                        prm.Add("@SalesItem", 3);
                        prm.Add("@Production", 4);
                        prm.Add("@Ingredients", 3);

                        await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where id=@rezerveid  ", prm);
                    
                    }
                    else if (missing<=ManufacturingQuantity+stokcontrol+rezervecount)
                    {
                        prm.Add("@RezerveCount", missing);

                        prm.Add("@SalesItem", 3);
                        prm.Add("@Production", 4);
                        prm.Add("@Ingredient", 3);

                        await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where id=@rezerveid ", prm);
                      
                    }
                    else
                    {
                        prm.Add("@RezerveCount", ManufacturingQuantity+stokcontrol+rezervecount);

                        prm.Add("@SalesItem", 0);
                        prm.Add("@Production", 0);
                        prm.Add("@Ingredient", 0);
                        await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where   id=@rezerveid  ", prm);
                     
                    }
                    await _db.ExecuteAsync($"Update SatisDetay set Malzemeler=@Ingredients,SatisOgesi=@SalesItem,Uretme=@Production where  SatisId=@SatisId and id=@SatisDetayId and StokId=@StokId ", prm);

                }
                var DepoStoklar = sorgu4.First().StokAdeti;
                var newlocationstock = ManufacturingQuantity + DepoStoklar;
                param.Add("@StockCount", newlocationstock);
                if (BomList.First().ParentId!=null)
                {
                    param.Add("@ParentId", BomList.First().ParentId);

                    string sql4c = $@"select UretimDetay.id,UretimDetay.UretimId,UretimDetay.StokId,UretimDetay.PlanlananMiktar from UretimDetay 
                    left join Uretim on Uretim.id=UretimDetay.UretimId
                    where Uretim.id=@ParentId and UretimDetay.StokId=@StokId and";
                    var sorgu9 = await _db.QueryAsync<UretimMake>(sql4c, param);
                    foreach (var parent in sorgu9)
                    {
                        param.Add("DepoId",BomList.First().DepoId);

                        var stokmiktari = await _control.Count(sorgu4.First().StokId, BomList.First().DepoId);
                        stokmiktari = stokmiktari >= 0 ? stokmiktari : 0;

                        param.Add("@Manuid", parent.OrderId);
                        param.Add("@Manuitemid", parent.id);
                        string sqld = $@"select id,ISNULL(RezerveDeger,0) as RezerveDeger from Rezerve where UretimId=@Manuid AND UretimDetayId=@Manuitemid and StokId=@StokId and DepoId=@DepoId and Durum=1";
                        
                        var rezervestockCount = await _db.QueryAsync<LocaVarmı>(sqld, param);
                        var rezervemiktar = rezervestockCount.First().RezerveDeger;
                        if (rezervemiktar >= parent.PlanlananMiktar)
                        {
                            param.Add("@RezerveCount", parent.PlanlananMiktar);
                            param.Add("@MalzemeDurum", 2);
                        }
                        else if (rezervemiktar+ ManufacturingQuantity>=parent.PlanlananMiktar)
                        {
                            param.Add("@RezerveCount", parent.PlanlananMiktar);
                            param.Add("@MalzemeDurum", 2);
                        }
                        else if (rezervemiktar+ ManufacturingQuantity+stokmiktari>=parent.PlanlananMiktar)
                        {
                            param.Add("@RezerveCount", parent.PlanlananMiktar);
                            param.Add("@MalzemeDurum", 2);
                        }
                        else
                        {
                            param.Add("@RezerveCount", rezervemiktar+stokmiktari+ManufacturingQuantity);
                            param.Add("@MalzemeDurum", 0);
                        }
                        await _db.ExecuteAsync($"Update Rezerve set RezerveDeger=@RezerveCount where  and UretimId=@Manuid and UretimDetayId=@Manuitemid  and Durum=1 and StokId=@StokId ", param);
                        await _db.ExecuteAsync($"Update UretimDetay set MalzemeDurum=@MalzemeDurum where OrderId=@Manuid and id=@Manuitemid and Itemıd=@StokId and ", param);

                    }

                }
                param.Add("@LocationStockId", sorgu4.First().DepoStokId);





                await _db.ExecuteAsync($"Update DepoStoklar Set StokAdeti=@StockCount where id=@LocationStockId ", param);
                await _db.ExecuteAsync($"Update Uretim Set Durum=@Durum where id=@id", param);


            }
            else if (T.Durum != 3 && eskiStatus == 3 && T.Durum != eskiStatus)
            {
                foreach (var item in BomList)
                {
                    if (item.Tip == "Ingredients")
                    {
                        param.Add("@StokId", item.StokId);
                        param.Add("@DepoId", item.DepoId);

                        string sqla = $@"    select 
                      (Select ISNULL(StokAdeti,0) from DepoStoklar where StokId =@StokId
                      and DepoId = @DepoId )as StokAdeti,
                      (Select ISNULL(id,0) from DepoStoklar where StokId =@StokId
                      and DepoId = @DepoId ) as  DepoStokId";
                        param.Add("@OrderItemId", item.id);
                        var sorgu = await _db.QueryAsync<StockAdjusmentStockUpdate>(sqla, param);//
                        var stockId = sorgu.First().StokId;
                        param.Add("@stockId", stockId);

                        float? stockQuantity = sorgu.First().Miktar;



                        float? stockCount = sorgu.First().StokAdeti;
                        float? NewStockCount = stockCount + item.PlanlananMiktar;
                        var stocklocationId = sorgu.First().DepoStokId;
                        param.Add("@ManufacturingOrderItemsId", item.id);
                        param.Add("@stocklocationId", stocklocationId);
                        param.Add("@NewStockCount", NewStockCount);
                        //Yeni count değerini tabloya güncelleştiriyoruz.
                        await _db.ExecuteAsync($"Update DepoStoklar SET StokAdeti =@NewStockCount where id = @stocklocationId ", param);
                        param.Add("@MalzemeDurum", 2);
                        await _db.ExecuteAsync($"Update UretimDetay Set MalzemeDurum=@MalzemeDurum where id=@OrderItemId ", param);
                        param.Add("@Statu", 1);
                        param.Add("@RezerveId", item.RezerveId);
                        await _db.ExecuteAsync($"Update Rezerve Set Durum=@Statu where id=@RezerveId ", param);

                    }
                    else
                    {

                        param.Add("@ManuId", item.id);
                        param.Add("@Durum", T.Durum);
                        await _db.ExecuteAsync($"Update UretimDetay Set Durum=@Durum where id=@ManuId", param);

                    }

                }


                string sqlc = $@"select  
                 (Select ISNULL(StokAdeti,0) from DepoStoklar where StokId = (select Uretim.StokId from Uretim where id=@id)
                 and DepoId = (Select ISNULL(DepoId,0) From Uretim where id =  @id)
                 )as StokAdeti ,(Select Uretim.PlanlananMiktar from Uretim where  and id=@id)as PlanlananMiktar,
                 (Select ISNULL(id,0) from DepoStoklar where StokId = (select Uretim.StokId from Uretim
                 where id=@id)
                 and DepoId = (Select ISNULL(DepoId,0) From Uretim where id =  @id)
                ) as DepoStokId,  
                (Select Uretim.StokId from Uretim where  id=@id)as StokId ";
                var sorgu4 = await _db.QueryAsync<StockAdjusmentSql>(sqlc, param);
                var ManufacturingQuantity = sorgu4.First().PlanlananMiktar;
                var AllStock = sorgu4.First().Miktar;
                var newstock = AllStock - ManufacturingQuantity;
                var DepoStoklar = sorgu4.First().StokAdeti;
                var newlocationstock = ManufacturingQuantity - DepoStoklar;
                param.Add("@StockCount", newlocationstock);
                param.Add("@AllStockQuantity", newstock);
                param.Add("@StokId", sorgu4.First().StokId);
                param.Add("@locationId", sorgu4.First().DepoStokId);



                await _db.ExecuteAsync($"Update DepoStoklar Set StokAdeti=@StockCount where id=@locationId", param);
                await _db.ExecuteAsync($"Update Uretim Set Durum=@Durum where id=@id", param);


                await _db.ExecuteAsync($"Update Uretim Set Durum=@Durum where id=@id", param);
            }
            else
            {
                await _db.ExecuteAsync($"Update Uretim Set Durum=@Durum where id=@id", param);
            }
        }
        public async Task BuyStockControl(PurchaseOrderInsert T, int? missing)
        {
            int? DepoId = T.DepoId;
            DynamicParameters param = new DynamicParameters();
            param.Add("@Tip", "Ingredients");
            param.Add("@UretimDetayId", T.UretimDetayId);
            param.Add("@OrderId", T.UretimId);
            param.Add("@StokId", T.StokId);
            param.Add("@DepoId", DepoId);
            // materyalin VarsayilanFiyat
            // Bul

             var expectedsorgu = await _db.QueryAsync<LocaVarmı>($@"select ISNULL(SUM(Miktar),0) as Miktar from SatinAlma 
                left join SatinAlmaDetay on SatinAlmaDetay.SatinAlmaId = SatinAlma.id and SatinAlma.DepoId={T.DepoId}
                and SatinAlmaDetay.StokId = {T.StokId} where  DurumBelirteci = 1 and SatinAlma.UretimId= {T.UretimId} and SatinAlma.UretimDetayId={T.UretimDetayId}  and SatinAlma.Aktif=1");

            float? expected = expectedsorgu.First().Miktari;

           var sorgu = await _db.QueryAsync<LocaVarmı>($"   select  (Select ISNULL(id, 0) from DepoStoklar where StokId =@StokId and DepoId = @DepoId )   as DepoStokId, (select ISNULL(VarsayilanFiyat, 0) From Urunler where id = @StokId)as  VarsayilanFiyat,(Select UretimDetay.[MalzemeDurum] from UretimDetay where Tip='Ingredients' and id=@UretimDetayId and UretimId=@OrderId and StokId=@StokId)as [MalzemeDurum]", param);
            float VarsayilanFiyat = sorgu.First().VarsayilanFiyat;
            int MalzemeDurum = sorgu.First().MalzemeDurum;
            float locationstock = await _control.Count(T.StokId, T.DepoId);
            locationstock = locationstock >= 0 ? locationstock : 0;

            param.Add("@Tutar", VarsayilanFiyat * T.Miktar);
            param.Add("@LocationStockId", sorgu.First().DepoStokId);


            var Count = await _db.QueryAsync<LocaVarmı>($"Select ISNULL(Rezerve.RezerveDeger,0)as RezerveDeger from Rezerve where  StokId=@StokId and UretimId=@OrderId and UretimDetayId=@UretimDetayId and Rezerve.Durum=1", param);
         
           var Counts = Count.Count()>0 ? Count.First().RezerveDeger :0;

            

            if (MalzemeDurum == 2)
            {
                param.Add("@MalzemeDurum", 2);
                param.Add("@RezerveCount", Counts);

            }
            else if (missing * (-1) <= expected && expected > 0)
            {
                param.Add("@MalzemeDurum", 1);
                param.Add("@RezerveCount", Counts);
            }

            else
            {
                param.Add("@RezerveCount", Counts + locationstock);
                param.Add("@MalzemeDurum", 0);
            }


            param.Add("@Durum", 1);

            await _db.ExecuteAsync($"Update Rezerve set  Tip=@Tip,StokId=@StokId,RezerveDeger=@RezerveCount,DepoId=@DepoId,Durum=@Durum where  UretimId=@OrderId and StokId=@StokId and UretimDetayId=@UretimDetayId ", param);


            string sql = $@"Update UretimDetay Set Tip=@Tip,StokId=@StokId,Tutar=@Tutar,MalzemeDurum=@MalzemeDurum where   UretimId=@OrderId and StokId=@StokId and id=@UretimDetayId  ";
            await _db.ExecuteAsync(sql, param);

        }







    }
}
