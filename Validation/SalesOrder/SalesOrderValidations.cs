using DAL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.SalesOrderDTO;

namespace Validation.SalesOrder
{
    public class SalesOrderInsertValidations:AbstractValidator<SalesOrderDTO.SalesOrder>
    {
        public SalesOrderInsertValidations()
        {
            RuleFor(x => x.ContactId).NotEmpty().WithMessage("ContactId bos gecilemez").NotNull().WithMessage("ContactId zorunlu alan");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("LocationId bos gecilemez").NotNull().WithMessage("LocationId zorunlu alan");
            RuleFor(x => x.OrderName).NotEmpty().WithMessage("OrderName bos gecilemez").NotNull().WithMessage("OrderName zorunlu alan");
            RuleFor(x => x.CreateDate).NotEmpty().WithMessage("CreateDate bos gecilemez").NotNull().WithMessage("CreateDate zorunlu alan");
            RuleFor(x => x.DeliveryDeadline).NotEmpty().WithMessage("DeliveryDeadline bos gecilemez").NotNull().WithMessage("DeliveryDeadline zorunlu alan");
        }
    }
    public class SalesOrderInsertItemValidations : AbstractValidator<SalesOrderDTO.SalesOrderItem>
    {
        public SalesOrderInsertItemValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("LocationId bos gecilemez").NotNull().WithMessage("LocationId zorunlu alan");
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId bos gecilemez").NotNull().WithMessage("ItemId zorunlu alan");
            RuleFor(x => x.TaxId).NotEmpty().WithMessage("TaxId bos gecilemez").NotNull().WithMessage("TaxId zorunlu alan");
            RuleFor(x => x.ContactId).NotEmpty().WithMessage("ContactId bos gecilemez").NotNull().WithMessage("ContactId zorunlu alan");
        }
    }
    public class SalesOrderUpdateValidations : AbstractValidator<SalesOrderUpdate>
    {
        public SalesOrderUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("LocationId bos gecilemez").NotNull().WithMessage("LocationId zorunlu alan");
            RuleFor(x => x.OrderName).NotEmpty().WithMessage("OrderName bos gecilemez").NotNull().WithMessage("OrderName zorunlu alan");
            RuleFor(x => x.ContactId).NotEmpty().WithMessage("ContactId bos gecilemez").NotNull().WithMessage("ContactId zorunlu alan");
        }
    }
    public class SalesOrderDeleteValidations : AbstractValidator<SalesDelete>
    {
        public SalesOrderDeleteValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");

        }
    }
    public class SalesOrderDeleteItemValidations : AbstractValidator<SalesDeleteItems>
    {
        public SalesOrderDeleteItemValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId bos gecilemez").NotNull().WithMessage("ItemId zorunlu alan");
            RuleFor(x => x.OrdersId).NotEmpty().WithMessage("OrdersId bos gecilemez").NotNull().WithMessage("OrdersId zorunlu alan");

        }
    }
    public class SalesOrderQuotesDoneValidations : AbstractValidator<Quotess>
    {
        public SalesOrderQuotesDoneValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("LocationId bos gecilemez").NotNull().WithMessage("LocationId zorunlu alan");
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId bos gecilemez").NotNull().WithMessage("ItemId zorunlu alan");
            RuleFor(x => x.ContactId).NotEmpty().WithMessage("ContactId bos gecilemez").NotNull().WithMessage("ContactId zorunlu alan");
        }
        
    }
    public class SalesOrderSalesDoneValidations : AbstractValidator<SalesDone>
    {
        public SalesOrderSalesDoneValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.DeliveryId).NotEmpty().WithMessage("DeliveryId bos gecilemez").NotNull().WithMessage("DeliveryId zorunlu alan");
        }
    }
  
}
