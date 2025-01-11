using System.Net;

namespace Core.Exceptions.Customs;

public class CustomBadRequestException : BaseException
{
    public CustomBadRequestException(string message = CoreConstants.BaseBadRequestExceptionResourceKey) : base(HttpStatusCode.BadRequest, message) { }
    public CustomBadRequestException(int code, string message) : base(HttpStatusCode.BadRequest, code, message) { }
    public CustomBadRequestException(string message, params string[]? args) : base(HttpStatusCode.BadRequest, message, args) { }
    public CustomBadRequestException(int code, string message, params string[]? args) : base(HttpStatusCode.BadRequest, code, message, args) { }
}