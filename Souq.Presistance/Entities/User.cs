using Microsoft.AspNetCore.Identity;

namespace Souq.Presistance.Entities;

public class User : IdentityUser<long>
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
