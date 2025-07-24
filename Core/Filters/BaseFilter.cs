using Core.BaseRepository;
using Core.Entities;
using Core.Exceptions;
using Core.LocalizedProberty;
using Core.Paginated;
using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Core.Filters;

public class RequestFilterDto
{
    public required string Name { get; set; }
    public object? Value { get; set; }
    public required string Operation { get; set; }

}
public class RequestOrdersDto
{
    public required string Name { get; set; }

    [Range(1, 2, ErrorMessage = $"Invalid Sort Direction")]
    public required int Direction { get; set; }
}
public enum SortDirection
{
    Asc = 1,
    Desc
}
public class OrderItem<T>(Expression<Func<T, object>> Exp, SortDirection Direction)
{
    public SortDirection Direction { set; get; } = Direction;
    public Expression<Func<T, object>> Exp { set; get; } = Exp;
}

public class RequestFiltersModelBinder : IModelBinder
{
    private static readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.ModelName;

        // Fetch the value of the argument by key and set it as the model state.
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult != ValueProviderResult.None)
        {
            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            List<RequestFilterDto>? result;
            try
            {
                result = JsonSerializer.Deserialize<List<RequestFilterDto>>(valueProviderResult.ToString(), options: options);
            }
            catch (Exception)
            {
                throw new BaseException(System.Net.HttpStatusCode.UnprocessableEntity, "CoreConstants.Error_InvalidFilterJsonFormat");
            }

            if (result != null && result.Count > 0)
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}

public class RequestOrdersModelBinder : IModelBinder
{
    private static readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.ModelName;

        // Fetch the value of the argument by key and set it as the model state.
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult != ValueProviderResult.None)
        {
            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            List<RequestOrdersDto>? result;
            try
            {
                result = JsonSerializer.Deserialize<List<RequestOrdersDto>>(valueProviderResult.ToString(), options);
            }
            catch (Exception)
            {
                throw new BaseException(System.Net.HttpStatusCode.UnprocessableEntity, "Invalid orders json format");
            }

            if (result != null && result.Count > 0)
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
} 

public class RequestFilterDtoComparer : IEqualityComparer<RequestFilterDto>
{
    public bool Equals(RequestFilterDto x, RequestFilterDto y)
    {
        if (x == null || y == null) return false;
        return x.Name == y.Name && Equals(x.Value, y.Value) && x.Operation == y.Operation;
    }

    public int GetHashCode(RequestFilterDto obj)
    {
        return HashCode.Combine(obj.Name, obj.Value, obj.Operation);
    }
}

public class RequestOrdersDtoComparer : IEqualityComparer<RequestOrdersDto>
{
    public bool Equals(RequestOrdersDto x, RequestOrdersDto y)
    {
        if (x == null || y == null) return false;
        return x.Name == y.Name && x.Direction == y.Direction;
    }

    public int GetHashCode(RequestOrdersDto obj)
    {
        return HashCode.Combine(obj.Name, obj.Direction);
    }
}

public class BaseFilter
{
    public bool Compare(BaseFilter other)
    {
        if (other == null) return false;

        return RequestFilters.SequenceEqual(other.RequestFilters, new RequestFilterDtoComparer()) &&
               RequestOrders.SequenceEqual(other.RequestOrders, new RequestOrdersDtoComparer()) &&
               SearchQuery == other.SearchQuery &&
               GetAll == other.GetAll &&
               Page == other.Page &&
               PerPage == other.PerPage;
    }

    [ModelBinder(BinderType = typeof(RequestFiltersModelBinder))]
    public List<RequestFilterDto> RequestFilters { get; set; } = [];

    [ModelBinder(BinderType = typeof(RequestOrdersModelBinder))]
    public List<RequestOrdersDto> RequestOrders { get; set; } = [];

    protected string HeaderLang = "";
    public string SearchQuery { get; set; } = "";
    public bool GetAll { get; set; } = false;

    [Range(1, 1000)]
    public int Page { get; set; } = 1;

    [Range(1, 1000)]
    public int PerPage { get; set; } = 20;
}

public class BaseFilter<TEntity> : BaseFilter where TEntity : IBaseEntity
{
    public int GetSkip() => (Page - 1) * PerPage;
    public Range GetRange() => ((Page - 1) * PerPage)..PerPage;

