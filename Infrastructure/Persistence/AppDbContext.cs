using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();
    public DbSet<StreetType> StreetTypes => Set<StreetType>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<Gender> Genders => Set<Gender>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<EmailDomain> EmailDomains => Set<EmailDomain>();
    public DbSet<PersonEmail> PersonEmails => Set<PersonEmail>();
    public DbSet<PersonPhone> PersonPhones => Set<PersonPhone>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<PersonRole> PersonRoles => Set<PersonRole>();
    public DbSet<User> Users => Set<User>();
    public DbSet<MechanicSpecialty> MechanicSpecialties => Set<MechanicSpecialty>();
    public DbSet<MechanicSpecialtyAssignment> MechanicSpecialtyAssignments => Set<MechanicSpecialtyAssignment>();
    public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
    public DbSet<VehicleBrand> VehicleBrands => Set<VehicleBrand>();
    public DbSet<VehicleModel> VehicleModels => Set<VehicleModel>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleOwnerHistory> VehicleOwnerHistories => Set<VehicleOwnerHistory>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
    public DbSet<VehicleEntryInventory> VehicleEntryInventories => Set<VehicleEntryInventory>();
    public DbSet<OrderService> OrderServices => Set<OrderService>();
    public DbSet<MechanicAssignment> MechanicAssignments => Set<MechanicAssignment>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<PartCategory> PartCategories => Set<PartCategory>();
    public DbSet<PartBrand> PartBrands => Set<PartBrand>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<OrderServicePart> OrderServiceParts => Set<OrderServicePart>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PartPurchase> PartPurchases => Set<PartPurchase>();
    public DbSet<PartPurchaseDetail> PartPurchaseDetails => Set<PartPurchaseDetail>();
    public DbSet<InvoiceStatus> InvoiceStatuses => Set<InvoiceStatus>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceDetail> InvoiceDetails => Set<InvoiceDetail>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<PaymentStatus> PaymentStatuses => Set<PaymentStatus>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CardType> CardTypes => Set<CardType>();
    public DbSet<PaymentCard> PaymentCards => Set<PaymentCard>();
    public DbSet<AuditActionType> AuditActionTypes => Set<AuditActionType>();
    public DbSet<Audit> Audits => Set<Audit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
