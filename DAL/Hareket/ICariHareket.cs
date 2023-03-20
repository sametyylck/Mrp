using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Hareket
{
    public interface ICariHareket
    {
        Task CariHareketInsert(CariHareketDTO T,int KullanıcıId);
    }
}
