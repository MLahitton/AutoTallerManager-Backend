using Application.Common.Results;
using Application.Features.parte_nueva.Dtos;
using Application.Features.parte_nueva.Requests;

namespace Application.Features.parte_nueva;

public interface Iparte_nuevaService
{
    Task<Result<IReadOnlyList<parte_nuevaDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<parte_nuevaDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<parte_nuevaDto>> CreateAsync(Createparte_nuevaRequest request, CancellationToken cancellationToken = default);

    Task<Result<parte_nuevaDto>> UpdateAsync(int id, Updateparte_nuevaRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
