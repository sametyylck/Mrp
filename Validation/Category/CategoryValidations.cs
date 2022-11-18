using DAL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.Category
{
    public class CategoryInsertValidations:AbstractValidator<CategoryDTO.CategoryInsert>
    {
        public CategoryInsertValidations()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Isim bos gecilmez").NotNull();
        }
    }
    public class CategoryUpdateValidations : AbstractValidator<CategoryDTO.CategoryUpdate>
    {
        public CategoryUpdateValidations()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Isim bos gecilmez").NotNull();
            RuleFor(x => x.id).NotEmpty().WithMessage("Id bos gecilmez").NotNull();
        }
    }
}
