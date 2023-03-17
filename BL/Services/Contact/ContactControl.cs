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

        public async Task<List<string>> Update(ContactDTO.CariUpdate T)
        {
            List<string> list = new List<string>();
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.CariKod);
            string sql = $"Select CariKod from Cari where CariKod=@id";
            var kontrol = await _db.QueryAsync<int>(sql, param);
            if (kontrol.Count() == 0)
            {
                list.Add("Boyle bir CariKod yok.");
            }

            return list;


        }

        public async Task<List<string>> UpdateAddress(ContactDTO.ContactsUpdateAddress T)
        {
            List<string> list = new();
            DynamicParameters param = new DynamicParameters();
            param.Add("@id", T.id);
            string sql = $"Select id from DepoVeAdresler where id=@id ";
            var kontrol = await _db.QueryAsync<int>(sql, param);
            if (kontrol.Count() == 0)
            {
                list.Add(" Boyle bir id yok.");
            }
            var tipbul = await _db.QueryAsync<ItemDTO.Items>($"Select Tip from DepoVeAdresler where  id = @id", param);
            string? tip = tipbul.First().Tip;
            if (T.Tip == tip)
            {
                return list;
            }
            else
            {
                list.Add("Tip uyuşmazlığı.Tip hatası.");
                return list;

            }
        }
    }
}
