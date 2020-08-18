namespace Helium.ErrorHandler
{
    public class HttpErrorType
    {
        public string Code { get; }

        public dynamic InnerError { get; }

        public string Message { get; }

        public int StatusCode { get; }

        public string Target { get; }

        public HttpErrorType(string code, dynamic innerError, string message, int statusCode, string target)
        {
            Code = code;
            InnerError = innerError;
            Message = message;
            StatusCode = statusCode;
            Target = target;
        }
    }

    public sealed class InvalidSearchParameterInnerError
    {
        public static readonly string[] CharacterTypes = {"LowerCase", "UpperCase", "Number", "Symbol"};

        public const string Code = "InvalidSearchParameter";

        public const string MaxLength = "20";

        public const string MinLength = "2";
    }

    public sealed class InvalidPageSizeParameterInnerError
    {

    }
}
