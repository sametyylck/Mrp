using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services.IdControl
{
    public interface IIDControl
    {
        Task<string> GetControl(string tabloadi,int id,int companyid);
    }
}
