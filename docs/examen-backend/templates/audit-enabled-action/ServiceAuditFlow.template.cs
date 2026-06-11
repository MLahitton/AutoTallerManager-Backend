// PLANTILLA DE ESTUDIO — NO COMPILAR
// Flujo completo: mutación + auditoría + transacción
// Referencia: Application/Features/Suppliers/SupplierService.cs (CreateAsync)
//             Application/Features/InventoryBusiness/InventoryBusinessService.cs (CancelPurchaseAsync)

using Application.Common.Auditing;
using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Domain.Entities;

namespace Application.Features.Example;

public class ExampleAuditedService
{
    private const string CreateAuditActionTypeName = "CREATE";
    private const string EntityName = "NewEntity";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;

    public ExampleAuditedService(IUnitOfWork unitOfWork, IAuditLogger auditLogger)
    {
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
    }

    public async Task<Result<int>> CreateWithAuditAsync(
        string name,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<NewEntity>();
        var entity = new NewEntity { Name = name };

        // ExecuteInTransactionAsync asegura atomicidad negocio + auditoría
        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await repository.AddAsync(entity, transactionCancellationToken);

            // Primer SaveChanges: obtiene el Id generado
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            await _auditLogger.LogAsync(
                currentUserId,
                CreateAuditActionTypeName,
                EntityName,
                entity.NewEntityId,
                $"{EntityName} {entity.NewEntityId} created.",
                transactionCancellationToken);

            // Segundo SaveChanges: persiste Audit
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return Result<int>.Success(entity.NewEntityId);
        }, cancellationToken);
    }
}

// currentUserId viene del controller:
//   User.FindFirstValue("userId") → int
// Ver SuppliersController.TryGetCurrentUserId
