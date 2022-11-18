using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.PurchaseOrderDTO;

namespace Validation.Orders
{
    public class OrdersDeleteValidations:AbstractValidator<Delete>
    {
        public OrdersDeleteValidations()
        {
            RuleFor(x => x.id).NotNull().WithMessage("id zorunlu alan").NotEmpty().WithMessage("id boş geçilmez");
            RuleFor(x => x.Tip).NotNull().WithMessage("Tip zorunlu alan").NotEmpty().WithMessage("Tip boş geçilmez");
        }
    }
    public class OrdersDeleteItemsValidations : AbstractValidator<DeleteItems>
    {
        public OrdersDeleteItemsValidations()
        {
            RuleFor(x => x.id).NotNull().WithMessage("id zorunlu alan").NotEmpty().WithMessage("id boş geçilmez");
            RuleFor(x => x.OrdersId).NotNull().WithMessage("OrdersId zorunlu alan").NotEmpty().WithMessage("OrdersId boş geçilmez");
            RuleFor(x => x.ItemId).NotNull().WithMessage("ItemId zorunlu alan").NotEmpty().WithMessage("ItemId boş geçilmez");
  
        }
    }
    public class OrdersInsertValidations : AbstractValidator<PurchaseOrderInsert>
    {
        public OrdersInsertValidations()
        {
            RuleFor(x => x.Tip).NotNull().WithMessage("Tip zorunlu alan").NotEmpty().WithMessage("Tip boş geçilmez");
            RuleFor(x => x.ContactId).NotNull().WithMessage("ContactId zorunlu alan").NotEmpty().WithMessage("ContactId boş geçilmez");
            RuleFor(x => x.OrderName).NotNull().WithMessage("OrderName zorunlu alan").NotEmpty().WithMessage("OrderName boş geçilmez");
            RuleFor(x => x.LocationId).NotNull().WithMessage("LocationId zorunlu alan").NotEmpty().WithMessage("LocationId boş geçilmez");
            RuleFor(x => x.ExpectedDate).NotNull().WithMessage("ExpectedDate zorunlu alan").NotEmpty().WithMessage("ExpectedDate boş geçilmez");
            RuleFor(x => x.CreateDate).NotNull().WithMessage("CreateDate zorunlu alan").NotEmpty().WithMessage("CreateDate boş geçilmez");
            RuleFor(x => x.SalesOrderId).NotNull().WithMessage("SalesOrderId zorunlu alan").NotEmpty().WithMessage("SalesOrderId boş geçilmez");
            RuleFor(x => x.SalesOrderItemId).NotNull().WithMessage("SalesOrderItemId zorunlu alan").NotEmpty().WithMessage("SalesOrderItemId boş geçilmez");
            RuleFor(x => x.ManufacturingOrderId).NotNull().WithMessage("ManufacturingOrderId zorunlu alan").NotEmpty().WithMessage("ManufacturingOrderId boş geçilmez");
            RuleFor(x => x.ManufacturingOrderItemId).NotNull().WithMessage("ManufacturingOrderItemId zorunlu alan").NotEmpty().WithMessage("ManufacturingOrderItemId boş geçilmez");




        }
    }
    public class OrdersInsertItemValidations : AbstractValidator<PurchaseOrderInsertItem>
    {
        public OrdersInsertItemValidations()
        {
                 RuleFor(x => x.OrderId).NotNull().WithMessage("OrderId zorunlu alan").NotEmpty().WithMessage("OrderId boş geçilmez");
            RuleFor(x => x.MeasureId).NotNull().WithMessage("MeasureId zorunlu alan").NotEmpty().WithMessage("MeasureId boş geçilmez");
            RuleFor(x => x.ItemId).NotNull().WithMessage("ItemId zorunlu alan").NotEmpty().WithMessage("ItemId boş geçilmez");
            RuleFor(x => x.TaxId).NotNull().WithMessage("TaxId zorunlu alan").NotEmpty().WithMessage("TaxId boş geçilmez");
            RuleFor(x => x.Quantity).NotNull().WithMessage("Quantity zorunlu alan").NotEmpty().WithMessage("Quantity boş geçilmez");

        }
    }
    public class OrdersUpdateValidations : AbstractValidator<PurchaseOrderUpdate>
    {
        public OrdersUpdateValidations()
        {
            RuleFor(x => x.id).NotNull().WithMessage("id zorunlu alan").NotEmpty().WithMessage("id boş geçilmez");
            RuleFor(x => x.LocationId).NotNull().WithMessage("LocationId zorunlu alan").NotEmpty().WithMessage("LocationId boş geçilmez");
            RuleFor(x => x.ExpectedDate).NotNull().WithMessage("ExpectedDate zorunlu alan").NotEmpty().WithMessage("ExpectedDate boş geçilmez");
        }
    }
    public class OrdersUpdatePurchaseItemValidations : AbstractValidator<PurchaseItem>
    {
        public OrdersUpdatePurchaseItemValidations()
        {
            RuleFor(x => x.id).NotNull().WithMessage("id zorunlu alan").NotEmpty().WithMessage("id boş geçilmez");
            RuleFor(x => x.TaxId).NotNull().WithMessage("TaxId zorunlu alan").NotEmpty().WithMessage("TaxId boş geçilmez");
            RuleFor(x => x.ItemId).NotNull().WithMessage("ItemId zorunlu alan").NotEmpty().WithMessage("ItemId boş geçilmez");
            RuleFor(x => x.MeasureId).NotNull().WithMessage("MeasureId zorunlu alan").NotEmpty().WithMessage("MeasureId boş geçilmez");
            RuleFor(x => x.Quantity).NotNull().WithMessage("Quantity zorunlu alan").NotEmpty().WithMessage("Quantity boş geçilmez");
            RuleFor(x => x.PricePerUnit).NotNull().WithMessage("PricePerUnit zorunlu alan").NotEmpty().WithMessage("PricePerUnit boş geçilmez");
        }
    }

    public class OrdersUpdateOrdersStockValidations : AbstractValidator<PurchaseOrderId>
    {
        public OrdersUpdateOrdersStockValidations()
        {
            RuleFor(x => x.id).NotNull().WithMessage("id zorunlu alan").NotEmpty().WithMessage("id boş geçilmez");
            RuleFor(x => x.DeliveryId).NotNull().WithMessage("TaxId zorunlu alan").NotEmpty().WithMessage("TaxId boş geçilmez");
        }
    }



}
