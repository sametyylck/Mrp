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
            RuleFor(x => x.ProductId).NotEmpty().WithMessage(" ProductId Bos geçilmez").NotNull().WithMessage("ProductId zorunlu alan");
            RuleFor(x => x.MaterialId).NotEmpty().WithMessage(" MaterialId Bos geçilmez").NotNull().WithMessage("MaterialId zorunlu alan");
            RuleFor(x => x.Quantity).NotEmpty().WithMessage(" Quantity Bos geçilmez").NotNull().NotNull().WithMessage("Quantity zorunlu alan");
        }
    }
    public class BomUpdateValidations : AbstractValidator<BomDTO.BOMUpdate>
    {
        public BomUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("Bos geçilmez").NotNull();
            RuleFor(x => x.MaterialId).NotEmpty().WithMessage("Bos geçilmez").NotNull();
            RuleFor(x => x.Quantity).NotEmpty().WithMessage("Bos geçilmez").NotNull();
            
        }
    }
}
