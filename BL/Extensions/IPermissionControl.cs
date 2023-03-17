using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Extensions
{
    public interface IPermissionControl
    {
        Task<bool> Kontrol(Permison permison, Permison permisons, int UserId);
    }
}
