using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.AdminMechanics.Dtos;
using Application.Features.AdminMechanics.Errors;
using Domain.Entities;

namespace Application.Features.AdminMechanics;

public class AdminMechanicsService : IAdminMechanicsService
{
    private const string MechanicRoleName = "Mechanic";
    private const string PendingStatusName = "Pending";
    private const string InProgressStatusName = "InProgress";
    private const string CompletedStatusName = "Completed";
    private const string CancelledStatusName = "Cancelled";
    private const string VoidedStatusName = "Voided";

    private readonly IUnitOfWork _unitOfWork;

    public AdminMechanicsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<AdminMechanicListItemDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var mechanicRoleId = await GetRoleIdByNameAsync(MechanicRoleName, cancellationToken);
        if (!mechanicRoleId.HasValue)
        {
            return Result<IReadOnlyList<AdminMechanicListItemDto>>.Success(Array.Empty<AdminMechanicListItemDto>());
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var mechanicPersonRoles = await personRoleRepository.FindAsync(
            x => x.RoleId == mechanicRoleId.Value,
            cancellationToken);

        var mechanicPersonIds = mechanicPersonRoles
            .Select(x => x.PersonId)
            .Distinct()
            .ToArray();

        if (mechanicPersonIds.Length == 0)
        {
            return Result<IReadOnlyList<AdminMechanicListItemDto>>.Success(Array.Empty<AdminMechanicListItemDto>());
        }

        var data = await LoadMechanicDataAsync(mechanicPersonIds, mechanicRoleId.Value, cancellationToken);

        var mechanics = mechanicPersonIds
            .Where(data.PersonsById.ContainsKey)
            .Select(x => BuildListItem(x, data))
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.PersonId)
            .ToList();

