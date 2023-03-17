using DAL.DTO;
using DAL.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.Bom
{
    public class BomInsertValidations:AbstractValidator<BomDTO.BOMInsert>
    {
        public BomInsertValidations()
        {
            RuleFor(x => x.MamulId).NotEmpty().WithMessage(" ProductId Bos geçilmez").NotNull().WithMessage("ProductId zorunlu alan");
            RuleFor(x => x.MalzemeId).NotEmpty().WithMessage(" MaterialId Bos geçilmez").NotNull().WithMessage("MaterialId zorunlu alan");
            RuleFor(x => x.Miktar).NotEmpty().WithMessage(" Quantity Bos geçilmez").NotNull().NotNull().WithMessage("Quantity zorunlu alan");
        }
    }
    public class BomUpdateValidations : AbstractValidator<BomDTO.BOMUpdate>
    {
        public BomUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("Bos geçilmez").NotNull();
            RuleFor(x => x.MalzemeId).NotEmpty().WithMessage("Bos geçilmez").NotNull();
            RuleFor(x => x.Miktar).NotEmpty().WithMessage("Bos geçilmez").NotNull();
            
        }
    }
}
