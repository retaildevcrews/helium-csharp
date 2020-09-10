using CSE.Helium.Model;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CSE.Helium.Validation
{
    public class ValidationProblemDetails
    {
        private readonly List<ValidationError> validationErrors = new List<ValidationError>();

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("instance")]
        public string Instance { get; set; }

        [JsonPropertyName("validationErrors")]
        public ICollection<ValidationError> ValidationErrors => validationErrors;
    }
}
