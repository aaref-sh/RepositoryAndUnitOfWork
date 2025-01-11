using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace Helper.Caching;

public class CacheProvider
{
    private static readonly Lazy<IMemoryCache> _cache = new(() => new MemoryCache(new MemoryCacheOptions()));
    private static readonly ConcurrentDictionary<string, HashSet<string>> CacheGroups = new();

    /// <summary>
    /// The related cache groups for this entity, for example: Users depends on Roles, and Roles depends on permissions, <br/>
    /// so if we changed the permissions we must clear its cache group with both Roles and Users
    /// </summary>
    private static readonly ReadOnlyDictionary<string, List<string>> RelatedCacheGroups = new(new Dictionary<string, List<string>>() { });

    public static IMemoryCache Instance => _cache.Value;


    /// <summary>
    /// Get (or set if not exist then get) the value from cache, <br/> make sure to not use lazy or future values (IQueriable for example)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">the cache key , must be unique per category</param>
    /// <param name="cacheBuilderFunction">The function that returns the wanted result if not cached , must returns an evaluated value </param>
    /// <param name="cacheDuration">The time before delete the value from memory (In hours) </param>
    /// <returns></returns>
    public static T? GetOrSet<T>(string key, Func<T> cacheBuilderFunction, Type? cacheType = null, int minutes = 45)
    {
        var cacheGroup = CacheGroupName(cacheType);
        // key = BitConverter.ToString(MD5.HashData(Encoding.ASCII.GetBytes(key)));
        if (!Instance.TryGetValue(key, out T? cacheEntry))
        {
            // Item not in cache, so create it
            cacheEntry = cacheBuilderFunction();

            // Save item in cache
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                        .SetSlidingExpiration(TimeSpan.FromMinutes(minutes))
                                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(minutes + minutes / 3));

            Instance.Set(key, cacheEntry, cacheEntryOptions);
            if (!string.IsNullOrEmpty(cacheGroup))
                CacheGroups.AddOrUpdate(cacheGroup, [key], (k, v) => { v.Add(key); return v; });
        }
        return cacheEntry;
    }
    // async
    public static async Task<T?> GetOrSet<T>(string key, Func<Task<T>> cacheBuilderFunction, Type? cacheType = null, int minutes = 45)
    {
        var cacheGroup = CacheGroupName(cacheType);
        // key = BitConverter.ToString(MD5.HashData(Encoding.ASCII.GetBytes(key)));
        if (!Instance.TryGetValue(key, out T? cacheEntry))
        {
            // Item not in cache, so create it
            cacheEntry = await cacheBuilderFunction();

            // Save item in cache
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                        .SetSlidingExpiration(TimeSpan.FromMinutes(minutes))
                                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(minutes + minutes / 3));

            Instance.Set(key, cacheEntry, cacheEntryOptions);
            if (!string.IsNullOrEmpty(cacheGroup))
                CacheGroups.AddOrUpdate(cacheGroup, [key], (k, v) => { v.Add(key); return v; });
        }
        return cacheEntry;
    }

    /// <summary>
    /// Delete the data from memory
    /// </summary>
    /// <param name="key">The key that will be deleted</param>
    public static void ClearCacheKey(string key)
    {
        Instance.Remove(key);
    }

    /// <summary>
    /// Delete group of data from memory
    /// </summary>
    /// <param name="group">The group that will be deleted</param>
    public static void ClearCacheGroup(string group)
    {
        if (CacheGroups.TryGetValue(group, out var cacheGroup))
        {
            foreach (var key in cacheGroup)
            {
                ClearCacheKey(key);
            }
            CacheGroups.TryRemove(group, out _);
        }
    }

    /// <summary>
    /// Wipe the entire cached data
    /// </summary>
    public static void ClearCache()
    {
        foreach (var group in CacheGroups.Keys)
        {
            ClearCacheGroup(group);
        }
    }

    /// <summary>
    /// Clear the cache on an entity with all of its dependency
    /// </summary>
    /// <param name="entityName">The entity name</param>
    public static void ClearCacheOf(Type cacheType)
    {
        var entityName = CacheGroupName(cacheType);
        ClearCacheGroup(entityName);
        ClearRelations(cacheType);
    }

    public static void ClearRelations(Type cacheType)
    {
        var entityName = CacheGroupName(cacheType);
        // clear the cache of entity dependencies
        if (RelatedCacheGroups.TryGetValue(entityName, out var relatedCacheGroups))
        {
            foreach (var relatedCacheGroup in relatedCacheGroups)
            {
                ClearCacheGroup(relatedCacheGroup);
            }
        }
    }

    /// <summary>
    /// Delete an item from cache by its ID
    /// </summary>
    /// <param name="cacheType">Cache groups to look in its Keys</param>
    /// <param name="itemId">Id of deleted item to be removed from cache</param>
    static void DeleteFromCachedList(Type cacheType, long itemId)
    {
        var entityName = CacheGroupName(cacheType);
        if (CacheGroups.TryGetValue(entityName, out var keys))
        {
            foreach (var key in keys)
            {
                try
                {
                    var list = (IList?)GetCachedValue(key);
                    if (list == null) continue;
                    foreach (var item in list)
                        if (((dynamic?)item)?.Id == itemId) { list.Remove(item); break; }
                }
                catch
                {
                    // clear cacheKey if updating cache failed
                    ClearCacheKey(key);
                }
            }
        }
        ClearRelations(cacheType);
    }
    /// <summary>
    /// Update an item from cache by its ID
    /// </summary>
    /// <param name="cacheType">Cache groups to look in its Keys</param>
    /// <param name="item">Id of changed item to be updated in cache</param>
    public static void UpdateFromCachedList(Type cacheType, object item)
    {
        string entityName = CacheGroupName(cacheType);
        if (CacheGroups.TryGetValue(entityName, out var keys))
        {
            foreach (var key in keys)
            {
                try
                {
                    var list = (IList?)GetCachedValue(key);
                    if (list == null) continue;
                    for (int i = 0; i < list.Count; i++)
                        if (((dynamic?)list[i])?.Id == ((dynamic?)item)?.Id) { list[i] = item; break; }
                }
                catch
                {
                    // clear cacheKey if updating cache failed
                    ClearCacheKey(key);
                }
            }
        }
        // uncomment in case you need to update reload relations cache when updating entity
        //ClearRelations(cacheType);
    }

    /// <summary>
    /// Delete List (probably one) of entities from cache 
    /// </summary>
    /// <param name="cacheType">Cache groups to look in its Keys</param>
    /// <param name="itemIds">List of long represents the IDs of deleted items to be removed from cache</param>
    public static void DeleteListFromCacheList(Type cacheType, List<long> itemIds)
    {
        foreach (var id in itemIds)
            DeleteFromCachedList(cacheType, id);
    }


    public static object? GetCachedValue(string key)
    {
        Instance.TryGetValue(key, out var cacheEntry);
        return cacheEntry;
    }

    public static void SetCachedValue(string key, string value) => Instance.Set(key, value);
    static string CacheGroupName(Type? cacheGroup) => cacheGroup?.Name ?? "";
}
