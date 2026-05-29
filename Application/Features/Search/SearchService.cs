using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Search.Dtos;
using Application.Features.Search.Errors;
using Domain.Entities;

namespace Application.Features.Search;

public class SearchService : ISearchService
{
    private const int MaxResults = 20;
    private const string ClientRoleName = "Client";
    private const string MechanicRoleName = "Mechanic";

    private readonly IUnitOfWork _unitOfWork;

    public SearchService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<ClientSearchResultDto>>> SearchClientsAsync(
        string? term,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateSearchTerm(term);
        if (validation.Error is not null)
        {
            return Result<IReadOnlyList<ClientSearchResultDto>>.Failure(validation.Error);
        }

        var normalizedTerm = validation.NormalizedTerm!;

        var clientRoleId = await GetRoleIdByNameAsync(ClientRoleName, cancellationToken);
        if (!clientRoleId.HasValue)
        {
            return Result<IReadOnlyList<ClientSearchResultDto>>.Success(Array.Empty<ClientSearchResultDto>());
        }

        var clientPersonIds = (await _unitOfWork.Repository<PersonRole>().FindAsync(
            x => x.RoleId == clientRoleId.Value && x.IsActive,
            cancellationToken))
            .Select(x => x.PersonId)
            .Distinct()
            .ToList();

        if (clientPersonIds.Count == 0)
        {
            return Result<IReadOnlyList<ClientSearchResultDto>>.Success(Array.Empty<ClientSearchResultDto>());
        }

        var people = await _unitOfWork.Repository<Person>().FindAsync(
            x => clientPersonIds.Contains(x.PersonId),
            cancellationToken);

        var personEmails = await _unitOfWork.Repository<PersonEmail>().FindAsync(
            x => clientPersonIds.Contains(x.PersonId),
            cancellationToken);

        var emailDomainIds = personEmails
            .Select(x => x.EmailDomainId)
            .Distinct()
            .ToList();

        var emailDomains = emailDomainIds.Count == 0
            ? Array.Empty<EmailDomain>()
            : (await _unitOfWork.Repository<EmailDomain>().FindAsync(
                x => emailDomainIds.Contains(x.EmailDomainId),
                cancellationToken)).ToArray();

        var emailDomainById = emailDomains.ToDictionary(x => x.EmailDomainId, x => x.Domain);
        var emailsByPerson = personEmails
            .GroupBy(x => x.PersonId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(email =>
                    emailDomainById.TryGetValue(email.EmailDomainId, out var domain)
                        ? $"{email.EmailUser}@{domain}"
                        : email.EmailUser)
                    .ToList());

        var primaryEmailByPerson = personEmails
            .GroupBy(x => x.PersonId)
            .ToDictionary(
                x => x.Key,
                x =>
                {
                    var primary = x.OrderByDescending(y => y.IsPrimary).ThenBy(y => y.PersonEmailId).FirstOrDefault();
                    if (primary is null)
                    {
                        return (string?)null;
                    }

                    return emailDomainById.TryGetValue(primary.EmailDomainId, out var domain)
                        ? $"{primary.EmailUser}@{domain}"
                        : primary.EmailUser;
                });

        var personPhones = await _unitOfWork.Repository<PersonPhone>().FindAsync(
            x => clientPersonIds.Contains(x.PersonId),
            cancellationToken);

        var phonesByPerson = personPhones
            .GroupBy(x => x.PersonId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.PhoneNumber).ToList());

