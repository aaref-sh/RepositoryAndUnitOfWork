using AutoMapper;
using Core.BaseController;
using Microsoft.AspNetCore.Mvc;
using MainService.Application.DTOs.User;
using MainService.Application.Services.Interfaces;
using MainService.Presistance.Entities.Users;

namespace MainService.API.Controllers;

[Route("api/User")]
public class UserController(IUserService userService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    : BaseController<User, UserDetailsDto, UserCreateDto, UserUpdateDto,UserLiteDto,UserListDto>(userService, mapper, httpContextAccessor)
{  

}
