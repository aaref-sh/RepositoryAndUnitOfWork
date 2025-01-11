using Core.LocalizedProberty;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Core.BaseRepository;

public static class ExpressionHelper
{
    public static Expression<Func<T, bool>> GetExpressionsOfType<T>(string searchTerm)
    {
        var expressions = new List<Expression<Func<T, bool>>>();
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedExpression = null;

        foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            List<Expression>? propertyExpressions = null;

            if (property.PropertyType == typeof(string))
            {
                var propertyAccess = Expression.Property(parameter, property);
                var likeExpression = BuildILikeExpression(propertyAccess, searchTerm);
                expressions.Add(Expression.Lambda<Func<T, bool>>(likeExpression, parameter));
            }
            else if (property.PropertyType == typeof(LocalizedProperty))
            {
                var propertyAccess = Expression.Property(parameter, property);
                propertyExpressions = BuildJsonbILikeExpression(propertyAccess, searchTerm);
            }

            if (propertyExpressions?.Count > 0)
            {
                foreach (var expr in propertyExpressions)
                {
                    combinedExpression = combinedExpression == null ? expr : Expression.OrElse(combinedExpression, expr);
                }
            }
        }

        if (combinedExpression == null) return x => true;

        return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
    }

    private static MethodCallExpression BuildILikeExpression(Expression propertyAccess, string searchTerm)
    {
        //  EF.Functions.ILike(x.Property, $"%{searchTerm}%")
        var likeMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod("ILike",
            BindingFlags.Static | BindingFlags.Public,
            null,
            [typeof(DbFunctions), typeof(string), typeof(string)],
            null)!;

        var dbFunctions = Expression.Constant(EF.Functions);
        var likePattern = Expression.Constant($"%{searchTerm}%");
        return Expression.Call(null, likeMethod, dbFunctions, propertyAccess, likePattern);
    }

    private static List<Expression> BuildJsonbILikeExpression(Expression propertyAccess, string searchTerm)
    {
        List<Expression> res = [];
        foreach (var lang in new[] { "ar", "en" })
        {
            //  EF.Functions.ILike(CustomDbFunctions.JsonbGetter(x.Property, lang), $"%{searchTerm}%")
            var jsonbGetterMethod = typeof(CustomDbFunctions).GetMethod(nameof(CustomDbFunctions.JsonbGetter));
            if (jsonbGetterMethod == null) continue;

            var langExpression = Expression.Constant(lang);
            var jsonbGetterCall = Expression.Call(null, jsonbGetterMethod, propertyAccess, langExpression);

            res.Add(BuildILikeExpression(jsonbGetterCall, searchTerm));
        }
        return res;
    }
}