using Helper.Helpers;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

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
}
