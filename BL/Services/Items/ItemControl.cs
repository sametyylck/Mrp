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

        public async Task<List<string>> Delete(ItemsDelete T, int CompanyId)
        {
            List<string> hatalar = new();
            string sql = $"select(Select id from Items where id={T.id} and CompanyId = {CompanyId})as id,(Select Tip from Items where id={T.id} and CompanyId = {CompanyId})as Tip";
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

        public async Task<List<string>> Insert(ItemsInsert T, int CompanyId)
        {
            List<string> hatalar = new();
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@MeasureId", T.MeasureId);
            prm.Add("@CategoryId", T.CategoryId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@ContactId", T.ContactId);


            if (T.Tip == "Material" || T.Tip == "Product" || T.Tip == "SemiProduct")
            {
                var tipbul = await _db.QueryAsync<ItemsInsert>($@"select
                (select id from Measure where id=@MeasureId and CompanyId=@CompanyId)as MeasureId,
                (select id from Categories where id=@CategoryId and CompanyId=@CompanyId)as CategoryId,
                (select id from Contacts where id=@ContactId and CompanyId=@CompanyId) as ContactId", prm);
                var measureid = tipbul.First().MeasureId;
                var categoryid = tipbul.First().CategoryId;
                var contactid = tipbul.First().ContactId;
          
                if (T.ContactId!=0)
                {
                    if (T.ContactId!=null)
                    {
                        if (contactid == null)
                        {
                            hatalar.Add("ContactId bulunamadı");
                        }
                    }
                  
                }
                else if(T.ContactId==0)
                {
                    T.ContactId = null;
                }
           
                if (measureid == null)
                {
                    hatalar.Add("measureid bulunamadi");
                }
                if (T.CategoryId !=0)
                {
                    if(T.CategoryId!=null)
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
