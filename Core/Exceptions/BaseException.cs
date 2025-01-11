using System.Net;

namespace Core.Exceptions;

public class BaseException : Exception
{
    public string[]? Args { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public int? Code { set; get; }
    public Dictionary<string, string>? SubErrors { get; set; }

    public BaseException(HttpStatusCode statusCode, string? message = null, params string[]? args) : base(message)
    {
        StatusCode = statusCode;
        Code = (int)statusCode;
        Args = args;
    }

    public BaseException(HttpStatusCode statusCode, int code, string message, params string[]? args) : base(message)
    {
        StatusCode = statusCode;
        Args = args;
        Code = code;
    }

    public BaseException(HttpStatusCode statusCode, int code, string message, Dictionary<string, string> subErrors) : base(message)
    {
        StatusCode = statusCode;
        SubErrors = subErrors;
        Code = code;
    }
}