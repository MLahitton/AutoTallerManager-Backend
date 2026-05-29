using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.ClientVehicleFlows.Dtos;
using Application.Features.ClientVehicleFlows.Errors;
using Application.Features.ClientVehicleFlows.Requests;
using Domain.Entities;

namespace Application.Features.ClientVehicleFlows;

public class ClientVehicleFlowService : IClientVehicleFlowService
{
    private const int DocumentNumberMaxLength = 30;
    private const int FirstNameMaxLength = 50;
    private const int MiddleNameMaxLength = 50;
    private const int LastNameMaxLength = 50;
    private const int SecondLastNameMaxLength = 50;
    private const int EmailUserMaxLength = 100;
    private const int PhoneNumberMaxLength = 20;
    private const int VinLength = 17;
    private const int ColorMaxLength = 30;

    private const string ClientRoleName = "Client";

    private readonly IUnitOfWork _unitOfWork;

    public ClientVehicleFlowService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClientWithVehicleDto>> CreateClientWithVehicleAsync(
        CreateClientWithVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var documentTypeId = request?.DocumentTypeId ?? 0;
        var documentNumber = NormalizeRequiredText(request?.DocumentNumber);
        var firstName = NormalizeRequiredText(request?.FirstName);
        var middleName = NormalizeOptionalText(request?.MiddleName);
        var lastName = NormalizeRequiredText(request?.LastName);
        var secondLastName = NormalizeOptionalText(request?.SecondLastName);
        var birthDate = request?.BirthDate;
        var genderId = request?.GenderId;
        var addressId = request?.AddressId;

        var normalizedEmail = NormalizeEmail(request?.Email);
        var emailProvided = request?.Email is not null;

        var phoneNumberProvided = request?.PhoneNumber is not null;
        var phoneCountryId = request?.PhoneCountryId;
        var phoneNumber = NormalizeOptionalText(request?.PhoneNumber);

        var modelId = request?.ModelId ?? 0;
        var vehicleTypeId = request?.VehicleTypeId ?? 0;
        var vin = NormalizeVin(request?.VIN);
        var year = request?.Year ?? 0;
        var color = NormalizeOptionalText(request?.Color);
        var mileage = request?.Mileage ?? 0;

        var validationError = ValidateCreateClientWithVehicleInput(
            documentTypeId,
            documentNumber,
            firstName,
            middleName,
            lastName,
            secondLastName,
            birthDate,
            genderId,
            addressId,
            emailProvided,
            normalizedEmail,
            phoneNumberProvided,
            phoneCountryId,
            phoneNumber,
            modelId,
            vehicleTypeId,
            vin,
            year,
            color,
            mileage);

        if (validationError is not null)
        {
            return Result<ClientWithVehicleDto>.Failure(validationError);
        }

        string? emailUser = null;
        string? domain = null;
        if (emailProvided)
        {
            if (!TrySplitEmail(normalizedEmail, out var splitEmailUser, out var splitDomain))
            {
                return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.EmailInvalid);
            }

            if (splitEmailUser.Length > EmailUserMaxLength)
            {
                return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.EmailInvalid);
            }

