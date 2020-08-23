using System;
using System.ComponentModel.DataAnnotations;
using CSE.Helium.Validation;

namespace CSE.Helium.Model
{
    public abstract class QueryParameters
    {
        [Range(minimum: 1, maximum: 10000, ErrorMessage = "Invalid PageNumber parameter")]
        public int PageNumber { get; set; } = 1;

        [Range(minimum: 1, maximum: 1000, ErrorMessage = "Invalid PageSize parameter")]
        public int PageSize { get; set; } = 100;

        [StringLength(maximumLength: 20, MinimumLength = 2, ErrorMessage = "Invalid q (search) parameter")]
        public string Q { get; set; }
    }

    public sealed class MovieQueryParameters : QueryParameters
    {
        // RegEx tested at https://regex101.com/r/GM1tjq/2
        [RegularExpression(@"^nm[0-9]{5,7}$", ErrorMessage = "Invalid Actor ID parameter")]
        public string ActorId { get; set; }

        [StringLength(maximumLength: 20, MinimumLength = 3, ErrorMessage = "Invalid Genre parameter")]
        public string Genre { get; set; }

        [YearValidation(errorMessage: "Invalid Year parameter")]
        public int Year { get; set; }
        
        [Range(minimum:0, maximum:10, ErrorMessage = "Invalid Rating parameter")]
        public double Rating { get; set; }
    }

    public sealed class ActorQueryParameters : QueryParameters
    {
        //[StringLength(maximumLength: 20, MinimumLength = 2)]
        //public string Q { get; set; }
    }

    public sealed class ActorIdParameter
    {
        // RegEx tested at https://regex101.com/r/GM1tjq/2
        [RegularExpression(@"^nm[0-9]{5,7}$", ErrorMessage = "Invalid Actor ID parameter")]
        public string ActorId { get; set; }
    }
}
