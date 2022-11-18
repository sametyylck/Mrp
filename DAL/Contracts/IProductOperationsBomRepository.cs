using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ProductOperationsBomDTO;

namespace DAL.Contracts
{
    public interface IProductOperationsBomRepository
    {
        Task<IEnumerable<ProductOperationsBOMList>> List(int CompanyId, int ItemId);
        Task<int> Insert(ProductOperationsBOMInsert T, int CompanyId);
        Task Update(ProductOperationsBOMUpdate T, int CompanyId);
        Task Delete(IdControl T, int CompanyId);
    }
}
