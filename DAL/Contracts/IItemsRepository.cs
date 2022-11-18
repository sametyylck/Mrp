using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ItemDTO;

namespace DAL.Contracts
{
    public interface IItemsRepository
    {
        Task<int> Insert(ItemsInsert T, int CompanyId);
        Task Update(ItemsUpdate T, int CompanyId);
        Task Delete(ItemsDelete T, int CompanyId);
        Task<IEnumerable<ListItems>> Detail(int id, int CompanyId);
        Task<IEnumerable<ItemsListele>> ListProduct(int CompanyId, ItemsListele T, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<ItemsListele>> ListSemiProduct(int CompanyId, ItemsListele T, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<ItemsListele>> ListMaterial(int CompanyId, ItemsListele T, int KAYITSAYISI, int SAYFA);
        Task<int> Count(ItemsListele T, int CompanyId);
    }
}
