using DAL.Contracts;
using DAL.DTO;
using DAL.Models;
using Dapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Security.Cryptography;


namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;
        private readonly ICompanyRepository _company;
        private readonly IDbConnection _db;
        private readonly ITaxRepository _taxRepository;
        private readonly IMeasureRepository _measure;
        private readonly ILocationsRepository _location;
        private readonly IGeneralDefaultRepository _generalDefault;
        private readonly ILogger<AuthController> _logger;
        private readonly IStockTransferRepository _transfer;
        private IValidator<CompanyRegisterDTO> _RegisterValidator;
        private IValidator<UserDto> _UserLoginValidator;




        public AuthController(IConfiguration configuration, ICompanyRepository company, IDbConnection db, ITaxRepository taxRepository, IMeasureRepository measure, ILocationsRepository location, IGeneralDefaultRepository generalDefault, ILogger<AuthController> logger, IStockTransferRepository transfer, IValidator<CompanyRegisterDTO> RegisterValidator, IValidator<UserDto> userLoginValidator)
        {
            _configuration = configuration;
            _company = company;
            _db = db;
            _taxRepository = taxRepository;
            _measure = measure;
            _location = location;
            _generalDefault = generalDefault;
            _logger = logger;
            _logger.LogDebug(1, "NLog injected into AuthController");
            _transfer = transfer;
            _RegisterValidator = RegisterValidator;
            _UserLoginValidator = userLoginValidator;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(CompanyRegisterDTO request)
        {
            ValidationResult result = await _RegisterValidator.ValidateAsync(request);
            if (result.IsValid)
            {
                CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
                user.Mail = request.Mail;
            
                int id = await _company.Register(request);

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber;
                user.CompanyId = Convert.ToString(id);
                user.Password = request.Password;
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                await _company.UserRegister(user, id);
                await _measure.Register(id);
                int taxid = await _taxRepository.Register(id);
                int locationid = await _location.Register(id);
                int legaladdress = await _location.RegisterLegalAddress(id);
                //Kullanıcı register olduktan sonra locations tablosuna settings locationdan ayrı olarak bir adet de Legal Adres ekliyor yukarıda burdada company tablosuna legal adress ıd yi vermek için güncelleme atıyoturuz
                DynamicParameters param = new DynamicParameters();
                param.Add("@id", id);
                param.Add("@LegalAddressId", legaladdress);
                string sql = $@"Update Company SET LocationId = @LegalAddressId where id = @id";
                await _db.ExecuteAsync(sql, param);
                await _generalDefault.Register(id, taxid, locationid);



                return Ok("Kayıt Başarılı");
            }
            else
            {


                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());

            }


        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto A)
        {
            ValidationResult result = await _UserLoginValidator.ValidateAsync(A);

            if (result.IsValid)
            {
                DynamicParameters prm = new DynamicParameters();
                prm.Add("@Passowrd", A.Password);
                prm.Add("@Mail", A.Mail);

                string sql = $@"Select * from Users where Mail=@Mail and Password=@Passowrd";
                var list = await _db.QueryAsync<Users>(sql,prm);
                if (list.Count() <= 0)
                {
                    return BadRequest("Giriş bilgileriniz kontrol ediniz.");

                }
                user.Mail = A.Mail;
                user.CompanyId = Convert.ToString(list.First().CompanyId);
                user.Id = list.First().Id;


                //if (!VerifyPasswordHash(request.Password, passwordhash, passwordsalt))
                //{
                //    return BadRequest("Wrong password.");
                //}
                string token = CreateToken(user);
                return Ok(token);
            }
            else
            {
                result.AddToModelState(this.ModelState);
                return BadRequest(result.ToString());
            }
          

        }


        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Mail),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.GivenName,value: user.CompanyId),
                new Claim(ClaimTypes.Gender,value:Convert.ToString(user.Id))
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
