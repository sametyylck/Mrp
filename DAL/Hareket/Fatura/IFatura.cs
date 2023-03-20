using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Hareket.Fatura
{
    public interface IFatura
    {
        Task FaturaOlustur(FaturaDTO T,int KullanıcıId);
    }
}
