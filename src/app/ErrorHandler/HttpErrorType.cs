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

        [JsonPropertyName("maxValue")]
        public int MaxIntValue { get; }

        [JsonPropertyName("minValue")]
        public int MinIntValue { get; }

        [JsonPropertyName("maxValue")]
        public double MaxDoubleValue { get; }

        [JsonPropertyName("minValue")]
        public double MinDoubleValue { get; }

        public InnerError(InnerErrorType innerErrorType)
        {
            switch(innerErrorType)
            {
                case InnerErrorType.SearchParameter:
                    CharacterTypes = new List<string> { "lowerCase", "upperCase", "number", "symbol" };
                    Code = "InvalidSearchParameter";
                    MinLength = "2";
                    MaxLength = "20";
                    break;
                case InnerErrorType.PageSizeParameter:
                    ValueTypes = new List<string> { "integer" };
                    Code = "InvalidPageSizeParameter";
                    MaxIntValue = 1000;
                    MinIntValue = 1;
                    break;
                case InnerErrorType.PageNumberParameter:
                    ValueTypes = new List<string> { "integer" };
                    Code = "InvalidPageNumberParameter";
                    MaxIntValue = 10000;
                    MinIntValue = 1;
                    break;
                case InnerErrorType.ActorIdParameter:
                    Code = "InvalidActorIDParameter";
                    break;
                case InnerErrorType.GenreParameter:
                    ValueTypes = new List<string> { "string" };
                    Code = "InvalidGenreParameter";
                    MinLength = "3";
                    MaxLength = "20";
                    break;
                case InnerErrorType.YearParameter:
                    ValueTypes = new List<string> { "integer" };
                    Code = "InvalidYearParameter";
                    MinIntValue = 1984;
                    MaxIntValue = 2025;
                    break;
                case InnerErrorType.RatingParameter:
                    ValueTypes = new List<string> { "integer" };
                    Code = "InvalidRatingParameter";
                    MinDoubleValue = 0;
                    MaxDoubleValue = 10;
                    break;
                case InnerErrorType.MovieIdParameter:
                    Code = "InvalidMovieIDParameter";
                    break;
            }
        }
    }

    public enum InnerErrorType
    {
        SearchParameter,
        PageSizeParameter,
        PageNumberParameter,
        ActorIdParameter,
        GenreParameter,
        YearParameter,
        RatingParameter,
        MovieIdParameter
    }
}
