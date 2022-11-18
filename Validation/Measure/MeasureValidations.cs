using DAL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.Measure
{
    public class MeasureInsertValidations:AbstractValidator<MeasureInsert>
    {
        public MeasureInsertValidations()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name boş geçilemez").NotNull().WithMessage("Name zorunlu alan");
        }
    }

    public class MeasureUpdateValidations : AbstractValidator<MeasureUpdate>
    {
        public MeasureUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id boş geçilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name boş geçilemez").NotNull().WithMessage("Name zorunlu alan");
        }
    }




}
