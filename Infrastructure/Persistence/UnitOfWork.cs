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
}
