using DAL.DTO;
using DAL.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.Company
{
    public class IdValidations:AbstractValidator<IdControl>
    {
        public IdValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilmez").NotNull();
        }
    }
    public class CompanyUpdateCompanyValidations : AbstractValidator<CompanyUpdateCompany>
    {
        public CompanyUpdateCompanyValidations()
        {
            RuleFor(x => x.GercekIsim).NotEmpty().WithMessage("Yasal isim bos gecilemez").NotNull();
            RuleFor(x => x.GorunenIsim).NotEmpty().WithMessage("Gorünen isim bos gecilemez").NotNull();


        }
    }
    public class CompanyInsertValidations : AbstractValidator<CompanyInsert>
    {
        public CompanyInsertValidations()
        {

            RuleFor(x => x.Ulke).NotEmpty().WithMessage("Ulke bos gecilemez").NotNull();

            RuleFor(x => x.Adres1).NotEmpty().WithMessage("Adress satırı bos gecilemez").NotNull();

                RuleFor(x => x.Sehir).NotEmpty().WithMessage("Sehir bos gecilemez").NotNull();
        }
    }
    public class CompanyUpdateValidations : AbstractValidator<CompanyUpdate>
    {
        public CompanyUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilmez").NotNull();

            RuleFor(x => x.Ulke).NotEmpty().WithMessage("Ulke bos gecilemez").NotNull();

            RuleFor(x => x.Adres1).NotEmpty().WithMessage("Adress satırı bos gecilemez").NotNull();

            RuleFor(x => x.Sehir).NotEmpty().WithMessage("Sehir bos gecilemez").NotNull();
        }

    }
    

}
