using Core.Entities;
using Core.Filters;
using Core.Paginated;
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
    Task Update(T entity);
    IEnumerable<string> GetIncludes();
}