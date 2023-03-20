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
            prm.Add("@KullaniciId", KullanıcıId);


            await _db.ExecuteAsync("Insert into StokHareket (Giris,EvrakNo,EvrakTipi,DepoId,AraToplam,BirimFiyat,BirimId,KDVOrani,Kur,Miktar,ParaBirimiId,StokAd,StokId,StokKodu,SubeId,Tutar,KullaniciId) values (@Giris,@EvrakNo,@EvrakTipi,@DepoId,@AraToplam,@BirimFiyat,@OlcuId,@KDVOrani,@Kur,@Miktar,@ParaBirimiId,@StokAd,@StokId,@StokKodu,@SubeId,@Tutar,@KullaniciId)",prm);
            
        }
    }
}
