using System.ComponentModel;

namespace MainService.Presistance.Enums;

public enum UserType
{
    [Description("مدير")]
    Admin,
    [Description("مدير مدرسة")]
    Manager,
    [Description("مشرف")]
    Supervisor,
    [Description("مدرس")]
    Teacher,
    [Description("طالب")]
    Student,
    [Description("أهل")]
    Parent,
}
