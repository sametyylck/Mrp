using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ItemDTO;

namespace Validation.Items
{
    public class ItemsListeValidations:AbstractValidator<ItemsListele>
    {
        public ItemsListeValidations()
        {
            RuleFor(x => x.Tip).NotEmpty().WithMessage("tip bos gecilmez").NotNull().WithMessage("tip alanı zorunlu");
        }
    }

    public class ItemsDeleteValidations : AbstractValidator<ItemsDelete>
    {
        public ItemsDeleteValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilmez").NotNull().WithMessage("id alanı zorunlu");

            RuleFor(x => x.Tip).NotEmpty().WithMessage("tip bos gecilmez").NotNull().WithMessage("tip alanı zorunlu");

        }
    }

    public class ItemsInsertValidations : AbstractValidator<ItemsInsert>
    {
        public ItemsInsertValidations()
        {
            RuleFor(x => x.Tip).NotEmpty().WithMessage("tip bos gecilmez").NotNull().WithMessage("tip alanı zorunlu");
            RuleFor(x => x.VarsayilanFiyat).NotEmpty().WithMessage("DefaultPrice bos gecilmez").NotNull().WithMessage("DefaultPrice alanı zorunlu");
            RuleFor(x => x.OlcuId).NotEmpty().WithMessage("MeasureId bos gecilmez").NotNull().WithMessage("MeasureId alanı zorunlu");




        }
    }

    public class ItemsUpdateValidations : AbstractValidator<ItemsUpdate>
    {
        public ItemsUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilmez").NotNull().WithMessage("id alanı zorunlu");

            RuleFor(x => x.Tip).NotEmpty().WithMessage("tip bos gecilmez").NotNull().WithMessage("tip alanı zorunlu");

        }
    }

    
}
