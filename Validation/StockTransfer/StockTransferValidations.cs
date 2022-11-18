using DAL.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockTransferDTO;

namespace Validation.StockTransfer
{
    public class StockTransferInsertValidations:AbstractValidator<StockTransferInsert>
    {
        public StockTransferInsertValidations()
        {
            RuleFor(x=>x.OriginId).NotEmpty().WithMessage("Origin bos gecilmez").NotNull().WithMessage("Origin alanı zorunlu");
            RuleFor(x => x.DestinationId).NotEmpty().WithMessage("DestinationId bos gecilmez").NotNull().WithMessage("DestinationId alanı zorunlu");
            RuleFor(x => x.TransferDate).NotEmpty().WithMessage("TransferDate bos gecilmez").NotNull().WithMessage("TransferDate alanı zorunlu");
            RuleFor(x => x.StockTransferName).NotEmpty().WithMessage("TransferDate bos gecilmez").NotNull().WithMessage("TransferDate alanı zorunlu");
            RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity bos gecilmez").NotNull().WithMessage("Quantity alanı zorunlu");
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId bos gecilmez").NotNull().WithMessage("ItemId alanı zorunlu");
        }
    }
    public class StockTransferInsertItemValidations : AbstractValidator<StockTransferInsertItem>
    {
        public StockTransferInsertItemValidations()
        {
            RuleFor(x => x.StockTransferId).NotEmpty().WithMessage("StockTransferId bos gecilmez").NotNull().WithMessage("StockTransferId alanı zorunlu");
            RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity bos gecilmez").NotNull().WithMessage("Quantity alanı zorunlu");
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId bos gecilmez").NotNull().WithMessage("ItemId alanı zorunlu");
        }
    }
    public class StockTransferDeleteItemsValidations : AbstractValidator<StockTransferDeleteItems>
    {
        public StockTransferDeleteItemsValidations()
        {
            RuleFor(x => x.StockTransferId).NotEmpty().WithMessage("StockTransferId bos gecilmez").NotNull().WithMessage("StockTransferId alanı zorunlu");
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilmez").NotNull().WithMessage("id alanı zorunlu");
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId bos gecilmez").NotNull().WithMessage("ItemId alanı zorunlu");
        }
    }
   
    public class StockTransferUpdateValidations : AbstractValidator<StockUpdate>
    {
        public StockTransferUpdateValidations()
        {
            RuleFor(x => x.StockTransferName).NotEmpty().WithMessage("StockTransferName bos gecilmez").NotNull().WithMessage("StockTransferName alanı zorunlu");
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilmez").NotNull().WithMessage("id alanı zorunlu");
            RuleFor(x => x.TransferDate).NotEmpty().WithMessage("TransferDate bos gecilmez").NotNull().WithMessage("TransferDate alanı zorunlu");
        }
    }
    public class StockTransferUpdateItemsValidations : AbstractValidator<StockTransferItems>
    {
        public StockTransferUpdateItemsValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilmez").NotNull().WithMessage("id alanı zorunlu");
            RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity bos gecilmez").NotNull().WithMessage("Quantity alanı zorunlu");
            RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId bos gecilmez").NotNull().WithMessage("ItemId alanı zorunlu");
            RuleFor(x => x.StockTransferId).NotEmpty().WithMessage("StockTransferId bos gecilmez").NotNull().WithMessage("StockTransferId alanı zorunlu");
        }
    }
}
