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
        Task<IEnumerable<ContactsFilters>> List(ContactsFilters T, int CompanyId, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<ContactsAll>> Details(int id, int CompanyId);
        Task<int> Insert(ContactsInsert T, int CompanyId);
        Task<int> InsertAddress(int CompanyId, string Tip);
        Task UpdateAddress(ContactsUpdateAddress T, int CompanyId, int id);
        Task Update(ContactsList T, int CompanyId,int id);

        Task Delete(ContactsDelete T, int CompanyId);
        Task<int> Count(ContactsFilters T, int CompanyId);
    }
}
