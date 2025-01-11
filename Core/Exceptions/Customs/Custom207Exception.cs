using System.Net;

namespace Core.Exceptions.Customs;

public class Custom207Exception
    : BaseException
{
    public Custom207Exception(string message = CoreConstants.BasePartialSuccessExceptionResourceKey) : base(HttpStatusCode.MultiStatus, message) { }
    public Custom207Exception(int code, string message) : base(HttpStatusCode.MultiStatus, code, message) { }
    public Custom207Exception(string message, params string[]? args) : base(HttpStatusCode.MultiStatus, message, args) { }
    public Custom207Exception(params string[]? args) : base(HttpStatusCode.MultiStatus, CoreConstants.BasePartialSuccessExceptionResourceKey, args) { }
    public Custom207Exception(Dictionary<string, string> SubErrors) : base(HttpStatusCode.MultiStatus, 207, CoreConstants.BasePartialSuccessExceptionResourceKey, SubErrors) { }
    public Custom207Exception(int code, string message, params string[]? args) : base(HttpStatusCode.MultiStatus, code, message, args) { }
}