using Application.Features.DocumentTypes;
using Application.Features.Genders;
using Application.Features.PaymentMethods;
using Application.Features.Roles;
using Application.Features.StreetTypes;
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

        return services;
    }
}
