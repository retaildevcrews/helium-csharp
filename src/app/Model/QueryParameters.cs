using System.ComponentModel.DataAnnotations;
using CSE.Helium.Validation;

namespace CSE.Helium.Model
{
    public abstract class QueryParameters
    {
        [Range(minimum: 1, maximum: 10000, ErrorMessage = "Valid page numbers are between {1} and {2}")]
        public int PageNumber { get; set; } = 1;

        [Range(minimum: 1, maximum: 1000, ErrorMessage = "Valid page sizes are between {1} and {2}")]
        public int PageSize { get; set; } = 100;

        [StringLength(maximumLength: 20, MinimumLength = 2, ErrorMessage = "Query search parameter should be between {2} and {1} characters")]
        public string Q { get; set; }
    }

    public sealed class MovieQueryParameters : QueryParameters
    {
        [RegularExpression(@"^nm(?!0+$)([0-9]{7,11})$", ErrorMessage = "Actor ID starts with \"nm\" and should be between 7 and 11 characters")]
        public string ActorId { get; set; }

        [StringLength(maximumLength: 20, MinimumLength = 3, ErrorMessage = "Genre parameter should be between {2} and {1} characters")]
        public string Genre { get; set; }

        [YearValidation()]
        public int Year { get; set; }
        
        [Range(minimum:0, maximum:10, ErrorMessage = "Rating parameter should be between {1} and {2}")]
        public double Rating { get; set; }
    }

    public sealed class ActorQueryParameters : QueryParameters
    {
        // for future expansion
    }

    public sealed class MovieIdParameter
    {
        [RegularExpression(@"^tt(?!0+$)([0-9]{7,11})$", ErrorMessage = "Movie ID starts with \"tt\" and should be between 7 and 11 characters")]
        public string MovieId { get; set; }
    }

    public sealed class ActorIdParameter
    {
        [RegularExpression(@"^nm(?!0+$)([0-9]{7,11})$", ErrorMessage = "Actor ID starts with \"nm\" and should be between 7 and 11 characters")]
        public string ActorId { get; set; }
    }
}
