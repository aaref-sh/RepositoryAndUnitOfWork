using Core.Exceptions;
using Core.Entities;
using Core.LocalizedProberty;
using Core.Paginated;
using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Core.BaseRepository;
using Microsoft.EntityFrameworkCore;

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

public class BaseFilter<TEntity> where TEntity : IBaseEntity
{

    [ModelBinder(BinderType = typeof(RequestFiltersModelBinder))]
    public List<RequestFilterDto> RequestFilters { get; set; } = [];


    [ModelBinder(BinderType = typeof(RequestOrdersModelBinder))]
    public List<RequestOrdersDto> RequestOrders { get; set; } = [];

    private string HeaderLang = "";
    public string SearchQuery { get; set; } = "";
    public bool GetAll { get; set; } = false;

    [Range(1, 1000)]
    public int Page { get; set; } = 1;

    [Range(1, 1000)]
    public int PerPage { get; set; } = 20;

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
        list = lst.ToList();
        var count = list.Count;
        if (!GetAll) list = list.GetRange(GetSkip(), PerPage);

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

    }

    private static Expression GetExpression(MemberExpression propertyExp, PropertyInfo propertyInfo, string? filter, string operation)
    {
        return (operation.ToLower()) switch
        {
            "in" or "notin" or "nin" => BaseFilter<TEntity>.GetListOperationExpression(propertyExp, propertyInfo, filter, operation),
            "any" or "nany" => GetListContainsAnyExpression(propertyExp, propertyInfo, filter, operation),
            _ => BaseFilter<TEntity>.GetOperationExpression(propertyExp, propertyInfo, filter, operation),
        };
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
        Type nullableType = isNullable ? propertyExp.Type : typeof(Nullable<>).MakeGenericType(propertyExp.Type);

        try
        {
            // If the property is enum, we need to handle it's conversion
            if (propertyInfo.PropertyType.IsEnum)
                filterValue = filter != null ? Enum.Parse(propertyInfo.PropertyType, filter) : null;
            else if (nonNullableType.IsEnum)
                filterValue = filter != null ? Enum.Parse(nonNullableType, filter) : null;
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
            "eq" or "equal" => filter == null ?
                 Expression.Equal(Expression.Convert(propertyExp, nullableType), Expression.Constant(null, nullableType))
                : Expression.Equal(propertyExp, constantExp),

            "ne" or "notequal" => filter == null ?
            Expression.NotEqual(Expression.Convert(propertyExp, nullableType),
                                Expression.Constant(null, nullableType))
            : Expression.NotEqual(propertyExp, constantExp),

            "gt" or "greaterthan" => Expression.GreaterThan(propertyExp, constantExp),
            "lt" or "lessthan" => Expression.LessThan(propertyExp, constantExp),
            "gte" or "greaterthanequal" => Expression.GreaterThanOrEqual(propertyExp, constantExp),
            "lte" or "lessthanequal" => Expression.LessThanOrEqual(propertyExp, constantExp),
            _ => throw new BaseException(System.Net.HttpStatusCode.UnprocessableEntity, $"Invalid operation: {operation}"),
        };
    }

    private static Expression GetListOperationExpression(MemberExpression propertyExp, PropertyInfo propertyInfo, string? filter, string operation)
    {
        // Remove brackets from the filter string if they exist
        var filters = filter?.Trim('[', ']').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
        if (filters?.Contains("null") ?? false)
        {
            throw new BaseException(System.Net.HttpStatusCode.BadRequest, "Invalid value (null) with (in) filter operation");
        }

        // Convert filter values to correct type
        var filterValues = filters?.Select(value =>
        {
            if (propertyInfo.PropertyType.IsEnum)
                return Enum.Parse(propertyInfo.PropertyType, value);
            else
                return Convert.ChangeType(value, propertyInfo.PropertyType);
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
            if (typeof(BaseEntity).IsAssignableFrom(propertyInfo.PropertyType))
            {
                // Access the Name property of LocalizedProperty and call getByLocale, handling null
                var nameProperty = propertyInfo.PropertyType.GetProperty("Name");
                var baseClassExp = Expression.Property(parameterExp, propertyInfo);
                var nullCheck = Expression.NotEqual(baseClassExp, Expression.Constant(null, typeof(BaseEntity)));
                var nameExp = Expression.Property(baseClassExp, nameProperty!);

                var methodInfo = typeof(CustomDbFunctions).GetMethod(nameof(CustomDbFunctions.JsonbGetter))!;
                var getByLocaleExp = Expression.Call(methodInfo, nameExp, Expression.Constant(HeaderLang));

                // If baseClassExp is null, return a default value
                propertyExp = Expression.Condition(nullCheck, getByLocaleExp, Expression.Constant(null, typeof(string)));
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