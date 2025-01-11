namespace Core;

public static class CoreConstants
{

    #region Date & time

    public const string FullDateTimeFormat = "yyyyMMddHHmmssffff";
    public const string DefaultLocale = "ar";

    #endregion

    #region Resources

    public const string BaseBadRequestExceptionResourceKey = "Error_BaseBadRequestExcption";
    public const string BaseConflictExceptionResourceKey = "Error_ConflictExcption";
    public const string BaseNotFoundExceptionResourceKey = "Error_BaseNotFound";
    public const string Error_FileNotFound = "Error_FileNotFound";
    public const string BaseUnauthorizedExceptionResourceKey = "Error_BaseUnauthorized";
    public const string BaseForbiddenExceptionResourceKey = "Error_BaseForbidden";
    public const string BaseUnProcessableEntityResourceKey = "Error_BaseUnProcessableEntity";
    public const string BaseInternalServerErrorResourceKey = "Error_BaseInternalServerError";
    public const string BasePartialSuccessExceptionResourceKey = "PartialSuccess";

    public const string NotFoundExceptionResourceKey = "Error_NotFound";
    public const string PrefixEntityResourceKey = "Entity_";
    public const string InvalidCredentialsKey = "Error_AuthenticationFailed";
    public const string InvalidPassword = "Error_InvalidPassword";
    public const string Error_SameFIleAlreadyExist = "Error_SameFIleAlreadyExist";
    public const string Error_ForeignKeyVoilated = "Error_ForeignKeyVoilated";
    public const string Error_UniqueKeyVoilated = "Error_UniqueKeyVoilated";
    public const string Order = "Order";
    public const string Error_UniqueFieldVoilated = "Error_UniqueFieldVoilated";
    public const string Error_MissingFiles = "Error_MissingFiles";
    public const string Error_ForeignKeyVoilated_DELETE = "Error_ForeignKeyVoilated_DELETE";

    public const string Error_InvalidFilterJsonFormat = "Error_InvalidFilterJsonFormat";
    #endregion

    #region Db  & EF Core constants

    public const int DefaultMaxLength = 255;
    public const int LanguageCodeLength = 2; //ar, en...

    public static readonly HashSet<string> SupportedLanguages = new()
    {
        "en",
        "ar"
    };

    public static List<string> AllowedOperation = [
            "in",
        "notin",
        "nin",
        "eq",
        "ne",
        "gt",
        "lt",
        "gte",
        "lte"
        ];

    public const bool WithDeletedRecord = false;
    public const bool WithTrackingOption = true;
    public const bool WithoutTrackingOption = false;

    public const int InitialPage = 1;
    public const int InitialPerPage = 30;
    public const int MaxPerPage = 100;
    public const string DefaultOperation = "eq";

    #endregion

    #region Regular expressions

    public const string PasswordRegex = @"^(?=.*[0-9])(?=.*[A-Z])(?=.*[_#]).*$";
    public const string NumbersRegex = @"[0-9]+";
    public const string EmailRegex = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
    public const string UrlRegex = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";
    public const string TimeOnlyRegex = @"[0-1][0-9]:[0-5][0-9]";

    #endregion
}
