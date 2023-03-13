using DAL.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Extensions
{
    public class PermissionControl : IPermissionControl
    {
        private readonly IDbConnection _db;

        public PermissionControl(IDbConnection db)
        {
            _db = db;
        }

        public async Task<bool> Kontrol(Permison permison, Permison permisons, int CompanyId, int UserId)
        {
            string sql1 = $@"Select RoleId from Users where CompanyId={CompanyId} and id={UserId}";
            var RoleId = await _db.QueryFirstAsync<int>(sql1);

            string sql2 = $@"Select Count(id) from Permision where CompanyId={CompanyId} and RoleId={RoleId} and PermisionId='{((int)permison)}' or  PermisionId='{((int)Permison.Hepsi)}' or PermisionId='{((int)permisons)}'";
            var roller = await _db.QueryFirstAsync<bool>(sql2);
            return roller;


        }
    }
    public enum Permison
    {
        Hepsi=1,
        UretimHepsi,
        SatisHepsi,
        AyarlarHepsi,
        SatinAlmaHepsi,
        StokTransferHepsi,
        AyarlarGenel,
        AyarlarOlcü,
        AyarlarVergi,
        AyarlarKategori,
        AyarlarOperasyonlar,
        AyarlarKaynaklar,
        AyarlarAdresler,
        IletisimHepsi,
        IletisimGoruntule,
        IletisimEkleyebilirVeDuzenleyebilir,
        IletisimSil,
        UretimSilebilir,
        UretimGoruntule,
        UretimEkleyebilirVeDuzenleyebilir,
        UretimTamamlama,
        SatisEkleyebilirVeDuzenleyebilir,
        SatisSilebilir,
        SatisGoruntule,
        SatisTamamlama,
        SatinAlmaGoruntule,
        SatinAlmaEkleyebilirVeDuzenleyebilir,
        SatinAlmaSilebilir,
        ItemSilebilir,
        ItemGoruntule,
        ItemEkleyebilirVeGuncelleyebilir,
        StokTransferEkle,
        StokTransferGoruntule,
        StokTransferSilebilir,
        StokDuzenlemeGoruntule,
        StokDuzenlemeEkleyebilirVeGuncelleyebilir,
        StokDuzenlemeSilme,
        ItemlerHepsi,
        SatinAlmaTamamlama,
        TeklifOnaylama,
        TeklifHepsi,
        TeklifEkleyebilirVeDuzenleyebilir,
        TeklifSilebilir,
        TeklifGoruntule,
        StokDuzenlemeHepsi,


    };
}
