using Helper.Caching;
using Helper.Helpers;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using System.Reflection;

namespace Helper;

public static partial class Extentions
{
    public static string GetAcceptLanguageHeader(this IHttpContextAccessor _httpContextAccessor)
    {
        string? lang = _httpContextAccessor.HttpContext?.Request.Headers.AcceptLanguage;
        if (string.IsNullOrEmpty(lang) || lang.Length > 3)
            return "ar";
        return lang;
    }

    public static Func<T, bool> Combine<T>(this IEnumerable<Expression<Func<T, bool>>> value)
    {
        return value.CombineExpressions().Compile();
    }

    public static Expression<Func<T, bool>> CombineExpressions<T>(this IEnumerable<Expression<Func<T, bool>>> expressions) 
        => Utils.CombineExpressions(expressions);

    public static bool In<T>(this T? obj, IEnumerable<T> list) => list.Contains(obj);

    public static string JoinStr(this IEnumerable<object> list, string separator = "") 
        => string.Join(separator, list);

    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? values) => values == null || !values.Any();
    public static void ForEach<T>(this IEnumerable<T>? values, Action<T> action)
    {
        if(values != null) foreach(T val in values) action(val);
    }

    public static bool HasAttribute<TAttribute>(this PropertyInfo prop) where TAttribute : Attribute => Attribute.IsDefined(prop, typeof(TAttribute));

    public static IEnumerable<string> GetIncludes(this Type type) =>
        CacheProvider.GetOrSet($"{type.Name}_includes",
                    () => type.GetProperties().Where(pi => pi.DeclaringType == type && (pi.GetMethod?.IsVirtual ?? false)).Select(x => x.Name).ToArray(),
                    minutes: 1000)!;

    public static bool IsArabic(this string? input)
    {
        foreach (var c in input ?? "")
        {
            if (!char.IsLetter(c)) continue;
            return c is >= '\u0600' and <= '\u06FF' or >= '\u0750' and <= '\u077F'or >= '\u08A0' and <= '\u08FF' or >= '\uFB50' and <= '\uFDFF' or >= '\uFE70' and <= '\uFEFF';
        }
        return false;
    }
}