        var primaryPhoneByPerson = personPhones
            .GroupBy(x => x.PersonId)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(y => y.IsPrimary).ThenBy(y => y.PersonPhoneId).Select(y => y.PhoneNumber).FirstOrDefault());

        var result = people
            .Where(person =>
            {
                var fullName = BuildFullName(person);
                var searchableText = string.Join(" ",
                    person.DocumentNumber,
                    person.FirstName,
                    person.MiddleName,
                    person.LastName,
                    person.SecondLastName,
                    fullName);

                var matchesPerson = Contains(searchableText, normalizedTerm);
                var matchesEmail = emailsByPerson.TryGetValue(person.PersonId, out var emails)
                    && emails.Any(email => Contains(email, normalizedTerm));
                var matchesPhone = phonesByPerson.TryGetValue(person.PersonId, out var phones)
                    && phones.Any(phone => Contains(phone, normalizedTerm));

                return matchesPerson || matchesEmail || matchesPhone;
            })
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.PersonId)
            .Take(MaxResults)
            .Select(person => new ClientSearchResultDto
            {
                PersonId = person.PersonId,
                DocumentNumber = person.DocumentNumber,
                FullName = BuildFullName(person),
                PrimaryEmail = primaryEmailByPerson.TryGetValue(person.PersonId, out var email) ? email : null,
                PrimaryPhoneNumber = primaryPhoneByPerson.TryGetValue(person.PersonId, out var phone) ? phone : null
            })
            .ToList();

        return Result<IReadOnlyList<ClientSearchResultDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<VehicleSearchResultDto>>> SearchVehiclesAsync(
        string? term,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateSearchTerm(term);
        if (validation.Error is not null)
        {
            return Result<IReadOnlyList<VehicleSearchResultDto>>.Failure(validation.Error);
        }

        var normalizedTerm = validation.NormalizedTerm!;
        var vehicles = await _unitOfWork.Repository<Vehicle>().GetAllAsync(cancellationToken);

        var result = vehicles
            .Where(x =>
                Contains(x.VIN, normalizedTerm) ||
                Contains(x.Year.ToString(), normalizedTerm) ||
                Contains(x.Color, normalizedTerm) ||
                Contains(x.ModelId.ToString(), normalizedTerm) ||
                Contains(x.VehicleId.ToString(), normalizedTerm))
            .OrderByDescending(x => x.VehicleId)
            .Take(MaxResults)
            .Select(x => new VehicleSearchResultDto
            {
                VehicleId = x.VehicleId,
                VIN = x.VIN,
                ModelId = x.ModelId,
                VehicleTypeId = x.VehicleTypeId,
                Year = x.Year,
                Color = x.Color,
                IsActive = x.IsActive
            })
            .ToList();

        return Result<IReadOnlyList<VehicleSearchResultDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<ServiceOrderSearchResultDto>>> SearchServiceOrdersAsync(
        string? term,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateSearchTerm(term);
        if (validation.Error is not null)
        {
            return Result<IReadOnlyList<ServiceOrderSearchResultDto>>.Failure(validation.Error);
        }

        var normalizedTerm = validation.NormalizedTerm!;
        var serviceOrders = await _unitOfWork.Repository<ServiceOrder>().GetAllAsync(cancellationToken);

        var result = serviceOrders
            .Where(x =>
                Contains(x.ServiceOrderId.ToString(), normalizedTerm) ||
                Contains(x.VehicleId.ToString(), normalizedTerm) ||
                Contains(x.GeneralDescription, normalizedTerm))
            .OrderByDescending(x => x.EntryDate)
            .ThenByDescending(x => x.ServiceOrderId)
            .Take(MaxResults)
            .Select(x => new ServiceOrderSearchResultDto
            {
                ServiceOrderId = x.ServiceOrderId,
                VehicleId = x.VehicleId,
                OrderStatusId = x.OrderStatusId,
                EntryDate = x.EntryDate,
                GeneralDescription = x.GeneralDescription
            })
            .ToList();

        return Result<IReadOnlyList<ServiceOrderSearchResultDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<InvoiceSearchResultDto>>> SearchInvoicesAsync(
        string? term,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateSearchTerm(term);
        if (validation.Error is not null)
        {
            return Result<IReadOnlyList<InvoiceSearchResultDto>>.Failure(validation.Error);
        }

        var normalizedTerm = validation.NormalizedTerm!;
        var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(cancellationToken);

        var result = invoices
            .Where(x =>
                Contains(x.InvoiceNumber, normalizedTerm) ||
                Contains(x.InvoiceId.ToString(), normalizedTerm) ||
                Contains(x.ServiceOrderId.ToString(), normalizedTerm))
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.InvoiceId)
            .Take(MaxResults)
            .Select(x => new InvoiceSearchResultDto
            {
                InvoiceId = x.InvoiceId,
                InvoiceNumber = x.InvoiceNumber,
                ServiceOrderId = x.ServiceOrderId,
                InvoiceStatusId = x.InvoiceStatusId,
                Total = x.Total,
                InvoiceDate = x.InvoiceDate
            })
            .ToList();

        return Result<IReadOnlyList<InvoiceSearchResultDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<PartSearchResultDto>>> SearchPartsAsync(
        string? term,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateSearchTerm(term);
        if (validation.Error is not null)
        {
            return Result<IReadOnlyList<PartSearchResultDto>>.Failure(validation.Error);
        }

        var normalizedTerm = validation.NormalizedTerm!;
        var parts = await _unitOfWork.Repository<Part>().GetAllAsync(cancellationToken);

        var result = parts
            .Where(x => Contains(x.Code, normalizedTerm) || Contains(x.Description, normalizedTerm))
            .OrderBy(x => x.Code)
            .ThenBy(x => x.PartId)
            .Take(MaxResults)
            .Select(x => new PartSearchResultDto
            {
                PartId = x.PartId,
                Code = x.Code,
                Description = x.Description,
                Stock = x.Stock,
                MinimumStock = x.MinimumStock,
                UnitPrice = x.UnitPrice,
                IsActive = x.IsActive
            })
            .ToList();

        return Result<IReadOnlyList<PartSearchResultDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<SupplierSearchResultDto>>> SearchSuppliersAsync(
        string? term,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateSearchTerm(term);
        if (validation.Error is not null)
        {
            return Result<IReadOnlyList<SupplierSearchResultDto>>.Failure(validation.Error);
        }

        var normalizedTerm = validation.NormalizedTerm!;
        var suppliers = await _unitOfWork.Repository<Supplier>().GetAllAsync(cancellationToken);

        var result = suppliers
            .Where(x =>
                Contains(x.Name, normalizedTerm) ||
                Contains(x.TaxId, normalizedTerm) ||
                Contains(x.Phone, normalizedTerm) ||
                Contains(x.Email, normalizedTerm))
            .OrderBy(x => x.Name)
            .ThenBy(x => x.SupplierId)
            .Take(MaxResults)
            .Select(x => new SupplierSearchResultDto
            {
                SupplierId = x.SupplierId,
                Name = x.Name,
                TaxId = x.TaxId,
                Phone = x.Phone,
                Email = x.Email,
                IsActive = x.IsActive
            })
            .ToList();

        return Result<IReadOnlyList<SupplierSearchResultDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<MechanicSearchResultDto>>> SearchMechanicsAsync(
        string? term,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateSearchTerm(term);
        if (validation.Error is not null)
        {
            return Result<IReadOnlyList<MechanicSearchResultDto>>.Failure(validation.Error);
        }

        var normalizedTerm = validation.NormalizedTerm!;

        var mechanicRoleId = await GetRoleIdByNameAsync(MechanicRoleName, cancellationToken);
        if (!mechanicRoleId.HasValue)
        {
            return Result<IReadOnlyList<MechanicSearchResultDto>>.Success(Array.Empty<MechanicSearchResultDto>());
        }

        var mechanicPersonIds = (await _unitOfWork.Repository<PersonRole>().FindAsync(
            x => x.RoleId == mechanicRoleId.Value && x.IsActive,
            cancellationToken))
            .Select(x => x.PersonId)
            .Distinct()
            .ToList();

        if (mechanicPersonIds.Count == 0)
        {
            return Result<IReadOnlyList<MechanicSearchResultDto>>.Success(Array.Empty<MechanicSearchResultDto>());
        }

        var mechanics = await _unitOfWork.Repository<Person>().FindAsync(
            x => mechanicPersonIds.Contains(x.PersonId),
            cancellationToken);

        var specialtyAssignments = await _unitOfWork.Repository<MechanicSpecialtyAssignment>().FindAsync(
            x => mechanicPersonIds.Contains(x.PersonId),
            cancellationToken);

        var specialtyIdsByPerson = specialtyAssignments
            .GroupBy(x => x.PersonId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<int>)x
                    .Select(y => y.SpecialtyId)
                    .Distinct()
                    .OrderBy(y => y)
                    .ToList());

        var result = mechanics
            .Where(person =>
                Contains(person.DocumentNumber, normalizedTerm) ||
                Contains(person.FirstName, normalizedTerm) ||
                Contains(person.MiddleName, normalizedTerm) ||
                Contains(person.LastName, normalizedTerm) ||
                Contains(person.SecondLastName, normalizedTerm) ||
                Contains(BuildFullName(person), normalizedTerm))
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.PersonId)
            .Take(MaxResults)
            .Select(person => new MechanicSearchResultDto
            {
                PersonId = person.PersonId,
                DocumentNumber = person.DocumentNumber,
                FullName = BuildFullName(person),
                SpecialtyIds = specialtyIdsByPerson.TryGetValue(person.PersonId, out var specialtyIds)
                    ? specialtyIds
                    : Array.Empty<int>()
            })
            .ToList();

        return Result<IReadOnlyList<MechanicSearchResultDto>>.Success(result);
    }

    private async Task<int?> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Repository<Role>().GetAllAsync(cancellationToken);
        return roles
            .Where(x => x.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
            .Select(x => (int?)x.RoleId)
            .FirstOrDefault();
    }

    private static (string? NormalizedTerm, Error? Error) ValidateSearchTerm(string? term)
    {
        var normalized = (term ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return (null, SearchErrors.SearchTermRequired);
        }

        if (normalized.Length < 2)
        {
            return (null, SearchErrors.SearchTermTooShort);
        }

        return (normalized, null);
    }

    private static bool Contains(string? text, string term)
    {
        return !string.IsNullOrWhiteSpace(text) &&
               text.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildFullName(Person person)
    {
        return string.Join(" ", new[] { person.FirstName, person.MiddleName, person.LastName, person.SecondLastName }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
