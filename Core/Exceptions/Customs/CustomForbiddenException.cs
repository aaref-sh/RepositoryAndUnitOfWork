using System.Net;

namespace Core.Exceptions.Customs;

public class CustomForbiddenException : BaseException
{
    public CustomForbiddenException(string message = CoreConstants.BaseForbiddenExceptionResourceKey) : base(HttpStatusCode.Forbidden, message) { }
    public CustomForbiddenException(int code, string message) : base(HttpStatusCode.Forbidden, code, message) { }
    public CustomForbiddenException(string message, params string[]? args) : base(HttpStatusCode.Forbidden, message, args) { }
    public CustomForbiddenException(int code, string message, params string[]? args) : base(HttpStatusCode.Forbidden, code, message, args) { }
}