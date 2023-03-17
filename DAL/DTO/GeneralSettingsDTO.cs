using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class GeneralSettingsDTO
    {
        public class GeneralDefaultSettings
        {
            public int id { get; set; }
      
            public int? ParaBirimiId { get; set; }
     
            public int? VarsayilanSatis { get; set; }
      
            public int? VarsayilanSatinAlim { get; set; }
     
            public int? VarsayilanSatisVergi { get; set; }
        
            public int? VarsayilanSatinAlimVergi { get; set; }
       
            public int? VarsayilanSatisDepo { get; set; }
         
            public int? VarsayilanSatinAlimDepo { get; set; }
       
            public int? VarsayilanUretimDepo { get; set; }

        }
        public class DefaultSettingList
        {
            public int id { get; set; }
            public int CurrencyId { get; set; }
            public string ParaBirimiIsmi { get; set; } = string.Empty;
            public int VarsayilanSatis { get; set; }
            public int VarsayilanSatinAlim { get; set; }
            public int VarsayilanSatisVergi { get; set; }
            public string SatisVergiIsmi { get; set; } = string.Empty;
            public float SatisVergiDegeri { get; set; }
            public int SatinAlimVergiId { get; set; }
            public string SatinAlimVergiIsmi { get; set; } = string.Empty;
            public float SatinAlimVergiDegeri { get; set; }
            public int SatisDepoId { get; set; }
            public string SatisDepoIsim { get; set; } = string.Empty;
            public int SatinAlimDepoId { get; set; }
            public string SatinAlimDepoIsim { get; set; } = string.Empty;
            public int UretimDepoId { get; set; }
            public string UretimDepoIsim { get; set; } = string.Empty;
        }
    }
}
