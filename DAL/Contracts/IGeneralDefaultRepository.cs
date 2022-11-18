using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.GeneralSettingsDTO;

namespace DAL.Contracts
{
    public interface IGeneralDefaultRepository
    {
        Task Register(int id, int taxid, int locationid);
        Task Update(GeneralDefaultSettings T, int CompanyId);
        Task<IEnumerable<DefaultSettingList>> List(int CompanyId);
    }
}
