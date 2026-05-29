using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.MechanicAssignments.Dtos;
using Application.Features.MechanicAssignments.Errors;
using Application.Features.MechanicAssignments.Requests;
using Domain.Entities;

namespace Application.Features.MechanicAssignments;

public class MechanicAssignmentService : IMechanicAssignmentService
{
    private const string MechanicRoleName = "Mechanic";
    private const string CancelledStatusName = "Cancelled";
    private const string VoidedStatusName = "Voided";

    private readonly IUnitOfWork _unitOfWork;

    public MechanicAssignmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<MechanicAssignmentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
        var mechanicAssignments = await mechanicAssignmentRepository.GetAllAsync(cancellationToken);

        var mechanicAssignmentDtos = mechanicAssignments
            .OrderBy(x => x.MechanicAssignmentId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<MechanicAssignmentDto>>.Success(mechanicAssignmentDtos);
    }

    public async Task<Result<MechanicAssignmentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
        var mechanicAssignment = await mechanicAssignmentRepository.GetByIdAsync(id, cancellationToken);

        if (mechanicAssignment is null)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.NotFound);
        }

        return Result<MechanicAssignmentDto>.Success(MapToDto(mechanicAssignment));
    }

    public async Task<Result<MechanicAssignmentDto>> CreateAsync(
        CreateMechanicAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var orderServiceId = request?.OrderServiceId ?? 0;
        var mechanicPersonId = request?.MechanicPersonId ?? 0;
        var specialtyId = request?.SpecialtyId ?? 0;

        var validationError = Validate(orderServiceId, mechanicPersonId, specialtyId);
        if (validationError is not null)
        {
            return Result<MechanicAssignmentDto>.Failure(validationError);
        }

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var orderService = await orderServiceRepository.GetByIdAsync(orderServiceId, cancellationToken);

        if (orderService is null)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.OrderServiceNotFound);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(orderService.ServiceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.OrderServiceNotFound);
        }

        var blockedOrderStatusIds = await GetOrderStatusIdsByNamesAsync(
            new[] { CancelledStatusName, VoidedStatusName },
            cancellationToken);

        if (blockedOrderStatusIds.Contains(serviceOrder.OrderStatusId))
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.OrderServiceCannotBeModifiedConflict);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == mechanicPersonId,
            cancellationToken);

        if (!personExists)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.PersonNotFound);
        }

        var mechanicRoleId = await GetRoleIdByNameAsync(MechanicRoleName, cancellationToken);
        if (!mechanicRoleId.HasValue)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.PersonIsNotMechanicInvalid);
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var hasActiveMechanicRole = await personRoleRepository.ExistsAsync(
            x => x.PersonId == mechanicPersonId && x.RoleId == mechanicRoleId.Value && x.IsActive,
            cancellationToken);

        if (!hasActiveMechanicRole)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.PersonIsNotMechanicInvalid);
        }

        var specialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        var specialtyExists = await specialtyRepository.ExistsAsync(
            x => x.SpecialtyId == specialtyId,
            cancellationToken);

        if (!specialtyExists)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.SpecialtyNotFound);
        }

        var mechanicSpecialtyAssignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var mechanicHasSpecialty = await mechanicSpecialtyAssignmentRepository.ExistsAsync(
            x => x.PersonId == mechanicPersonId && x.SpecialtyId == specialtyId,
            cancellationToken);

        if (!mechanicHasSpecialty)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.MechanicDoesNotHaveSpecialtyInvalid);
        }

        var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
        var duplicateMechanicForOrderService = await mechanicAssignmentRepository.ExistsAsync(
            x => x.OrderServiceId == orderServiceId && x.MechanicPersonId == mechanicPersonId,
            cancellationToken);

        if (duplicateMechanicForOrderService)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.DuplicateMechanicForOrderServiceConflict);
        }

        var mechanicAssignment = new MechanicAssignment
        {
            OrderServiceId = orderServiceId,
            MechanicPersonId = mechanicPersonId,
            SpecialtyId = specialtyId
        };

        await mechanicAssignmentRepository.AddAsync(mechanicAssignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MechanicAssignmentDto>.Success(MapToDto(mechanicAssignment));
    }

    public async Task<Result<MechanicAssignmentDto>> UpdateAsync(
        int id,
        UpdateMechanicAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
        var mechanicAssignment = await mechanicAssignmentRepository.GetByIdAsync(id, cancellationToken);

        if (mechanicAssignment is null)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.NotFound);
        }

        var orderServiceId = request?.OrderServiceId ?? 0;
        var mechanicPersonId = request?.MechanicPersonId ?? 0;
        var specialtyId = request?.SpecialtyId ?? 0;

        var validationError = Validate(orderServiceId, mechanicPersonId, specialtyId);
        if (validationError is not null)
        {
            return Result<MechanicAssignmentDto>.Failure(validationError);
        }

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var orderService = await orderServiceRepository.GetByIdAsync(orderServiceId, cancellationToken);

        if (orderService is null)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.OrderServiceNotFound);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(orderService.ServiceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.OrderServiceNotFound);
        }

        var blockedOrderStatusIds = await GetOrderStatusIdsByNamesAsync(
            new[] { CancelledStatusName, VoidedStatusName },
            cancellationToken);

        if (blockedOrderStatusIds.Contains(serviceOrder.OrderStatusId))
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.OrderServiceCannotBeModifiedConflict);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == mechanicPersonId,
            cancellationToken);

        if (!personExists)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.PersonNotFound);
        }

        var mechanicRoleId = await GetRoleIdByNameAsync(MechanicRoleName, cancellationToken);
        if (!mechanicRoleId.HasValue)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.PersonIsNotMechanicInvalid);
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var hasActiveMechanicRole = await personRoleRepository.ExistsAsync(
            x => x.PersonId == mechanicPersonId && x.RoleId == mechanicRoleId.Value && x.IsActive,
            cancellationToken);

        if (!hasActiveMechanicRole)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.PersonIsNotMechanicInvalid);
        }

        var specialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        var specialtyExists = await specialtyRepository.ExistsAsync(
            x => x.SpecialtyId == specialtyId,
            cancellationToken);

        if (!specialtyExists)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.SpecialtyNotFound);
        }

        var mechanicSpecialtyAssignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var mechanicHasSpecialty = await mechanicSpecialtyAssignmentRepository.ExistsAsync(
            x => x.PersonId == mechanicPersonId && x.SpecialtyId == specialtyId,
            cancellationToken);

        if (!mechanicHasSpecialty)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.MechanicDoesNotHaveSpecialtyInvalid);
        }

        var duplicateMechanicForOrderService = await mechanicAssignmentRepository.ExistsAsync(
            x => x.OrderServiceId == orderServiceId && x.MechanicPersonId == mechanicPersonId && x.MechanicAssignmentId != id,
            cancellationToken);

        if (duplicateMechanicForOrderService)
        {
            return Result<MechanicAssignmentDto>.Failure(MechanicAssignmentErrors.DuplicateMechanicForOrderServiceConflict);
        }

        mechanicAssignment.OrderServiceId = orderServiceId;
        mechanicAssignment.MechanicPersonId = mechanicPersonId;
        mechanicAssignment.SpecialtyId = specialtyId;

        mechanicAssignmentRepository.Update(mechanicAssignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MechanicAssignmentDto>.Success(MapToDto(mechanicAssignment));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
        var mechanicAssignment = await mechanicAssignmentRepository.GetByIdAsync(id, cancellationToken);

        if (mechanicAssignment is null)
        {
            return Result.Failure(MechanicAssignmentErrors.NotFound);
        }

        mechanicAssignmentRepository.Remove(mechanicAssignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static MechanicAssignmentDto MapToDto(MechanicAssignment mechanicAssignment)
    {
        return new MechanicAssignmentDto
        {
            MechanicAssignmentId = mechanicAssignment.MechanicAssignmentId,
            OrderServiceId = mechanicAssignment.OrderServiceId,
            MechanicPersonId = mechanicAssignment.MechanicPersonId,
            SpecialtyId = mechanicAssignment.SpecialtyId
        };
    }

    private static Error? Validate(int orderServiceId, int mechanicPersonId, int specialtyId)
    {
        if (orderServiceId <= 0)
        {
            return MechanicAssignmentErrors.OrderServiceIdInvalid;
        }

        if (mechanicPersonId <= 0)
        {
            return MechanicAssignmentErrors.MechanicPersonIdInvalid;
        }

        if (specialtyId <= 0)
        {
            return MechanicAssignmentErrors.SpecialtyIdInvalid;
        }

        return null;
    }

    private async Task<int[]> GetOrderStatusIdsByNamesAsync(
        IReadOnlyCollection<string> orderStatusNames,
        CancellationToken cancellationToken)
    {
        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();
        var orderStatuses = await orderStatusRepository.GetAllAsync(cancellationToken);
        var orderStatusNameSet = new HashSet<string>(orderStatusNames, StringComparer.OrdinalIgnoreCase);

        return orderStatuses
            .Where(x => orderStatusNameSet.Contains(x.Name))
            .Select(x => x.OrderStatusId)
            .Distinct()
            .ToArray();
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
}
