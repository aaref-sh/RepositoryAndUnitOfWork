using Core.DTO;

using System.ComponentModel;

namespace MainService.Application.DTOs.User;

public class UserListDto : BaseListDto
{
    [Description("الاسم")]
    public string FirstName { get; set; } = "";
    [Description("الكنية")]
    public string LastName { get; set; } = "";
    [Description("اسم المستخدم")]
    public string? UserName { get; set; }
    [Description("رقم الهاتف")]
    public string? PhoneNumber { get; set; }
    [Description("البريد")]
    public string? Email { get; set; }

    public override string ToString() => $"{FirstName} {LastName}";
}
