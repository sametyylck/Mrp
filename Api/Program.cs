
global using BL.Services.UserService;
using DAL.Contracts;
using DAL.Models;
using DAL.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Data;
using System.Text;
using System;
using FluentValidation.AspNetCore;
using System.Reflection;
using FluentValidation;
using BL.Services.Bom;
using BL.Services.Items;
using BL.Services.StockTransfer;
using BL.Services.Orders;
using DAL.StockControl;
using DAL.DTO;
using Validation.Auth;
using static Validation.Auth.AuthValidation;
using Validation.Bom;
using Validation.Category;
using Validation.Company;
using static DAL.DTO.ContactDTO;
using Validation.Contact;
using Validation.GeneralDefault;
using static DAL.DTO.GeneralSettingsDTO;
using static DAL.DTO.ItemDTO;
using Validation.Items;
using static DAL.DTO.ManufacturingOrderDTO;
using Validation.ManufacturingOrder;
using static DAL.DTO.ManufacturingOrderItemDTO;
using Validation.ManufacturingOrderItem;
using Validation.Measure;
using Validation.Operations;
using static DAL.DTO.ProductOperationsBomDTO;
using Validation.OperationsBom;
using Validation.Orders;
using static DAL.DTO.PurchaseOrderDTO;
using Validation.Resource;
using static DAL.DTO.StockAdjusmentDTO;
using Validation.StockAdjusment;
using static DAL.DTO.TaxDTO;
using Validation.Tax;
using static DAL.DTO.StockTransferDTO;
using Validation.StockTransfer;
using static DAL.DTO.StockTakesDTO;
using Validation.StockTakes;
using static DAL.DTO.SalesOrderDTO;
using Validation.SalesOrder;
using Validation.SalesOrderItem;
using BL.Services.IdControl;
using BL.Services.Contact;
using BL.Services.ManufacturingOrder;
using BL.Services.OperationsBom;
using Validation.Locations;
using BL.Services.GeneralDefaultSettings;
using BL.Services.SalesOrder;
using BL.Services.StockAdjusment;
using BL.Services.StockTakes;
using BL.Services.LocationStock;
using PurchaseBuy = DAL.DTO.PurchaseBuy;
using BL.Extensions;
using BL.Services.Kullanýcý;
using DAL.StokHareket;
using DAL.Hareket;
using DAL.Hareket.Fatura;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
// Add services to the container.



builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standart Yetkilendirme Baslýgý (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddSingleton<kolaymrpContext>();
//fluent validation baþlangýç
builder.Services.AddScoped<IValidator<CompanyRegisterDTO>, AuthRegisterValidations>();
builder.Services.AddScoped<IValidator<BomDTO.BOMInsert>, BomInsertValidations>();
builder.Services.AddScoped<IValidator<BomDTO.BOMUpdate>, BomUpdateValidations>();
builder.Services.AddScoped<IValidator<CategoryDTO.CategoryInsert>, CategoryInsertValidations>();
builder.Services.AddScoped<IValidator<CategoryDTO.CategoryUpdate>, CategoryUpdateValidations>();
builder.Services.AddScoped<IValidator<CompanyInsert>, CompanyInsertValidations>();
builder.Services.AddScoped<IValidator<CompanyUpdateCompany>, CompanyUpdateCompanyValidations>();
builder.Services.AddScoped<IValidator<IdControl>, IdValidations>();
builder.Services.AddScoped<IValidator<CompanyUpdate>, CompanyUpdateValidations>();
builder.Services.AddScoped<IValidator<UserDto>, AuthLoginValidations>();
builder.Services.AddScoped<IValidator<ContactsInsert>, ContactInsertValidations>();
builder.Services.AddScoped<IValidator<ContactsDelete>, ContactDeleteValidations>();
builder.Services.AddScoped<IValidator<CariUpdate>, ContactUpdateValidations>();
builder.Services.AddScoped<IValidator<ContactsUpdateAddress>, ContactUpdateAddressValidations>();
builder.Services.AddScoped<IValidator<GeneralDefaultSettings>, GeneralDefaultValidations>();
builder.Services.AddScoped<IValidator<ItemsListele>, ItemsListeValidations>();
builder.Services.AddScoped<IValidator<ItemsInsert>, ItemsInsertValidations>();
builder.Services.AddScoped<IValidator<ItemsDelete>, ItemsDeleteValidations>();
builder.Services.AddScoped<IValidator<ItemsUpdate>, ItemsUpdateValidations>();
builder.Services.AddScoped<IValidator<StockTakesUpdateItems>, StockTakesUpdateItemsValidations>();
builder.Services.AddScoped<IValidator<LocationsDTO>, LocationsUpdateValidations>();
builder.Services.AddScoped<IValidator<LocationsInsert>, LocationsInsertValidations>();
builder.Services.AddScoped<IValidator<UretimDTO>, ManufacturingOrderInsertOrderValidations>();
builder.Services.AddScoped<IValidator<UretimUpdate>, ManufacturingOrderUpdateValidations>();
builder.Services.AddScoped<IValidator<UretimTamamlama>, ManufacturingOrderDoneValidations>();
builder.Services.AddScoped<IValidator<ManufacturingTaskDone>, ManufacturingOrderTaskDoneValidations>();
builder.Services.AddScoped<IValidator<UretimDeleteItems>, ManufacturingOrderDeleteItemsValidations>();
builder.Services.AddScoped<IValidator<UretimIngredientsUpdate>, ManufacturingOrderItemIngredientUpdateValidations>();
builder.Services.AddScoped<IValidator<UretimIngredientsInsert>, ManufacturingOrderItemIngredientInsertValidations>();
builder.Services.AddScoped<IValidator<UretimOperationsInsert>, ManufacturingOrderItemOperationInsertValidations>();
builder.Services.AddScoped<IValidator<UretimOperationsUpdate>, ManufacturingOrderItemOperationUpdateValidations>();
builder.Services.AddScoped<IValidator<PurchaseBuy>, ManufacturingOrderPurchaseOrderValidations>();
builder.Services.AddScoped<IValidator<MeasureInsert>, MeasureInsertValidations>();
builder.Services.AddScoped<IValidator<MeasureUpdate>, MeasureUpdateValidations>();
builder.Services.AddScoped<IValidator<OperationsInsert>, OperationsInsertValidations>();
builder.Services.AddScoped<IValidator<OperationsUpdate>, OperationsUpdateValidations>();
builder.Services.AddScoped<IValidator<ProductOperationsBOMUpdate>, OperationBomUpdateValidations>();
builder.Services.AddScoped<IValidator<ProductOperationsBOMInsert>, OperationBomInsertValidations>();
builder.Services.AddScoped<IValidator<Delete>, OrdersDeleteValidations>();
builder.Services.AddScoped<IValidator<PurchaseOrderInsert>, OrdersInsertValidations>();
builder.Services.AddScoped<IValidator<PurchaseOrderInsertItem>, OrdersInsertItemValidations>();
builder.Services.AddScoped<IValidator<PurchaseOrderUpdate>, OrdersUpdateValidations>();
builder.Services.AddScoped<IValidator<PurchaseItem>, OrdersUpdatePurchaseItemValidations>();
builder.Services.AddScoped<IValidator<PurchaseOrderId>, OrdersUpdateOrdersStockValidations>();
builder.Services.AddScoped<IValidator<DeleteItems>, OrdersDeleteItemsValidations>();
builder.Services.AddScoped<IValidator<ResourcesInsert>, ResourceInsertValidations>();
builder.Services.AddScoped<IValidator<ResourcesUpdate>, ResourceUpdateValidations>();
builder.Services.AddScoped<IValidator<StockAdjusmentItemDelete>, StockAdjusmentItemDeleteValidations>();
builder.Services.AddScoped<IValidator<StockAdjusmentInsertItem>, StockAdjusmentInsertItemValidations>();
builder.Services.AddScoped<IValidator<StockAdjusmentInsert>, StockAdjusmentInsertValidations>();
builder.Services.AddScoped<IValidator<StockAdjusmentUpdateItems>, StockAdjusmentValidations>();
builder.Services.AddScoped<IValidator<StockAdjusmentUpdate>, StockAdjusmentUpdateValidations>();
builder.Services.AddScoped<IValidator<StockAdjusmentUpdateItems>, StockAdjusmentValidations>();
builder.Services.AddScoped<IValidator<TaxInsert>, TaxInsertValidations>();
builder.Services.AddScoped<IValidator<TaxUpdate>, TaxUpdateValidations>();
builder.Services.AddScoped<IValidator<StockTransferInsert>, StockTransferInsertValidations>();
builder.Services.AddScoped<IValidator<StockTransferInsertItem>, StockTransferInsertItemValidations>();
builder.Services.AddScoped<IValidator<StockTransferDeleteItems>, StockTransferDeleteItemsValidations>();
builder.Services.AddScoped<IValidator<StockUpdate>, StockTransferUpdateValidations>();
builder.Services.AddScoped<IValidator<StokAktarimDetay>, StockTransferUpdateItemsValidations>();
builder.Services.AddScoped<IValidator<StockTakeDelete>, StockTakesDeleteItemsValidations>();
builder.Services.AddScoped<IValidator<StockTakesInsert>, StockTakesInsertValidations>();
builder.Services.AddScoped<IValidator<StockTakeInsertItems>, StockTakesInsertItemValidations>();
builder.Services.AddScoped<IValidator<StockTakesUpdate>, StockTakesUpdateValidations>();
builder.Services.AddScoped<IValidator<StockTakesDone>, StockTakesTaskDoneValidations>();
builder.Services.AddScoped<IValidator<SatýsDTO>, SalesOrderInsertValidations>();
builder.Services.AddScoped<IValidator<SatýsInsertItem>, SalesOrderInsertItemValidations>();
builder.Services.AddScoped<IValidator<SalesOrderUpdate>, SalesOrderUpdateValidations>();
builder.Services.AddScoped<IValidator<SalesDeleteItems>, SalesOrderDeleteItemValidations>();
builder.Services.AddScoped<IValidator<SalesDone>, SalesOrderSalesDoneValidations>();
builder.Services.AddScoped<IValidator<SalesOrderDTO.Quotess>, SalesOrderQuotesDoneValidations>();
builder.Services.AddScoped<IValidator<SalesOrderMake>, SalesOrderItemMakeValidations>();
builder.Services.AddScoped<IValidator<SatýsUpdateItems>, SalesOrderItemUpdateItemValidations>();
//fluent validations bitis

//DAL Interface Baslangýc

builder.Services.AddScoped<IBomRepository, BomRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IContactsRepository, ContactsRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IGeneralDefaultRepository, GeneralDefaultSettingsRepository>();
builder.Services.AddScoped<ILocationStockRepository, LocationStockRepository>();
builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();
builder.Services.AddScoped<IItemsRepository, ItemsRepository>();
builder.Services.AddScoped<IMeasureRepository, MeasureRepository>();
builder.Services.AddScoped<IOperationsRepository, OperationsRepository>();
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
builder.Services.AddScoped<IOrderStockRepository, OrderStockRepository>();
builder.Services.AddScoped<IProductOperationsBomRepository, ProductOperationsBomRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<IStockAdjusmentRepository, StockAdjusmentRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IStockTakesRepository, StockTakesRepository>();
builder.Services.AddScoped<IStockTransferRepository, StockTransferRepository>();
builder.Services.AddScoped<ITaxRepository, TaxRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUretimRepository, UretimRepository>();
builder.Services.AddScoped<IUretimList, UretimListRepository>();
builder.Services.AddScoped<ISatýsRepository, SatýsRepository>();
builder.Services.AddScoped<ISatýsListRepository, SatýsListRepository>();
builder.Services.AddScoped<ITeklifRepository, TeklifRepository>();
builder.Services.AddScoped<IPermissionControl, PermissionControl>();
builder.Services.AddScoped<IKullanýcýRepository, KullanýcýRepository>();
builder.Services.AddScoped<IKullanýcýKontrol, KullanýcýKontrol>();
builder.Services.AddScoped<IStokHareket, StokHareket>();
builder.Services.AddScoped<ICariHareket, CariHareket>();
builder.Services.AddScoped<IEvrakNumarasýOLusturucu, EvrakNumarasýOlusturucu>();
builder.Services.AddScoped<IFatura, Fatura>();



//Dal Interface Bitis
//---------------------------------------------------------------------------------------------------

//BL Control Service Baslangýc.
builder.Services.AddScoped<IIDControl, IDControl>();
builder.Services.AddScoped<IContactControl, ContactControl>();
builder.Services.AddScoped<IManufacturingOrderControl, ManufacturingOrderControl>();
builder.Services.AddScoped<IOperationBomControl, OperationBomControl>();
builder.Services.AddScoped<IBomControl, BomControl>();
builder.Services.AddScoped<IItemsControl, ItemControl>();
builder.Services.AddScoped<IStockTransferControl, StockTransferControl>();
builder.Services.AddScoped<IPurchaseOrderControl, PurchaseOrderControl>();
builder.Services.AddScoped<IStockControl, StockControl>();
builder.Services.AddScoped<IGeneralDefaultSettingsControl, GeneralDefaultSettingsControl>();
builder.Services.AddScoped<ISalesOrderControl, SalesOrderControl>();
builder.Services.AddScoped<IStockAdjusmentControl, StockAdjusmentControl>();
builder.Services.AddScoped<IStockTakesControl, StockTakesControl>();
builder.Services.AddScoped<IStockTransferControl, StockTransferControl>();
builder.Services.AddScoped<IStockControl, StockControl>();
builder.Services.AddScoped<ILocationStockControl, LocationStockControl>();


builder.Services.AddHttpContextAccessor();

string connection = builder.Configuration.GetConnectionString("SqlConnection");
builder.Services.AddTransient<IDbConnection>((sp) => new SqlConnection(connection));
builder.Services.AddCors(options => options.AddPolicy(name: "NgOrigins",
    policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    }));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("NgOrigins");
app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



