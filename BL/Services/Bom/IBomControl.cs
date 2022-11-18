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
        Task<string> Insert(BOMInsert T, int CompanyId);
        Task<string> Update(BOMUpdate T, int CompanyId);
    }
}
