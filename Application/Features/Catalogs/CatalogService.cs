using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Catalogs.Dtos;
using Domain.Entities;

namespace Application.Features.Catalogs;

public class CatalogService : ICatalogService
{
    private readonly IUnitOfWork _unitOfWork;

    public CatalogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PublicRegistrationCatalogsDto>> GetPublicRegistrationCatalogsAsync(CancellationToken cancellationToken = default)
    {
        var documentTypes = await _unitOfWork.Repository<DocumentType>().GetAllAsync(cancellationToken);
        var genders = await _unitOfWork.Repository<Gender>().GetAllAsync(cancellationToken);
        var countries = await _unitOfWork.Repository<Country>().GetAllAsync(cancellationToken);
        var departments = await _unitOfWork.Repository<Department>().GetAllAsync(cancellationToken);
        var cities = await _unitOfWork.Repository<City>().GetAllAsync(cancellationToken);
        var streetTypes = await _unitOfWork.Repository<StreetType>().GetAllAsync(cancellationToken);
        var neighborhoods = await _unitOfWork.Repository<Neighborhood>().GetAllAsync(cancellationToken);

        var dto = new PublicRegistrationCatalogsDto
        {
            DocumentTypes = documentTypes
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.DocumentTypeId, Name = x.Name })
                .ToList(),
            Genders = genders
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.GenderId, Name = x.Name })
                .ToList(),
            Countries = countries
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.CountryId, Name = x.Name })
                .ToList(),
            Departments = departments
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.DepartmentId, Name = x.Name })
                .ToList(),
            Cities = cities
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.CityId, Name = x.Name })
                .ToList(),
            StreetTypes = streetTypes
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.StreetTypeId, Name = x.Name })
                .ToList(),
            Neighborhoods = neighborhoods
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.NeighborhoodId, Name = x.Name })
                .ToList()
        };

        return Result<PublicRegistrationCatalogsDto>.Success(dto);
    }

    public async Task<Result<WorkshopCatalogsDto>> GetWorkshopCatalogsAsync(CancellationToken cancellationToken = default)
    {
        var vehicleTypes = await _unitOfWork.Repository<VehicleType>().GetAllAsync(cancellationToken);
        var vehicleBrands = await _unitOfWork.Repository<VehicleBrand>().GetAllAsync(cancellationToken);
        var vehicleModels = await _unitOfWork.Repository<VehicleModel>().GetAllAsync(cancellationToken);
        var serviceTypes = await _unitOfWork.Repository<ServiceType>().GetAllAsync(cancellationToken);
        var orderStatuses = await _unitOfWork.Repository<OrderStatus>().GetAllAsync(cancellationToken);
        var invoiceStatuses = await _unitOfWork.Repository<InvoiceStatus>().GetAllAsync(cancellationToken);
        var paymentMethods = await _unitOfWork.Repository<PaymentMethod>().GetAllAsync(cancellationToken);
        var paymentStatuses = await _unitOfWork.Repository<PaymentStatus>().GetAllAsync(cancellationToken);
        var cardTypes = await _unitOfWork.Repository<CardType>().GetAllAsync(cancellationToken);
        var mechanicSpecialties = await _unitOfWork.Repository<MechanicSpecialty>().GetAllAsync(cancellationToken);
        var partCategories = await _unitOfWork.Repository<PartCategory>().GetAllAsync(cancellationToken);
        var partBrands = await _unitOfWork.Repository<PartBrand>().GetAllAsync(cancellationToken);
        var auditActionTypes = await _unitOfWork.Repository<AuditActionType>().GetAllAsync(cancellationToken);

        var dto = new WorkshopCatalogsDto
        {
            VehicleTypes = vehicleTypes
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.VehicleTypeId, Name = x.Name })
                .ToList(),
            VehicleBrands = vehicleBrands
                .OrderBy(x => x.BrandName)
                .Select(x => new CatalogItemDto { Id = x.BrandId, Name = x.BrandName })
                .ToList(),
            VehicleModels = vehicleModels
                .OrderBy(x => x.ModelName)
                .Select(x => new CatalogItemDto { Id = x.ModelId, Name = x.ModelName })
                .ToList(),
            ServiceTypes = serviceTypes
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.ServiceTypeId, Name = x.Name })
                .ToList(),
            OrderStatuses = orderStatuses
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.OrderStatusId, Name = x.Name })
                .ToList(),
            InvoiceStatuses = invoiceStatuses
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.InvoiceStatusId, Name = x.Name })
                .ToList(),
            PaymentMethods = paymentMethods
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.PaymentMethodId, Name = x.Name })
                .ToList(),
            PaymentStatuses = paymentStatuses
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.PaymentStatusId, Name = x.Name })
                .ToList(),
            CardTypes = cardTypes
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.CardTypeId, Name = x.Name })
                .ToList(),
            MechanicSpecialties = mechanicSpecialties
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.SpecialtyId, Name = x.Name })
                .ToList(),
            PartCategories = partCategories
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.PartCategoryId, Name = x.Name })
                .ToList(),
            PartBrands = partBrands
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.PartBrandId, Name = x.Name })
                .ToList(),
            AuditActionTypes = auditActionTypes
                .OrderBy(x => x.Name)
                .Select(x => new CatalogItemDto { Id = x.AuditActionTypeId, Name = x.Name })
                .ToList()
        };

        return Result<WorkshopCatalogsDto>.Success(dto);
    }
}
