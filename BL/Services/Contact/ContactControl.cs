using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services.Contact
{
    public class ContactControl : IContactControl
    {
        private readonly IDbConnection _db;

        public ContactControl(IDbConnection db)
        {
            _db = db;
        }

        public async Task<string> Update(ContactDTO.ContactsList T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@CompanyId", CompanyId);
            string sql = $"Select id from Contacts where id=@id and CompanyId = @CompanyId";
            var kontrol = await _db.QueryAsync<int>(sql, param);
            if (kontrol.Count() == 0)
            {
                return ("Boyle bir id yok.");
            }
            var tipbul = await _db.QueryAsync<ItemDTO.Items>($"Select Tip from Contacts where CompanyId = @CompanyId and id = @id", param);
            string? tip = tipbul.First().Tip;
            if (T.Tip==tip)
            {
                return ("true");
            }
            else
            {
                return ("Tip uyuşmazlığı.Tip hatası.");
            }

        }

        public async Task<string> UpdateAddress(ContactDTO.ContactsUpdateAddress T, int CompanyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            param.Add("@CompanyId", CompanyId);
            string sql = $"Select id from Locations where id=@id and CompanyId = @CompanyId";
            var kontrol = await _db.QueryAsync<int>(sql, param);
            if (kontrol.Count() == 0)
            {
                return (" Boyle bir id yok.");
            }
            var tipbul = await _db.QueryAsync<ItemDTO.Items>($"Select Tip from Locations where CompanyId = @CompanyId and id = @id", param);
            string? tip = tipbul.First().Tip;
            if (T.Tip == tip)
            {
                return ("true");
            }
            else
            {
                return ("Tip uyuşmazlığı.Tip hatası.");
            }
        }
    }
}
