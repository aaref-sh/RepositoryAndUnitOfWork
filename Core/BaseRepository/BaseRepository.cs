using Core.Entities;
using Core.Exceptions;
using Core.Filters;
using Core.Paginated;
using Helper;
using Helper.Caching;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Core.BaseRepository;

public class BaseRepository<T>(DbContext dbContext) : IBaseRepository<T> where T : class, IBaseEntity
{
    private readonly DbSet<T> set = dbContext.Set<T>();

    public Task<List<T>> FindAll(Expression<Func<T, bool>> exp)
    {
        return set.Where(exp).ToListAsync();
    }

    public async Task<PaginatedList<T>> GetAll(IEnumerable<string> includes, BaseFilter<T>? filter = null)
    {
        filter ??= new();
        if (string.IsNullOrEmpty(filter.SearchQuery))
        {
            var lst = await GetAllCached(includes);
            return filter.ApplyTo(lst);
        }

        var query = set.AsNoTracking();
        foreach (var include in includes) query = query.Include(include);

        return await filter.ApplyTo(query);
    }


    public async Task<List<T>> GetAllCached(IEnumerable<string> includes)
    {
        var cacheKey = $"{typeof(T).Name}CompleteList{includes.JoinStr()}";
        return (await CacheProvider.GetOrSet(cacheKey, async () => await GetAll(includes), typeof(T)))!;
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

    public async Task Update(T entity)
    {
        var entry = await set.FirstOrDefaultAsync(x => x.Id == entity.Id) ?? throw new BaseException(System.Net.HttpStatusCode.NotFound);
        dbContext.Entry(entry).CurrentValues.SetValues(entity);
        CacheProvider.ClearCacheOf(typeof(T));
    }
}
