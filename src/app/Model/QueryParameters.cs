using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CSE.Helium.Validation;
using Helium.Validation;

namespace CSE.Helium.Model
{
    public abstract class QueryParameters
    {
        [JsonPropertyName(name:"pageNumber")]
        //[Range(minimum: 1, maximum: 10000, ErrorMessage = "The parameter should be between {1} and {2}.")]
        [IntegerRangeValidation(minValue:1, maxValue:1000)]
        public int PageNumber { get; set; } = 1;

        [JsonPropertyName(name:"pageSize")]
        //[Range(minimum: 1, maximum: 1000, ErrorMessage = "The parameter should be between {1} and {2}.")]
        [IntegerRangeValidation(minValue:1, maxValue:1000)]
        public int PageSize { get; set; } = 100;

        [JsonPropertyName(name:"q")]
        [StringLength(maximumLength: 20, MinimumLength = 2, ErrorMessage = "The search parameter should be between {2} and {1} characters.")]
        public string Q { get; set; }
    }

    public sealed class MovieQueryParameters : QueryParameters
    {
        [JsonPropertyName(name:"actorId")]
        [IdValidation(startingCharacters:"nm", minimumCharacters:7, maximumCharacters:11, true)]
        public string ActorId { get; set; }

        [JsonPropertyName(name: "movieId")]
        [IdValidation(startingCharacters: "tt", minimumCharacters: 7, maximumCharacters: 11, true)]
        public string MovieId { get; set; }

        [JsonPropertyName(name:"genre")]
        [StringLength(maximumLength: 20, MinimumLength = 3, ErrorMessage = "The parameter should be between {2} and {1} characters.")]
        public string Genre { get; set; }

        [JsonPropertyName(name:"year")]
        [YearValidation]
        public int Year { get; set; }
        
        [JsonPropertyName(name:"rating")]
        [Range(minimum:0, maximum:10.0, ErrorMessage = "The parameter should be between {1} and {2}.")]
        public double Rating { get; set; }
    }

    public sealed class ActorQueryParameters : QueryParameters
    {
        // for future expansion
    }

    public sealed class MovieIdParameter
    {
        //[RegularExpression(@"^tt(?!0+$)([0-9]{7,11})$", ErrorMessage = "Movie ID starts with 'tt' and should be between 7 and 11 characters")]
        [JsonPropertyName(name:"movieId")]
        [IdValidation(startingCharacters:"tt", minimumCharacters:7, maximumCharacters:11, false)]
        public string MovieId { get; set; }
    }

    public sealed class ActorIdParameter
    {
        [JsonPropertyName(name:"actorId")]
        [IdValidation(startingCharacters:"nm", minimumCharacters:7, maximumCharacters:11, false)]
        public string ActorId { get; set; }
    }
}