        return Result<IReadOnlyList<AdminMechanicListItemDto>>.Success(mechanics);
    }

    public async Task<Result<AdminMechanicDetailDto>> GetByPersonIdAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        var validationError = await ValidateMechanicAsync(personId, cancellationToken);
        if (validationError is not null)
        {
            return Result<AdminMechanicDetailDto>.Failure(validationError);
        }

        var mechanicRoleId = await GetRoleIdByNameAsync(MechanicRoleName, cancellationToken);
        var data = await LoadMechanicDataAsync(new[] { personId }, mechanicRoleId!.Value, cancellationToken);

        return Result<AdminMechanicDetailDto>.Success(BuildDetail(personId, data));
    }

    public async Task<Result<AdminMechanicWorkloadDto>> GetWorkloadAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        var validationError = await ValidateMechanicAsync(personId, cancellationToken);
        if (validationError is not null)
        {
            return Result<AdminMechanicWorkloadDto>.Failure(validationError);
        }

        var mechanicRoleId = await GetRoleIdByNameAsync(MechanicRoleName, cancellationToken);
        var data = await LoadMechanicDataAsync(new[] { personId }, mechanicRoleId!.Value, cancellationToken);

        return Result<AdminMechanicWorkloadDto>.Success(BuildWorkload(personId, data));
    }

    private async Task<Error?> ValidateMechanicAsync(int personId, CancellationToken cancellationToken)
    {
        if (personId <= 0)
        {
            return AdminMechanicErrors.PersonIdInvalid;
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return AdminMechanicErrors.PersonNotFound;
        }

        var mechanicRoleId = await GetRoleIdByNameAsync(MechanicRoleName, cancellationToken);
        if (!mechanicRoleId.HasValue)
        {
            return AdminMechanicErrors.MechanicNotFound;
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var hasMechanicRole = await personRoleRepository.ExistsAsync(
            x => x.PersonId == personId && x.RoleId == mechanicRoleId.Value,
            cancellationToken);

        return hasMechanicRole ? null : AdminMechanicErrors.MechanicNotFound;
    }

    private async Task<MechanicLookupData> LoadMechanicDataAsync(
        int[] mechanicPersonIds,
        int mechanicRoleId,
        CancellationToken cancellationToken)
    {
        var personIds = mechanicPersonIds
            .Where(x => x > 0)
            .Distinct()
            .ToArray();

        var personRepository = _unitOfWork.Repository<Person>();
        var persons = await personRepository.FindAsync(
            x => personIds.Contains(x.PersonId),
            cancellationToken);

        var userRepository = _unitOfWork.Repository<User>();
        var users = await userRepository.FindAsync(
            x => personIds.Contains(x.PersonId),
            cancellationToken);

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var personRoles = await personRoleRepository.FindAsync(
            x => personIds.Contains(x.PersonId),
            cancellationToken);

        var roleRepository = _unitOfWork.Repository<Role>();
        var roles = await roleRepository.GetAllAsync(cancellationToken);
        var roleNameById = roles.ToDictionary(x => x.RoleId, x => x.RoleName);

        var documentTypeIds = persons
            .Select(x => x.DocumentTypeId)
            .Distinct()
            .ToArray();

        var documentTypeRepository = _unitOfWork.Repository<DocumentType>();
        var documentTypes = documentTypeIds.Length == 0
            ? Array.Empty<DocumentType>()
            : await documentTypeRepository.FindAsync(x => documentTypeIds.Contains(x.DocumentTypeId), cancellationToken);

        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        var personEmails = await personEmailRepository.FindAsync(
            x => personIds.Contains(x.PersonId),
            cancellationToken);

        var emailDomainIds = personEmails
            .Select(x => x.EmailDomainId)
            .Distinct()
            .ToArray();

        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var emailDomains = emailDomainIds.Length == 0
            ? Array.Empty<EmailDomain>()
            : await emailDomainRepository.FindAsync(x => emailDomainIds.Contains(x.EmailDomainId), cancellationToken);

        var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
        var personPhones = await personPhoneRepository.FindAsync(
            x => personIds.Contains(x.PersonId),
            cancellationToken);

        var specialtyAssignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var specialtyAssignments = await specialtyAssignmentRepository.FindAsync(
            x => personIds.Contains(x.PersonId),
            cancellationToken);

        var specialtyIds = specialtyAssignments
            .Select(x => x.SpecialtyId)
            .Distinct()
            .ToArray();

        var specialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        var specialties = specialtyIds.Length == 0
            ? Array.Empty<MechanicSpecialty>()
            : await specialtyRepository.FindAsync(x => specialtyIds.Contains(x.SpecialtyId), cancellationToken);

        var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
        var mechanicAssignments = await mechanicAssignmentRepository.FindAsync(
            x => personIds.Contains(x.MechanicPersonId),
            cancellationToken);

        var orderServiceIds = mechanicAssignments
            .Select(x => x.OrderServiceId)
            .Distinct()
            .ToArray();

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var orderServices = orderServiceIds.Length == 0
            ? Array.Empty<OrderService>()
            : await orderServiceRepository.FindAsync(x => orderServiceIds.Contains(x.OrderServiceId), cancellationToken);

        var serviceOrderIds = orderServices
            .Select(x => x.ServiceOrderId)
            .Distinct()
            .ToArray();

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrders = serviceOrderIds.Length == 0
            ? Array.Empty<ServiceOrder>()
            : await serviceOrderRepository.FindAsync(x => serviceOrderIds.Contains(x.ServiceOrderId), cancellationToken);

        var vehicleIds = serviceOrders
            .Select(x => x.VehicleId)
            .Distinct()
            .ToArray();

        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vehicles = vehicleIds.Length == 0
            ? Array.Empty<Vehicle>()
            : await vehicleRepository.FindAsync(x => vehicleIds.Contains(x.VehicleId), cancellationToken);

        var serviceTypeIds = orderServices
            .Select(x => x.ServiceTypeId)
            .Distinct()
            .ToArray();

        var serviceTypeRepository = _unitOfWork.Repository<ServiceType>();
        var serviceTypes = serviceTypeIds.Length == 0
            ? Array.Empty<ServiceType>()
            : await serviceTypeRepository.FindAsync(x => serviceTypeIds.Contains(x.ServiceTypeId), cancellationToken);

        var orderStatusIds = serviceOrders
            .Select(x => x.OrderStatusId)
            .Distinct()
            .ToArray();

        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();
        var orderStatuses = orderStatusIds.Length == 0
            ? Array.Empty<OrderStatus>()
            : await orderStatusRepository.FindAsync(x => orderStatusIds.Contains(x.OrderStatusId), cancellationToken);

        var ownerHistoryRepository = _unitOfWork.Repository<VehicleOwnerHistory>();
        var ownerHistories = vehicleIds.Length == 0
            ? Array.Empty<VehicleOwnerHistory>()
            : await ownerHistoryRepository.FindAsync(x => vehicleIds.Contains(x.VehicleId), cancellationToken);

        var customerPersonIds = ownerHistories
            .Select(x => x.PersonId)
            .Distinct()
            .ToArray();

        var customerPersons = customerPersonIds.Length == 0
            ? Array.Empty<Person>()
            : await personRepository.FindAsync(x => customerPersonIds.Contains(x.PersonId), cancellationToken);

        return new MechanicLookupData
        {
            PersonsById = persons.ToDictionary(x => x.PersonId),
            UsersByPersonId = BuildUsersByPersonId(users),
            RolesByPersonId = BuildRolesByPersonId(personRoles, roleNameById),
            MechanicRoleActiveByPersonId = BuildMechanicRoleActiveByPersonId(personRoles, mechanicRoleId),
            DocumentTypeNamesById = documentTypes.ToDictionary(x => x.DocumentTypeId, x => x.Name),
            EmailsByPersonId = BuildEmailsByPersonId(personEmails, emailDomains),
            PhonesByPersonId = BuildPhonesByPersonId(personPhones),
            SpecialtiesByPersonId = BuildSpecialtiesByPersonId(specialtyAssignments, specialties),
            AssignmentsByPersonId = mechanicAssignments
                .GroupBy(x => x.MechanicPersonId)
                .ToDictionary(x => x.Key, x => x.OrderBy(y => y.MechanicAssignmentId).ToList()),
            OrderServicesById = orderServices.ToDictionary(x => x.OrderServiceId),
            ServiceOrdersById = serviceOrders.ToDictionary(x => x.ServiceOrderId),
            VehiclesById = vehicles.ToDictionary(x => x.VehicleId),
            ServiceTypesById = serviceTypes.ToDictionary(x => x.ServiceTypeId),
            OrderStatusesById = orderStatuses.ToDictionary(x => x.OrderStatusId),
            CustomersByServiceOrderId = BuildCustomersByServiceOrderId(serviceOrders, ownerHistories, customerPersons)
        };
    }

    private AdminMechanicListItemDto BuildListItem(int personId, MechanicLookupData data)
    {
        var person = data.PersonsById[personId];
        data.UsersByPersonId.TryGetValue(personId, out var user);
        data.EmailsByPersonId.TryGetValue(personId, out var email);

        var assignments = GetDistinctAssignments(personId, data);

        return new AdminMechanicListItemDto
        {
            PersonId = person.PersonId,
            FullName = BuildFullName(person),
            DocumentNumber = person.DocumentNumber,
            UserId = user?.UserId,
            Email = email,
            IsUserActive = user?.IsActive,
            RoleActive = IsMechanicRoleActive(personId, data),
            Specialties = GetSpecialties(personId, data),
            AssignedServicesCount = assignments.Count,
            ActiveOrdersCount = CountActiveOrders(assignments, data)
        };
    }

    private AdminMechanicDetailDto BuildDetail(int personId, MechanicLookupData data)
    {
        var listItem = BuildListItem(personId, data);
        var person = data.PersonsById[personId];
        data.UsersByPersonId.TryGetValue(personId, out var user);
        data.PhonesByPersonId.TryGetValue(personId, out var phoneNumber);
        data.DocumentTypeNamesById.TryGetValue(person.DocumentTypeId, out var documentTypeName);
        data.RolesByPersonId.TryGetValue(personId, out var roles);

        var assignments = GetDistinctAssignments(personId, data);

        return new AdminMechanicDetailDto
        {
            PersonId = listItem.PersonId,
            FullName = listItem.FullName,
            DocumentNumber = listItem.DocumentNumber,
            DocumentTypeName = documentTypeName,
            UserId = listItem.UserId,
            Email = listItem.Email,
            PhoneNumber = phoneNumber,
            IsUserActive = listItem.IsUserActive,
            RoleActive = listItem.RoleActive,
            CreatedAt = person.CreatedAt,
            Specialties = listItem.Specialties,
            Roles = roles is null ? Array.Empty<AdminMechanicRoleDto>() : roles,
            User = user is null
                ? null
                : new AdminMechanicUserDto
                {
                    UserId = user.UserId,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                },
            AssignedServicesCount = listItem.AssignedServicesCount,
            ActiveOrdersCount = listItem.ActiveOrdersCount,
            AssignedServices = BuildWorkloadServices(assignments, data),
            ActiveOrders = BuildActiveOrders(assignments, data)
        };
    }

    private AdminMechanicWorkloadDto BuildWorkload(int personId, MechanicLookupData data)
    {
        var person = data.PersonsById[personId];
        var assignments = GetDistinctAssignments(personId, data);
        var services = BuildWorkloadServices(assignments, data);

        return new AdminMechanicWorkloadDto
        {
            PersonId = person.PersonId,
            FullName = BuildFullName(person),
            AssignedServicesCount = assignments.Count,
            ActiveOrdersCount = CountActiveOrders(assignments, data),
            PendingServicesCount = CountServicesByOrderStatus(assignments, data, PendingStatusName),
            InProgressServicesCount = CountServicesByOrderStatus(assignments, data, InProgressStatusName),
            CompletedServicesCount = CountServicesByOrderStatus(assignments, data, CompletedStatusName),
            Services = services
        };
    }

    private static IReadOnlyList<AdminMechanicWorkloadServiceDto> BuildWorkloadServices(
        IReadOnlyList<MechanicAssignment> assignments,
        MechanicLookupData data)
    {
        return assignments
            .Select(x => MapWorkloadService(x, data))
            .OrderBy(x => x.ServiceOrderId)
            .ThenBy(x => x.OrderServiceId)
            .ToList();
    }

    private static AdminMechanicWorkloadServiceDto MapWorkloadService(
        MechanicAssignment assignment,
        MechanicLookupData data)
    {
        data.OrderServicesById.TryGetValue(assignment.OrderServiceId, out var orderService);
        var serviceOrder = orderService is not null && data.ServiceOrdersById.TryGetValue(orderService.ServiceOrderId, out var foundServiceOrder)
            ? foundServiceOrder
            : null;
        var serviceType = orderService is not null && data.ServiceTypesById.TryGetValue(orderService.ServiceTypeId, out var foundServiceType)
            ? foundServiceType
            : null;
        var vehicle = serviceOrder is not null && data.VehiclesById.TryGetValue(serviceOrder.VehicleId, out var foundVehicle)
            ? foundVehicle
            : null;
        var orderStatus = serviceOrder is not null && data.OrderStatusesById.TryGetValue(serviceOrder.OrderStatusId, out var foundOrderStatus)
            ? foundOrderStatus
            : null;
        var customerName = serviceOrder is not null && data.CustomersByServiceOrderId.TryGetValue(serviceOrder.ServiceOrderId, out var foundCustomerName)
            ? foundCustomerName
            : null;

        return new AdminMechanicWorkloadServiceDto
        {
            MechanicAssignmentId = assignment.MechanicAssignmentId,
            OrderServiceId = assignment.OrderServiceId,
            ServiceOrderId = orderService?.ServiceOrderId ?? 0,
            ServiceTypeName = serviceType?.Name,
            VehiclePlate = NormalizeOptionalText(vehicle?.Plate),
            OrderStatusName = orderStatus?.Name,
            CustomerName = customerName,
            CustomerApproved = orderService?.CustomerApproved ?? false,
            ApprovalDate = orderService?.ApprovalDate,
            WorkReported = orderService is not null && !string.IsNullOrWhiteSpace(orderService.WorkPerformed)
        };
    }

    private static IReadOnlyList<AdminMechanicActiveOrderDto> BuildActiveOrders(
        IReadOnlyList<MechanicAssignment> assignments,
        MechanicLookupData data)
    {
        var orderServices = assignments
            .Select(x => data.OrderServicesById.TryGetValue(x.OrderServiceId, out var orderService) ? orderService : null)
            .Where(x => x is not null)
            .Cast<OrderService>()
            .ToList();

        return orderServices
            .Select(x => data.ServiceOrdersById.TryGetValue(x.ServiceOrderId, out var serviceOrder) ? serviceOrder : null)
            .Where(x => x is not null && IsActiveOrder(x, data))
            .Cast<ServiceOrder>()
            .GroupBy(x => x.ServiceOrderId)
            .Select(x => x.First())
            .OrderByDescending(x => x.EntryDate)
            .ThenBy(x => x.ServiceOrderId)
            .Select(x =>
            {
                var assignedServices = orderServices
                    .Where(y => y.ServiceOrderId == x.ServiceOrderId)
                    .ToList();
                var vehicle = data.VehiclesById.TryGetValue(x.VehicleId, out var foundVehicle)
                    ? foundVehicle
                    : null;
                var orderStatus = data.OrderStatusesById.TryGetValue(x.OrderStatusId, out var foundOrderStatus)
                    ? foundOrderStatus
                    : null;

                return new AdminMechanicActiveOrderDto
                {
                    ServiceOrderId = x.ServiceOrderId,
                    VehicleId = x.VehicleId,
                    VehiclePlate = NormalizeOptionalText(vehicle?.Plate),
                    OrderStatusName = orderStatus?.Name ?? string.Empty,
                    EntryDate = x.EntryDate,
                    EstimatedDeliveryDate = x.EstimatedDeliveryDate,
                    AssignedServicesCount = assignedServices.Count,
                    PendingWorkReportsCount = assignedServices.Count(y => string.IsNullOrWhiteSpace(y.WorkPerformed))
                };
            })
            .ToList();
    }

    private static int CountServicesByOrderStatus(
        IReadOnlyList<MechanicAssignment> assignments,
        MechanicLookupData data,
        string statusName)
    {
        return assignments.Count(x =>
        {
            if (!data.OrderServicesById.TryGetValue(x.OrderServiceId, out var orderService))
            {
                return false;
            }

            if (!data.ServiceOrdersById.TryGetValue(orderService.ServiceOrderId, out var serviceOrder))
            {
                return false;
            }

            if (!data.OrderStatusesById.TryGetValue(serviceOrder.OrderStatusId, out var orderStatus))
            {
                return false;
            }

            return IsStatusName(orderStatus.Name, statusName);
        });
    }

    private static int CountActiveOrders(IReadOnlyList<MechanicAssignment> assignments, MechanicLookupData data)
    {
        return assignments
            .Select(x => data.OrderServicesById.TryGetValue(x.OrderServiceId, out var orderService) ? orderService : null)
            .Where(x => x is not null)
            .Cast<OrderService>()
            .Select(x => data.ServiceOrdersById.TryGetValue(x.ServiceOrderId, out var serviceOrder) ? serviceOrder : null)
            .Where(x => x is not null && IsActiveOrder(x, data))
            .Cast<ServiceOrder>()
            .Select(x => x.ServiceOrderId)
            .Distinct()
            .Count();
    }

    private static bool IsActiveOrder(ServiceOrder? serviceOrder, MechanicLookupData data)
    {
        if (serviceOrder is null)
        {
            return false;
        }

        if (!data.OrderStatusesById.TryGetValue(serviceOrder.OrderStatusId, out var orderStatus))
        {
            return false;
        }

        return !IsStatusName(orderStatus.Name, CompletedStatusName) &&
               !IsStatusName(orderStatus.Name, CancelledStatusName) &&
               !IsStatusName(orderStatus.Name, VoidedStatusName);
    }

    private static IReadOnlyList<MechanicAssignment> GetDistinctAssignments(int personId, MechanicLookupData data)
    {
        if (!data.AssignmentsByPersonId.TryGetValue(personId, out var assignments))
        {
            return Array.Empty<MechanicAssignment>();
        }

        return assignments
            .GroupBy(x => x.OrderServiceId)
            .Select(x => x.OrderBy(y => y.MechanicAssignmentId).First())
            .OrderBy(x => x.MechanicAssignmentId)
            .ToList();
    }

    private static IReadOnlyList<AdminMechanicSpecialtyDto> GetSpecialties(int personId, MechanicLookupData data)
    {
        return data.SpecialtiesByPersonId.TryGetValue(personId, out var specialties)
            ? specialties
            : Array.Empty<AdminMechanicSpecialtyDto>();
    }

    private static bool IsMechanicRoleActive(int personId, MechanicLookupData data)
    {
        return data.MechanicRoleActiveByPersonId.TryGetValue(personId, out var roleActive) && roleActive;
    }

    private async Task<int?> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        var roleRepository = _unitOfWork.Repository<Role>();
        var roles = await roleRepository.GetAllAsync(cancellationToken);

        return roles
            .Where(x => x.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
            .Select(x => (int?)x.RoleId)
            .FirstOrDefault();
    }

    private static Dictionary<int, User> BuildUsersByPersonId(IReadOnlyList<User> users)
    {
        return users
            .GroupBy(x => x.PersonId)
            .ToDictionary(
                x => x.Key,
                x => x.OrderBy(y => y.UserId).First());
    }

    private static Dictionary<int, List<AdminMechanicRoleDto>> BuildRolesByPersonId(
        IReadOnlyList<PersonRole> personRoles,
        IReadOnlyDictionary<int, string> roleNameById)
    {
        return personRoles
            .GroupBy(x => x.PersonId)
            .ToDictionary(
                x => x.Key,
                x => x
                    .OrderBy(y => y.PersonRoleId)
                    .Select(y => new AdminMechanicRoleDto
                    {
                        PersonRoleId = y.PersonRoleId,
                        RoleId = y.RoleId,
                        RoleName = roleNameById.TryGetValue(y.RoleId, out var roleName) ? roleName : string.Empty,
                        IsActive = y.IsActive
                    })
                    .ToList());
    }

    private static Dictionary<int, bool> BuildMechanicRoleActiveByPersonId(
        IReadOnlyList<PersonRole> personRoles,
        int mechanicRoleId)
    {
        return personRoles
            .Where(x => x.RoleId == mechanicRoleId)
            .GroupBy(x => x.PersonId)
            .ToDictionary(x => x.Key, x => x.Any(y => y.IsActive));
    }

    private static Dictionary<int, string> BuildEmailsByPersonId(
        IReadOnlyList<PersonEmail> personEmails,
        IReadOnlyList<EmailDomain> emailDomains)
    {
        var emailDomainById = emailDomains.ToDictionary(x => x.EmailDomainId, x => x.Domain);
        var result = new Dictionary<int, string>();

        foreach (var emailGroup in personEmails.GroupBy(x => x.PersonId))
        {
            var email = emailGroup
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.PersonEmailId)
                .FirstOrDefault();

            if (email is null ||
                string.IsNullOrWhiteSpace(email.EmailUser) ||
                !emailDomainById.TryGetValue(email.EmailDomainId, out var domain) ||
                string.IsNullOrWhiteSpace(domain))
            {
                continue;
            }

            result[emailGroup.Key] = $"{email.EmailUser}@{domain}";
        }

        return result;
    }

    private static Dictionary<int, string> BuildPhonesByPersonId(IReadOnlyList<PersonPhone> personPhones)
    {
        var result = new Dictionary<int, string>();

        foreach (var phoneGroup in personPhones.GroupBy(x => x.PersonId))
        {
            var phone = phoneGroup
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.PersonPhoneId)
                .FirstOrDefault();

            var phoneNumber = NormalizeOptionalText(phone?.PhoneNumber);
            if (phoneNumber is not null)
            {
                result[phoneGroup.Key] = phoneNumber;
            }
        }

        return result;
    }

    private static Dictionary<int, List<AdminMechanicSpecialtyDto>> BuildSpecialtiesByPersonId(
        IReadOnlyList<MechanicSpecialtyAssignment> specialtyAssignments,
        IReadOnlyList<MechanicSpecialty> specialties)
    {
        var specialtyNameById = specialties.ToDictionary(x => x.SpecialtyId, x => x.Name);

        return specialtyAssignments
            .GroupBy(x => x.PersonId)
            .ToDictionary(
                x => x.Key,
                x => x
                    .GroupBy(y => y.SpecialtyId)
                    .Select(y => y.OrderBy(z => z.AssignmentId).First())
                    .Select(y => new AdminMechanicSpecialtyDto
                    {
                        SpecialtyId = y.SpecialtyId,
                        SpecialtyName = specialtyNameById.TryGetValue(y.SpecialtyId, out var specialtyName)
                            ? specialtyName
                            : string.Empty
                    })
                    .OrderBy(y => y.SpecialtyName)
                    .ThenBy(y => y.SpecialtyId)
                    .ToList());
    }

    private static Dictionary<int, string> BuildCustomersByServiceOrderId(
        IReadOnlyList<ServiceOrder> serviceOrders,
        IReadOnlyList<VehicleOwnerHistory> ownerHistories,
        IReadOnlyList<Person> customerPersons)
    {
        var customersById = customerPersons.ToDictionary(x => x.PersonId);
        var historiesByVehicleId = ownerHistories
            .GroupBy(x => x.VehicleId)
            .ToDictionary(x => x.Key, x => x.ToList());
        var result = new Dictionary<int, string>();

        foreach (var serviceOrder in serviceOrders)
        {
            if (!historiesByVehicleId.TryGetValue(serviceOrder.VehicleId, out var histories))
            {
                continue;
            }

            var ownerHistory = histories
                .Where(x => x.StartDate <= serviceOrder.EntryDate &&
                            (!x.EndDate.HasValue || x.EndDate.Value >= serviceOrder.EntryDate))
                .OrderByDescending(x => x.StartDate)
                .ThenByDescending(x => x.VehicleOwnerHistoryId)
                .FirstOrDefault()
                ?? histories
                    .Where(x => !x.EndDate.HasValue)
                    .OrderByDescending(x => x.StartDate)
                    .ThenByDescending(x => x.VehicleOwnerHistoryId)
                    .FirstOrDefault();

            if (ownerHistory is null || !customersById.TryGetValue(ownerHistory.PersonId, out var customer))
            {
                continue;
            }

            result[serviceOrder.ServiceOrderId] = BuildFullName(customer);
        }

        return result;
    }

    private static string BuildFullName(Person person)
    {
        return string.Join(
            " ",
            new[]
            {
                person.FirstName,
                person.MiddleName,
                person.LastName,
                person.SecondLastName
            }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static bool IsStatusName(string statusName, string expectedStatusName)
    {
        return statusName.Equals(expectedStatusName, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class MechanicLookupData
    {
        public IReadOnlyDictionary<int, Person> PersonsById { get; init; } = new Dictionary<int, Person>();
        public IReadOnlyDictionary<int, User> UsersByPersonId { get; init; } = new Dictionary<int, User>();
        public IReadOnlyDictionary<int, List<AdminMechanicRoleDto>> RolesByPersonId { get; init; } = new Dictionary<int, List<AdminMechanicRoleDto>>();
        public IReadOnlyDictionary<int, bool> MechanicRoleActiveByPersonId { get; init; } = new Dictionary<int, bool>();
        public IReadOnlyDictionary<int, string> DocumentTypeNamesById { get; init; } = new Dictionary<int, string>();
        public IReadOnlyDictionary<int, string> EmailsByPersonId { get; init; } = new Dictionary<int, string>();
        public IReadOnlyDictionary<int, string> PhonesByPersonId { get; init; } = new Dictionary<int, string>();
        public IReadOnlyDictionary<int, List<AdminMechanicSpecialtyDto>> SpecialtiesByPersonId { get; init; } = new Dictionary<int, List<AdminMechanicSpecialtyDto>>();
        public IReadOnlyDictionary<int, List<MechanicAssignment>> AssignmentsByPersonId { get; init; } = new Dictionary<int, List<MechanicAssignment>>();
        public IReadOnlyDictionary<int, OrderService> OrderServicesById { get; init; } = new Dictionary<int, OrderService>();
        public IReadOnlyDictionary<int, ServiceOrder> ServiceOrdersById { get; init; } = new Dictionary<int, ServiceOrder>();
        public IReadOnlyDictionary<int, Vehicle> VehiclesById { get; init; } = new Dictionary<int, Vehicle>();
        public IReadOnlyDictionary<int, ServiceType> ServiceTypesById { get; init; } = new Dictionary<int, ServiceType>();
        public IReadOnlyDictionary<int, OrderStatus> OrderStatusesById { get; init; } = new Dictionary<int, OrderStatus>();
        public IReadOnlyDictionary<int, string> CustomersByServiceOrderId { get; init; } = new Dictionary<int, string>();
    }
}
