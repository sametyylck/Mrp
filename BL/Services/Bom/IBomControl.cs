using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.BomDTO;

namespace BL.Services.Bom
{
    public interface IBomControl
    {
        Task<List<string>> Insert(BOMInsert T);
        Task<List<string>> Update(BOMUpdate T);
    }
}
