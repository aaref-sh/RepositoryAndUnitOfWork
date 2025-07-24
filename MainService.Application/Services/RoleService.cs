using AutoMapper;
using Core.BaseService;
using Core.UOW;
using MainService.Application.Services.Interfaces;
using MainService.Presistance.Entities;

namespace MainService.Application.Services;

public class RoleService(IUnitOfWork<Role> uow, IMapper mapper) : BaseService<Role>(uow, mapper), IRoleService
{
}
