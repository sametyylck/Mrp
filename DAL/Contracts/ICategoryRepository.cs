﻿using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.CategoryDTO;

namespace DAL.Contracts
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryClass>> List(int CompanyId);
        Task<int> Insert(CategoryInsert T, int CompanyId);
        Task Update(CategoryUpdate T, int CompanyId);
        Task Delete(IdControl T, int CompanyId);
    }
}