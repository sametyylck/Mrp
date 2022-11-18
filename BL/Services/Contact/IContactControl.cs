using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ContactDTO;

namespace BL.Services.Contact
{
    public interface IContactControl
    {
        Task<string> Update(ContactsList T, int CompanyId);
        Task<string> UpdateAddress(ContactsUpdateAddress T, int CompanyId);


    }
}
