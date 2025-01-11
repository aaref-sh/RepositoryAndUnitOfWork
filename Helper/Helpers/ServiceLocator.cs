using Microsoft.Extensions.DependencyInjection;

namespace Helper.Helpers;

public static class ServiceLocator
{
    private static IServiceProvider _serviceProvider;
    public static IServiceScopeFactory _scopeFactory;

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    public static T GetService<T>()
    {
        return (T)_serviceProvider.GetService(typeof(T));
    }

    public static T GetScopedService<T>()
    {
        var scope = _scopeFactory.CreateScope();
        return GetScopedService<T>(scope);
    }

    public static IServiceScope CreateScope()
    {
        return _scopeFactory.CreateScope();
    }

    public static T GetScopedService<T>(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<T>();
    }
}