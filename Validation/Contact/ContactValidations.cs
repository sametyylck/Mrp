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
            RuleFor(x => x.Tip).NotEmpty().WithMessage("Tip bos gecilmez").NotNull().WithMessage("Tip Zorunlu alan");
        }
    }
    public class ContactInsertValidations : AbstractValidator<ContactsInsert>
    {
        public ContactInsertValidations()
        {
            
            RuleFor(x => x.Tip).NotEmpty().WithMessage("Tip bos gecilmez").NotNull().WithMessage("Tip Zorunlu alan");

            RuleFor(x => x.DisplayName).NotEmpty().WithMessage("Görünen isim bos gecilmez").NotNull().WithMessage("Görünen isim Zorunlu alan");

            RuleFor(x => x.Mail).NotEmpty().WithMessage("Mail bos gecilmez").NotNull().WithMessage(" Mail Zorunlu alan");



        }
    }
    public class ContactUpdateValidations : AbstractValidator<ContactsList>
    {
        public ContactUpdateValidations()
        {
            RuleFor(x => x.id).NotNull().WithMessage("Zorunlu alan").NotEmpty().WithMessage(" id zorunlu alan");

            RuleFor(x => x.Tip).NotEmpty().WithMessage("Tip bos gecilmez").NotNull().WithMessage("Tip zorunlu alan");

            RuleFor(x => x.DisplayName).NotEmpty().WithMessage("Görünen isim bos gecilmez").NotNull().WithMessage("Görünen isim Zorunlu alan");

            RuleFor(x => x.Mail).NotEmpty().WithMessage("Mail bos gecilmez").NotNull().WithMessage("Mail  Zorunlu alan");
        }
    }
    public class ContactUpdateAddressValidations : AbstractValidator<ContactsUpdateAddress>
    {
        public ContactUpdateAddressValidations()
        {

            RuleFor(x => x.id).NotNull().WithMessage("Zorunlu alan").NotEmpty().WithMessage("id alanı zorunlu");

            RuleFor(x => x.Tip).NotEmpty().WithMessage("Tip bos gecilmez").NotNull().WithMessage("Zorunlu alan");

            RuleFor(x => x.AddressLine1).NotNull().WithMessage("Zorunlu alan").NotEmpty().WithMessage("Adres satırı zorunlu");

            RuleFor(x => x.AddressPhone).NotEmpty().WithMessage("Tip bos gecilmez").NotNull().WithMessage("telefon Zorunlu alan");

            RuleFor(x => x.AddressCountry).NotNull().WithMessage("Zorunlu alan").NotEmpty().WithMessage("Ulke zorunlu alan");

            RuleFor(x => x.AddressFirstName).NotEmpty().WithMessage("Tip bos gecilmez").NotNull().WithMessage("İsim Zorunlu alan");
            RuleFor(x => x.AddressLastName).NotEmpty().WithMessage("Tip bos gecilmez").NotNull().WithMessage("Soyisim Zorunlu alan");

        }
    }
}
