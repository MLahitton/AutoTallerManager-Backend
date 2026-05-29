using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.MechanicSpecialtyAssignments.Dtos;
using Application.Features.MechanicSpecialtyAssignments.Errors;
using Application.Features.MechanicSpecialtyAssignments.Requests;
using Domain.Entities;

namespace Application.Features.MechanicSpecialtyAssignments;

public class MechanicSpecialtyAssignmentService : IMechanicSpecialtyAssignmentService
{
    private const string MechanicRoleName = "Mechanic";

    private readonly IUnitOfWork _unitOfWork;

    public MechanicSpecialtyAssignmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<MechanicSpecialtyAssignmentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var assignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var assignments = await assignmentRepository.GetAllAsync(cancellationToken);

        var assignmentDtos = assignments
            .OrderBy(x => x.AssignmentId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<MechanicSpecialtyAssignmentDto>>.Success(assignmentDtos);
    }

    public async Task<Result<MechanicSpecialtyAssignmentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var assignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var assignment = await assignmentRepository.GetByIdAsync(id, cancellationToken);

        if (assignment is null)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(MechanicSpecialtyAssignmentErrors.NotFound);
        }

        return Result<MechanicSpecialtyAssignmentDto>.Success(MapToDto(assignment));
    }

    public async Task<Result<MechanicSpecialtyAssignmentDto>> CreateAsync(
        CreateMechanicSpecialtyAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var personId = request?.PersonId ?? 0;
        var specialtyId = request?.SpecialtyId ?? 0;

        var validationError = Validate(personId, specialtyId);
        if (validationError is not null)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(validationError);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(MechanicSpecialtyAssignmentErrors.PersonNotFound);
        }

        var mechanicRoleId = await GetRoleIdByNameAsync(MechanicRoleName, cancellationToken);
        if (!mechanicRoleId.HasValue)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(MechanicSpecialtyAssignmentErrors.PersonIsNotMechanicInvalid);
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var hasActiveMechanicRole = await personRoleRepository.ExistsAsync(
            x => x.PersonId == personId && x.RoleId == mechanicRoleId.Value && x.IsActive,
            cancellationToken);

        if (!hasActiveMechanicRole)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(MechanicSpecialtyAssignmentErrors.PersonIsNotMechanicInvalid);
        }

        var specialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        var specialtyExists = await specialtyRepository.ExistsAsync(
            x => x.SpecialtyId == specialtyId,
            cancellationToken);

        if (!specialtyExists)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(MechanicSpecialtyAssignmentErrors.SpecialtyNotFound);
        }

        var assignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var duplicateAssignment = await assignmentRepository.ExistsAsync(
            x => x.PersonId == personId && x.SpecialtyId == specialtyId,
            cancellationToken);

        if (duplicateAssignment)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(MechanicSpecialtyAssignmentErrors.DuplicateAssignmentConflict);
        }

        var assignment = new MechanicSpecialtyAssignment
        {
            PersonId = personId,
            SpecialtyId = specialtyId
        };

        await assignmentRepository.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MechanicSpecialtyAssignmentDto>.Success(MapToDto(assignment));
    }

    public async Task<Result<MechanicSpecialtyAssignmentDto>> UpdateAsync(
        int id,
        UpdateMechanicSpecialtyAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var assignment = await assignmentRepository.GetByIdAsync(id, cancellationToken);

        if (assignment is null)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(MechanicSpecialtyAssignmentErrors.NotFound);
        }

        var personId = request?.PersonId ?? 0;
        var specialtyId = request?.SpecialtyId ?? 0;

        var validationError = Validate(personId, specialtyId);
        if (validationError is not null)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(validationError);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(MechanicSpecialtyAssignmentErrors.PersonNotFound);
        }

        var mechanicRoleId = await GetRoleIdByNameAsync(MechanicRoleName, cancellationToken);
        if (!mechanicRoleId.HasValue)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(MechanicSpecialtyAssignmentErrors.PersonIsNotMechanicInvalid);
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var hasActiveMechanicRole = await personRoleRepository.ExistsAsync(
            x => x.PersonId == personId && x.RoleId == mechanicRoleId.Value && x.IsActive,
            cancellationToken);

        if (!hasActiveMechanicRole)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(MechanicSpecialtyAssignmentErrors.PersonIsNotMechanicInvalid);
        }

        var specialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        var specialtyExists = await specialtyRepository.ExistsAsync(
            x => x.SpecialtyId == specialtyId,
            cancellationToken);

        if (!specialtyExists)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(MechanicSpecialtyAssignmentErrors.SpecialtyNotFound);
        }

        var duplicateAssignment = await assignmentRepository.ExistsAsync(
            x => x.PersonId == personId && x.SpecialtyId == specialtyId && x.AssignmentId != id,
            cancellationToken);

        if (duplicateAssignment)
        {
            return Result<MechanicSpecialtyAssignmentDto>.Failure(MechanicSpecialtyAssignmentErrors.DuplicateAssignmentConflict);
        }

        assignment.PersonId = personId;
        assignment.SpecialtyId = specialtyId;

        assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MechanicSpecialtyAssignmentDto>.Success(MapToDto(assignment));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var assignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var assignment = await assignmentRepository.GetByIdAsync(id, cancellationToken);

        if (assignment is null)
        {
            return Result.Failure(MechanicSpecialtyAssignmentErrors.NotFound);
        }

        var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
        var inUse = await mechanicAssignmentRepository.ExistsAsync(
            x => x.MechanicPersonId == assignment.PersonId && x.SpecialtyId == assignment.SpecialtyId,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(MechanicSpecialtyAssignmentErrors.InUse);
        }

        assignmentRepository.Remove(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static MechanicSpecialtyAssignmentDto MapToDto(MechanicSpecialtyAssignment assignment)
    {
        return new MechanicSpecialtyAssignmentDto
        {
            AssignmentId = assignment.AssignmentId,
            PersonId = assignment.PersonId,
            SpecialtyId = assignment.SpecialtyId
        };
    }

    private static Error? Validate(int personId, int specialtyId)
    {
        if (personId <= 0)
        {
            return MechanicSpecialtyAssignmentErrors.PersonIdInvalid;
        }

        if (specialtyId <= 0)
        {
            return MechanicSpecialtyAssignmentErrors.SpecialtyIdInvalid;
        }

        return null;
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
