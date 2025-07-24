using AutoMapper;
using Core.DTO;
using Core.Entities;
using Core.Exceptions;
using Core.Filters;
using Core.Paginated;
using Core.UOW;
using Helper;
using System.Collections;

namespace Core.BaseService;

public class BaseService<T>(IUnitOfWork<T> uow, IMapper mapper) : IBaseService<T> where T : class, IBaseEntity
{
    public IUnitOfWork<T> Uow { get; } = uow;

    public virtual async Task<T> GetById(long id)
    {
        return (await Uow.Repository.GetAllCached(Uow.Repository.GetIncludes())).Find(x => x.Id == id)
            ?? throw new BaseException(System.Net.HttpStatusCode.NotFound, "Not found");
    }

    public virtual async Task<PaginatedList<T>> GetList(BaseFilter<T> filter)
    {
        if (filter.SearchQuery.IsNullOrEmpty())
        {
            var all = await Uow.Repository.GetAllCached(Uow.Repository.GetIncludes());
            return filter.ApplyTo(all);
        }
        return await Uow.Repository.GetAll(Uow.Repository.GetIncludes(), filter);
    }

    public virtual async Task Update<TUpdateDto>(TUpdateDto entity) where TUpdateDto : BaseUpdateDto
    {
        await Uow.Repository.Update(entity);
        await Uow.SaveChangesAsync();
    }
    public virtual async Task Create<TCreateDto>(TCreateDto dto) where TCreateDto : BaseCreateDto
    {
        T entity = await BuildEntity(dto);
        await Uow.Repository.Insert(entity);
        await Uow.SaveChangesAsync();
    }

    protected virtual Task<T> BuildEntity<TCreateDto>(TCreateDto createRequest) where TCreateDto : BaseCreateDto
    {
        return BuildEntityOf<T>(createRequest);
    }
    protected virtual async Task<TRes> BuildEntityOf<TRes>(object createRequest)
    {
        var entity = mapper.Map<TRes>(createRequest);

        foreach (var dtoProperty in createRequest.GetType().GetProperties())
        {
            var entityProperty = typeof(TRes).GetProperty(dtoProperty.Name[..^2]);
            if (entityProperty == null || !entityProperty.CanWrite) continue;

            if (dtoProperty.GetValue(createRequest) is List<long> ids && entityProperty.PropertyType != typeof(List<long>))
            {
                var relationType = entityProperty.PropertyType.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(entityProperty.PropertyType)!;
                foreach (var id in ids)
                {
                    var item = (IBaseEntity)Activator.CreateInstance(relationType)!;
                    item.Id = id;
                    uow.DbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                    list.Add(item);
                }
                entityProperty.SetValue(entity, list);
            }
        }

        return entity;
    }

    public virtual async Task Delete(long id)
    {
        try
        {
            var item = await GetById(id);
            await Uow.Repository.Delete(item);
            await Uow.SaveChangesAsync();
        }
        catch { 
            var item = Uow.DbContext.ChangeTracker.Entries<T>().FirstOrDefault(x => x.Entity.Id == id);

            if (item?.State == Microsoft.EntityFrameworkCore.EntityState.Deleted) 
                item.State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
            throw;
        }
    }
}
