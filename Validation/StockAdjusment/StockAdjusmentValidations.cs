using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockAdjusmentDTO;

namespace Validation.StockAdjusment
{
    public class StockAdjusmentItemDeleteValidations:AbstractValidator<StockAdjusmentItemDelete>
    {
        public StockAdjusmentItemDeleteValidations()
        {
            RuleFor(x=>x.StockAdjusmentId).NotEmpty().WithMessage("StockAdjusmentId boş gecilmez").NotNull().WithMessage("StockAdjusmentId zorunlu alan");
            RuleFor(x => x.id).NotEmpty().WithMessage("id boş gecilmez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId boş gecilmez").NotNull().WithMessage("ItemId zorunlu alan");
        }
    }
    public class StockAdjusmentInsertItemValidations : AbstractValidator<StockAdjusmentInsertItem>
    {
        public StockAdjusmentInsertItemValidations()
        {
            RuleFor(x => x.CostPerUnit).NotEmpty().WithMessage("CostPerUnit boş gecilmez").NotNull().WithMessage("CostPerUnit zorunlu alan");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("LocationId boş gecilmez").NotNull().WithMessage("LocationId zorunlu alan");
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId boş gecilmez").NotNull().WithMessage("ItemId zorunlu alan");
            RuleFor(x => x.Adjusment).NotEmpty().WithMessage("Adjusment boş gecilmez").NotNull().WithMessage("Adjusment zorunlu alan");
            RuleFor(x => x.StockAdjusmentId).NotEmpty().WithMessage("StockAdjusmentId boş gecilmez").NotNull().WithMessage("StockAdjusmentId zorunlu alan");


        }
    }
    public class StockAdjusmentInsertValidations : AbstractValidator<StockAdjusmentInsert>
    {
        public StockAdjusmentInsertValidations()
        {
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("LocationId boş gecilmez").NotNull().WithMessage("LocationId zorunlu alan");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Date boş gecilmez").NotNull().WithMessage("Date zorunlu alan");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name boş gecilmez").NotNull().WithMessage("Name zorunlu alan");
        }
    }
    public class StockAdjusmentUpdateValidations : AbstractValidator<StockAdjusmentUpdate>
    {
        public StockAdjusmentUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("StockAdjusmentId boş gecilmez").NotNull().WithMessage("StockAdjusmentId zorunlu alan");
            RuleFor(x => x.Date).NotEmpty().WithMessage("id boş gecilmez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.Name).NotEmpty().WithMessage("ItemId boş gecilmez").NotNull().WithMessage("ItemId zorunlu alan");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("LocationId boş gecilmez").NotNull().WithMessage("LocationId zorunlu alan");
        }
    }
    public class StockAdjusmentValidations : AbstractValidator<StockAdjusmentUpdateItems>
    {
        public StockAdjusmentValidations()
        {
            RuleFor(x => x.CostPerUnit).NotEmpty().WithMessage("CostPerUnit boş gecilmez").NotNull().WithMessage("CostPerUnit zorunlu alan");
            RuleFor(x => x.StockAdjusmentId).NotEmpty().WithMessage("StockAdjusmentId boş gecilmez").NotNull().WithMessage("StockAdjusmentId zorunlu alan");
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId boş gecilmez").NotNull().WithMessage("ItemId zorunlu alan");
            RuleFor(x => x.Adjusment).NotEmpty().WithMessage("Adjusment boş gecilmez").NotNull().WithMessage("Adjusment zorunlu alan");
            RuleFor(x => x.StockAdjusmentId).NotEmpty().WithMessage("StockAdjusmentId boş gecilmez").NotNull().WithMessage("StockAdjusmentId zorunlu alan");


        }
    }
}
