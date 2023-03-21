using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.BomDTO;

namespace DAL.Contracts
{
    public interface IBomRepository
    {
        Task<IEnumerable<ListBOM>> List(int ProductId);
        Task<int> Insert(BOMInsert T);
        Task Update(BOMUpdate T);

        Task Delete(IdControl T);
    }
}
