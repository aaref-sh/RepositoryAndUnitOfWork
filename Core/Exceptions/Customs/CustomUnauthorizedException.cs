using System.Net;

namespace Core.Exceptions.Customs;

public class CustomUnauthorizedException : BaseException
{
    public CustomUnauthorizedException(string message = CoreConstants.BaseUnauthorizedExceptionResourceKey) : base(HttpStatusCode.Unauthorized, message) { }
    public CustomUnauthorizedException(int code, string message) : base(HttpStatusCode.Unauthorized, code, message) { }
    public CustomUnauthorizedException(string message, params string[]? args) : base(HttpStatusCode.Unauthorized, message, args) { }
    public CustomUnauthorizedException(int code, string message, params string[]? args) : base(HttpStatusCode.Unauthorized, code, message, args) { }
}