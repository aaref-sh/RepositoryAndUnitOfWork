using System.ComponentModel.DataAnnotations;

namespace MainService.Application.DTOs;
public class RegisterModel
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class AuthRequest
{
    [Required]
    public string? Username { get; set; }
    [Required]
    public string? Password { get; set; }
}

public record AuthResponse
{
    public long? UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public List<string>? Roles { get; set; }
    public string? Token { get; set; }
}