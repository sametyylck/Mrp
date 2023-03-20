using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Hareket
{
    public class EvrakNumarasıOlusturucu : IEvrakNumarasıOLusturucu
    {
        public async Task<string> Olustur(int id)
        {
            var tarih = DateTime.Now;
            var day = tarih.Day;
            var month=tarih.Month;  
            Random rdn = new();
           var sayi=rdn.Next(10000, 99999);
            string evrak = $"{day}{month}{id}{sayi}";
            return evrak;
        }
    }
}
