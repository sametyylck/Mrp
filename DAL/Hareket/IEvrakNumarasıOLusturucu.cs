using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Hareket
{
    public interface IEvrakNumarasıOLusturucu
    {
        Task<string> Olustur(int id);
    }
}
