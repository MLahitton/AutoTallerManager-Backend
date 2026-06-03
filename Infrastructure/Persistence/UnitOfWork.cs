using Application.Common.Interfaces.Persistence;
using Infrastructure.Persistence.Repositories;

namespace Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = [];

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        var entityType = typeof(T);

        if (_repositories.TryGetValue(entityType, out var repository))
        {
            return (IGenericRepository<T>)repository;
        }

        var genericRepository = new GenericRepository<T>(_dbContext);
        _repositories[entityType] = genericRepository;

        return genericRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
