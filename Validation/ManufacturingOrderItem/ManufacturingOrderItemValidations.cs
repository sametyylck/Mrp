using DAL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderItemDTO;

namespace Validation.ManufacturingOrderItem
{
    public class ManufacturingOrderItemIngredientUpdateValidations:AbstractValidator<UretimIngredientsUpdate>
    {
        public ManufacturingOrderItemIngredientUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id boş geçilemez").NotNull().WithMessage("id alanı zorunlu");
            RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId boş geçilemez").NotNull().WithMessage("ItemId alanı zorunlu");
            RuleFor(x => x.UretimId).NotEmpty().WithMessage("OrderId boş geçilemez").NotNull().WithMessage("OrderId alanı zorunlu");
            RuleFor(x => x.DepoId).NotEmpty().WithMessage("LocationId boş geçilemez").NotNull().WithMessage("LocationId alanı zorunlu");
            RuleFor(x => x.Miktar).NotEmpty().WithMessage("Quantity boş geçilemez").NotNull().WithMessage("Quantity alanı zorunlu");

        }
    }

    public class ManufacturingOrderItemIngredientInsertValidations : AbstractValidator<UretimIngredientsInsert>
    {
        public ManufacturingOrderItemIngredientInsertValidations()
        {
            RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId boş geçilemez").NotNull().WithMessage("ItemId alanı zorunlu");
            RuleFor(x => x.UretimId).NotEmpty().WithMessage("OrderId boş geçilemez").NotNull().WithMessage("OrderId alanı zorunlu");
            RuleFor(x => x.DepoId).NotEmpty().WithMessage("LocationId boş geçilemez").NotNull().WithMessage("LocationId alanı zorunlu");
            RuleFor(x => x.Miktar).NotEmpty().WithMessage("Quantity boş geçilemez").NotNull().WithMessage("Quantity alanı zorunlu");

        }
    }

    public class ManufacturingOrderItemOperationUpdateValidations : AbstractValidator<UretimOperationsUpdate>
    {
        public ManufacturingOrderItemOperationUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id boş geçilemez").NotNull().WithMessage("id alanı zorunlu");
            RuleFor(x => x.UretimId).NotEmpty().WithMessage("OrderId boş geçilemez").NotNull().WithMessage("OrderId alanı zorunlu");
            RuleFor(x => x.OperasyonId).NotEmpty().WithMessage("OperationId boş geçilemez").NotNull().WithMessage("OperationId alanı zorunlu");
            RuleFor(x => x.KaynakId).NotEmpty().WithMessage("ResourceId boş geçilemez").NotNull().WithMessage("ResourceId alanı zorunlu");
            RuleFor(x => x.PlanlananZaman).NotEmpty().WithMessage("PlannedTime boş geçilemez").NotNull().WithMessage("PlannedTime alanı zorunlu");
            RuleFor(x => x.SaatlikUcret).NotEmpty().WithMessage("CostPerHour boş geçilemez").NotNull().WithMessage("CostPerHour alanı zorunlu");
            RuleFor(x => x.Durum).NotEmpty().WithMessage("Status boş geçilemez").NotNull().WithMessage("Status alanı zorunlu");
        }
    }

    public class ManufacturingOrderItemOperationInsertValidations : AbstractValidator<UretimOperationsInsert>
    {
        public ManufacturingOrderItemOperationInsertValidations()
        {
            RuleFor(x => x.UretimId).NotEmpty().WithMessage("OrderId boş geçilemez").NotNull().WithMessage("OrderId alanı zorunlu");
            RuleFor(x => x.OperasyonId).NotEmpty().WithMessage("OperationId boş geçilemez").NotNull().WithMessage("OperationId alanı zorunlu");
            RuleFor(x => x.KaynakId).NotEmpty().WithMessage("ResourceId boş geçilemez").NotNull().WithMessage("ResourceId alanı zorunlu");
            RuleFor(x => x.PlanlananZaman).NotEmpty().WithMessage("PlannedTime boş geçilemez").NotNull().WithMessage("PlannedTime alanı zorunlu");
        }
    }
    public class ManufacturingOrderPurchaseOrderValidations : AbstractValidator<PurchaseBuy>
    {
        public ManufacturingOrderPurchaseOrderValidations()
        {
        RuleFor(x => x.UretimId).NotEmpty().WithMessage("ManufacturingOrderId boş geçilemez").NotNull().WithMessage("ManufacturingOrderId alanı zorunlu");
        RuleFor(x => x.UretimDetayId).NotEmpty().WithMessage("ManufacturingOrderItemId boş geçilemez").NotNull().WithMessage("ManufacturingOrderItemId alanı zorunlu");
        RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId boş geçilemez").NotNull().WithMessage("ItemId alanı zorunlu");
        RuleFor(x => x.DepoId).NotEmpty().WithMessage("LocationId boş geçilemez").NotNull().WithMessage("LocationId alanı zorunlu");

            RuleFor(x => x.Miktar).NotEmpty().WithMessage("Quantity boş geçilemez").NotNull().WithMessage("Quantity alanı zorunlu");
            RuleFor(x => x.BeklenenTarih).NotEmpty().WithMessage("ExpectedDate boş geçilemez").NotNull().WithMessage("ExpectedDate alanı zorunlu");
            RuleFor(x => x.SatinAlmaIsim).NotEmpty().WithMessage("OrderName boş geçilemez").NotNull().WithMessage("OrderName alanı zorunlu");
            RuleFor(x => x.TedarikciId).NotEmpty().WithMessage("ContactId boş geçilemez").NotNull().WithMessage("ContactId alanı zorunlu");
            RuleFor(x => x.Tip).NotEmpty().WithMessage("Tip boş geçilemez").NotNull().WithMessage("Tip alanı zorunlu");
        }
    }

}
