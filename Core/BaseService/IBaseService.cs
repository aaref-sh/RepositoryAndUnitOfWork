using Core.DTO;
using Core.Entities;
using Core.Filters;
using Core.Paginated;

namespace Core.BaseService;

public interface IBaseService<T> where T : IBaseEntity
{
    Task<T> GetById(long id);
    Task<PaginatedList<T>> GetList(BaseFilter<T> filter);
    Task Update<TUpdateDto>(TUpdateDto entity) where TUpdateDto : BaseUpdateDto;
    Task Create<TCreateDto>(TCreateDto dto) where TCreateDto : BaseCreateDto;
    Task Delete(long id);
}
