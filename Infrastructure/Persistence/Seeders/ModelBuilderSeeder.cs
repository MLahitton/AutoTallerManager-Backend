using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders;

public static class ModelBuilderSeeder
{
    public static void SeedData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "Client" },
            new Role { RoleId = 3, RoleName = "Mechanic" },
            new Role { RoleId = 4, RoleName = "Receptionist" }
        );

        modelBuilder.Entity<OrderStatus>().HasData(
            new OrderStatus { OrderStatusId = 1, Name = "Pending" },
            new OrderStatus { OrderStatusId = 2, Name = "InProgress" },
            new OrderStatus { OrderStatusId = 3, Name = "Completed" },
            new OrderStatus { OrderStatusId = 4, Name = "Cancelled" },
            new OrderStatus { OrderStatusId = 5, Name = "Voided" }
        );

        modelBuilder.Entity<MechanicSpecialty>().HasData(
            new MechanicSpecialty { SpecialtyId = 1, Name = "Engine" },
            new MechanicSpecialty { SpecialtyId = 2, Name = "Electrical" },
            new MechanicSpecialty { SpecialtyId = 3, Name = "AirConditioning" },
            new MechanicSpecialty { SpecialtyId = 4, Name = "Suspension" },
            new MechanicSpecialty { SpecialtyId = 5, Name = "Brakes" },
            new MechanicSpecialty { SpecialtyId = 6, Name = "GeneralDiagnostics" },
            new MechanicSpecialty { SpecialtyId = 7, Name = "Bodywork" }
        );

        modelBuilder.Entity<PaymentMethod>().HasData(
            new PaymentMethod { PaymentMethodId = 1, Name = "Cash" },
            new PaymentMethod { PaymentMethodId = 2, Name = "Card" },
            new PaymentMethod { PaymentMethodId = 3, Name = "BankTransfer" }
        );

        modelBuilder.Entity<PaymentStatus>().HasData(
            new PaymentStatus { PaymentStatusId = 1, Name = "Pending" },
            new PaymentStatus { PaymentStatusId = 2, Name = "Completed" },
            new PaymentStatus { PaymentStatusId = 3, Name = "Refunded" },
            new PaymentStatus { PaymentStatusId = 4, Name = "Failed" }
        );

        modelBuilder.Entity<InvoiceStatus>().HasData(
            new InvoiceStatus { InvoiceStatusId = 1, Name = "Draft" },
            new InvoiceStatus { InvoiceStatusId = 2, Name = "Issued" },
            new InvoiceStatus { InvoiceStatusId = 3, Name = "Paid" },
            new InvoiceStatus { InvoiceStatusId = 4, Name = "Cancelled" }
        );

        modelBuilder.Entity<ServiceType>().HasData(
            new ServiceType { ServiceTypeId = 1, Name = "Preventive Maintenance", EstimatedDays = 1 },
            new ServiceType { ServiceTypeId = 2, Name = "Mechanical Repair", EstimatedDays = 3 },
            new ServiceType { ServiceTypeId = 3, Name = "Diagnostics", EstimatedDays = 1 },
            new ServiceType { ServiceTypeId = 4, Name = "Air Conditioning", EstimatedDays = 2 },
            new ServiceType { ServiceTypeId = 5, Name = "Electrical", EstimatedDays = 2 },
            new ServiceType { ServiceTypeId = 6, Name = "Bodywork and Paint", EstimatedDays = 5 }
        );

        modelBuilder.Entity<PartCategory>().HasData(
            new PartCategory { PartCategoryId = 1, Name = "Filters" },
            new PartCategory { PartCategoryId = 2, Name = "Oils and Lubricants" },
            new PartCategory { PartCategoryId = 3, Name = "Brakes" },
            new PartCategory { PartCategoryId = 4, Name = "Suspension" },
            new PartCategory { PartCategoryId = 5, Name = "Electrical" },
            new PartCategory { PartCategoryId = 6, Name = "Air Conditioning" },
            new PartCategory { PartCategoryId = 7, Name = "Engine" },
            new PartCategory { PartCategoryId = 8, Name = "Bodywork" }
        );

        modelBuilder.Entity<AuditActionType>().HasData(
            new AuditActionType { AuditActionTypeId = 1, Name = "CREATE" },
            new AuditActionType { AuditActionTypeId = 2, Name = "UPDATE" },
            new AuditActionType { AuditActionTypeId = 3, Name = "DELETE" },
            new AuditActionType { AuditActionTypeId = 4, Name = "CANCEL" },
            new AuditActionType { AuditActionTypeId = 5, Name = "VOID" },
            new AuditActionType { AuditActionTypeId = 6, Name = "LOGIN" }
        );

        modelBuilder.Entity<VehicleType>().HasData(
            new VehicleType { VehicleTypeId = 1, Name = "Sedan" },
            new VehicleType { VehicleTypeId = 2, Name = "SUV" },
            new VehicleType { VehicleTypeId = 3, Name = "Pickup" },
            new VehicleType { VehicleTypeId = 4, Name = "Van" },
            new VehicleType { VehicleTypeId = 5, Name = "Motorcycle" },
            new VehicleType { VehicleTypeId = 6, Name = "Truck" }
        );

        modelBuilder.Entity<DocumentType>().HasData(
            new DocumentType { DocumentTypeId = 1, Code = "CC", Name = "Cedula de Ciudadania" },
            new DocumentType { DocumentTypeId = 2, Code = "NIT", Name = "NIT" },
            new DocumentType { DocumentTypeId = 3, Code = "CE", Name = "Cedula de Extranjeria" },
            new DocumentType { DocumentTypeId = 4, Code = "PAS", Name = "Pasaporte" }
        );

        modelBuilder.Entity<Gender>().HasData(
            new Gender { GenderId = 1, Name = "Male" },
            new Gender { GenderId = 2, Name = "Female" },
            new Gender { GenderId = 3, Name = "Other" },
            new Gender { GenderId = 4, Name = "PreferNotToSay" }
        );

        modelBuilder.Entity<CardType>().HasData(
            new CardType { CardTypeId = 1, Name = "Visa" },
            new CardType { CardTypeId = 2, Name = "Mastercard" },
            new CardType { CardTypeId = 3, Name = "AmericanExpress" },
            new CardType { CardTypeId = 4, Name = "Debit" }
        );

        modelBuilder.Entity<StreetType>().HasData(
            new StreetType { StreetTypeId = 1, Name = "Calle" },
            new StreetType { StreetTypeId = 2, Name = "Carrera" },
            new StreetType { StreetTypeId = 3, Name = "Avenida" },
            new StreetType { StreetTypeId = 4, Name = "Diagonal" },
            new StreetType { StreetTypeId = 5, Name = "Transversal" },
            new StreetType { StreetTypeId = 6, Name = "Circular" }
        );

        modelBuilder.Entity<Country>().HasData(
            new Country { CountryId = 1, Name = "Colombia", PhoneCode = "+57" },
            new Country { CountryId = 2, Name = "Venezuela", PhoneCode = "+58" },
            new Country { CountryId = 3, Name = "Ecuador", PhoneCode = "+593" }
        );

        modelBuilder.Entity<Department>().HasData(
            new Department { DepartmentId = 1, CountryId = 1, Name = "Santander" },
            new Department { DepartmentId = 2, CountryId = 1, Name = "Cundinamarca" },
            new Department { DepartmentId = 3, CountryId = 1, Name = "Antioquia" }
        );

        modelBuilder.Entity<City>().HasData(
            new City { CityId = 1, DepartmentId = 1, Name = "Bucaramanga" },
            new City { CityId = 2, DepartmentId = 1, Name = "Floridablanca" },
            new City { CityId = 3, DepartmentId = 1, Name = "Giron" },
            new City { CityId = 4, DepartmentId = 2, Name = "Bogota" },
            new City { CityId = 5, DepartmentId = 3, Name = "Medellin" }
        );
    }
}
