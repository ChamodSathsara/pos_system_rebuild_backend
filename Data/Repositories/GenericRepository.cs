using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        if (id is null)
        {
            return null;
        }

        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString = Context.Database.GetConnectionString();
        var databaseName = Context.Database.GetDbConnection().Database;
        var serverName = Context.Database.GetDbConnection().DataSource;

        Console.WriteLine($"Connection string : {connectionString}");
        Console.WriteLine($"Database Name     : {databaseName}");
        Console.WriteLine($"Server Name       : {serverName}");

        var branches = await DbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        Console.WriteLine($"Entity Type       : {typeof(TEntity).Name}");
        Console.WriteLine($"Record Count      : {branches.Count}");

        return branches;
    }

    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(cancellationToken);
    }
}
