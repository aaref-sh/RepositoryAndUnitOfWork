using Core.DTO;
using MainService.Application.DTOs.Role;
using MainService.Presistance.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MainService.Application.DTOs.User;

public class UserUpdateDto : BaseUpdateDto
{
    [Description("اسم المستخدم")]
    public string UserName { get; set; }

    [Description("البريد الإلكتروني")]
    public string Email { get; set; }

    [Description("رقم الهاتف")]
    public string PhoneNumber { get; set; }
}

public class UserCreateDto : BaseCreateDto
{
    [Required]
    [Description("اسم المستخدم")]
    public string UserName { get; set; }

    [Description("نوع المستخدم")]
    public UserType UserType { get; set; }

    [Description("البريد الإلكتروني")]
    public string Email { get; set; }

    [Description("الاسم")]
    public string FirstName { get; set; }

    [Description("الكنية")]
    public string LastName { get; set; }

    [PasswordPropertyText]
    [Required]
    [Length(4,12)]
    [Description("كلمة المرور")]
    public string Password { get; set; }

    [Phone]
    [Description("رقم الهاتف")]
    public string PhoneNumber { get; set; }

    public long RoleId { get; set; }
    [Description("الدور")]
    public RoleLiteDto Role { get; set; }
}