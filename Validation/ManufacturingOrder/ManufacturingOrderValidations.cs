using DAL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;

namespace Validation.ManufacturingOrder
{
    public class ManufacturingOrderInsertOrderValidations:AbstractValidator<UretimDTO>
    {
        private bool BeAValidDate(string value)
        {
            DateTime date;
            return DateTime.TryParse(value, out date);
        }
        public ManufacturingOrderInsertOrderValidations()
        {
            RuleFor(x => x.PlananlananMiktar).NotEmpty().WithMessage("PlannedQuantity bos gecilemez").NotNull().WithMessage("PlannedQuantity zorunlu alan");
            RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId bos gecilemez").NotNull().WithMessage("ItemId zorunlu alan");
            RuleFor(x => x.DepoId).NotEmpty().WithMessage("LocationId bos gecilemez").NotNull().WithMessage("LocationId zorunlu alan");
        }
    }
    public class ManufacturingOrderUpdateValidations : AbstractValidator<UretimUpdate>
    {
        public ManufacturingOrderUpdateValidations()
        {

            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId bos gecilemez").NotNull().WithMessage("ItemId zorunlu alan");
            RuleFor(x => x.DepoId).NotEmpty().WithMessage("LocationId bos gecilemez").NotNull().WithMessage("LocationId zorunlu alan");
     
        }
    }
    public class ManufacturingOrderDoneValidations : AbstractValidator<UretimTamamlama>
    {
        public ManufacturingOrderDoneValidations()
        {

            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.Durum).NotEmpty().WithMessage("Status bos gecilemez").NotNull().WithMessage("OrdersId zorunlu alan");
        }
    }
    public class ManufacturingOrderTaskDoneValidations : AbstractValidator<ManufacturingTaskDone>
    {
        public ManufacturingOrderTaskDoneValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.UretimId).NotEmpty().WithMessage("OrdersId bos gecilemez").NotNull().WithMessage("OrdersId zorunlu alan");

            RuleFor(x => x.Durum).NotEmpty().WithMessage("Status bos gecilemez").NotNull().WithMessage("Status zorunlu alan");
        }
    }
    public class ManufacturingOrderDeleteItemsValidations : AbstractValidator<UretimDeleteItems>
    {
        public ManufacturingOrderDeleteItemsValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.UretimId).NotEmpty().WithMessage("OrdersId bos gecilemez").NotNull().WithMessage("OrdersId zorunlu alan");
        }
    }
}
