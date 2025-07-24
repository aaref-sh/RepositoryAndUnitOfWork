using Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MainService.Application.DTOs.User;

public class UserLiteDto : BaseLiteDto
{
    [Description("الاسم")]
    public string FirstName { get; set; } = "";

    [Description("الاسم الأخير")]
    public string LastName { get; set; } = "";

    public override string ToString() => $"{FirstName} {LastName}";
}
