using Core.DTO;
using System.ComponentModel;

namespace MainService.Application.DTOs.User;

public class UserDetailsDto : BaseDetailsDto
{
    [Description("الاسم")]
    public string FirstName { get; set; } = "";

    [Description("الاسم الأخير")]
    public string LastName { get; set; } = "";

    [Description("نهاية القفل")]
    public DateTimeOffset? LockoutEnd { get; set; }

    [Description("المصادقة الثنائية مفعلة")]
    public bool TwoFactorEnabled { get; set; }

    [Description("رقم الهاتف")]
    public string? PhoneNumber { get; set; }

    [Description("البريد الإلكتروني مؤكد")]
    public bool EmailConfirmed { get; set; }

    [Description("البريد الإلكتروني")]
    public string? Email { get; set; }

    [Description("اسم المستخدم")]
    public string? UserName { get; set; }


    public override string ToString() => $"{FirstName} {LastName}";
}
