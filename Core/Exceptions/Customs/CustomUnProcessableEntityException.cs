using System.Net;

namespace Core.Exceptions.Customs;

public class CustomUnProcessableEntityException : BaseException
{
    public CustomUnProcessableEntityException(string message = CoreConstants.BaseUnProcessableEntityResourceKey) : base(HttpStatusCode.UnprocessableEntity, message) { }
    public CustomUnProcessableEntityException(int code, string message) : base(HttpStatusCode.UnprocessableEntity, code, message) { }
    public CustomUnProcessableEntityException(string message, params string[]? args) : base(HttpStatusCode.UnprocessableEntity, message, args) { }
    public CustomUnProcessableEntityException(int code, string message, params string[]? args) : base(HttpStatusCode.UnprocessableEntity, code, message, args) { }
    public CustomUnProcessableEntityException(int code, string message, Dictionary<string, string> SubErrors) : base(HttpStatusCode.UnprocessableEntity, code, message, SubErrors) { }
}