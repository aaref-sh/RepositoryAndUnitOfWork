using Core.LocalizedProberty;
using Helper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Core.BaseRepository;

public static class ExpressionHelper
{
    public static Expression<Func<T, bool>> GetExpressionsOfType<T>(string searchTerm)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedExpression = null;
        List<Expression> propertyExpressions = [];

        foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.HasAttribute<NotMappedAttribute>()) continue;
            var propertyAccess = Expression.Property(parameter, property);

            if (property.PropertyType == typeof(string))
            {
                propertyExpressions.Add(BuildLikeExpression(propertyAccess, searchTerm));
            }
            else if (property.PropertyType == typeof(LocalizedProperty))
            {
                propertyExpressions.AddRange( BuildJsonbILikeExpression(propertyAccess, searchTerm));
            }
        }

        foreach (var expr in propertyExpressions ?? [])
        {
            combinedExpression = combinedExpression == null ? expr : Expression.OrElse(combinedExpression, expr);
        }

        if (combinedExpression == null) return x => true;

        return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
    }

    private static MethodCallExpression BuildLikeExpression(Expression propertyAccess, string searchTerm)
    {
        // Create expressions for lowercasing the property and the search term
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;

        // Convert the propertyAccess to lowercase
        var lowerPropertyAccess = Expression.Call(propertyAccess, toLowerMethod);

        // Convert the searchTerm to lowercase and prepare the pattern with '%'
        var lowerSearchTerm = searchTerm.ToLower();
        var likePattern = Expression.Constant($"%{lowerSearchTerm}%");

        // Create the LIKE expression using EF.Functions.Like
        var likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like",
            BindingFlags.Static | BindingFlags.Public,
            null,
            [typeof(DbFunctions), typeof(string), typeof(string)],
            null)!;

        var dbFunctions = Expression.Constant(EF.Functions);

        return Expression.Call(null, likeMethod, dbFunctions, lowerPropertyAccess, likePattern);
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

            res.Add(BuildLikeExpression(jsonbGetterCall, searchTerm));
        }
        return res;
    }
}