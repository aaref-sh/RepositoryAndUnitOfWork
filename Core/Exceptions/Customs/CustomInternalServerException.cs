using System.Net;

namespace Core.Exceptions.Customs;

public class CustomInternalServerException : BaseException
{
    public CustomInternalServerException(string message = CoreConstants.BaseInternalServerErrorResourceKey) : base(HttpStatusCode.InternalServerError, message) { }
    public CustomInternalServerException(int code, string message) : base(HttpStatusCode.InternalServerError, code, message) { }
    public CustomInternalServerException(string message, params string[]? args) : base(HttpStatusCode.InternalServerError, message, args) { }
    public CustomInternalServerException(int code, string message, params string[]? args) : base(HttpStatusCode.InternalServerError, code, message, args) { }
}