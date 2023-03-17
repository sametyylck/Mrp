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
            RuleFor(x => x.SatinAlmaId).NotNull().WithMessage("OrdersId zorunlu alan").NotEmpty().WithMessage("OrdersId boş geçilmez");
            RuleFor(x => x.StokId).NotNull().WithMessage("ItemId zorunlu alan").NotEmpty().WithMessage("ItemId boş geçilmez");
  
        }
    }
    public class OrdersInsertValidations : AbstractValidator<PurchaseOrderInsert>
    {
        public OrdersInsertValidations()
        {
            RuleFor(x => x.Tip).NotNull().WithMessage("Tip zorunlu alan").NotEmpty().WithMessage("Tip boş geçilmez");
            RuleFor(x => x.TedarikciId).NotNull().WithMessage("ContactId zorunlu alan").NotEmpty().WithMessage("ContactId boş geçilmez");
            RuleFor(x => x.SatinAlmaIsmi).NotNull().WithMessage("OrderName zorunlu alan").NotEmpty().WithMessage("OrderName boş geçilmez");
            RuleFor(x => x.DepoId).NotNull().WithMessage("LocationId zorunlu alan").NotEmpty().WithMessage("LocationId boş geçilmez");
            RuleFor(x => x.BeklenenTarih).NotNull().WithMessage("ExpectedDate zorunlu alan").NotEmpty().WithMessage("ExpectedDate boş geçilmez");
            RuleFor(x => x.OlusturmaTarihi).NotNull().WithMessage("CreateDate zorunlu alan").NotEmpty().WithMessage("CreateDate boş geçilmez");
            RuleFor(x => x.SatisId).NotNull().WithMessage("SalesOrderId zorunlu alan").NotEmpty().WithMessage("SalesOrderId boş geçilmez");
            RuleFor(x => x.SatisDetayId).NotNull().WithMessage("SalesOrderItemId zorunlu alan").NotEmpty().WithMessage("SalesOrderItemId boş geçilmez");
            RuleFor(x => x.UretimId).NotNull().WithMessage("ManufacturingOrderId zorunlu alan").NotEmpty().WithMessage("ManufacturingOrderId boş geçilmez");
            RuleFor(x => x.UretimDetayId).NotNull().WithMessage("ManufacturingOrderItemId zorunlu alan").NotEmpty().WithMessage("ManufacturingOrderItemId boş geçilmez");




        }
    }
    public class OrdersInsertItemValidations : AbstractValidator<PurchaseOrderInsertItem>
    {
        public OrdersInsertItemValidations()
        {
                 RuleFor(x => x.SatinAlmaId).NotNull().WithMessage("OrderId zorunlu alan").NotEmpty().WithMessage("OrderId boş geçilmez");
            RuleFor(x => x.OlcuId).NotNull().WithMessage("MeasureId zorunlu alan").NotEmpty().WithMessage("MeasureId boş geçilmez");
            RuleFor(x => x.StokId).NotNull().WithMessage("ItemId zorunlu alan").NotEmpty().WithMessage("ItemId boş geçilmez");
            RuleFor(x => x.VergiId).NotNull().WithMessage("TaxId zorunlu alan").NotEmpty().WithMessage("TaxId boş geçilmez");
            RuleFor(x => x.Miktar).NotNull().WithMessage("Quantity zorunlu alan").NotEmpty().WithMessage("Quantity boş geçilmez");

        }
    }
    public class OrdersUpdateValidations : AbstractValidator<PurchaseOrderUpdate>
    {
        public OrdersUpdateValidations()
        {
            RuleFor(x => x.id).NotNull().WithMessage("id zorunlu alan").NotEmpty().WithMessage("id boş geçilmez");
            RuleFor(x => x.DepoId).NotNull().WithMessage("LocationId zorunlu alan").NotEmpty().WithMessage("LocationId boş geçilmez");
            RuleFor(x => x.BeklenenTarih).NotNull().WithMessage("ExpectedDate zorunlu alan").NotEmpty().WithMessage("ExpectedDate boş geçilmez");
        }
    }
    public class OrdersUpdatePurchaseItemValidations : AbstractValidator<PurchaseItem>
    {
        public OrdersUpdatePurchaseItemValidations()
        {
            RuleFor(x => x.id).NotNull().WithMessage("id zorunlu alan").NotEmpty().WithMessage("id boş geçilmez");
            RuleFor(x => x.VergiId).NotNull().WithMessage("TaxId zorunlu alan").NotEmpty().WithMessage("TaxId boş geçilmez");
            RuleFor(x => x.StokId).NotNull().WithMessage("ItemId zorunlu alan").NotEmpty().WithMessage("ItemId boş geçilmez");
            RuleFor(x => x.OlcuId).NotNull().WithMessage("MeasureId zorunlu alan").NotEmpty().WithMessage("MeasureId boş geçilmez");
            RuleFor(x => x.Miktar).NotNull().WithMessage("Quantity zorunlu alan").NotEmpty().WithMessage("Quantity boş geçilmez");
            RuleFor(x => x.BirimFiyat).NotNull().WithMessage("PricePerUnit zorunlu alan").NotEmpty().WithMessage("PricePerUnit boş geçilmez");
        }
    }

    public class OrdersUpdateOrdersStockValidations : AbstractValidator<PurchaseOrderId>
    {
        public OrdersUpdateOrdersStockValidations()
        {
            RuleFor(x => x.id).NotNull().WithMessage("id zorunlu alan").NotEmpty().WithMessage("id boş geçilmez");
            RuleFor(x => x.DurumBelirteci).NotNull().WithMessage("TaxId zorunlu alan").NotEmpty().WithMessage("TaxId boş geçilmez");
        }
    }



}
