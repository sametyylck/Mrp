using DAL.DTO;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ContactDTO;

namespace DAL.Contracts
{
    public interface IContactsRepository
    {
        Task<IEnumerable<ContactsFilters>> List(ContactsFilters T,int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<ContactsAll>> Details(int id);
        Task<int> Insert(ContactsInsert T, int KullaniciId);
        Task<int> InsertAddress(string Tip);
        Task UpdateAddress(ContactsUpdateAddress T, int id);
        Task Update(CariUpdate T);
        Task Delete(ContactsDelete T);
    }
}
