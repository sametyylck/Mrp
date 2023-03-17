using DAL.DTO;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface IMeasureRepository
    {
        Task Register(int UserId);
        Task<IEnumerable<MeasureDTO>> List();
        Task<int> Insert(MeasureInsert T, int UserId);
        Task Update(MeasureUpdate T);
        Task Delete(IdControl T);
    }
}
