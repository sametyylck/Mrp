using DAL.DTO;
using DAL.Models;
using DAL.StockControl;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockTransferDTO;

namespace BL.Services.LocationStock
{
    public class LocationStockControl : ILocationStockControl
    {
        private readonly IDbConnection _db;
        private readonly IStockControl _control;

        public LocationStockControl(IDbConnection db, IStockControl control)
        {
            _db = db;
            _control = control;
        }

        public async Task Kontrol(int? ItemId, int LocationId,string Tip)
        {
            string sqlf = $@"select * from DepoStoklar where StokId={ItemId} and DepoId={LocationId}";
            var sorgu = await _db.QueryAsync<LocationStockDTO>(sqlf);
            if (sorgu.Count()==0)
            {
                DynamicParameters prm = new DynamicParameters();
                prm.Add("@Tip", Tip);
                prm.Add("@LocationId", LocationId);
                prm.Add("@StockCount", 0);
                prm.Add("@ItemId", ItemId);
                prm.Add("@IsActive", true);
                await _db.ExecuteAsync($"Insert into DepoStoklar (Tip,DepoId,StokId,StokAdeti,Aktif) values (@Tip,@LocationId,@ItemId,@StockCount,@IsActive)", prm);
            }
        }

        public async Task<List<string>> AdresStokKontrol(int? ItemId, int OriginId,int DesId,float? Quantity)
        {
            List<string> hatalar = new();
            var origincount = await _control.Count(ItemId, OriginId);
            var DesCount = await _control.Count(ItemId, DesId);
            if (origincount>0)
            {
                if (origincount-Quantity<0)
                {
                    string hata = "Girdiğiniz miktar stok miktarini aşıyor";
                    hatalar.Add(hata);
                }
            }
            else
            {
                string hata = "Adreste kullanılabilir stok bulunmamaktadir.";
                hatalar.Add(hata);
            }

            return hatalar;
        }

    }
}
