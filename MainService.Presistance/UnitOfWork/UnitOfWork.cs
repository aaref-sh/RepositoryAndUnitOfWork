using Core.BaseRepository;
using Core.Entities;
using Core.UOW;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MainService.Presistance.Repository;
using System.Linq.Expressions;

namespace MainService.Presistance.UnitOfWork;


public class UnitOfWork(
    DbContext dbContext, 
    RoleRepository roleRepository,
    UserRepository userRepository
    ) : IUnitOfWork
{

    private readonly Dictionary<Type, object> repos = new()
    {
        { typeof(RoleRepository), roleRepository },
        { typeof(UserRepository), userRepository },
    };

    public DbContext DbContext => dbContext;

    public void Dispose()
    {
        DbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    public TRepo Get<TRepo>() => (TRepo)repos[typeof(TRepo)];

    public async Task<int> SaveChangesAsync()
    {
        return await dbContext.SaveChangesAsync();
    }

    private readonly Dictionary<Type, object> repoOf = [];
    public IBaseRepository<TEntity> Repo<TEntity>() where TEntity : class, IBaseEntity
    {
        if(!repoOf.ContainsKey(typeof(TEntity)))
        {
            repoOf.Add(typeof(TEntity), new BaseRepository<TEntity>(DbContext));
        }
        return (IBaseRepository<TEntity>)repoOf[typeof(TEntity)];
    }
}

public class UnitOfWork<T>(DbContext dbContext, 
    RoleRepository roleRepository,
    UserRepository userRepository
    ) : UnitOfWork(dbContext, roleRepository, userRepository), IUnitOfWork<T> where T : class, IBaseEntity
{
    public IBaseRepository<T> Repository => new BaseRepository<T>(DbContext);

    public Task UpdateValue(Expression<Func<T, bool>> exp, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls)
    {
        return dbContext.Set<T>().Where(exp).ExecuteUpdateAsync(setPropertyCalls);
    }

}
