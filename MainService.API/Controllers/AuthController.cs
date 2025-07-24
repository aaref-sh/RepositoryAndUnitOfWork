using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MainService.Application.DTOs;
using MainService.Presistance.Entities;
using MainService.Presistance.Entities.Users;
using MainService.Presistance.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MainService.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(UserManager<User> userManager, RoleManager<Role> roleManager, IConfiguration config) : ControllerBase
{

    // User Registration
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model, UserType userType)
    {
        var user = new User { UserName = model.Username, Email = model.Email };
        var result = await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await userManager.AddToRoleAsync(user, userType.ToString());

        return Ok("User registered successfully!");
    }

    [HttpGet("SeedAdmin")]
    public async Task<IActionResult> SeedAdmin()
    {
        for (int i = 0; i < 6; i++)
        {
            var type = ((UserType)i).ToString();
            if(!await roleManager.RoleExistsAsync(type))
            {
                Role role = new() { Name = type };
                await roleManager.CreateAsync(role);
            }
        }

        return await Register(new() { Email = "admin@aa.bb", Password = "admin",Username="Admin" }, UserType.Admin);
    }

    // User Login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest model)
    {
        var user = await userManager.FindByNameAsync(model.Username);
        if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized("Invalid credentials.");

        var token = await GenerateJwtToken(user);

        return Ok(new AuthResponse { Token = token, UserId = user.Id, Email = user.Email, Username = user.UserName });
    }

    [HttpGet("Envs")]
    public IActionResult GetVars()
    {
        return Ok(Environment.GetEnvironmentVariables());
    }

    [NonAction]
    private async Task<string> GenerateJwtToken(User user)
    {
        var userRoles = await userManager.GetRolesAsync(user);
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            .. userRoles.Select(x => new Claim(ClaimTypes.Role , x))
        ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["JWT:Issuer"],
            audience: config["JWT:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
