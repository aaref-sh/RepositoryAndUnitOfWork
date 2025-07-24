using Core.Entities;
using Microsoft.AspNetCore.Identity;
using MainService.Presistance.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainService.Presistance.Entities.Users;

public class User : IdentityUser<long>, IBaseEntity
{
    [MaxLength(100)]
    public string FirstName { get; set; } = "";
    [MaxLength(100)]
    public string LastName { get; set; } = "";
    public UserType UserType { get; set; }

    [NotMapped]
    public string Name => $"{FirstName} {LastName}";
}
