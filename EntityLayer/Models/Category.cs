using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Category
    {
        public Category()
        {
            Items = new HashSet<Item>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? CompanyId { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? DeletedUser { get; set; }

        public virtual Company? Company { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }
}
