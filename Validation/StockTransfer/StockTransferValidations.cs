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
            RuleFor(x=>x.BaslangicDepo).NotEmpty().WithMessage("Origin bos gecilmez").NotNull().WithMessage("Origin alanı zorunlu");
            RuleFor(x => x.HedefDepo).NotEmpty().WithMessage("DestinationId bos gecilmez").NotNull().WithMessage("DestinationId alanı zorunlu");
            RuleFor(x => x.AktarmaTarihi).NotEmpty().WithMessage("TransferDate bos gecilmez").NotNull().WithMessage("TransferDate alanı zorunlu");
            RuleFor(x => x.AktarimIsmi).NotEmpty().WithMessage("TransferDate bos gecilmez").NotNull().WithMessage("TransferDate alanı zorunlu");
            RuleFor(x => x.Miktar).NotEmpty().WithMessage("Quantity bos gecilmez").NotNull().WithMessage("Quantity alanı zorunlu");
            RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId bos gecilmez").NotNull().WithMessage("ItemId alanı zorunlu");
        }
    }
    public class StockTransferInsertItemValidations : AbstractValidator<StockTransferInsertItem>
    {
        public StockTransferInsertItemValidations()
        {
            RuleFor(x => x.StokAktarimId).NotEmpty().WithMessage("StockTransferId bos gecilmez").NotNull().WithMessage("StockTransferId alanı zorunlu");
            RuleFor(x => x.Miktar).NotEmpty().WithMessage("Quantity bos gecilmez").NotNull().WithMessage("Quantity alanı zorunlu");
            RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId bos gecilmez").NotNull().WithMessage("ItemId alanı zorunlu");
        }
    }
    public class StockTransferDeleteItemsValidations : AbstractValidator<StockTransferDeleteItems>
    {
        public StockTransferDeleteItemsValidations()
        {
            RuleFor(x => x.StokAktarimId).NotEmpty().WithMessage("StockTransferId bos gecilmez").NotNull().WithMessage("StockTransferId alanı zorunlu");
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilmez").NotNull().WithMessage("id alanı zorunlu");
            RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId bos gecilmez").NotNull().WithMessage("ItemId alanı zorunlu");
        }
    }
   
    public class StockTransferUpdateValidations : AbstractValidator<StockUpdate>
    {
        public StockTransferUpdateValidations()
        {
            RuleFor(x => x.AktarimIsmi).NotEmpty().WithMessage("StockTransferName bos gecilmez").NotNull().WithMessage("StockTransferName alanı zorunlu");
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilmez").NotNull().WithMessage("id alanı zorunlu");
            RuleFor(x => x.AktarmaTarihi).NotEmpty().WithMessage("TransferDate bos gecilmez").NotNull().WithMessage("TransferDate alanı zorunlu");
        }
    }
    public class StockTransferUpdateItemsValidations : AbstractValidator<StokAktarimDetay>
    {
        public StockTransferUpdateItemsValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilmez").NotNull().WithMessage("id alanı zorunlu");
            RuleFor(x => x.Miktar).NotEmpty().WithMessage("Quantity bos gecilmez").NotNull().WithMessage("Quantity alanı zorunlu");
            RuleFor(x => x.StokId).NotEmpty().WithMessage("ItemId bos gecilmez").NotNull().WithMessage("ItemId alanı zorunlu");
            RuleFor(x => x.StokAktarimId).NotEmpty().WithMessage("StockTransferId bos gecilmez").NotNull().WithMessage("StockTransferId alanı zorunlu");
        }
    }
}
