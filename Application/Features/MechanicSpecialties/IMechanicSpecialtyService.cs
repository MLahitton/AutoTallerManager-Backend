using Application.Common.Results;
using Application.Features.MechanicSpecialties.Dtos;
using Application.Features.MechanicSpecialties.Requests;

namespace Application.Features.MechanicSpecialties;

public interface IMechanicSpecialtyService
{
    Task<Result<IReadOnlyList<MechanicSpecialtyDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<MechanicSpecialtyDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<MechanicSpecialtyDto>> CreateAsync(CreateMechanicSpecialtyRequest request, CancellationToken cancellationToken = default);

    Task<Result<MechanicSpecialtyDto>> UpdateAsync(int id, UpdateMechanicSpecialtyRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
