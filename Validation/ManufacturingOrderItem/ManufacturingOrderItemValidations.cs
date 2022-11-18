using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderItemDTO;

namespace Validation.ManufacturingOrderItem
{
    public class ManufacturingOrderItemIngredientUpdateValidations:AbstractValidator<ManufacturingOrderItemsIngredientsUpdate>
    {
        public ManufacturingOrderItemIngredientUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id boş geçilemez").NotNull().WithMessage("id alanı zorunlu");
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId boş geçilemez").NotNull().WithMessage("ItemId alanı zorunlu");
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId boş geçilemez").NotNull().WithMessage("OrderId alanı zorunlu");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("LocationId boş geçilemez").NotNull().WithMessage("LocationId alanı zorunlu");
            RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity boş geçilemez").NotNull().WithMessage("Quantity alanı zorunlu");
            RuleFor(x => x.SalesOrderId).NotEmpty().WithMessage("SalesOrderId boş geçilemez").NotNull().WithMessage("SalesOrderId alanı zorunlu");
            RuleFor(x => x.SalesOrderItemId).NotEmpty().WithMessage("SalesOrderItemId boş geçilemez").NotNull().WithMessage("SalesOrderItemId alanı zorunlu");





        }
    }

    public class ManufacturingOrderItemIngredientInsertValidations : AbstractValidator<ManufacturingOrderItemsIngredientsInsert>
    {
        public ManufacturingOrderItemIngredientInsertValidations()
        {
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId boş geçilemez").NotNull().WithMessage("ItemId alanı zorunlu");
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId boş geçilemez").NotNull().WithMessage("OrderId alanı zorunlu");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("LocationId boş geçilemez").NotNull().WithMessage("LocationId alanı zorunlu");
            RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity boş geçilemez").NotNull().WithMessage("Quantity alanı zorunlu");
            RuleFor(x => x.SalesOrderId).NotEmpty().WithMessage("SalesOrderId boş geçilemez").NotNull().WithMessage("SalesOrderId alanı zorunlu");
            RuleFor(x => x.SalesOrderItemId).NotEmpty().WithMessage("SalesOrderItemId boş geçilemez").NotNull().WithMessage("SalesOrderItemId alanı zorunlu");
        }
    }

    public class ManufacturingOrderItemOperationUpdateValidations : AbstractValidator<ManufacturingOrderItemsOperationsUpdate>
    {
        public ManufacturingOrderItemOperationUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id boş geçilemez").NotNull().WithMessage("id alanı zorunlu");
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId boş geçilemez").NotNull().WithMessage("OrderId alanı zorunlu");
            RuleFor(x => x.OperationId).NotEmpty().WithMessage("OperationId boş geçilemez").NotNull().WithMessage("OperationId alanı zorunlu");
            RuleFor(x => x.ResourceId).NotEmpty().WithMessage("ResourceId boş geçilemez").NotNull().WithMessage("ResourceId alanı zorunlu");
            RuleFor(x => x.PlannedTime).NotEmpty().WithMessage("PlannedTime boş geçilemez").NotNull().WithMessage("PlannedTime alanı zorunlu");
            RuleFor(x => x.CostPerHour).NotEmpty().WithMessage("CostPerHour boş geçilemez").NotNull().WithMessage("CostPerHour alanı zorunlu");
            RuleFor(x => x.Status).NotEmpty().WithMessage("Status boş geçilemez").NotNull().WithMessage("Status alanı zorunlu");
        }
    }

    public class ManufacturingOrderItemOperationInsertValidations : AbstractValidator<ManufacturingOrderItemsOperationsInsert>
    {
        public ManufacturingOrderItemOperationInsertValidations()
        {
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId boş geçilemez").NotNull().WithMessage("OrderId alanı zorunlu");
            RuleFor(x => x.OperationId).NotEmpty().WithMessage("OperationId boş geçilemez").NotNull().WithMessage("OperationId alanı zorunlu");
            RuleFor(x => x.ResourceId).NotEmpty().WithMessage("ResourceId boş geçilemez").NotNull().WithMessage("ResourceId alanı zorunlu");
            RuleFor(x => x.PlannedTime).NotEmpty().WithMessage("PlannedTime boş geçilemez").NotNull().WithMessage("PlannedTime alanı zorunlu");
        }
    }
    public class ManufacturingOrderPurchaseOrderValidations : AbstractValidator<ManufacturingPurchaseOrder>
    {
        public ManufacturingOrderPurchaseOrderValidations()
        {
        RuleFor(x => x.ManufacturingOrderId).NotEmpty().WithMessage("ManufacturingOrderId boş geçilemez").NotNull().WithMessage("ManufacturingOrderId alanı zorunlu");
        RuleFor(x => x.ManufacturingOrderItemId).NotEmpty().WithMessage("ManufacturingOrderItemId boş geçilemez").NotNull().WithMessage("ManufacturingOrderItemId alanı zorunlu");
        RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId boş geçilemez").NotNull().WithMessage("ItemId alanı zorunlu");
        RuleFor(x => x.LocationId).NotEmpty().WithMessage("LocationId boş geçilemez").NotNull().WithMessage("LocationId alanı zorunlu");

            RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity boş geçilemez").NotNull().WithMessage("Quantity alanı zorunlu");
            RuleFor(x => x.ExpectedDate).NotEmpty().WithMessage("ExpectedDate boş geçilemez").NotNull().WithMessage("ExpectedDate alanı zorunlu");
            RuleFor(x => x.OrderName).NotEmpty().WithMessage("OrderName boş geçilemez").NotNull().WithMessage("OrderName alanı zorunlu");

            RuleFor(x => x.SalesOrderId).NotEmpty().WithMessage("SalesOrderId boş geçilemez").NotNull().WithMessage("SalesOrderId alanı zorunlu");
            RuleFor(x => x.SalesOrderItemId).NotEmpty().WithMessage("SalesOrderItemId boş geçilemez").NotNull().WithMessage("SalesOrderItemId alanı zorunlu");
            RuleFor(x => x.ContactId).NotEmpty().WithMessage("ContactId boş geçilemez").NotNull().WithMessage("ContactId alanı zorunlu");
            RuleFor(x => x.Tip).NotEmpty().WithMessage("Tip boş geçilemez").NotNull().WithMessage("Tip alanı zorunlu");
        }
    }

}
