using Core.BaseRepository;
using Microsoft.EntityFrameworkCore;
using MainService.Presistance.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Presistance.Repository;

public class RoleRepository(DbContext dbContext) : BaseRepository<Role>(dbContext)
{
}