    public List<Expression<Func<TEntity, bool>>> GetFilters() => _filters;
    private readonly List<Expression<Func<TEntity, bool>>> _filters = [];
    public PaginatedList<TEntity> ApplyTo(List<TEntity> list)
    {
        var lst = list.Where(_filters.Combine());
        foreach (var order in _orders)
        {
            var exp = order.Exp.Compile();
            lst = lst is IOrderedEnumerable<TEntity> ordered ?
                order.Direction == SortDirection.Asc ? ordered.ThenBy(exp) : ordered.ThenByDescending(exp) :
                order.Direction == SortDirection.Asc ? lst.OrderBy(exp) : lst.OrderByDescending(exp);
        }
        list = [.. lst];
        var count = list.Count;
        if (!GetAll) list = [.. list.Skip(GetSkip()).Take(PerPage)];

        return new(list, Page, PerPage, count);
    }
    public async Task<PaginatedList<TEntity>> ApplyTo(IQueryable<TEntity> query)
    {
        // apply filters
        query = ApplyFilters(query);

        var count = await query.CountAsync();

        // apply order by 
        query = ApplyOrderBy(query);

        // apply pagination
        var res = await ApplyPagination(query);

        return new(res, Page, GetAll ? count : PerPage, count);
    }

    private async Task<List<TEntity>> ApplyPagination(IQueryable<TEntity> query)
    {
        if (!GetAll) query = query.Skip(GetSkip()).Take(PerPage);

        return await query.ToListAsync();
    }

