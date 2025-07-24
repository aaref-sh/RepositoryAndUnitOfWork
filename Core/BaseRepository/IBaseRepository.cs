using Core.DTO;
using Core.Entities;
using Core.Filters;
using Core.Paginated;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.BaseRepository;

public interface IBaseRepository<T> where T : class, IBaseEntity
{
    Task<T?> GetById(long id);
    Task<PaginatedList<T>> GetAll(IEnumerable<string> includes, BaseFilter<T>? filter = null);
    Task<List<T>> GetAllCached(IEnumerable<string> includes);
    Task<List<T>> FindAll(Expression<Func<T, bool>> exp);
    Task Insert(T entity);
    Task Insert(IEnumerable<T> entities);
    Task Update<TUpdateDto>(TUpdateDto entity) where TUpdateDto : BaseUpdateDto;
    Task UpdateValue(Expression<Func<T, bool>> exp, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls);
    Task Update(long id, object entity);
    IEnumerable<string> GetIncludes();
    Task Delete(T item);
    Task<T?> FirstOrDefault(Expression<Func<T, bool>> exp, IEnumerable<string>? includes = null, Expression<Func<T, object>>? orderKey = null);
    Task<T?> LastOrDefault(Expression<Func<T, bool>> exp, IEnumerable<string>? includes = null, Expression<Func<T, object>>? orderKey = null);

}