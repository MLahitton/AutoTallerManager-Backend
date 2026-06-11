// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Application/Features/NewFeature/NewFeatureBusinessService.cs
// Referencia: InventoryBusinessService.CancelPurchaseAsync
//             InvoiceBusinessService.GenerateFromServiceOrderAsync
//             PaymentBusinessService.RecordPaymentAsync
//             ClientApprovalService.ApproveOrderServiceAsync

using Application.Common.Auditing;
using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.NewFeature.Dtos;
using Application.Features.NewFeature.Errors;
using Application.Features.NewFeature.Requests;
using Domain.Entities;

namespace Application.Features.NewFeature;

public class NewFeatureBusinessService : INewFeatureBusinessService
{
    private const int ReasonMaxLength = 500;
    private const string UpdateAuditActionTypeName = "UPDATE";
    private const string CancelAuditActionTypeName = "CANCEL";
    private const string EntityName = "NewEntity";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;

    public NewFeatureBusinessService(IUnitOfWork unitOfWork, IAuditLogger auditLogger)
    {
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
    }

    public async Task<Result<NewFeatureActionResultDto>> ExecuteActionAsync(
        int newEntityId,
        ExecuteNewFeatureActionRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        // --- Validación de parámetros ---
        if (newEntityId <= 0)
        {
            return Result<NewFeatureActionResultDto>.Failure(NewFeatureErrors.NewEntityIdInvalid);
        }

        if (currentUserId <= 0)
        {
            return Result<NewFeatureActionResultDto>.Failure(NewFeatureErrors.CurrentUserInvalid);
        }

        var reason = NormalizeOptionalText(request?.Reason);
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<NewFeatureActionResultDto>.Failure(NewFeatureErrors.ReasonRequired);
        }

        if (reason.Length > ReasonMaxLength)
        {
            return Result<NewFeatureActionResultDto>.Failure(NewFeatureErrors.ReasonRequired); // Crear error TooLong si aplica
        }

        // --- Cargar entidad ---
        var repository = _unitOfWork.Repository<NewEntity>();
        var entity = await repository.GetByIdAsync(newEntityId, cancellationToken);

        if (entity is null)
        {
            return Result<NewFeatureActionResultDto>.Failure(NewFeatureErrors.NewEntityNotFound);
        }

        // --- Reglas de negocio (ejemplo: ya procesado) ---
        // if (entity.IsProcessed)
        // {
        //     return Result<NewFeatureActionResultDto>.Failure(NewFeatureErrors.AlreadyProcessedConflict);
        // }

        // --- Transacción si afecta más de una tabla ---
        // Referencia: InventoryBusinessService.RegisterPurchaseAsync
        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            // Mutar entidad(es)
            // entity.Status = "...";
            // repository.Update(entity);

            // Si afecta otras tablas (stock, historial, etc.):
            // var otherRepo = _unitOfWork.Repository<OtherEntity>();
            // otherRepo.Update(...);

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            // Auditoría (segundo SaveChanges después del LogAsync)
            await _auditLogger.LogAsync(
                currentUserId,
                CancelAuditActionTypeName,  // o UPDATE, CREATE según la acción
                EntityName,
                entity.NewEntityId,
                $"Action executed on {EntityName} {entity.NewEntityId}. Reason: {reason}",
                transactionCancellationToken);

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return Result<NewFeatureActionResultDto>.Success(new NewFeatureActionResultDto
            {
                NewEntityId = entity.NewEntityId,
                Status = "Processed",
                ActionDate = DateTime.UtcNow
            });
        }, cancellationToken);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}

// Interfaz INewFeatureBusinessService con la firma del método anterior.
// Registrar: services.AddScoped<INewFeatureBusinessService, NewFeatureBusinessService>();
