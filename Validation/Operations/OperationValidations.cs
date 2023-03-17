using DAL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.Operations
{
    public class OperationsInsertValidations:AbstractValidator<OperationsInsert>
    {
        public OperationsInsertValidations()
        {
            RuleFor(x => x.Isim).NotEmpty().WithMessage("name bos gecilemez").NotNull().WithMessage("name zorunlu");
        }
    }
    public class OperationsUpdateValidations : AbstractValidator<OperationsUpdate>
    {
        public OperationsUpdateValidations()
        {
            RuleFor(x => x.Isim).NotEmpty().WithMessage("name bos gecilemez").NotNull().WithMessage("name zorunlu");
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu");
        }
    }
}
