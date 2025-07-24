using Core.BaseRepository;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.UOW;

public interface IUnitOfWork : IDisposable
{
    DbContext DbContext { get; }
    IBaseRepository<TEntity> Repo<TEntity>() where TEntity : class, IBaseEntity;
    TRepo Get<TRepo>();
    Task<int> SaveChangesAsync();
}

public interface IUnitOfWork<T> : IUnitOfWork where T : class, IBaseEntity
{
    IBaseRepository<T> Repository { get; }
    Task UpdateValue(Expression<Func<T, bool>> exp, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls);
}
