using System.Text.Json.Serialization;

namespace CSE.Helium.Model
{
    public class ValidationError
    {
        [JsonPropertyName("code")]
        public string Code { get; }

        [JsonPropertyName("target")]
        public string Target { get; }

        [JsonPropertyName("message")]
        public string Message { get; }

        public ValidationError(string code, string target, string message)
        {
            Code = code;
            Target = target;
            Message = message;
        }
    }
}
