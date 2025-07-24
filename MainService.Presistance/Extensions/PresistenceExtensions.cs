using Core.BaseRepository;
using Core.Entities;
using Core.UOW;
using Helper.Caching;
using MainService.Presistance.Context;
using MainService.Presistance.Entities;
using MainService.Presistance.Entities.Users;
using MainService.Presistance.Repository;
using MainService.Presistance.UnitOfWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MainService.Presistance.Extensions;

public static class PresistenceExtensions
{
    public static WebApplication ApplyMigrations(this WebApplication app)
    {
        Console.Write("Applying ProjectDbContext migrations.. ");
        using var scope = app.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Console.WriteLine("Done");
        return app;
    }

    public static void AddPresistenceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(GetConnectionString(configuration)));
        services.AddScoped<DbContext, AppDbContext>();


        services.AddScoped<RoleRepository>();
        services.AddScoped<UserRepository>();

        services.RegisterRepos();
        services.RegisterUOW();

        services.AddScoped<IUnitOfWork<User>, UnitOfWork<User>>();
        services.AddScoped<IUnitOfWork<Role>, UnitOfWork<Role>>();

        services.RegistereUserManagerService<User>();
    }

    public static Type[] ProjectEntities = [.. Assembly.Load("MainService.Presistance")
                                .GetTypes()
                                .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IBaseEntity)))];
    static void RegisterRepos(this IServiceCollection services)
    {

        foreach (var entityType in ProjectEntities)
        {
            var repositoryType = typeof(BaseRepository<>).MakeGenericType(entityType);
            services.AddScoped(repositoryType);
        }
    }

    public static void RegisterUOW(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        foreach (var entityType in ProjectEntities)
        {
            var uowInterface = typeof(IUnitOfWork<>).MakeGenericType(entityType);
            var uowImplementation = typeof(UnitOfWork<>).MakeGenericType(entityType);
            services.AddScoped(uowInterface, uowImplementation);
        }
    }

    static void RegistereUserManagerService<TUserType>(this IServiceCollection services) where TUserType : User
    {
        services.AddIdentityCore<TUserType>(options =>
        {
            options.Password.RequireDigit = false;       // Require at least one number
            options.Password.RequireLowercase = false;   // Require at least one lowercase character
            options.Password.RequireUppercase = false;   // Require at least one uppercase character
            options.Password.RequireNonAlphanumeric = false; // Require at least one special character
            options.Password.RequiredLength = 4;        // Set the minimum password length
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();
    }


    static string? GetConnectionString(IConfiguration configuration)
        => CacheProvider.GetOrSet("AppConnectionString", () => configuration.GetConnectionString("Default"), minutes: 9999);
}
