using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security;
using System.Text.Json.Serialization;

namespace Helium.ErrorHandler
{
    public class HttpErrorType
    {
        public string Code { get; }

        [JsonPropertyName("innererror")]
        public InnerError InnerError { get; }

        public string Message { get; }

        public int StatusCode { get; }

        public string Target { get; }

        public HttpErrorType(string code, InnerError innerError, string message, int statusCode, string target)
        {
            Code = code;
            InnerError = innerError;
            Message = message;
            StatusCode = statusCode;
            Target = target;
        }
    }

    public class InnerError
    {
        public List<string> CharacterTypes { get; }

        public List<string> ValueTypes { get; }

        public string Code { get; }

        public string MaxLength { get; }

        public string MinLength { get; }

        public InnerError(InnerErrorType innerErrorType)
        {
            switch(innerErrorType)
            {
                case InnerErrorType.SearchParameter:
                    CharacterTypes = new List<string> { "lowerCase", "upperCase", "number", "symbol" };
                    break;
                case InnerErrorType.PageSizeParameter:
                    ValueTypes = new List<string> { "integer" };
                    break;
            }

            Code = "InvalidSearchParameter";
            MinLength = "2";
            MaxLength = "20";
        }
    }

    public enum InnerErrorType
    {
        SearchParameter,
        PageSizeParameter
    }

}
