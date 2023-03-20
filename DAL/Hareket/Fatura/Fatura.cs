using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Hareket.Fatura
{
    public class Fatura : IFatura
    {
        private readonly IDbConnection _db;

        public Fatura(IDbConnection db)
        {
            _db = db;
        }

        public async Task FaturaOlustur(FaturaDTO T,int KullanıcıId)
        {
            DynamicParameters prm = new();
            prm.Add("@CariAd", T.CariAd);
            prm.Add("@EvrakNo", T.EvrakNo);
            prm.Add("@EvrakTipi", T.EvrakTipi);
            prm.Add("@DepoId", T.DepoId);
            prm.Add("@AraToplam", T.AraToplam);
            prm.Add("@CariKod", T.CariKod);
            prm.Add("@Tarih", T.FaturaTarihi);
            prm.Add("@KDVTutari", T.KDVTutari);
            prm.Add("@Kur", T.OlusturmaTarihi);
            prm.Add("@ParaBirimiId", T.ParaBirimiId);
            prm.Add("@SubeId", T.SubeId);
            prm.Add("@Tutar", T.GenelToplam);
            prm.Add("@KullaniciId", KullanıcıId);


            await _db.ExecuteAsync("Inser into StokHareket (CariAd,EvrakNo,EvrakTipi,DepoId,AraToplam,CariKod,Tarih,KDVTutari,Kur,VadeTarihi,ParaBirimiId,SubeId,Tutar,KullaniciId) values (@CariAd,@EvrakNo,@EvrakTipi,@DepoId,@AraToplam,@CariKod,@Tarih,@KDVTutari,@Kur,@VadeTarihi,@ParaBirimiId,@SubeId,@Tutar,@KullaniciId)");
        }
    }
}
