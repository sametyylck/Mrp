using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ItemDTO;

namespace BL.Services.Items
{
    public interface IItemsControl
    {
        Task<List<string>> Insert(ItemsInsert T, int CompanyId);
        Task<List<string>> Delete(ItemsDelete T, int CompanyId);

    }
}
