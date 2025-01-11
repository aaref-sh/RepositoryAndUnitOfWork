using Core.BaseRepository;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.UOW;

public interface IUnitOfWork<T> : IDisposable where T : class, IBaseEntity
{
    DbContext DbContext { get; }
    IBaseRepository<T> Repository { get; }
    TRepo Get<TRepo>();
    Task UpdateValue(Expression<Func<T, bool>> exp, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls);
    Task<int> SaveChangesAsync();
}
