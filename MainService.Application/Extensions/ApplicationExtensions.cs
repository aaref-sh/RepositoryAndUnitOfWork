using Core.BaseService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MainService.Application.Mapper;
using MainService.Application.Services;
using MainService.Application.Services.Interfaces;
using MainService.Presistance.Extensions;

namespace MainService.Application.Extensions;

public static class ApplicationExtensions
{
    public static void AddApplicationExtensions(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile(new MappingProfile()));

        services.AddHttpContextAccessor();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        foreach(var type in PresistenceExtensions.ProjectEntities)
        {
            var iService = typeof(IBaseService<>).MakeGenericType(type);
            var service = typeof(BaseService<>).MakeGenericType(type);
            services.AddScoped(iService, service);
        }
    }
}
