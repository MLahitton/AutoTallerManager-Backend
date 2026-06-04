using System.Linq.Expressions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(context, cancellationToken);
        await SeedDocumentTypesAsync(context, cancellationToken);
        await SeedDepartmentsAndCitiesAsync(context, cancellationToken);
        await SeedVehicleTypesAsync(context, cancellationToken);
        await SeedVehicleBrandsAndModelsAsync(context, cancellationToken);
        await SeedOrderStatusesAsync(context, cancellationToken);
        await SeedInvoiceStatusesAsync(context, cancellationToken);
        await SeedPaymentStatusesAsync(context, cancellationToken);
        await SeedPaymentMethodsAsync(context, cancellationToken);
        await SeedCardTypesAsync(context, cancellationToken);
        await SeedServiceTypesAsync(context, cancellationToken);
        await SeedMechanicSpecialtiesAsync(context, cancellationToken);
        await SeedPartCategoriesAsync(context, cancellationToken);
        await SeedPartBrandsAsync(context, cancellationToken);
        await SeedSuppliersAsync(context, cancellationToken);
        await SeedPartsAsync(context, cancellationToken);
        await SeedAuditActionTypesAsync(context, cancellationToken);
    }

    private static async Task SeedRolesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var roleName in new[] { "Admin", "Receptionist", "Mechanic", "Client" })
        {
            await AddIfMissingAsync(
                context.Roles,
                x => x.RoleName == roleName,
                new Role { RoleName = roleName },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDocumentTypesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var documentTypes = new[]
        {
            new DocumentType { Code = "CC", Name = "Cedula de Ciudadania" },
            new DocumentType { Code = "CE", Name = "Cedula de Extranjeria" },
            new DocumentType { Code = "NIT", Name = "NIT" },
            new DocumentType { Code = "PASSPORT", Name = "Passport" },
            new DocumentType { Code = "TI", Name = "Tarjeta de Identidad" }
        };

        foreach (var documentType in documentTypes)
        {
            await AddIfMissingAsync(
                context.DocumentTypes,
                x => x.Code == documentType.Code,
                documentType,
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDepartmentsAndCitiesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var colombia = await context.Countries.FirstOrDefaultAsync(x => x.Name == "Colombia", cancellationToken);
        if (colombia is null)
        {
            colombia = new Country { Name = "Colombia", PhoneCode = "+57" };
            await context.Countries.AddAsync(colombia, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        foreach (var departmentName in new[]
        {
            "Santander",
            "Norte de Santander",
            "Cundinamarca",
            "Antioquia",
            "Valle del Cauca"
        })
        {
            await AddIfMissingAsync(
                context.Departments,
                x => x.CountryId == colombia.CountryId && x.Name == departmentName,
                new Department { CountryId = colombia.CountryId, Name = departmentName },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        var departments = await context.Departments
            .Where(x => x.CountryId == colombia.CountryId)
            .ToDictionaryAsync(x => x.Name, x => x.DepartmentId, cancellationToken);

        var cities = new[]
        {
            new CitySeed("Bucaramanga", "Santander"),
            new CitySeed("Floridablanca", "Santander"),
            new CitySeed("Giron", "Santander"),
            new CitySeed("Piedecuesta", "Santander"),
            new CitySeed("Cucuta", "Norte de Santander"),
            new CitySeed("Bogota", "Cundinamarca"),
            new CitySeed("Medellin", "Antioquia"),
            new CitySeed("Cali", "Valle del Cauca")
        };

        foreach (var city in cities)
        {
            if (!departments.TryGetValue(city.DepartmentName, out var departmentId))
            {
                continue;
            }

            await AddIfMissingAsync(
                context.Cities,
                x => x.DepartmentId == departmentId && x.Name == city.Name,
                new City { DepartmentId = departmentId, Name = city.Name },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedVehicleTypesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var name in new[] { "Sedan", "Hatchback", "SUV", "Pickup", "Van", "Truck", "Coupe", "Motorcycle" })
        {
            await AddIfMissingAsync(
                context.VehicleTypes,
                x => x.Name == name,
                new VehicleType { Name = name },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedVehicleBrandsAndModelsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var brandName in new[]
        {
            "Toyota",
            "Chevrolet",
            "Renault",
            "Mazda",
            "Kia",
            "Hyundai",
            "Nissan",
            "Ford",
            "Volkswagen",
            "Honda",
            "Suzuki",
            "Mercedes-Benz",
            "BMW"
        })
        {
            await AddIfMissingAsync(
                context.VehicleBrands,
                x => x.BrandName == brandName,
                new VehicleBrand { BrandName = brandName },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        var brands = await context.VehicleBrands
            .ToDictionaryAsync(x => x.BrandName, x => x.BrandId, cancellationToken);

        var models = new[]
        {
            new VehicleModelSeed("Toyota", "Corolla"),
            new VehicleModelSeed("Toyota", "Hilux"),
            new VehicleModelSeed("Toyota", "Fortuner"),
            new VehicleModelSeed("Toyota", "Yaris"),
            new VehicleModelSeed("Chevrolet", "Onix"),
            new VehicleModelSeed("Chevrolet", "Tracker"),
            new VehicleModelSeed("Chevrolet", "Spark"),
            new VehicleModelSeed("Chevrolet", "Sail"),
            new VehicleModelSeed("Renault", "Logan"),
            new VehicleModelSeed("Renault", "Sandero"),
            new VehicleModelSeed("Renault", "Duster"),
            new VehicleModelSeed("Renault", "Kwid"),
            new VehicleModelSeed("Mazda", "Mazda 2"),
            new VehicleModelSeed("Mazda", "Mazda 3"),
            new VehicleModelSeed("Mazda", "CX-5"),
            new VehicleModelSeed("Mazda", "CX-30"),
            new VehicleModelSeed("Kia", "Picanto"),
            new VehicleModelSeed("Kia", "Rio"),
            new VehicleModelSeed("Kia", "Sportage"),
            new VehicleModelSeed("Kia", "K3"),
            new VehicleModelSeed("Hyundai", "i10"),
            new VehicleModelSeed("Hyundai", "Tucson"),
            new VehicleModelSeed("Hyundai", "Accent"),
            new VehicleModelSeed("Hyundai", "Creta"),
            new VehicleModelSeed("Nissan", "March"),
            new VehicleModelSeed("Nissan", "Versa"),
            new VehicleModelSeed("Nissan", "Frontier"),
            new VehicleModelSeed("Nissan", "X-Trail"),
            new VehicleModelSeed("Ford", "Fiesta"),
            new VehicleModelSeed("Ford", "Ranger"),
            new VehicleModelSeed("Ford", "Escape"),
            new VehicleModelSeed("Volkswagen", "Gol"),
            new VehicleModelSeed("Volkswagen", "Jetta"),
            new VehicleModelSeed("Volkswagen", "Amarok"),
            new VehicleModelSeed("Honda", "Civic"),
            new VehicleModelSeed("Honda", "CR-V"),
            new VehicleModelSeed("Honda", "Fit")
        };

        foreach (var model in models)
        {
            if (!brands.TryGetValue(model.BrandName, out var brandId))
            {
                continue;
            }

            await AddIfMissingAsync(
                context.VehicleModels,
                x => x.BrandId == brandId && x.ModelName == model.ModelName,
                new VehicleModel { BrandId = brandId, ModelName = model.ModelName },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedOrderStatusesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var name in new[] { "Pending", "InProgress", "Completed", "Cancelled", "Voided" })
        {
            await AddIfMissingAsync(
                context.OrderStatuses,
                x => x.Name == name,
                new OrderStatus { Name = name },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedInvoiceStatusesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var name in new[] { "Draft", "Issued", "Paid", "Cancelled" })
        {
            await AddIfMissingAsync(
                context.InvoiceStatuses,
                x => x.Name == name,
                new InvoiceStatus { Name = name },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPaymentStatusesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var name in new[] { "Pending", "Completed", "Refunded", "Failed", "Cancelled" })
        {
            await AddIfMissingAsync(
                context.PaymentStatuses,
                x => x.Name == name,
                new PaymentStatus { Name = name },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPaymentMethodsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var name in new[] { "Cash", "BankTransfer", "CreditCard", "DebitCard", "Nequi", "Daviplata", "PSE" })
        {
            await AddIfMissingAsync(
                context.PaymentMethods,
                x => x.Name == name,
                new PaymentMethod { Name = name },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCardTypesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var name in new[] { "Credit", "Debit" })
        {
            await AddIfMissingAsync(
                context.CardTypes,
                x => x.Name == name,
                new CardType { Name = name },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedServiceTypesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var serviceTypes = new[]
        {
            new ServiceType { Name = "Preventive Maintenance", EstimatedDays = 1 },
            new ServiceType { Name = "Oil Change", EstimatedDays = 1 },
            new ServiceType { Name = "Brake Inspection", EstimatedDays = 1 },
            new ServiceType { Name = "Brake Replacement", EstimatedDays = 1 },
            new ServiceType { Name = "Engine Diagnostics", EstimatedDays = 1 },
            new ServiceType { Name = "Electrical Diagnostics", EstimatedDays = 1 },
            new ServiceType { Name = "Battery Replacement", EstimatedDays = 1 },
            new ServiceType { Name = "Alignment and Balancing", EstimatedDays = 1 },
            new ServiceType { Name = "Suspension Repair", EstimatedDays = 2 },
            new ServiceType { Name = "Transmission Check", EstimatedDays = 2 },
            new ServiceType { Name = "Air Conditioning Service", EstimatedDays = 2 },
            new ServiceType { Name = "Tire Rotation", EstimatedDays = 1 },
            new ServiceType { Name = "General Inspection", EstimatedDays = 1 },
            new ServiceType { Name = "Scanner Diagnostics", EstimatedDays = 1 },
            new ServiceType { Name = "Cooling System Service", EstimatedDays = 2 },
            new ServiceType { Name = "Clutch Repair", EstimatedDays = 3 }
        };

        foreach (var serviceType in serviceTypes)
        {
            await AddIfMissingAsync(
                context.ServiceTypes,
                x => x.Name == serviceType.Name,
                serviceType,
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedMechanicSpecialtiesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var name in new[]
        {
            "General Mechanics",
            "Preventive Maintenance",
            "Brakes",
            "Electrical",
            "Suspension",
            "Engine",
            "Transmission",
            "Air Conditioning",
            "Diagnostics",
            "Tires and Alignment"
        })
        {
            await AddIfMissingAsync(
                context.MechanicSpecialties,
                x => x.Name == name,
                new MechanicSpecialty { Name = name },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPartCategoriesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var name in new[]
        {
            "Filters",
            "Oils and Fluids",
            "Brakes",
            "Electrical",
            "Suspension",
            "Engine",
            "Transmission",
            "Cooling",
            "Tires",
            "Batteries",
            "Belts",
            "Lights",
            "Sensors"
        })
        {
            await AddIfMissingAsync(
                context.PartCategories,
                x => x.Name == name,
                new PartCategory { Name = name },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPartBrandsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var name in new[]
        {
            "Bosch",
            "NGK",
            "Denso",
            "Gates",
            "Monroe",
            "Brembo",
            "ACDelco",
            "Mobil",
            "Castrol",
            "Motul",
            "Yokohama",
            "Michelin",
            "Hankook",
            "Willard",
            "Exide",
            "Valeo"
        })
        {
            await AddIfMissingAsync(
                context.PartBrands,
                x => x.Name == name,
                new PartBrand { Name = name },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedSuppliersAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var suppliers = new[]
        {
            new Supplier { Name = "Repuestos Santander S.A.S.", TaxId = "900100001-1", Phone = "+5776071001", Email = "ventas@repuestossantander.com", IsActive = true },
            new Supplier { Name = "AutoPartes Bucaramanga", TaxId = "900100002-2", Phone = "+5776071002", Email = "contacto@autopartesbga.com", IsActive = true },
            new Supplier { Name = "Distribuidora Nacional de Repuestos", TaxId = "900100003-3", Phone = "+5716011003", Email = "comercial@dnrepuestos.com", IsActive = true },
            new Supplier { Name = "Lubricantes del Oriente", TaxId = "900100004-4", Phone = "+5776071004", Email = "ventas@lubrioriente.com", IsActive = true },
            new Supplier { Name = "Frenos y Suspension Express", TaxId = "900100005-5", Phone = "+5776071005", Email = "servicio@frenosexpress.com", IsActive = true },
            new Supplier { Name = "Baterias Colombia", TaxId = "900100006-6", Phone = "+5716011006", Email = "ventas@bateriascolombia.com", IsActive = true },
            new Supplier { Name = "TecnoPartes Automotriz", TaxId = "900100007-7", Phone = "+5746041007", Email = "info@tecnopartesauto.com", IsActive = true },
            new Supplier { Name = "Importadora MotorParts", TaxId = "900100008-8", Phone = "+5716011008", Email = "importaciones@motorparts.com", IsActive = true }
        };

        foreach (var supplier in suppliers)
        {
            await AddIfMissingAsync(
                context.Suppliers,
                x => x.TaxId == supplier.TaxId,
                supplier,
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPartsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var categories = await context.PartCategories
            .ToDictionaryAsync(x => x.Name, x => x.PartCategoryId, cancellationToken);

        var brands = await context.PartBrands
            .ToDictionaryAsync(x => x.Name, x => x.PartBrandId, cancellationToken);

        var parts = new[]
        {
            new PartSeed("FLT-OIL-COR-001", "Oil Filter Toyota Corolla", "Filters", "Bosch", 18, 5, 42000m),
            new PartSeed("FLT-AIR-DUS-001", "Air Filter Renault Duster", "Filters", "Denso", 12, 5, 38000m),
            new PartSeed("FLT-FUEL-SAI-001", "Fuel Filter Chevrolet Sail", "Filters", "ACDelco", 4, 5, 45000m),
            new PartSeed("FLT-CAB-MAZ3-001", "Cabin Filter Mazda 3", "Filters", "Bosch", 9, 4, 36000m),
            new PartSeed("OIL-10W30-001", "Engine Oil 10W-30", "Oils and Fluids", "Mobil", 24, 6, 58000m),
            new PartSeed("OIL-5W30-001", "Engine Oil 5W-30", "Oils and Fluids", "Castrol", 20, 6, 64000m),
            new PartSeed("FLD-BRAKE-DOT4-001", "Brake Fluid DOT 4", "Oils and Fluids", "Motul", 8, 4, 32000m),
            new PartSeed("FLD-COOL-GRN-001", "Coolant Green 1L", "Cooling", "ACDelco", 3, 5, 28000m),
            new PartSeed("FLD-ATF-001", "Transmission Fluid ATF", "Transmission", "Mobil", 7, 3, 72000m),
            new PartSeed("BRK-PAD-FRT-SED-001", "Front Brake Pads Generic Sedan", "Brakes", "Brembo", 16, 5, 135000m),
            new PartSeed("BRK-PAD-REAR-SUV-001", "Rear Brake Pads SUV", "Brakes", "Brembo", 10, 4, 148000m),
            new PartSeed("BRK-DISC-COR-001", "Brake Disc Toyota Corolla", "Brakes", "Bosch", 6, 4, 210000m),
            new PartSeed("BRK-DRUM-SPK-001", "Brake Drum Chevrolet Spark", "Brakes", "ACDelco", 0, 2, 180000m),
            new PartSeed("ELC-SPARK-NGK-001", "Spark Plug NGK", "Electrical", "NGK", 40, 10, 26000m),
            new PartSeed("BAT-12V-45AH-001", "Battery 12V 45Ah", "Batteries", "Willard", 5, 3, 310000m),
            new PartSeed("BAT-12V-60AH-001", "Battery 12V 60Ah", "Batteries", "Exide", 2, 3, 390000m),
            new PartSeed("LGT-H4-001", "Headlight Bulb H4", "Lights", "Valeo", 14, 5, 24000m),
            new PartSeed("LGT-H7-001", "Headlight Bulb H7", "Lights", "Bosch", 13, 5, 26000m),
            new PartSeed("BELT-ALT-001", "Alternator Belt", "Belts", "Gates", 9, 4, 52000m),
            new PartSeed("SUS-SHOCK-FRT-001", "Shock Absorber Front", "Suspension", "Monroe", 6, 4, 230000m),
            new PartSeed("SUS-SHOCK-REAR-001", "Shock Absorber Rear", "Suspension", "Monroe", 5, 4, 210000m),
            new PartSeed("SUS-LINK-001", "Stabilizer Link", "Suspension", "Denso", 12, 5, 68000m),
            new PartSeed("SUS-BUSH-ARM-001", "Control Arm Bushing", "Suspension", "ACDelco", 15, 6, 42000m),
            new PartSeed("ENG-TIMING-BELT-001", "Timing Belt", "Engine", "Gates", 7, 3, 95000m),
            new PartSeed("ENG-SERP-BELT-001", "Serpentine Belt", "Engine", "Gates", 10, 4, 76000m),
            new PartSeed("ENG-WATER-PUMP-001", "Water Pump", "Cooling", "Denso", 4, 3, 185000m),
            new PartSeed("ENG-THERMO-001", "Thermostat", "Cooling", "Valeo", 3, 4, 67000m),
            new PartSeed("SEN-OXY-001", "Oxygen Sensor", "Sensors", "Bosch", 6, 3, 220000m),
            new PartSeed("TIRE-185-65R15-001", "Tire 185/65 R15", "Tires", "Hankook", 18, 6, 285000m),
            new PartSeed("TIRE-205-55R16-001", "Tire 205/55 R16", "Tires", "Michelin", 11, 6, 380000m),
            new PartSeed("TIRE-225-65R17-001", "Tire 225/65 R17", "Tires", "Yokohama", 4, 6, 520000m),
            new PartSeed("SEN-MAF-001", "Mass Air Flow Sensor", "Sensors", "Denso", 2, 3, 260000m),
            new PartSeed("ENG-IGN-COIL-001", "Ignition Coil", "Electrical", "Bosch", 8, 4, 155000m),
            new PartSeed("TRN-CLUTCH-KIT-001", "Clutch Kit Generic Sedan", "Transmission", "Valeo", 3, 2, 620000m),
            new PartSeed("COOL-RADIATOR-001", "Radiator Generic Sedan", "Cooling", "Denso", 2, 2, 480000m)
        };

        foreach (var part in parts)
        {
            if (!categories.TryGetValue(part.CategoryName, out var categoryId) ||
                !brands.TryGetValue(part.BrandName, out var brandId))
            {
                continue;
            }

            await AddIfMissingAsync(
                context.Parts,
                x => x.Code == part.Code,
                new Part
                {
                    Code = part.Code,
                    Description = part.Description,
                    PartCategoryId = categoryId,
                    PartBrandId = brandId,
                    Stock = part.Stock,
                    MinimumStock = part.MinimumStock,
                    UnitPrice = part.UnitPrice,
                    IsActive = true
                },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAuditActionTypesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var name in new[]
        {
            "CREATE",
            "UPDATE",
            "DELETE",
            "CANCEL",
            "VOID",
            "LOGIN",
            "CreateUser",
            "UpdateUser",
            "ChangeServiceOrderStatus",
            "CancelServiceOrder",
            "CompleteServiceOrder",
            "GenerateInvoice",
            "IssueInvoice",
            "CancelInvoice",
            "RecordPayment",
            "RefundPayment",
            "RegisterInventoryPurchase",
            "CancelInventoryPurchase",
            "AdjustStock",
            "AssignMechanic",
            "UnassignMechanic",
            "RequestPart",
            "ApprovePart",
            "RejectPart",
            "ClientApproveService",
            "ClientRejectService",
            "ClientApprovePart",
            "ClientRejectPart"
        })
        {
            await AddIfMissingAsync(
                context.AuditActionTypes,
                x => x.Name == name,
                new AuditActionType { Name = name },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task AddIfMissingAsync<TEntity>(
        DbSet<TEntity> dbSet,
        Expression<Func<TEntity, bool>> predicate,
        TEntity entity,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var exists = await dbSet.AnyAsync(predicate, cancellationToken);
        if (!exists)
        {
            await dbSet.AddAsync(entity, cancellationToken);
        }
    }

    private sealed record CitySeed(string Name, string DepartmentName);
    private sealed record VehicleModelSeed(string BrandName, string ModelName);
    private sealed record PartSeed(
        string Code,
        string Description,
        string CategoryName,
        string BrandName,
        int Stock,
        int MinimumStock,
        decimal UnitPrice);
}
