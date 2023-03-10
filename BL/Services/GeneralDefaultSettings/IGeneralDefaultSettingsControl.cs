using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.GeneralSettingsDTO;

namespace BL.Services.GeneralDefaultSettings
{
    public interface IGeneralDefaultSettingsControl
    {
        Task<List<string>> Update(GeneralSettingsDTO.GeneralDefaultSettings T, int CompanyId);
    }
}
