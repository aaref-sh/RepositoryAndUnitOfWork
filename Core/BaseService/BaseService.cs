using Core.Exceptions;
using Core.Entities;
using Core.Filters;
using Core.Paginated;
using Core.UOW;

namespace Core.BaseService;

public class BaseService<T>(IUnitOfWork<T> uow) : IBaseService<T> where T : class, IBaseEntity
{
    public IUnitOfWork<T> Uow { get; } = uow;

    public async Task<T> GetById(long id)
    {
        return (await Uow.Repository.GetAllCached([])).Find(x => x.Id == id)
            ?? throw new BaseException(System.Net.HttpStatusCode.NotFound);
    }

    public Task<PaginatedList<T>> GetList(BaseFilter<T> filter)
    {
        return Uow.Repository.GetAll(Uow.Repository.GetIncludes(), filter);
    }

    public async Task Update(T entity)
    {
        await Uow.Repository.Update(entity);
        await Uow.SaveChangesAsync();
    }
    public async Task Insert(T entity)
    {
        await Uow.Repository.Insert(entity);
        await Uow.SaveChangesAsync();
    }
}
