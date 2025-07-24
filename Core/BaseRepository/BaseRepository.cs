using AutoMapper.Internal;
using Core.DTO;
using Core.Entities;
using Core.Exceptions;
using Core.Filters;
using Core.Paginated;
using Helper;
using Helper.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using System.Linq.Expressions;
using System.Net;

namespace Core.BaseRepository;

public class BaseRepository<T>(DbContext dbContext) : IBaseRepository<T> where T : class, IBaseEntity
{
    private readonly DbSet<T> set = dbContext.Set<T>();

    public Task<List<T>> FindAll(Expression<Func<T, bool>> exp)
    {
        return set.AsSplitQuery().Where(exp).OrderBy(x => x.Id).ToListAsync();
    }

    public Task<PaginatedList<T>> GetAll(IEnumerable<string> includes, BaseFilter<T>? filter = null)
    {
        filter ??= new();
        var query = set.AsNoTracking().AsSplitQuery();
        foreach (var include in includes) query = query.Include(include);

        return filter.ApplyTo(query);
    }


    public async Task<List<T>> GetAllCached(IEnumerable<string> includes)
    {
        var cacheKey = $"{typeof(T).Name}CompleteList{includes.JoinStr()}";
        return (await CacheProvider.GetOrSet(cacheKey, async () =>
        {
            var query = set.AsNoTrackingWithIdentityResolution();
            foreach (var include in includes) query = query.Include(include);
            return await query.ToListAsync();
        }, typeof(T)))!;
    }

    public async Task<T?> GetById(long id)
    {
        return await set.FindAsync(id);
    }

    public async Task Insert(T entity)
    {
        await set.AddAsync(entity);
        CacheProvider.ClearCacheOf(typeof(T));
    }
    public IEnumerable<string> GetIncludes() =>
        CacheProvider.GetOrSet($"{typeof(T).Name}_includes",
                    () => typeof(T).GetProperties().Where(pi => pi.DeclaringType == (Type?)typeof(T) && (pi.GetMethod?.IsVirtual ?? false)).Select(x => x.Name).ToArray(),
                    minutes: 1000)!;

    public async Task Insert(IEnumerable<T> entities)
    {
        await set.AddRangeAsync(entities);
        CacheProvider.ClearCacheOf(typeof(T));
    }

    public Task UpdateValue(Expression<Func<T, bool>> exp, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls)
    {
        return set.Where(exp).ExecuteUpdateAsync(setPropertyCalls);
    }

    public async Task Update(long id, object entity)
    {
        var entry = await set.FirstOrDefaultAsync(x => x.Id == id) ?? throw new BaseException(HttpStatusCode.NotFound);
        dbContext.Entry(entry).CurrentValues.SetValues(entity);
        await UpdateNavigations(entity, entry);
        CacheProvider.ClearCacheOf(typeof(T));
    }

    public async Task Update<TUpdateDto>(TUpdateDto dto) where TUpdateDto : BaseUpdateDto
    {
        var entity = await set.FirstOrDefaultAsync(e => e.Id == dto.Id) ?? throw new BaseException(HttpStatusCode.NotFound);
        dbContext.Entry(entity).CurrentValues.SetValues(dto!);
        await UpdateNavigations(dto, entity);
        CacheProvider.ClearCacheOf(typeof(T));
    }

    private async Task UpdateNavigations(object dto, T entity)
    {
        foreach (var nav in typeof(T).GetProperties().Where(p => p.PropertyType.IsCollection()))
        {
            var dtoProp = dto.GetType().GetProperty($"{nav.Name}Id");
            if (!(dtoProp?.PropertyType.IsAssignableTo(typeof(IEnumerable<long>)) ?? false)) continue;
            var ids = ((IEnumerable<long>)dtoProp.GetValue(dto)!).Distinct().ToHashSet();

            await dbContext.Entry(entity).Collection(nav.Name).LoadAsync();
            var currentList = (IList)nav.GetValue(entity)!;

            for (int i = 0; i < currentList.Count; i++)
            {
                var itemId = ((IBaseEntity)currentList[i]!).Id;
                if (!ids.Contains(itemId)) currentList.RemoveAt(i--);
                ids.Remove(itemId);
            }

            var childType = nav.PropertyType.GetGenericArguments()[0];
            var method = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!.MakeGenericMethod(childType);
            var typedSet = method.Invoke(dbContext, null);
            
            foreach(var id in ids)
            {
                var item = (IBaseEntity)Activator.CreateInstance(childType)!;
                item.Id = id;
                dbContext.Entry(item).State = EntityState.Unchanged;
                currentList.Add(item);
            }
        }
    }

    public Task Delete(T item)
    {
        set.Remove(item);
        CacheProvider.ClearCacheOf(typeof(T));
        return Task.CompletedTask;
    }

    public Task<T?> FirstOrDefault(Expression<Func<T, bool>> exp, IEnumerable<string>? includes, Expression<Func<T, object>>? orderKey = null)
    {
        var query = set.AsQueryable();
        foreach(var item in includes ?? []) query = query.Include(item);
        query = orderKey is null ? query.OrderBy(x => x.Id) : query.OrderBy(orderKey);
        return query.FirstOrDefaultAsync(exp);
    }

    public Task<T?> LastOrDefault(Expression<Func<T, bool>> exp, IEnumerable<string>? includes, Expression<Func<T, object>>? orderKey = null)
    {
        var query = set.AsQueryable();
        foreach(var item in includes ?? []) query = query.Include(item);
        query = orderKey is null ? query.OrderBy(x => x.Id) : query.OrderBy(orderKey);
        return query.LastOrDefaultAsync(exp);
    }

}
