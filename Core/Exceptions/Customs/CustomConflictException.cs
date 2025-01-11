using System.Net;

namespace Core.Exceptions.Customs;

public class CustomConflictException : BaseException
{
    public CustomConflictException(string message = CoreConstants.BaseBadRequestExceptionResourceKey) : base(HttpStatusCode.Conflict, message) { }
    public CustomConflictException(int code, string message) : base(HttpStatusCode.Conflict, code, message) { }
    public CustomConflictException(string message, params string[]? args) : base(HttpStatusCode.Conflict, message, args) { }
    public CustomConflictException(int code, string message, params string[]? args) : base(HttpStatusCode.Conflict, code, message, args) { }
}
