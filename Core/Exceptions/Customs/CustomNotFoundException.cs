using System.Net;

namespace Core.Exceptions.Customs;

public class CustomNotFoundException : BaseException
{
    public CustomNotFoundException(string message = CoreConstants.BaseNotFoundExceptionResourceKey) : base(HttpStatusCode.NotFound, message) { }
    public CustomNotFoundException(int code, string message) : base(HttpStatusCode.NotFound, code, message) { }
    public CustomNotFoundException(string message, params string[]? args) : base(HttpStatusCode.NotFound, message, args) { }
    public CustomNotFoundException(int code, string message, params string[]? args) : base(HttpStatusCode.NotFound, code, message, args) { }
}