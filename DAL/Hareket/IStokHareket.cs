using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.StokHareket
{
    public interface IStokHareket
    {
        Task StokHareketInsert(StokHareketDTO T,int KullanıcıId);
    }
}
