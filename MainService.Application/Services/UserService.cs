using AutoMapper;
using Core.BaseService;
using Core.Exceptions;
using Core.Exceptions.Customs;
using Core.UOW;
using Helper.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MainService.Application.DTOs.User;
using MainService.Application.Services.Interfaces;
using MainService.Presistance.Entities;
using MainService.Presistance.Entities.Users;
using System.Security.Claims;

namespace MainService.Application.Services;

public class UserService(IUnitOfWork<User> UOW, IHttpContextAccessor httpContextAccessor, RoleManager<Role> _roleManager, IMapper mapper, UserManager<User> _userManager
    ) : BaseService<User>(UOW, mapper), IUserService
{
    public async Task AssignRoleToUser(string username, string roleName)
    {
        var user = await _userManager.FindByNameAsync(username) ?? throw new BaseException(System.Net.HttpStatusCode.NotFound, "");

        if (!await _roleManager.RoleExistsAsync(roleName))
            throw new BaseException(System.Net.HttpStatusCode.BadRequest, "Role does not exist.");

        await _userManager.AddToRoleAsync(user, roleName);
    }

    public override async Task Create<TCreateDto>(TCreateDto dto)
    {
        var createDto = dto as UserCreateDto ?? throw new CustomUnProcessableEntityException(); 
        var user = mapper.Map<User>(dto);
        var res = await _userManager.CreateAsync(user);

        if (res.Succeeded)
        {
            var role = await Uow.Repo<Role>().GetById(createDto.RoleId);
            await AssignRoleToUser(createDto.UserName, role.Name);
            CacheProvider.ClearCacheOf(typeof(User));
        }
    }
    
    public async Task<List<string>> GetUserRoles(long userId)
    {
        var user = await GetById(userId);
        var roles = await _userManager.GetRolesAsync(user);
        return [.. roles];
    }

    public List<string> GetMyRoles() => 
        [.. GetMyClaims()
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)];

    public List<Claim> GetMyClaims()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null) return [];
        return [.. user.Claims];
    }

}
