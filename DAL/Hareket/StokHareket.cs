using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.StokHareket
{
    public class StokHareket : IStokHareket
    {
        private readonly IDbConnection _db;

        public StokHareket(IDbConnection db)
        {
            _db = db;
        }

        public async Task StokHareketInsert(StokHareketDTO T, int KullanıcıId)
        {
            DynamicParameters prm = new();
            prm.Add("@Giris", T.Giris);
            prm.Add("@EvrakNo", T.EvrakNo);
            prm.Add("@EvrakTipi", T.EvrakTipi); 
            prm.Add("@DepoId", T.DepoId);
            prm.Add("@AraToplam", T.AraToplam);
            prm.Add("@BirimFiyat", T.BirimFiyat);
            prm.Add("@OlcuId", T.OlcuId);
            prm.Add("@KDVOrani", T.KDVOrani);
            prm.Add("@Kur", T.Kur);
            prm.Add("@Miktar", T.Miktar);
            prm.Add("@ParaBirimiId", T.ParaBirimiId);
            prm.Add("@StokAd", T.StokAd);
            prm.Add("@StokId", T.StokId);
            prm.Add("@StokKodu", T.StokKodu);
            prm.Add("@SubeId", T.SubeId);
            prm.Add("@Tutar", T.Tutar);
            prm.Add("@KDVTutari", T.KDVTutari);
            prm.Add("@Tarih", DateTime.Now);

            prm.Add("@KullaniciId", KullanıcıId);


            await _db.ExecuteAsync("Insert into StokHareket (Tarih,Giris,EvrakNo,EvrakTipi,DepoId,AraToplam,BirimFiyat,BirimId,KDVOrani,KDVTutari,Kur,Miktar,ParaBirimiId,StokAd,StokId,StokKodu,SubeId,Tutar,KullaniciId) values (@Tarih,@Giris,@EvrakNo,@EvrakTipi,@DepoId,@AraToplam,@BirimFiyat,@OlcuId,@KDVOrani,@KDVTutari,@Kur,@Miktar,@ParaBirimiId,@StokAd,@StokId,@StokKodu,@SubeId,@Tutar,@KullaniciId)", prm);
            
        }

        public Task StokHareketUpdate(StokHareketDTO T, int KullanıcıId)
        {
            throw new NotImplementedException();
        }
    }
}
