using DAL.DTO;
using Dapper;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Validation.Auth
{
    public class AuthValidation
    {
        public class AuthRegisterValidations : AbstractValidator<CompanyRegisterDTO>
        {
            IDbConnection _db;



            private bool HasValidPassword(string pw)
            {
                var lowercase = new Regex("[a-z]+");
                var uppercase = new Regex("[A-Z]+");
                var digit = new Regex("(\\d)+");
                var symbol = new Regex("(\\W)+");

                return (lowercase.IsMatch(pw) && uppercase.IsMatch(pw) && digit.IsMatch(pw) && symbol.IsMatch(pw));
            }
            private bool Mail(string mail)
            {
                DynamicParameters prm = new();
                prm.Add("@mail", mail);
                string sqlquery = $@"Select Count(*)as varmı from Users where Mail=@mail";
                var list =_db.Query<int>(sqlquery,prm);
               
                return true;
            }
            public AuthRegisterValidations(IDbConnection db)
            {
                RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name bos geçilmez.").MaximumLength(50).MinimumLength(2);
                RuleFor(x => x.LastName).NotEmpty().WithMessage("Last Name bos geçilemez").MaximumLength(50).MinimumLength(2).NotNull(); ;
                RuleFor(x => x.DisplayName).NotEmpty().WithMessage("DisplayName bos geçilemez").MaximumLength(50).MinimumLength(2).NotNull(); ;
                RuleFor(x => x.LegalName).NotEmpty().WithMessage("LegalName bos gecilemez").MaximumLength(50).MinimumLength(2).NotNull(); ;
                RuleFor(p => p.PhoneNumber)
                .NotEmpty()
                .NotNull().WithMessage("Phone Number null olamaz.")
                .MinimumLength(10).WithMessage("PhoneNumber 10 karakterden az olamaz ")
                .MaximumLength(20).WithMessage("PhoneNumber  20 karakterden fazla olamaz.");
                RuleFor(x => x.Mail).NotEmpty().WithMessage("Mail bos gecilemez").EmailAddress().WithMessage("Mail formati yanlıs").Must(x=>Mail(x)).WithMessage("Boyle bir mail kayitli.");
                RuleFor(x => x.Password).NotEmpty().WithMessage("Password bos gecilemez").Length(5, 20).Must(x => HasValidPassword(x)).WithMessage("Sifreniz büyük,küçük harf,özel karakter ve sayi içermelidir.").NotNull();
                _db = db;
            }
        }
        public class AuthLoginValidations : AbstractValidator<UserDto>
        {
            public AuthLoginValidations()
            {
                //RuleFor(x => x.Mail).NotEmpty().WithMessage("Mail bos gecilemez").EmailAddress().WithMessage("Mail formati yanlıs");
                RuleFor(x => x.Password).NotEmpty().WithMessage("Password bos gecilemez");

            }
        }


    }
    



}
