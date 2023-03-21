using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockTakesDTO;

namespace Validation.StockTakes
{
    public class StockTakesDeleteItemsValidations:AbstractValidator<StockTakeDelete>
    {
        public StockTakesDeleteItemsValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id boş gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.StokId).NotEmpty().WithMessage("id boş gecilemez").NotNull().WithMessage("id zorunlu alan");
        }
    }
    public class StockTakesInsertValidations : AbstractValidator<StockTakesInsert>
    {
        public StockTakesInsertValidations()
        {
            RuleFor(x => x.Isim).NotEmpty().WithMessage("StockTake boş gecilemez").NotNull().WithMessage("StockTake zorunlu alan");
            RuleFor(x => x.OlusturmaTarihi).NotEmpty().WithMessage("CreadtedDate boş gecilemez").NotNull().WithMessage("CreadtedDate zorunlu alan");
            RuleFor(x => x.DepoId).NotEmpty().WithMessage("LocationId boş gecilemez").NotNull().WithMessage("LocationId zorunlu alan");

        }
    }
    public class StockTakesInsertItemValidations : AbstractValidator<StockTakeInsertItems>
    {
        public StockTakesInsertItemValidations()
        {
            RuleFor(x => x.StokSayimId).NotEmpty().WithMessage("StockTakesId boş gecilemez").NotNull().WithMessage("StockTakesId zorunlu alan");
            RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId boş gecilemez").NotNull().WithMessage("ItemId zorunlu alan");
        }
    }
    public class StockTakesUpdateValidations : AbstractValidator<StockTakesUpdate>
    {
        public StockTakesUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id boş gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.Isim).NotEmpty().WithMessage("StockTake boş gecilemez").NotNull().WithMessage("StockTake zorunlu alan");
        }
    }
    public class StockTakesTaskDoneValidations : AbstractValidator<StockTakesDone>
    {
        public StockTakesTaskDoneValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id boş gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.Durum).NotEmpty().WithMessage("Status boş gecilemez").NotNull().WithMessage("Status zorunlu alan");
        }
    }
    public class StockTakesUpdateItemsValidations : AbstractValidator<StockTakesUpdateItems>
    {
        public StockTakesUpdateItemsValidations()
        {
            RuleFor(x => x.StokSayimId).NotEmpty().WithMessage("StockTakesId boş gecilemez").NotNull().WithMessage("StockTakesId zorunlu alan");
            RuleFor(x => x.StokSayimDetayId).NotEmpty().WithMessage("StockTakesItemId boş gecilemez").NotNull().WithMessage("StockTakesItemId zorunlu alan");
        }
    }
}
