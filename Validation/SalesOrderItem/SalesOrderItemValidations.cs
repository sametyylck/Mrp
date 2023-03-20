using DAL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ManufacturingOrderDTO;
using static DAL.DTO.SalesOrderDTO;

namespace Validation.SalesOrderItem
{
    public class SalesOrderItemMakeValidations:AbstractValidator<SalesOrderMake>
    {
        public SalesOrderItemMakeValidations()
        {
            RuleFor(x => x.CariId).NotEmpty().WithMessage("ContactId bos gecilemez").NotNull().WithMessage("ContactId zorunlu alan");
            RuleFor(x => x.DepoId).NotEmpty().WithMessage("LocationId bos gecilemez").NotNull().WithMessage("LocationId zorunlu alan");
            RuleFor(x => x.SatisId).NotEmpty().WithMessage("SalesOrderId bos gecilemez").NotNull().WithMessage("SalesOrderId zorunlu alan");
            RuleFor(x => x.SatisDetayId).NotEmpty().WithMessage("SalesOrderItemId bos gecilemez").NotNull().WithMessage("SalesOrderItemId zorunlu alan");
            RuleFor(x => x.BeklenenTarih).NotEmpty().WithMessage("ExpectedDate bos gecilemez").NotNull().WithMessage("ExpectedDate zorunlu alan");
            RuleFor(x => x.PlanlananMiktar).NotEmpty().WithMessage("PlannedQuantity bos gecilemez").NotNull().WithMessage("PlannedQuantity zorunlu alan");
            RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId bos gecilemez").NotNull().WithMessage("ItemId zorunlu alan");
            RuleFor(x => x.UretimTarihi).NotEmpty().WithMessage("ProductionDeadline bos gecilemez").NotNull().WithMessage("ProductionDeadline zorunlu alan");
            RuleFor(x => x.Isim).NotEmpty().WithMessage("Name bos gecilemez").NotNull().WithMessage("Name zorunlu alan");
        }
    }
    public class SalesOrderItemUpdateItemValidations : AbstractValidator<SatısUpdateItems>
    {
        public SalesOrderItemUpdateItemValidations()
        {
            RuleFor(x => x.CariId).NotEmpty().WithMessage("ContactId bos gecilemez").NotNull().WithMessage("ContactId zorunlu alan");
            RuleFor(x => x.DepoId).NotEmpty().WithMessage("LocationId bos gecilemez").NotNull().WithMessage("LocationId zorunlu alan");
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.SatisId).NotEmpty().WithMessage("OrderItemId bos gecilemez").NotNull().WithMessage("OrderItemId zorunlu alan");
            RuleFor(x => x.BirimFiyat).NotEmpty().WithMessage("PricePerUnit bos gecilemez").NotNull().WithMessage("PricePerUnit zorunlu alan");
            RuleFor(x => x.VergiId).NotEmpty().WithMessage("TaxId bos gecilemez").NotNull().WithMessage("TaxId zorunlu alan");
            RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId bos gecilemez").NotNull().WithMessage("ItemId zorunlu alan");
            RuleFor(x => x.Miktar).NotEmpty().WithMessage("Quantity bos gecilemez").NotNull().WithMessage("Quantity zorunlu alan");

        }
    }
}
