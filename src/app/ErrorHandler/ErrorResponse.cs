using System.Text.Json.Serialization;

namespace Helium.ErrorHandler
{
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public HttpErrorType HttpErrorType { get; set; }

        public ErrorResponse(HttpErrorType httpErrorType)
        {
            HttpErrorType = httpErrorType;
        }
    }
}