    private IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query)
    {
        if (!string.IsNullOrEmpty(SearchQuery))
            AddFilter(ExpressionHelper.GetExpressionsOfType<TEntity>(SearchQuery));

        GetFilters().ForEach(x => query = query.Where(x));
        return query;
    }

    private IQueryable<TEntity> ApplyOrderBy(IQueryable<TEntity> query)
    {
        bool firstOrder = true;
        foreach (var order in GetOrder())
        {
            query = firstOrder ?
                order.Direction == SortDirection.Asc ? query.OrderBy(order.Exp) : query.OrderByDescending(order.Exp) :
                order.Direction == SortDirection.Asc ? ((IOrderedQueryable<TEntity>)query).ThenBy(order.Exp)
                                                      : ((IOrderedQueryable<TEntity>)query).ThenByDescending(order.Exp);
            firstOrder = false;
        }
        if (firstOrder) query = query.OrderByDescending(x => x.Id);
        return query;
    }
    public void AddFilter(Expression<Func<TEntity, bool>> filter)
    {
        _filters.Add(filter);
    }
    public void SetLang(string lang)
    {
        HeaderLang = lang;
    }
    public void Init(string lang)
    {
        SetLang(lang);
        SetFilters();
        SetOrder();
    }
    public void SetFilters()
    {
        var entityType = typeof(TEntity);

        foreach (var filter in RequestFilters)
        {
            var propertyInfo = entityType.GetProperty(filter.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo != null)
            {
                if (propertyInfo.PropertyType == typeof(LocalizedProperty))
                    throw new BaseException(System.Net.HttpStatusCode.BadRequest, $"Cannot Perform filtering on {propertyInfo.Name} field, Use Search Query for filtering on localized strings");

                var parameterExp = Expression.Parameter(entityType, "e");
                var propertyExp = Expression.Property(parameterExp, propertyInfo);
                var exp = BaseFilter<TEntity>.GetExpression(propertyExp, propertyInfo, filter.Value?.ToString(), filter.Operation.ToLower());
                var lambdaExp = Expression.Lambda<Func<TEntity, bool>>(exp, parameterExp);
                AddFilter(lambdaExp);
            }
        }
        RequestFilters.Clear();
    }

    private static Expression GetExpression(MemberExpression propertyExp, PropertyInfo propertyInfo, string? filter, string operation)
    {
        return (operation.ToLower()) switch
        {
            "in" or "notin" or "nin" => BaseFilter<TEntity>.GetListOperationExpression(propertyExp, propertyInfo, filter, operation),
            "any" or "nany" => GetListContainsAnyExpression(propertyExp, propertyInfo, filter, operation),
            "contains" or "notcontains" or "startswith" or "endswith" => GetStringOperationExpression(propertyExp, propertyInfo, filter, operation),
            "thesamedatewith" or "between" => GetDateOperationExpression(propertyExp, propertyInfo, filter, operation),
            _ => BaseFilter<TEntity>.GetOperationExpression(propertyExp, propertyInfo, filter, operation),
        };
    }
    private static BinaryExpression GetDateOperationExpression(MemberExpression propertyExp, PropertyInfo propertyInfo, string? filter, string operation)
    {
        object? filterValue1, filterValue2;
        var targetType = propertyInfo.PropertyType;
        bool isNullable = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
        Type nonNullableType = isNullable ? Nullable.GetUnderlyingType(targetType)! : targetType;
        bool isDateTimeOffset = nonNullableType == typeof(DateTimeOffset);

        switch (operation)
        {
            case "between":
                var dates = filter!
                    .Replace("\"", "")
                    .Trim('[', ']')
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                filterValue1 = GetDateFilterValue(dates[0], nonNullableType);
                filterValue2 = GetDateFilterValue(dates[1], nonNullableType);

                var lowerBound = Expression.GreaterThanOrEqual(propertyExp, Expression.Constant(filterValue1, propertyExp.Type));
                var upperBound = Expression.LessThanOrEqual(propertyExp, Expression.Constant(filterValue2, propertyExp.Type));
                return Expression.AndAlso(lowerBound, upperBound);

            case "thesamedatewith":
                filterValue1 = GetDateFilterValue(filter, nonNullableType);
                if (isNullable)
                {
                    var hasValueExp = Expression.Property(propertyExp, "HasValue");
                    var valueExp = Expression.Property(propertyExp, "Value");
                    var equalExp = isDateTimeOffset
                        ? Expression.Equal(Expression.Property(valueExp, "Date"), Expression.Constant(((DateTimeOffset)filterValue1).Date))
                        : Expression.Equal(valueExp, Expression.Constant(filterValue1, propertyExp.Type));
                    return Expression.AndAlso(hasValueExp, equalExp);
                }
                else
                {
                    return isDateTimeOffset
                        ? Expression.Equal(Expression.Property(propertyExp, "Date"), Expression.Constant(((DateTimeOffset)filterValue1).Date))
                        : Expression.Equal(propertyExp,Expression.Constant(filterValue1, propertyExp.Type));
                }

            default:
                throw new BaseException(System.Net.HttpStatusCode.UnprocessableEntity, $"Invalid date operation: {operation}");
        }
    }

    private static Expression GetStringOperationExpression(MemberExpression propertyExp, PropertyInfo propertyInfo, string? filter, string operation)
    {
        if (filter == null)
        {
            // If filter is null, return false expression (no match)
            return Expression.Constant(false);
        }

        var filterExp = Expression.Constant(filter, typeof(string));

        var notNullExp = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // MethodInfo for string methods
        MethodInfo? method = operation switch
        {
            "contains" or "notcontains" => typeof(string).GetMethod(nameof(string.Contains), [typeof(string)]),
            "startswith" => typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) }),
            "endswith" => typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) }),
            _ => null
        } ?? throw new BaseException(System.Net.HttpStatusCode.UnprocessableEntity, $"Invalid string operation: {operation}");

        // Call expression: propertyExp.Method(filter)
        Expression callExp = Expression.Call(propertyExp, method, filterExp);

        // For "notcontains", negate the call expression
        if (operation == "notcontains")
        {
            callExp = Expression.Not(callExp);
        }

        // Combine null check and call expression: propertyExp != null && propertyExp.Method(filter)
        return Expression.AndAlso(notNullExp, callExp);
    }

    private static Expression GetListContainsAnyExpression(MemberExpression propertyExp, PropertyInfo propertyInfo, string? filter, string operation)
    {
        // Remove brackets from the filter string if they exist
        filter = filter?.Trim('[', ']');
        var filters = filter?.Trim('[', ']').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
        if (filters?.Contains("null") ?? false)
        {
            throw new BaseException(System.Net.HttpStatusCode.BadRequest, "Invalid value (null) with (any) filter operation");
        }

        // Convert filter values to correct type (List of long in this case)
        var filterValues = filters?.Select(long.Parse).ToList();

        if (filterValues == null || filterValues.Count == 0)
            return Expression.Constant(false);

        // Assuming the list contains objects with a property named "Id" of type long
        var elementType = propertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();
        var idProperty = elementType!.GetProperty("Id");

        if (idProperty == null || idProperty.PropertyType != typeof(long))
            throw new InvalidOperationException("The elements in the list must have an 'Id' property of type long.");

        // Create a parameter for the inner lambda (list item)
        var innerParameter = Expression.Parameter(elementType, "x");

        // Access the Id property of the inner parameter
        var idPropertyExp = Expression.Property(innerParameter, idProperty);

        // Create a constant expression for the filter values (List<long>)
        var filterValuesType = typeof(List<long>);
        var filterValuesConstant = Expression.Constant(filterValues, filterValuesType);

        // Create a call to List<long>.Contains for the filter values
        var containsMethod = filterValuesType.GetMethod("Contains", [typeof(long)]);
        var containsCall = Expression.Call(filterValuesConstant, containsMethod!, idPropertyExp);

        // Create a lambda for "Any" method (x => filterValues.Contains(x.Id))
        var anyPredicate = Expression.Lambda(containsCall, innerParameter);

        // Call Enumerable.Any on the property expression
        var anyMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(elementType);
        Expression exp = Expression.Call(null, anyMethod, propertyExp, anyPredicate);
        return operation == "nany" ? Expression.Not(exp) : exp;
    }

    private static BinaryExpression GetOperationExpression(MemberExpression propertyExp, PropertyInfo propertyInfo, string? filter, string operation)
    {
        // Convert filter value to correct type
        object? filterValue;

        var targetType = propertyInfo.PropertyType;
        bool isNullable = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
        Type nonNullableType = (isNullable ? Nullable.GetUnderlyingType(targetType) : targetType)!;
        Type nullableType = isNullable || propertyExp.Type == typeof(string) ? propertyExp.Type : typeof(Nullable<>).MakeGenericType(propertyExp.Type);

        try
        {
            // If the property is enum, we need to handle it's conversion
            if (propertyInfo.PropertyType.IsEnum)
                filterValue = filter != null ? Enum.Parse(propertyInfo.PropertyType, filter) : null;
            else if (nonNullableType.IsEnum)
                filterValue = filter != null ? Enum.Parse(nonNullableType, filter) : null;
            else if (nonNullableType.In([typeof(DateOnly), typeof(DateTime), typeof(DateTimeOffset)]))
                filterValue = GetDateFilterValue(filter, nonNullableType);
            else
                filterValue = filter == null ? null : Convert.ChangeType(filter, nonNullableType);
        }
        catch
        {
            // Return an empty binary expression (no-op)
            return Expression.Equal(Expression.Constant(true), Expression.Constant(true));
        }


        ConstantExpression constantExp = filterValue != null ?
                                            Expression.Constant(filterValue, propertyExp.Type) :
                                            Expression.Constant(null, typeof(object));

        return operation switch
        {
            "eq" or "equal" or "equals" => filter == null ?
                 Expression.Equal(Expression.Convert(propertyExp, nullableType), Expression.Constant(null, nullableType))
                : Expression.Equal(propertyExp, constantExp),

            "ne" or "notequal" or "notequals" => filter == null ?
            Expression.NotEqual(Expression.Convert(propertyExp, nullableType),
                                Expression.Constant(null, nullableType))
            : Expression.NotEqual(propertyExp, constantExp),

            "gt" or "greaterthan" => Expression.GreaterThan(propertyExp, constantExp),
            "lt" or "lessthan" => Expression.LessThan(propertyExp, constantExp),
            "gte" or "greaterthanequal" or "greaterthanorequals" => Expression.GreaterThanOrEqual(propertyExp, constantExp),
            "lte" or "lessthanequal" or "lessthanorequals" => Expression.LessThanOrEqual(propertyExp, constantExp),
            _ => throw new BaseException(System.Net.HttpStatusCode.UnprocessableEntity, $"Invalid operation: {operation}"),
        };
    }

    private static object? GetDateFilterValue(string? filter, Type nonNullableType)
    {
        object? filterValue = null;
        DateTimeOffset? val = filter == null ? null : DateTimeOffset.Parse(filter);
        if (nonNullableType == typeof(DateTimeOffset)) filterValue = val;
        if (nonNullableType == typeof(DateTime)) filterValue = val?.Date;
        if (nonNullableType == typeof(DateOnly) && val.HasValue) filterValue = DateOnly.FromDateTime(val.Value.Date);
        return filterValue;
    }

    private static Expression GetListOperationExpression(MemberExpression propertyExp, PropertyInfo propertyInfo, string? filter, string operation)
    {
        // Remove brackets from the filter string if they exist
        var filters = filter?.Trim('[', ']').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
        if (filters?.Contains("null") ?? false)
        {
            throw new BaseException(System.Net.HttpStatusCode.BadRequest, "Invalid value (null) with (in) filter operation");
        }
        var targetType = propertyInfo.PropertyType;
        bool isNullable = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
        Type nonNullableType = (isNullable ? Nullable.GetUnderlyingType(targetType) : targetType)!;
        // Convert filter values to correct type
        var filterValues = filters?.Select(value =>
        {
            if (propertyInfo.PropertyType.IsEnum)
                return Enum.Parse(propertyInfo.PropertyType, value);
            else
                return Convert.ChangeType(value, nonNullableType);
        }).ToList();

        var filterValuesType = typeof(List<>).MakeGenericType(propertyInfo.PropertyType);
        var filterValuesConverted = Activator.CreateInstance(filterValuesType);

        foreach (var value in filterValues ?? [])
        {
            filterValuesType?.GetMethod("Add")?.Invoke(filterValuesConverted, [value]);
        }

        // Create a method call expression for the Contains method
        var containsMethod = filterValuesType!.GetMethod("Contains")!;
        var constantExp = Expression.Constant(filterValuesConverted, filterValuesType);

        return operation switch
        {
            "nin" or "notin" => Expression.Not(Expression.Call(constantExp, containsMethod, propertyExp)),
            _ => Expression.Call(constantExp, containsMethod, propertyExp),
        };
    }

    private readonly List<OrderItem<TEntity>> _orders = [];
    public List<OrderItem<TEntity>> GetOrder() => _orders;
    private readonly List<string> orderBys = [];
    public void SetOrder()
    {
        var entityType = typeof(TEntity);
        Type stringType = typeof(string);

        foreach (var order in RequestOrders)
        {
            var propertyInfo = typeof(TEntity).GetProperty(order.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null) continue;
            var sortDirc = order.Direction == 1 ? SortDirection.Asc : SortDirection.Desc;

            var parameterExp = Expression.Parameter(entityType, "u");
            Expression propertyExp = Expression.Property(parameterExp, propertyInfo);

            // Check if the property is of type BaseClass
            if (typeof(IBaseEntity).IsAssignableFrom(propertyInfo.PropertyType))
            {
                var nameProperty = propertyInfo.PropertyType.GetProperty("Name");
                var baseClassExp = Expression.Property(parameterExp, propertyInfo);
                var nullCheck = Expression.NotEqual(baseClassExp, Expression.Constant(null, typeof(IBaseEntity)));
                var nameExp = Expression.Property(baseClassExp, nameProperty!);

                // If baseClassExp is null, return a default value
                propertyExp = Expression.Condition(nullCheck, nameExp, Expression.Constant(null, typeof(string)));
            }
            else if (propertyInfo.PropertyType == typeof(LocalizedProperty))
            {
                var methodInfo = typeof(CustomDbFunctions).GetMethod(nameof(CustomDbFunctions.JsonbGetter))!;
                propertyExp = Expression.Call(methodInfo, propertyExp, Expression.Constant(HeaderLang));
            }

            var castExp = Expression.Convert(propertyExp, typeof(object));
            var lambdaExp = Expression.Lambda<Func<TEntity, object>>(castExp, parameterExp);

            AddOrder(new(lambdaExp, sortDirc));
        }
        RequestOrders.Clear();
    }
    protected void AddOrder(OrderItem<TEntity> order)
    {
        _orders.Add(order);
    }
}