            emailUser = splitEmailUser;
            domain = splitDomain;
        }

        var documentTypeRepository = _unitOfWork.Repository<DocumentType>();
        var documentTypeExists = await documentTypeRepository.ExistsAsync(
            x => x.DocumentTypeId == documentTypeId,
            cancellationToken);

        if (!documentTypeExists)
        {
            return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.DocumentTypeNotFound);
        }

        if (genderId.HasValue)
        {
            var genderRepository = _unitOfWork.Repository<Gender>();
            var genderExists = await genderRepository.ExistsAsync(
                x => x.GenderId == genderId.Value,
                cancellationToken);

            if (!genderExists)
            {
                return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.GenderNotFound);
            }
        }

        if (addressId.HasValue)
        {
            var addressRepository = _unitOfWork.Repository<Address>();
            var addressExists = await addressRepository.ExistsAsync(
                x => x.AddressId == addressId.Value,
                cancellationToken);

            if (!addressExists)
            {
                return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.AddressNotFound);
            }
        }

        if (phoneNumberProvided)
        {
            var countryRepository = _unitOfWork.Repository<Country>();
            var countryExists = await countryRepository.ExistsAsync(
                x => x.CountryId == phoneCountryId!.Value,
                cancellationToken);

            if (!countryExists)
            {
                return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.PhoneCountryNotFound);
            }
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var documentNumberAlreadyExists = await personRepository.ExistsAsync(
            x => x.DocumentNumber == documentNumber,
            cancellationToken);

        if (documentNumberAlreadyExists)
        {
            return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.DocumentNumberAlreadyExists);
        }

        var roleRepository = _unitOfWork.Repository<Role>();
        var roles = await roleRepository.GetAllAsync(cancellationToken);
        var clientRole = roles.FirstOrDefault(x =>
            x.RoleName.Equals(ClientRoleName, StringComparison.OrdinalIgnoreCase));

        if (clientRole is null)
        {
            return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.ClientRoleNotFound);
        }

        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        EmailDomain? emailDomain = null;
        if (emailProvided)
        {
            var emailDomains = await emailDomainRepository.FindAsync(
                x => x.Domain == domain!,
                cancellationToken);
            emailDomain = emailDomains.FirstOrDefault();

            if (emailDomain is not null)
            {
                var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
                var emailAlreadyExists = await personEmailRepository.ExistsAsync(
                    x => x.EmailUser == emailUser && x.EmailDomainId == emailDomain.EmailDomainId,
                    cancellationToken);

                if (emailAlreadyExists)
                {
                    return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.EmailAlreadyExists);
                }
            }
        }

        if (phoneNumberProvided)
        {
            var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
            var phoneAlreadyExists = await personPhoneRepository.ExistsAsync(
                x => x.CountryId == phoneCountryId!.Value && x.PhoneNumber == phoneNumber!,
                cancellationToken);

            if (phoneAlreadyExists)
            {
                return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.PhoneNumberAlreadyExists);
            }
        }

        var vehicleModelRepository = _unitOfWork.Repository<VehicleModel>();
        var modelExists = await vehicleModelRepository.ExistsAsync(
            x => x.ModelId == modelId,
            cancellationToken);

        if (!modelExists)
        {
            return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.VehicleModelNotFound);
        }

        var vehicleTypeRepository = _unitOfWork.Repository<VehicleType>();
        var vehicleTypeExists = await vehicleTypeRepository.ExistsAsync(
            x => x.VehicleTypeId == vehicleTypeId,
            cancellationToken);

        if (!vehicleTypeExists)
        {
            return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.VehicleTypeNotFound);
        }

        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vinAlreadyExists = await vehicleRepository.ExistsAsync(
            x => x.VIN == vin,
            cancellationToken);

        if (vinAlreadyExists)
        {
            return Result<ClientWithVehicleDto>.Failure(ClientVehicleFlowErrors.VinAlreadyExists);
        }

        var person = new Person
        {
            DocumentTypeId = documentTypeId,
            DocumentNumber = documentNumber,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            SecondLastName = secondLastName,
            BirthDate = birthDate?.Date,
            GenderId = genderId,
            AddressId = addressId
        };

        await personRepository.AddAsync(person, cancellationToken);

        if (emailProvided)
        {
            if (emailDomain is null)
            {
                emailDomain = new EmailDomain
                {
                    Domain = domain!
                };

                await emailDomainRepository.AddAsync(emailDomain, cancellationToken);
            }

            var personEmail = new PersonEmail
            {
                Person = person,
                EmailUser = emailUser!,
                IsPrimary = true
            };

            if (emailDomain.EmailDomainId > 0)
            {
                personEmail.EmailDomainId = emailDomain.EmailDomainId;
            }
            else
            {
                personEmail.EmailDomain = emailDomain;
            }

            await _unitOfWork.Repository<PersonEmail>().AddAsync(personEmail, cancellationToken);
        }

        if (phoneNumberProvided)
        {
            var personPhone = new PersonPhone
            {
                Person = person,
                CountryId = phoneCountryId!.Value,
                PhoneNumber = phoneNumber!,
                IsPrimary = true
            };

            await _unitOfWork.Repository<PersonPhone>().AddAsync(personPhone, cancellationToken);
        }

        var personRole = new PersonRole
        {
            Person = person,
            RoleId = clientRole.RoleId,
            IsActive = true
        };

        await _unitOfWork.Repository<PersonRole>().AddAsync(personRole, cancellationToken);

        var vehicle = new Vehicle
        {
            ModelId = modelId,
            VehicleTypeId = vehicleTypeId,
            VIN = vin,
            Year = year,
            Color = color,
            Mileage = mileage,
            IsActive = true
        };

        await vehicleRepository.AddAsync(vehicle, cancellationToken);

        var ownerHistory = new VehicleOwnerHistory
        {
            Vehicle = vehicle,
            Person = person,
            StartDate = DateTime.UtcNow.Date,
            EndDate = null
        };

        await _unitOfWork.Repository<VehicleOwnerHistory>().AddAsync(ownerHistory, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ClientWithVehicleDto>.Success(new ClientWithVehicleDto
        {
            PersonId = person.PersonId,
            VehicleId = vehicle.VehicleId,
            VehicleOwnerHistoryId = ownerHistory.VehicleOwnerHistoryId,
            DocumentNumber = person.DocumentNumber,
            FullName = BuildFullName(person.FirstName, person.MiddleName, person.LastName, person.SecondLastName),
            PrimaryEmail = emailProvided ? normalizedEmail : null,
            PrimaryPhoneNumber = phoneNumberProvided ? phoneNumber : null,
            VIN = vehicle.VIN
        });
    }

    public async Task<Result<ClientVehicleDto>> AddVehicleToClientAsync(
        int personId,
        AddVehicleToClientRequest request,
        CancellationToken cancellationToken = default)
    {
        var clientValidation = await ValidateClientPersonAsync(personId, cancellationToken);
        if (clientValidation.Error is not null)
        {
            return Result<ClientVehicleDto>.Failure(clientValidation.Error);
        }

        var modelId = request?.ModelId ?? 0;
        var vehicleTypeId = request?.VehicleTypeId ?? 0;
        var vin = NormalizeVin(request?.VIN);
        var year = request?.Year ?? 0;
        var color = NormalizeOptionalText(request?.Color);
        var mileage = request?.Mileage ?? 0;

        var vehicleValidationError = ValidateVehicleInput(modelId, vehicleTypeId, vin, year, color, mileage);
        if (vehicleValidationError is not null)
        {
            return Result<ClientVehicleDto>.Failure(vehicleValidationError);
        }

        var vehicleModelRepository = _unitOfWork.Repository<VehicleModel>();
        var modelExists = await vehicleModelRepository.ExistsAsync(
            x => x.ModelId == modelId,
            cancellationToken);

        if (!modelExists)
        {
            return Result<ClientVehicleDto>.Failure(ClientVehicleFlowErrors.VehicleModelNotFound);
        }

        var vehicleTypeRepository = _unitOfWork.Repository<VehicleType>();
        var vehicleTypeExists = await vehicleTypeRepository.ExistsAsync(
            x => x.VehicleTypeId == vehicleTypeId,
            cancellationToken);

        if (!vehicleTypeExists)
        {
            return Result<ClientVehicleDto>.Failure(ClientVehicleFlowErrors.VehicleTypeNotFound);
        }

        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vinAlreadyExists = await vehicleRepository.ExistsAsync(
            x => x.VIN == vin,
            cancellationToken);

        if (vinAlreadyExists)
        {
            return Result<ClientVehicleDto>.Failure(ClientVehicleFlowErrors.VinAlreadyExists);
        }

        var vehicle = new Vehicle
        {
            ModelId = modelId,
            VehicleTypeId = vehicleTypeId,
            VIN = vin,
            Year = year,
            Color = color,
            Mileage = mileage,
            IsActive = true
        };

        await vehicleRepository.AddAsync(vehicle, cancellationToken);

        var ownerHistory = new VehicleOwnerHistory
        {
            Vehicle = vehicle,
            PersonId = personId,
            StartDate = DateTime.UtcNow.Date,
            EndDate = null
        };

        await _unitOfWork.Repository<VehicleOwnerHistory>().AddAsync(ownerHistory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ClientVehicleDto>.Success(MapToClientVehicleDto(vehicle, ownerHistory));
    }

    public async Task<Result<ClientVehicleDto>> TransferVehicleOwnershipAsync(
        int vehicleId,
        TransferVehicleOwnershipRequest request,
        CancellationToken cancellationToken = default)
    {
        if (vehicleId <= 0)
        {
            return Result<ClientVehicleDto>.Failure(ClientVehicleFlowErrors.VehicleIdInvalid);
        }

        var newOwnerPersonId = request?.NewOwnerPersonId ?? 0;
        if (newOwnerPersonId <= 0)
        {
            return Result<ClientVehicleDto>.Failure(ClientVehicleFlowErrors.PersonIdInvalid);
        }

        var transferDate = request?.TransferDate ?? DateTime.UtcNow;
        if (transferDate == default)
        {
            return Result<ClientVehicleDto>.Failure(ClientVehicleFlowErrors.TransferDateInvalid);
        }

        transferDate = transferDate.Date;

        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null)
        {
            return Result<ClientVehicleDto>.Failure(ClientVehicleFlowErrors.VehicleNotFound);
        }

        if (!vehicle.IsActive)
        {
            return Result<ClientVehicleDto>.Failure(ClientVehicleFlowErrors.VehicleInactive);
        }

        var newOwnerValidation = await ValidateClientPersonAsync(newOwnerPersonId, cancellationToken);
        if (newOwnerValidation.Error is not null)
        {
            return Result<ClientVehicleDto>.Failure(newOwnerValidation.Error);
        }

        var vehicleOwnerHistoryRepository = _unitOfWork.Repository<VehicleOwnerHistory>();
        var currentOwnerHistory = (await vehicleOwnerHistoryRepository.FindAsync(
                x => x.VehicleId == vehicleId && x.EndDate == null,
                cancellationToken))
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.VehicleOwnerHistoryId)
            .FirstOrDefault();

        if (currentOwnerHistory is null)
        {
            return Result<ClientVehicleDto>.Failure(ClientVehicleFlowErrors.CurrentOwnerNotFound);
        }

        if (currentOwnerHistory.PersonId == newOwnerPersonId)
        {
            return Result<ClientVehicleDto>.Failure(ClientVehicleFlowErrors.SameOwnerTransferConflict);
        }

        if (transferDate < currentOwnerHistory.StartDate.Date)
        {
            return Result<ClientVehicleDto>.Failure(ClientVehicleFlowErrors.TransferDateInvalid);
        }

        currentOwnerHistory.EndDate = transferDate;
        vehicleOwnerHistoryRepository.Update(currentOwnerHistory);

        var newOwnerHistory = new VehicleOwnerHistory
        {
            VehicleId = vehicleId,
            PersonId = newOwnerPersonId,
            StartDate = transferDate,
            EndDate = null
        };

        await vehicleOwnerHistoryRepository.AddAsync(newOwnerHistory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ClientVehicleDto>.Success(MapToClientVehicleDto(vehicle, newOwnerHistory));
    }

    public async Task<Result<IReadOnlyList<ClientVehicleDto>>> GetClientVehiclesAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        var clientValidation = await ValidateClientPersonAsync(personId, cancellationToken);
        if (clientValidation.Error is not null)
        {
            return Result<IReadOnlyList<ClientVehicleDto>>.Failure(clientValidation.Error);
        }

        var vehicleOwnerHistoryRepository = _unitOfWork.Repository<VehicleOwnerHistory>();
        var ownerHistories = await vehicleOwnerHistoryRepository.FindAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (ownerHistories.Count == 0)
        {
            return Result<IReadOnlyList<ClientVehicleDto>>.Success(Array.Empty<ClientVehicleDto>());
        }

        var vehicleIds = ownerHistories
            .Select(x => x.VehicleId)
            .Distinct()
            .ToList();

        var vehicles = await _unitOfWork.Repository<Vehicle>().FindAsync(
            x => vehicleIds.Contains(x.VehicleId),
            cancellationToken);

        var vehicleById = vehicles.ToDictionary(x => x.VehicleId, x => x);

        var result = ownerHistories
            .OrderBy(x => x.EndDate.HasValue)
            .ThenByDescending(x => x.StartDate)
            .ThenByDescending(x => x.VehicleOwnerHistoryId)
            .Where(x => vehicleById.ContainsKey(x.VehicleId))
            .Select(x => MapToClientVehicleDto(vehicleById[x.VehicleId], x))
            .ToList();

        return Result<IReadOnlyList<ClientVehicleDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<ClientServiceOrderSummaryDto>>> GetClientServiceOrdersAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        var clientValidation = await ValidateClientPersonAsync(personId, cancellationToken);
        if (clientValidation.Error is not null)
        {
            return Result<IReadOnlyList<ClientServiceOrderSummaryDto>>.Failure(clientValidation.Error);
        }

        var vehicleIds = await GetClientVehicleIdsAsync(personId, cancellationToken);
        if (vehicleIds.Count == 0)
        {
            return Result<IReadOnlyList<ClientServiceOrderSummaryDto>>.Success(Array.Empty<ClientServiceOrderSummaryDto>());
        }

        var serviceOrders = await _unitOfWork.Repository<ServiceOrder>().FindAsync(
            x => vehicleIds.Contains(x.VehicleId),
            cancellationToken);

        var result = serviceOrders
            .OrderByDescending(x => x.EntryDate)
            .ThenByDescending(x => x.ServiceOrderId)
            .Select(x => new ClientServiceOrderSummaryDto
            {
                ServiceOrderId = x.ServiceOrderId,
                VehicleId = x.VehicleId,
                OrderStatusId = x.OrderStatusId,
                EntryDate = x.EntryDate,
                EstimatedDeliveryDate = x.EstimatedDeliveryDate,
                GeneralDescription = x.GeneralDescription,
                CancellationReason = x.CancellationReason,
                CancellationDate = x.CancellationDate,
                CreatedAt = x.CreatedAt
            })
            .ToList();

        return Result<IReadOnlyList<ClientServiceOrderSummaryDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<ClientInvoiceSummaryDto>>> GetClientInvoicesAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        var clientValidation = await ValidateClientPersonAsync(personId, cancellationToken);
        if (clientValidation.Error is not null)
        {
            return Result<IReadOnlyList<ClientInvoiceSummaryDto>>.Failure(clientValidation.Error);
        }

        var vehicleIds = await GetClientVehicleIdsAsync(personId, cancellationToken);
        if (vehicleIds.Count == 0)
        {
            return Result<IReadOnlyList<ClientInvoiceSummaryDto>>.Success(Array.Empty<ClientInvoiceSummaryDto>());
        }

        var serviceOrders = await _unitOfWork.Repository<ServiceOrder>().FindAsync(
            x => vehicleIds.Contains(x.VehicleId),
            cancellationToken);

        var serviceOrderIds = serviceOrders
            .Select(x => x.ServiceOrderId)
            .Distinct()
            .ToList();

        if (serviceOrderIds.Count == 0)
        {
            return Result<IReadOnlyList<ClientInvoiceSummaryDto>>.Success(Array.Empty<ClientInvoiceSummaryDto>());
        }

        var invoices = await _unitOfWork.Repository<Invoice>().FindAsync(
            x => serviceOrderIds.Contains(x.ServiceOrderId),
            cancellationToken);

        var result = invoices
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.InvoiceId)
            .Select(x => new ClientInvoiceSummaryDto
            {
                InvoiceId = x.InvoiceId,
                InvoiceNumber = x.InvoiceNumber,
                ServiceOrderId = x.ServiceOrderId,
                InvoiceStatusId = x.InvoiceStatusId,
                InvoiceDate = x.InvoiceDate,
                Subtotal = x.Subtotal,
                Tax = x.Tax,
                Total = x.Total,
                Observations = x.Observations
            })
            .ToList();

        return Result<IReadOnlyList<ClientInvoiceSummaryDto>>.Success(result);
    }

    private async Task<List<int>> GetClientVehicleIdsAsync(int personId, CancellationToken cancellationToken)
    {
        var ownerHistories = await _unitOfWork.Repository<VehicleOwnerHistory>().FindAsync(
            x => x.PersonId == personId,
            cancellationToken);

        return ownerHistories
            .Select(x => x.VehicleId)
            .Distinct()
            .ToList();
    }

    private async Task<(Person? Person, Error? Error)> ValidateClientPersonAsync(int personId, CancellationToken cancellationToken)
    {
        if (personId <= 0)
        {
            return (null, ClientVehicleFlowErrors.PersonIdInvalid);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var person = await personRepository.GetByIdAsync(personId, cancellationToken);

        if (person is null)
        {
            return (null, ClientVehicleFlowErrors.PersonNotFound);
        }

        var clientRoleId = await GetClientRoleIdAsync(cancellationToken);
        if (!clientRoleId.HasValue)
        {
            return (null, ClientVehicleFlowErrors.ClientRoleNotFound);
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var hasClientRole = await personRoleRepository.ExistsAsync(
            x => x.PersonId == personId && x.RoleId == clientRoleId.Value && x.IsActive,
            cancellationToken);

        if (!hasClientRole)
        {
            return (null, ClientVehicleFlowErrors.PersonIsNotClientInvalid);
        }

        return (person, null);
    }

    private async Task<int?> GetClientRoleIdAsync(CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Repository<Role>().GetAllAsync(cancellationToken);

        return roles
            .Where(x => x.RoleName.Equals(ClientRoleName, StringComparison.OrdinalIgnoreCase))
            .Select(x => (int?)x.RoleId)
            .FirstOrDefault();
    }

    private static ClientVehicleDto MapToClientVehicleDto(Vehicle vehicle, VehicleOwnerHistory ownerHistory)
    {
        return new ClientVehicleDto
        {
            VehicleId = vehicle.VehicleId,
            ModelId = vehicle.ModelId,
            VehicleTypeId = vehicle.VehicleTypeId,
            VIN = vehicle.VIN,
            Year = vehicle.Year,
            Color = vehicle.Color,
            Mileage = vehicle.Mileage,
            IsActive = vehicle.IsActive,
            OwnershipStartDate = ownerHistory.StartDate,
            OwnershipEndDate = ownerHistory.EndDate
        };
    }

    private static Error? ValidateCreateClientWithVehicleInput(
        int documentTypeId,
        string documentNumber,
        string firstName,
        string? middleName,
        string lastName,
        string? secondLastName,
        DateTime? birthDate,
        int? genderId,
        int? addressId,
        bool emailProvided,
        string normalizedEmail,
        bool phoneNumberProvided,
        int? phoneCountryId,
        string? phoneNumber,
        int modelId,
        int vehicleTypeId,
        string vin,
        int year,
        string? color,
        int mileage)
    {
        var personValidationError = ValidatePersonInput(
            documentTypeId,
            documentNumber,
            firstName,
            middleName,
            lastName,
            secondLastName,
            birthDate,
            genderId,
            addressId,
            emailProvided,
            normalizedEmail,
            phoneNumberProvided,
            phoneCountryId,
            phoneNumber);

        if (personValidationError is not null)
        {
            return personValidationError;
        }

        return ValidateVehicleInput(modelId, vehicleTypeId, vin, year, color, mileage);
    }

    private static Error? ValidatePersonInput(
        int documentTypeId,
        string documentNumber,
        string firstName,
        string? middleName,
        string lastName,
        string? secondLastName,
        DateTime? birthDate,
        int? genderId,
        int? addressId,
        bool emailProvided,
        string normalizedEmail,
        bool phoneNumberProvided,
        int? phoneCountryId,
        string? phoneNumber)
    {
        if (documentTypeId <= 0)
        {
            return ClientVehicleFlowErrors.DocumentTypeIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return ClientVehicleFlowErrors.DocumentNumberRequired;
        }

        if (documentNumber.Length > DocumentNumberMaxLength)
        {
            return ClientVehicleFlowErrors.DocumentNumberTooLong;
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            return ClientVehicleFlowErrors.FirstNameRequired;
        }

        if (firstName.Length > FirstNameMaxLength)
        {
            return ClientVehicleFlowErrors.FirstNameTooLong;
        }

        if (middleName is not null && middleName.Length > MiddleNameMaxLength)
        {
            return ClientVehicleFlowErrors.MiddleNameTooLong;
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return ClientVehicleFlowErrors.LastNameRequired;
        }

        if (lastName.Length > LastNameMaxLength)
        {
            return ClientVehicleFlowErrors.LastNameTooLong;
        }

        if (secondLastName is not null && secondLastName.Length > SecondLastNameMaxLength)
        {
            return ClientVehicleFlowErrors.SecondLastNameTooLong;
        }

        if (birthDate.HasValue && birthDate.Value.Date > DateTime.UtcNow.Date)
        {
            return ClientVehicleFlowErrors.BirthDateInvalid;
        }

        if (genderId.HasValue && genderId.Value <= 0)
        {
            return ClientVehicleFlowErrors.GenderIdInvalid;
        }

        if (addressId.HasValue && addressId.Value <= 0)
        {
            return ClientVehicleFlowErrors.AddressIdInvalid;
        }

        if (emailProvided && string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return ClientVehicleFlowErrors.EmailInvalid;
        }

        if (phoneNumberProvided)
        {
            if (!phoneCountryId.HasValue || phoneCountryId.Value <= 0)
            {
                return ClientVehicleFlowErrors.PhoneCountryIdRequired;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return ClientVehicleFlowErrors.PhoneNumberInvalid;
            }

            if (phoneNumber.Length > PhoneNumberMaxLength)
            {
                return ClientVehicleFlowErrors.PhoneNumberTooLong;
            }

            if (!IsValidPhoneNumber(phoneNumber))
            {
                return ClientVehicleFlowErrors.PhoneNumberInvalid;
            }
        }

        return null;
    }

    private static Error? ValidateVehicleInput(
        int modelId,
        int vehicleTypeId,
        string vin,
        int year,
        string? color,
        int mileage)
    {
        if (modelId <= 0)
        {
            return ClientVehicleFlowErrors.ModelIdInvalid;
        }

        if (vehicleTypeId <= 0)
        {
            return ClientVehicleFlowErrors.VehicleTypeIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(vin))
        {
            return ClientVehicleFlowErrors.VinRequired;
        }

        if (vin.Length != VinLength || !vin.All(char.IsLetterOrDigit))
        {
            return ClientVehicleFlowErrors.VinInvalid;
        }

        var maxYear = DateTime.UtcNow.Year + 1;
        if (year < 1900 || year > maxYear)
        {
            return ClientVehicleFlowErrors.YearInvalid;
        }

        if (color is not null && color.Length > ColorMaxLength)
        {
            return ClientVehicleFlowErrors.ColorTooLong;
        }

        if (mileage < 0)
        {
            return ClientVehicleFlowErrors.MileageInvalid;
        }

        return null;
    }

    private static string BuildFullName(string firstName, string? middleName, string lastName, string? secondLastName)
    {
        var parts = new[]
        {
            firstName,
            middleName,
            lastName,
            secondLastName
        };

        return string.Join(" ", parts.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim()));
    }

    private static string NormalizeRequiredText(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string NormalizeEmail(string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static string NormalizeVin(string? vin)
    {
        return (vin ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static bool TrySplitEmail(string email, out string emailUser, out string domain)
    {
        emailUser = string.Empty;
        domain = string.Empty;

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
        {
            return false;
        }

        if (atIndex != email.LastIndexOf('@'))
        {
            return false;
        }

        if (atIndex >= email.Length - 1)
        {
            return false;
        }

        emailUser = email[..atIndex];
        domain = email[(atIndex + 1)..];

        if (string.IsNullOrWhiteSpace(emailUser) || string.IsNullOrWhiteSpace(domain))
        {
            return false;
        }

        if (!domain.Contains('.'))
        {
            return false;
        }

        return true;
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        var digitCount = 0;

        for (var i = 0; i < phoneNumber.Length; i++)
        {
            var c = phoneNumber[i];

            if (i == 0 && c == '+')
            {
                continue;
            }

            if (!char.IsDigit(c))
            {
                return false;
            }

            digitCount++;
        }

        return digitCount > 0;
    }
}
