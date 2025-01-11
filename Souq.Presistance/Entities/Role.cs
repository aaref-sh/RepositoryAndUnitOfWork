using Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Souq.Presistance.Entities;

public class Role : IdentityRole<long>, IBaseEntity
{

}