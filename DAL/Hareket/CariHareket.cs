using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Hareket
{
    public class CariHareket : ICariHareket
    {
        private readonly IDbConnection _db;
        public CariHareket(IDbConnection db)
        {
            _db = db;
        }

        public async Task CariHareketInsert(CariHareketDTO T, int KullanıcıId)
        {
            DynamicParameters prm = new();
            prm.Add("@CariAd", T.CariAdSoyad);
            prm.Add("@EvrakNo", T.EvrakNo);
            prm.Add("@EvrakTipi", T.EvrakTipi);
            prm.Add("@DepoId", T.DepoId);
            prm.Add("@AraToplam", T.AraToplam);
            prm.Add("@CariKod", T.CariKod);
            prm.Add("@BugunTarih", DateTime.Now);
            prm.Add("@KDVTutari", T.KDVTutari);
            prm.Add("@Kur", T.Kur);
            prm.Add("@VadeTarihi", T.VadeTarihi);
            prm.Add("@ParaBirimiId", T.ParaBirimiId);
            prm.Add("@SubeId", T.SubeId);
            prm.Add("@Tutar", T.Tutar);
            prm.Add("@KullaniciId", KullanıcıId);


            await _db.ExecuteAsync("Insert into CariHareket (CariAd,EvrakNo,EvrakTipi,DepoId,AraToplam,CariKod,Tarih,KDVTutari,Kur,VadeTarihi,ParaBirimiId,SubeId,Tutar,KullaniciId) values (@CariAd,@EvrakNo,@EvrakTipi,@DepoId,@AraToplam,@CariKod,@BugunTarih,@KDVTutari,@Kur,@VadeTarihi,@ParaBirimiId,@SubeId,@Tutar,@KullaniciId)",prm);
        }
    }
}
