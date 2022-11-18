using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ProductOperationsBomDTO;

namespace Validation.OperationsBom
{
    public class OperationBomInsertValidations:AbstractValidator<ProductOperationsBOMInsert>
    {
        public OperationBomInsertValidations()
        {
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId bos gecilmez").NotNull().WithMessage("ItemId zorunlu alan");
            RuleFor(x => x.ResourceId).NotEmpty().WithMessage("ResourceId bos gecilmez").NotNull().WithMessage("ResourceId zorunlu alan");
            RuleFor(x => x.OperationId).NotEmpty().WithMessage("OperationId bos gecilmez").NotNull().WithMessage("OperationId zorunlu alan");
            RuleFor(x => x.CostHour).NotEmpty().WithMessage("CostHour bos gecilmez").NotNull().WithMessage("CostHour zorunlu alan");
            RuleFor(x => x.OperationTime).NotEmpty().WithMessage("OperationTime bos gecilmez").NotNull().WithMessage("OperationTime zorunlu alan");

        }
    }
    public class OperationBomUpdateValidations : AbstractValidator<ProductOperationsBOMUpdate>
    {
        public OperationBomUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilmez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.ResourceId).NotEmpty().WithMessage("ResourceId bos gecilmez").NotNull().WithMessage("ResourceId zorunlu alan");
            RuleFor(x => x.OperationId).NotEmpty().WithMessage("OperationId bos gecilmez").NotNull().WithMessage("OperationId zorunlu alan");
            RuleFor(x => x.CostHour).NotEmpty().WithMessage("CostHour bos gecilmez").NotNull().WithMessage("CostHour zorunlu alan");
            RuleFor(x => x.OperationTime).NotEmpty().WithMessage("OperationTime bos gecilmez").NotNull().WithMessage("OperationTime zorunlu alan");
        }
    }
}
