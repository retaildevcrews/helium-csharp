using CSE.Helium.Model;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CSE.Helium.Validation
{
    public class ValidationProblemDetails
    {
        private readonly List<ValidationError> validationErrors = new List<ValidationError>();

        [JsonPropertyName("type")]
        public string Type { get; }

        [JsonPropertyName("title")]
        public string Title { get; }

        [JsonPropertyName("detail")]
        public string Detail { get; }

        [JsonPropertyName("status")]
        public int Status { get; }

        [JsonPropertyName("instance")]
        public string Instance { get; }

        [JsonPropertyName("validationErrors")]
        public ICollection<ValidationError> ValidationErrors => validationErrors;

        public ValidationProblemDetails(string type, string title, string detail, int status, string instance)
        {
            Type = type;
            Title = title;
            Detail = detail;
            Status = status;
            Instance = instance;
        }
    }
}
