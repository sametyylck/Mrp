using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services.OperationsBom
{
    public class OperationBomControl : IOperationBomControl
    {
        private readonly IDbConnection _db;

        public OperationBomControl(IDbConnection db)
        {
            _db = db;
        }

        public async Task<List<string>> Insert(ProductOperationsBomDTO.ProductOperationsBOMInsert T)
        {
            List<string> hatalar = new();
            var resoruce = await _db.QueryAsync<int>($"Select id From Kaynaklar where id = {T.KaynakId} and Aktif=1");
            if (resoruce.Count() == 0)
            {
                hatalar.Add("ResourceId bulunamiyor.");
            }
            var operationId = await _db.QueryAsync<int>($"Select id From Kaynaklar where  id = {T.OperasyonId} and Aktif=1");
            if (operationId.Count() == 0)
            {
                hatalar.Add("OperationId bulunamıyor..");
            }
            string Tip = await _db.QueryFirstAsync<string>($"Select Tip  From Urunler where id={T.StokId}");
            if (Tip == "Product" || Tip == "SemiProduct")
            {
                return hatalar;
            }
            else
            {
                hatalar.Add("Make tip hatası");
                return hatalar;
            }
        }

        public async Task<List<string>> Update(ProductOperationsBomDTO.ProductOperationsBOMUpdate T)
        {
            List<string> hatalar = new();

            var LocationVarmi = await _db.QueryAsync<int>($"Select Count(*) as varmi From UrunKaynakRecetesi where id = {T.id} and Aktif=1 ");
            if (LocationVarmi.Count() == 0)
            {
                hatalar.Add("id,Böyle Bir id Yok");
            }
            var resoruce = await _db.QueryAsync<int>($"Select id From Kaynaklar where id = {T.KaynakId} and Aktif=1");
            if (resoruce.Count() == 0)
            {
                hatalar.Add("ResourceId bulunamiyor.");
            }
            var operationId = await _db.QueryAsync<int>($"Select id From Operasyonlar where  id = {T.OperasyonId} and Aktif=1");
            if (operationId.Count() == 0)
            {
                hatalar.Add("OperationId bulunamıyor..");
                return hatalar;
            }
            else
            {
                return hatalar;
            }
        }
    }
}
