using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ProductOperationsBomDTO;

namespace BL.Services.OperationsBom
{
    public interface IOperationBomControl
    {
        Task<List<string>> Insert(ProductOperationsBOMInsert T, int CompanyId);
        Task<List<string>> Update(ProductOperationsBOMUpdate T, int CompanyId);
    }
}
