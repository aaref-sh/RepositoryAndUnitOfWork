using Core.BaseRepository;
using Microsoft.EntityFrameworkCore;
using MainService.Presistance.Entities.Users;

namespace MainService.Presistance.Repository;

public class UserRepository(DbContext dbContext) : BaseRepository<User>(dbContext)
{
}
