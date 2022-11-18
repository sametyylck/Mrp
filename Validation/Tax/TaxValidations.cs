using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.TaxDTO;

namespace Validation.Tax
{
    public class TaxInsertValidations:AbstractValidator<TaxInsert>
    {
        public TaxInsertValidations()
        {
            RuleFor(x => x.Rate).NotEmpty().WithMessage("rate boş gecilemez").NotNull().WithMessage("rate zorunlu alan");
            RuleFor(x => x.TaxName).NotEmpty().WithMessage("TaxName boş gecilemez").NotNull().WithMessage("TaxName zorunlu alan");
        }
    }
    public class TaxUpdateValidations : AbstractValidator<TaxUpdate>
    {
        public TaxUpdateValidations()
        {
            RuleFor(x => x.Rate).NotEmpty().WithMessage("rate boş gecilemez").NotNull().WithMessage("rate zorunlu alan");
            RuleFor(x => x.TaxName).NotEmpty().WithMessage("TaxName boş gecilemez").NotNull().WithMessage("TaxName zorunlu alan");

            RuleFor(x => x.id).NotEmpty().WithMessage("id boş gecilemez").NotNull().WithMessage("id zorunlu alan");
        }
    }
}
