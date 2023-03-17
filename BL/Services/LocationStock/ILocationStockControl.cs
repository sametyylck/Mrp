using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ItemDTO;

namespace BL.Services.LocationStock
{
    public interface ILocationStockControl
    {
        Task Kontrol(int? ItemId,int LocationId,string Tip);
        Task<List<string>> AdresStokKontrol(int? ItemId, int OriginId, int DesId, float? Quantity);

    }
}
