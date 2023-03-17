using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.ContactDTO;

namespace Validation.Contact
{
    public class ContactDeleteValidations:AbstractValidator<ContactsDelete>
    {
        public ContactDeleteValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id bos gecilmez").NotNull().WithMessage("id zorunlu alan");
        }
    }
    public class ContactInsertValidations : AbstractValidator<ContactsInsert>
    {
        public ContactInsertValidations()
        {
            
            RuleFor(x => x.CariTipId).NotEmpty().WithMessage("CariTipId bos gecilmez").NotNull().WithMessage("CariTipId Zorunlu alan");

            RuleFor(x => x.AdSoyad).NotEmpty().WithMessage("AdSoyad  bos gecilmez").NotNull().WithMessage("AdSoyad Zorunlu alan");

            RuleFor(x => x.Mail).NotEmpty().WithMessage("Mail bos gecilmez").NotNull().WithMessage(" Mail Zorunlu alan");



        }
    }
    public class ContactUpdateValidations : AbstractValidator<CariUpdate>
    {
        public ContactUpdateValidations()
        {
            RuleFor(x => x.CariKod).NotNull().WithMessage("Zorunlu alan").NotEmpty().WithMessage(" id zorunlu alan");

            RuleFor(x => x.Mail).NotEmpty().WithMessage("Mail bos gecilmez").NotNull().WithMessage("Mail  Zorunlu alan");
        }
    }
    public class ContactUpdateAddressValidations : AbstractValidator<ContactsUpdateAddress>
    {
        public ContactUpdateAddressValidations()
        {

            RuleFor(x => x.id).NotNull().WithMessage("Zorunlu alan").NotEmpty().WithMessage("id alanı zorunlu");

            RuleFor(x => x.Tip).NotEmpty().WithMessage("Tip bos gecilmez").NotNull().WithMessage("Zorunlu alan");

            RuleFor(x => x.Adres1).NotNull().WithMessage("Zorunlu alan").NotEmpty().WithMessage("Adres satırı zorunlu");

            RuleFor(x => x.AdresTelefon).NotEmpty().WithMessage("Tip bos gecilmez").NotNull().WithMessage("telefon Zorunlu alan");

            RuleFor(x => x.AdresUlke).NotNull().WithMessage("Zorunlu alan").NotEmpty().WithMessage("Ulke zorunlu alan");

            RuleFor(x => x.AdresAd).NotEmpty().WithMessage("Tip bos gecilmez").NotNull().WithMessage("İsim Zorunlu alan");
            RuleFor(x => x.AdresSoyisim).NotEmpty().WithMessage("Tip bos gecilmez").NotNull().WithMessage("Soyisim Zorunlu alan");

        }
    }
}
