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
            RuleFor(x => x.Make).NotEmpty().WithMessage("make boş geçilemez").NotNull().WithMessage("make zorunlu alan");
            RuleFor(x => x.Sell).NotEmpty().WithMessage("Sell boş geçilemez").NotNull().WithMessage("Sell zorunlu alan");
            RuleFor(x => x.Buy).NotEmpty().WithMessage("Buy boş geçilemez").NotNull().WithMessage("Buy zorunlu alan");
            RuleFor(x => x.LocationName).NotEmpty().WithMessage("LocationName boş geçilemez").NotNull().WithMessage("LocationName zorunlu alan");
            RuleFor(x => x.Country).NotEmpty().WithMessage("Country boş geçilemez").NotNull().WithMessage("Country zorunlu alan");
            RuleFor(x => x.AddressLine1).NotEmpty().WithMessage("AddressLine1 boş geçilemez").NotNull().WithMessage("AddressLine1 zorunlu alan");
            RuleFor(x => x.CityTown).NotEmpty().WithMessage("CityTown boş geçilemez").NotNull().WithMessage("CityTown zorunlu alan");

        }
    }
    public class LocationsUpdateValidations : AbstractValidator<LocationsDTO>
    {
        public LocationsUpdateValidations()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("id boş geçilemez").NotNull().WithMessage("id zorunlu alan");
            RuleFor(x => x.Make).NotEmpty().WithMessage("make boş geçilemez").NotNull().WithMessage("make zorunlu alan");
            RuleFor(x => x.Sell).NotEmpty().WithMessage("make boş geçilemez").NotNull().WithMessage("make zorunlu alan");
            RuleFor(x => x.Buy).NotEmpty().WithMessage("make boş geçilemez").NotNull().WithMessage("make zorunlu alan");
            RuleFor(x => x.LocationName).NotEmpty().WithMessage("LocationName boş geçilemez").NotNull().WithMessage("LocationName zorunlu alan");
            RuleFor(x => x.Country).NotEmpty().WithMessage("Country boş geçilemez").NotNull().WithMessage("Country zorunlu alan");
            RuleFor(x => x.AddressLine1).NotEmpty().WithMessage("AddressLine1 boş geçilemez").NotNull().WithMessage("AddressLine1 zorunlu alan");
            RuleFor(x => x.CityTown).NotEmpty().WithMessage("CityTown boş geçilemez").NotNull().WithMessage("CityTown zorunlu alan");

        }
    }

}
