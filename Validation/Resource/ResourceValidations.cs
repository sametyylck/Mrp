using DAL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.Resource
{
    public class ResourceInsertValidations:AbstractValidator<ResourcesInsert>
    {
        public ResourceInsertValidations()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("name bos gecilemez").NotNull().WithMessage("name zorunlu alan");
            RuleFor(x => x.DefaultCostHour).NotEmpty().WithMessage("DefaultCostHour bos gecilemez").NotNull().WithMessage("DefaultCostHour zorunlu alan");
        }
    }
    public class ResourceUpdateValidations : AbstractValidator<ResourcesUpdate>
    {
        public ResourceUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.Name).NotEmpty().WithMessage("name bos gecilemez").NotNull().WithMessage("name zorunlu alan");
            RuleFor(x => x.DefaultCostHour).NotEmpty().WithMessage("DefaultCostHour bos gecilemez").NotNull().WithMessage("DefaultCostHour zorunlu alan");
        }
    }
}
