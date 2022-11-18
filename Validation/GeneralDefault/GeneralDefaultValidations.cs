﻿using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.GeneralSettingsDTO;

namespace Validation.GeneralDefault
{
    public class GeneralDefaultValidations:AbstractValidator<GeneralDefaultSettings>
    {
        public GeneralDefaultValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilemez").NotNull().WithMessage("id zorunlu alan");

            RuleFor(x => x.DefaultPurchaseOrder).NotEmpty().WithMessage("DefaultPurchaseOrder bos gecilemez").NotNull().WithMessage("DefaultPurchaseOrder zorunlu alan");

            RuleFor(x => x.DefaultManufacturingLocationId).NotEmpty().WithMessage("DefaultManufacturingLocationId bos gecilemez").NotNull().WithMessage("DefaultManufacturingLocationId zorunlu alan");

            RuleFor(x => x.DefaultTaxPurchaseOrderId).NotEmpty().WithMessage("DefaultTaxPurchaseOrderId bos gecilemez").NotNull().WithMessage("DefaultTaxPurchaseOrderId zorunlu alan");

            RuleFor(x => x.CurrencyId).NotEmpty().WithMessage("CurrencyId bos gecilemez").NotNull().WithMessage("CurrencyId zorunlu alan");

            RuleFor(x => x.DefaultSalesLocationId).NotEmpty().WithMessage("DefaultSalesLocationId bos gecilemez").NotNull().WithMessage("DefaultSalesLocationId zorunlu alan");

            RuleFor(x => x.DefaultSalesOrder).NotEmpty().WithMessage("DefaultSalesOrder bos gecilemez").NotNull().WithMessage("DefaultSalesOrder zorunlu alan");

            RuleFor(x => x.DefaultTaxSalesOrderId).NotEmpty().WithMessage("TaxSalesOrderId bos gecilemez").NotNull().WithMessage("TaxSalesOrderId zorunlu alan");

            RuleFor(x => x.DefaultPurchaseLocationId).NotEmpty().WithMessage("Purchase bos gecilemez").NotNull().WithMessage("Purchase zorunlu alan");

        }
    }
}
