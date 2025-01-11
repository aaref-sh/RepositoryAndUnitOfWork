using Core.Entities;
using Core.Filters;
using Core.Paginated;

namespace Core.BaseService;

public interface IBaseService<T> where T : IBaseEntity
{
    Task<T> GetById(long id);
    Task<PaginatedList<T>> GetList(BaseFilter<T> filter);
    Task Update(T entity);
    Task Insert(T entity);
}
