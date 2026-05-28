using Application.Common.Results;
using Application.Features.Departments.Dtos;
using Application.Features.Departments.Requests;

namespace Application.Features.Departments;

public interface IDepartmentService
{
    Task<Result<IReadOnlyList<DepartmentDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<DepartmentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<DepartmentDto>> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);

    Task<Result<DepartmentDto>> UpdateAsync(int id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
