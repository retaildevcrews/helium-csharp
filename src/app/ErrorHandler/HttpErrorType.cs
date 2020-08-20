using System;
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

        public object MaxLength { get; }

        public object MinLength { get; }

        public object MaxValue { get; }

        public object MinValue { get; }

        public InnerError(InnerErrorType innerErrorType, object minValue, object maxValue)
        {
            switch(innerErrorType)
            {
                case InnerErrorType.SearchParameter:
                    CharacterTypes = new List<string> { "lowerCase", "upperCase", "number", "symbol" };
                    Code = "InvalidSearchParameter";
                    MinLength = minValue;
                    MaxLength = maxValue;
                    break;
                case InnerErrorType.PageSizeParameter:
                    ValueTypes = new List<string> { "integer" };
                    Code = "InvalidPageSizeParameter";
                    MaxValue = maxValue;
                    MinValue = minValue;
                    break;
                case InnerErrorType.PageNumberParameter:
                    ValueTypes = new List<string> { "integer" };
                    Code = "InvalidPageNumberParameter";
                    MaxValue = maxValue;
                    MinValue = minValue;
                    break;

                case InnerErrorType.GenreParameter:
                    ValueTypes = new List<string> { "integer" };
                    Code = "InvalidGenreParameter";
                    MinLength = minValue;
                    MaxLength = maxValue;
                    break;
                case InnerErrorType.YearParameter:
                    ValueTypes = new List<string> { "integer" };
                    Code = "InvalidYearParameter";
                    MaxValue = maxValue;
                    MinValue = minValue;
                    break;
                case InnerErrorType.RatingParameter:
                    ValueTypes = new List<string> { "double" };
                    Code = "InvalidRatingParameter";
                    MaxValue = maxValue;
                    MinValue = minValue;
                    break;
                case InnerErrorType.ActorIdParameter:
                    Code = "InvalidActorIDParameter";
                    break;
                case InnerErrorType.MovieIdParameter:
                    Code = "InvalidMovieIDParameter";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(innerErrorType), innerErrorType, null);
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
