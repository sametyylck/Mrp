using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Currency
    {
        public Currency()
        {
            GeneralDefaultSettings = new HashSet<GeneralDefaultSetting>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }

        public virtual ICollection<GeneralDefaultSetting> GeneralDefaultSettings { get; set; }
    }
}
