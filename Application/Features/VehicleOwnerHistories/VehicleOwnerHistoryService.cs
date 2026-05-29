using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.VehicleOwnerHistories.Dtos;
using Application.Features.VehicleOwnerHistories.Errors;
using Application.Features.VehicleOwnerHistories.Requests;
using Domain.Entities;

namespace Application.Features.VehicleOwnerHistories;

public class VehicleOwnerHistoryService : IVehicleOwnerHistoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public VehicleOwnerHistoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<VehicleOwnerHistoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vehicleOwnerHistoryRepository = _unitOfWork.Repository<VehicleOwnerHistory>();
        var vehicleOwnerHistories = await vehicleOwnerHistoryRepository.GetAllAsync(cancellationToken);

        var vehicleOwnerHistoryDtos = vehicleOwnerHistories
            .OrderBy(x => x.VehicleOwnerHistoryId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<VehicleOwnerHistoryDto>>.Success(vehicleOwnerHistoryDtos);
    }

    public async Task<Result<VehicleOwnerHistoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicleOwnerHistoryRepository = _unitOfWork.Repository<VehicleOwnerHistory>();
        var vehicleOwnerHistory = await vehicleOwnerHistoryRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleOwnerHistory is null)
        {
            return Result<VehicleOwnerHistoryDto>.Failure(VehicleOwnerHistoryErrors.NotFound);
        }

        return Result<VehicleOwnerHistoryDto>.Success(MapToDto(vehicleOwnerHistory));
    }

    public async Task<Result<VehicleOwnerHistoryDto>> CreateAsync(
        CreateVehicleOwnerHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var vehicleId = request?.VehicleId ?? 0;
        var personId = request?.PersonId ?? 0;
        var startDate = NormalizeStartDate(request?.StartDate ?? default);
        var endDate = NormalizeEndDate(request?.EndDate);

        var validationError = Validate(vehicleId, personId, startDate, endDate);
        if (validationError is not null)
        {
            return Result<VehicleOwnerHistoryDto>.Failure(validationError);
        }

        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vehicleExists = await vehicleRepository.ExistsAsync(
            x => x.VehicleId == vehicleId,
            cancellationToken);

        if (!vehicleExists)
        {
            return Result<VehicleOwnerHistoryDto>.Failure(VehicleOwnerHistoryErrors.VehicleNotFound);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return Result<VehicleOwnerHistoryDto>.Failure(VehicleOwnerHistoryErrors.PersonNotFound);
        }

        var vehicleOwnerHistoryRepository = _unitOfWork.Repository<VehicleOwnerHistory>();

        if (!endDate.HasValue)
        {
            var currentOwnerAlreadyExists = await vehicleOwnerHistoryRepository.ExistsAsync(
                x => x.VehicleId == vehicleId && x.EndDate == null,
                cancellationToken);

            if (currentOwnerAlreadyExists)
            {
                return Result<VehicleOwnerHistoryDto>.Failure(VehicleOwnerHistoryErrors.CurrentOwnerAlreadyExists);
            }
        }

        var relationAlreadyExists = await vehicleOwnerHistoryRepository.ExistsAsync(
            x => x.VehicleId == vehicleId && x.PersonId == personId && x.StartDate == startDate,
            cancellationToken);

        if (relationAlreadyExists)
        {
            return Result<VehicleOwnerHistoryDto>.Failure(VehicleOwnerHistoryErrors.RelationAlreadyExists);
        }

        var vehicleOwnerHistory = new VehicleOwnerHistory
        {
            VehicleId = vehicleId,
            PersonId = personId,
            StartDate = startDate,
            EndDate = endDate
        };

        await vehicleOwnerHistoryRepository.AddAsync(vehicleOwnerHistory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleOwnerHistoryDto>.Success(MapToDto(vehicleOwnerHistory));
    }

    public async Task<Result<VehicleOwnerHistoryDto>> UpdateAsync(
        int id,
        UpdateVehicleOwnerHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var vehicleOwnerHistoryRepository = _unitOfWork.Repository<VehicleOwnerHistory>();
        var vehicleOwnerHistory = await vehicleOwnerHistoryRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleOwnerHistory is null)
        {
            return Result<VehicleOwnerHistoryDto>.Failure(VehicleOwnerHistoryErrors.NotFound);
        }

        var vehicleId = request?.VehicleId ?? 0;
        var personId = request?.PersonId ?? 0;
        var startDate = NormalizeStartDate(request?.StartDate ?? default);
        var endDate = NormalizeEndDate(request?.EndDate);

        var validationError = Validate(vehicleId, personId, startDate, endDate);
        if (validationError is not null)
        {
            return Result<VehicleOwnerHistoryDto>.Failure(validationError);
        }

        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vehicleExists = await vehicleRepository.ExistsAsync(
            x => x.VehicleId == vehicleId,
            cancellationToken);

        if (!vehicleExists)
        {
            return Result<VehicleOwnerHistoryDto>.Failure(VehicleOwnerHistoryErrors.VehicleNotFound);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return Result<VehicleOwnerHistoryDto>.Failure(VehicleOwnerHistoryErrors.PersonNotFound);
        }

        if (!endDate.HasValue)
        {
            var currentOwnerAlreadyExists = await vehicleOwnerHistoryRepository.ExistsAsync(
                x => x.VehicleId == vehicleId && x.EndDate == null && x.VehicleOwnerHistoryId != id,
                cancellationToken);

            if (currentOwnerAlreadyExists)
            {
                return Result<VehicleOwnerHistoryDto>.Failure(VehicleOwnerHistoryErrors.CurrentOwnerAlreadyExists);
            }
        }

        var relationAlreadyExists = await vehicleOwnerHistoryRepository.ExistsAsync(
            x => x.VehicleId == vehicleId && x.PersonId == personId && x.StartDate == startDate && x.VehicleOwnerHistoryId != id,
            cancellationToken);

        if (relationAlreadyExists)
        {
            return Result<VehicleOwnerHistoryDto>.Failure(VehicleOwnerHistoryErrors.RelationAlreadyExists);
        }

        vehicleOwnerHistory.VehicleId = vehicleId;
        vehicleOwnerHistory.PersonId = personId;
        vehicleOwnerHistory.StartDate = startDate;
        vehicleOwnerHistory.EndDate = endDate;

        vehicleOwnerHistoryRepository.Update(vehicleOwnerHistory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleOwnerHistoryDto>.Success(MapToDto(vehicleOwnerHistory));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicleOwnerHistoryRepository = _unitOfWork.Repository<VehicleOwnerHistory>();
        var vehicleOwnerHistory = await vehicleOwnerHistoryRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleOwnerHistory is null)
        {
            return Result.Failure(VehicleOwnerHistoryErrors.NotFound);
        }

        vehicleOwnerHistoryRepository.Remove(vehicleOwnerHistory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static VehicleOwnerHistoryDto MapToDto(VehicleOwnerHistory vehicleOwnerHistory)
    {
        return new VehicleOwnerHistoryDto
        {
            VehicleOwnerHistoryId = vehicleOwnerHistory.VehicleOwnerHistoryId,
            VehicleId = vehicleOwnerHistory.VehicleId,
            PersonId = vehicleOwnerHistory.PersonId,
            StartDate = vehicleOwnerHistory.StartDate,
            EndDate = vehicleOwnerHistory.EndDate
        };
    }

    private static DateTime NormalizeStartDate(DateTime startDate)
    {
        return startDate.Date;
    }

    private static DateTime? NormalizeEndDate(DateTime? endDate)
    {
        return endDate?.Date;
    }

    private static Error? Validate(int vehicleId, int personId, DateTime startDate, DateTime? endDate)
    {
        if (vehicleId <= 0)
        {
            return VehicleOwnerHistoryErrors.VehicleIdInvalid;
        }

        if (personId <= 0)
        {
            return VehicleOwnerHistoryErrors.PersonIdInvalid;
        }

        if (startDate == default || startDate > DateTime.UtcNow.Date)
        {
            return VehicleOwnerHistoryErrors.StartDateInvalid;
        }

        if (endDate.HasValue && endDate.Value < startDate)
        {
            return VehicleOwnerHistoryErrors.EndDateInvalid;
        }

        return null;
    }
}
