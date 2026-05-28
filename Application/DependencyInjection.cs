using Application.Features.AuditActionTypes;
using Application.Features.Addresses;
using Application.Features.CardTypes;
using Application.Features.Cities;
using Application.Features.Countries;
using Application.Features.Departments;
using Application.Features.DocumentTypes;
using Application.Features.Genders;
using Application.Features.InvoiceStatuses;
using Application.Features.MechanicSpecialties;
using Application.Features.OrderStatuses;
using Application.Features.PartBrands;
using Application.Features.PartCategories;
using Application.Features.PaymentStatuses;
using Application.Features.PaymentMethods;
using Application.Features.Roles;
using Application.Features.ServiceTypes;
using Application.Features.StreetTypes;
using Application.Features.Neighborhoods;
using Application.Features.VehicleBrands;
using Application.Features.VehicleTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IDocumentTypeService, DocumentTypeService>();
        services.AddScoped<IGenderService, GenderService>();
        services.AddScoped<IStreetTypeService, StreetTypeService>();
        services.AddScoped<IVehicleTypeService, VehicleTypeService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<IPaymentStatusService, PaymentStatusService>();
        services.AddScoped<IInvoiceStatusService, InvoiceStatusService>();
        services.AddScoped<IOrderStatusService, OrderStatusService>();
        services.AddScoped<ICardTypeService, CardTypeService>();
        services.AddScoped<IAuditActionTypeService, AuditActionTypeService>();
        services.AddScoped<IMechanicSpecialtyService, MechanicSpecialtyService>();
        services.AddScoped<IServiceTypeService, ServiceTypeService>();
        services.AddScoped<IPartCategoryService, PartCategoryService>();
        services.AddScoped<IPartBrandService, PartBrandService>();
        services.AddScoped<IVehicleBrandService, VehicleBrandService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<INeighborhoodService, NeighborhoodService>();
        services.AddScoped<IAddressService, AddressService>();

        return services;
    }
}