public static class LambdaParser
{
    static readonly ParsingConfig config = new();
    public static Expression<Func<T, bool>> ParseLambda<T>(string lambdaString)
    {
        if (string.IsNullOrWhiteSpace(lambdaString))
            throw new ArgumentException("Lambda expression string cannot be null or whitespace.", nameof(lambdaString));
        try
        {
            return DynamicExpressionParser.ParseLambda<T, bool>(config, false, lambdaString);
        }
        catch (ParseException parseEx)
        {
            throw new ArgumentException(
                $"Failed to parse lambda expression: '{lambdaString}'. Details: {parseEx.Message}", parseEx);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"An error occurred while parsing lambda expression: '{lambdaString}'", ex);
        }
    }

    public static Expression<Func<T, bool>> ParseLambda<T>(string lambdaString, IDictionary<string, object> externalParameters)
    {
        if (string.IsNullOrWhiteSpace(lambdaString))
            throw new ArgumentException("Lambda expression string cannot be null or whitespace.", nameof(lambdaString));

        if (externalParameters == null)
            throw new ArgumentNullException(nameof(externalParameters), "External parameters dictionary cannot be null.");

        try
        {
            Dictionary<string, int> paramMapping = [];
            int index = 0;
            foreach (var paramName in externalParameters.Keys) paramMapping[paramName] = index++;
            
            foreach (var kvp in paramMapping)
            {
                string paramName = kvp.Key;
                int placeholderIndex = kvp.Value;
                lambdaString = Regex.Replace(lambdaString, $@"\b{Regex.Escape(paramName)}\b", $"@{placeholderIndex}");
            }

            object[] externalValues = [.. paramMapping.OrderBy(kvp => kvp.Value).Select(kvp => externalParameters[kvp.Key])];

            return DynamicExpressionParser.ParseLambda<T, bool>(config, false, lambdaString, externalValues);
        }
        catch (ParseException parseEx)
        {
            throw new ArgumentException(
                $"Failed to parse lambda expression: '{lambdaString}' with external parameters. Details: {parseEx.Message}", parseEx);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"An error occurred while parsing lambda expression: '{lambdaString}' with external parameters.", ex);
        }
    }
}