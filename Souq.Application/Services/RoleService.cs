using Core.BaseService;
using Core.UOW;
using Souq.Application.Services.Interfaces;
using Souq.Presistance.Entities;
using Souq.Presistance.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Souq.Application.Services;

public class RoleService(UnitOfWork<Role> uow) : BaseService<Role>(uow), IRoleService
{
}
