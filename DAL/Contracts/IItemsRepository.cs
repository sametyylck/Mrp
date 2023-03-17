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
        Task<int> Insert(ItemsInsert T,int UserId);
        Task Update(ItemsUpdate T);
        Task Delete(ItemsDelete T);
        Task<IEnumerable<ListItems>> Detail(int id);
        Task<IEnumerable<ItemsListele>> ListProduct(ItemsListele T, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<ItemsListele>> ListSemiProduct(ItemsListele T, int KAYITSAYISI, int SAYFA);
        Task<IEnumerable<ItemsListele>> ListMaterial( ItemsListele T, int KAYITSAYISI, int SAYFA);
    }
}
