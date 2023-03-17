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
        Task<List<string>> Update(CariUpdate T);
        Task<List<string>> UpdateAddress(ContactsUpdateAddress T);


    }
}
