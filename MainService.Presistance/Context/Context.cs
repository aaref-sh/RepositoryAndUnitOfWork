using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MainService.Presistance.Entities;
using MainService.Presistance.Entities.Users;

namespace MainService.Presistance.Context;


public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, Role, long>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public new DbSet<User> Users { set; get; }


}