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
    public class SalesOrderInsertValidations:AbstractValidator<SatısDTO>
    {
        public SalesOrderInsertValidations()
        {
            RuleFor(x => x.CariId).NotEmpty().WithMessage("ContactId bos gecilemez").NotNull().WithMessage("ContactId zorunlu alan");
            RuleFor(x => x.DepoId).NotEmpty().WithMessage("LocationId bos gecilemez").NotNull().WithMessage("LocationId zorunlu alan");
            RuleFor(x => x.SatisIsmi).NotEmpty().WithMessage("OrderName bos gecilemez").NotNull().WithMessage("OrderName zorunlu alan");
            RuleFor(x => x.OlusturmaTarihi).NotEmpty().WithMessage("CreateDate bos gecilemez").NotNull().WithMessage("CreateDate zorunlu alan");
            RuleFor(x => x.TeslimSuresi).NotEmpty().WithMessage("DeliveryDeadline bos gecilemez").NotNull().WithMessage("DeliveryDeadline zorunlu alan");
        }
    }
    public class SalesOrderInsertItemValidations : AbstractValidator<SatısInsertItem>
    {
        public SalesOrderInsertItemValidations()
        {
            RuleFor(x => x.SatisId).NotEmpty().WithMessage("SalesOrderId bos gecilemez").NotNull().WithMessage("SalesOrderId zorunlu alan");
            RuleFor(x => x.DepoId).NotEmpty().WithMessage("LocationId bos gecilemez").NotNull().WithMessage("LocationId zorunlu alan");
            RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId bos gecilemez").NotNull().WithMessage("ItemId zorunlu alan");
            RuleFor(x => x.VergiId).NotEmpty().WithMessage("TaxId bos gecilemez").NotNull().WithMessage("TaxId zorunlu alan");
            RuleFor(x => x.CariId).NotEmpty().WithMessage("ContactId bos gecilemez").NotNull().WithMessage("ContactId zorunlu alan");
        }
    }
    public class SalesOrderUpdateValidations : AbstractValidator<SalesOrderUpdate>
    {
        public SalesOrderUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.DepoId).NotEmpty().WithMessage("LocationId bos gecilemez").NotNull().WithMessage("LocationId zorunlu alan");
            RuleFor(x => x.SatisIsmi).NotEmpty().WithMessage("OrderName bos gecilemez").NotNull().WithMessage("OrderName zorunlu alan");
            RuleFor(x => x.CariId).NotEmpty().WithMessage("ContactId bos gecilemez").NotNull().WithMessage("ContactId zorunlu alan");
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
            RuleFor(x => x.DurumBelirteci).NotEmpty().WithMessage("DeliveryId bos gecilemez").NotNull().WithMessage("DeliveryId zorunlu alan");
        }
    }
  
}
