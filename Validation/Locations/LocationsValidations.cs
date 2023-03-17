using DAL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.Locations
{
    public class LocationsInsertValidations:AbstractValidator<LocationsInsert>
    {
        public LocationsInsertValidations()
        {
            RuleFor(x => x.Uretim).NotEmpty().WithMessage("make boş geçilemez").NotNull().WithMessage("make zorunlu alan");
            RuleFor(x => x.Satis).NotEmpty().WithMessage("Sell boş geçilemez").NotNull().WithMessage("Sell zorunlu alan");
            RuleFor(x => x.SatinAlma).NotEmpty().WithMessage("Buy boş geçilemez").NotNull().WithMessage("Buy zorunlu alan");
            RuleFor(x => x.Isim).NotEmpty().WithMessage("LocationName boş geçilemez").NotNull().WithMessage("LocationName zorunlu alan");
            RuleFor(x => x.Ulke).NotEmpty().WithMessage("Country boş geçilemez").NotNull().WithMessage("Country zorunlu alan");
            RuleFor(x => x.Adres1).NotEmpty().WithMessage("AddressLine1 boş geçilemez").NotNull().WithMessage("AddressLine1 zorunlu alan");
            RuleFor(x => x.Sehir).NotEmpty().WithMessage("CityTown boş geçilemez").NotNull().WithMessage("CityTown zorunlu alan");

        }
    }
    public class LocationsUpdateValidations : AbstractValidator<LocationsDTO>
    {
        public LocationsUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id boş geçilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.Uretim).NotEmpty().WithMessage("make boş geçilemez").NotNull().WithMessage("make zorunlu alan");
            RuleFor(x => x.Satis).NotEmpty().WithMessage("make boş geçilemez").NotNull().WithMessage("make zorunlu alan");
            RuleFor(x => x.SatinAlma).NotEmpty().WithMessage("make boş geçilemez").NotNull().WithMessage("make zorunlu alan");
            RuleFor(x => x.Isim).NotEmpty().WithMessage("LocationName boş geçilemez").NotNull().WithMessage("LocationName zorunlu alan");
            RuleFor(x => x.Ulke).NotEmpty().WithMessage("Country boş geçilemez").NotNull().WithMessage("Country zorunlu alan");
            RuleFor(x => x.Adres1).NotEmpty().WithMessage("AddressLine1 boş geçilemez").NotNull().WithMessage("AddressLine1 zorunlu alan");
            RuleFor(x => x.Sehir).NotEmpty().WithMessage("CityTown boş geçilemez").NotNull().WithMessage("CityTown zorunlu alan");

        }
    }

}
