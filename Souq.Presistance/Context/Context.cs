using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Souq.Presistance.Entities;

namespace Souq.Presistance.Context;


public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, Role, long>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }

}