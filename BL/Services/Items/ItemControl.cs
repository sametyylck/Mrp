using BL.Services.UserService;
using DAL.DTO;
using DAL.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ItemDTO;

namespace BL.Services.Items
{
    public class ItemControl : IItemsControl
    {
        private readonly IUserService _user;
        private readonly IDbConnection _db;

        public ItemControl(IUserService user, IDbConnection db)
        {
            _user = user;
            _db = db;
        }

        public async Task<List<string>> Delete(ItemsDelete T)
        {
            List<string> hatalar = new();
            string sql = $"select(Select id from Urunler where id={T.id})as id,(Select Tip from Urunler where id={T.id})as Tip";
            var kontrol = await _db.QueryAsync<PurchaseOrderDTO.PurchaseOrderLogsList>(sql);
            if (kontrol.First().id == null)
            {
                hatalar.Add("Boyle bir id yok");
            }
            if (T.Tip==kontrol.First().Tip)
            {
                return hatalar;
            }
            else
            {
                hatalar.Add("Tip uyusmazlıgi hatasi");
                return hatalar;
            }


        }

        public async Task<List<string>> Insert(ItemsInsert T)
        {
            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@MeasureId", T.OlcuId);
            prm.Add("@CategoryId", T.KategoriId);
            prm.Add("@ContactId", T.TedarikciId);


            if (T.Tip == "Material" || T.Tip == "Product" || T.Tip == "SemiProduct")
            {
                var tipbul = await _db.QueryAsync<ItemsInsert>($@"select
                (select id from Olcu where id=@MeasureId)as OlcuId,
                (select id from Kategoriler where id=@CategoryId)as KategoriId,
                (select CariKod from Cari where CariKod=@ContactId ) as TedarikciId", prm);
                var measureid = tipbul.First().OlcuId;
                var categoryid = tipbul.First().KategoriId;
                var contactid = tipbul.First().TedarikciId;
          
                if (T.TedarikciId!=0)
                {
                    if (T.TedarikciId!=null)
                    {
                        if (contactid == null)
                        {
                            hatalar.Add("ContactId bulunamadı");
                        }
                    }
                  
                }
                else if(T.TedarikciId==0)
                {
                    T.TedarikciId = null;
                }
           
                if (measureid == null)
                {
                    hatalar.Add("measureid bulunamadi");
                }
                if (T.KategoriId !=0)
                {
                    if(T.KategoriId!=null)
                    {
                        if (categoryid == null)
                        {
                            hatalar.Add("Categoryid bulunamadı");
                        }
                        else
                        {
                            return hatalar;
                        }
                    }
              
                }
                return hatalar;

            }
            hatalar.Add("Tip hatası");
            return hatalar;
        }

    }
}
