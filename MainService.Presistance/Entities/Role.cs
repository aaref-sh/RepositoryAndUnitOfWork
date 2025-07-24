using Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace MainService.Presistance.Entities;

public class Role : IdentityRole<long>, IBaseEntity
{

}