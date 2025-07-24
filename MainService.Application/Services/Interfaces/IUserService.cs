using Core.BaseService;
using MainService.Presistance.Entities.Users;
using System.Security.Claims;

namespace MainService.Application.Services.Interfaces;

public interface IUserService : IBaseService<User>
{
    List<Claim> GetMyClaims();
}
