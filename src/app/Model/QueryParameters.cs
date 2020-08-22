using System.ComponentModel.DataAnnotations;

namespace Helium.Model
{
    public abstract class QueryParameters
    {
        [Range(minimum: 1, maximum: 10000)]
        public int PageNumber { get; set; } = 1;

        [Range(minimum: 1, maximum: 1000)]
        public int PageSize { get; set; } = 100;
    }

    public sealed class MovieQueryParameters : QueryParameters
    {

    }

    public sealed class ActorQueryParameters : QueryParameters
    {
        [StringLength(maximumLength: 20, MinimumLength = 2)]
        public string Q { get; set; }
    }

    public sealed class ActorIdParameter
    {
        [Required]
        [RegularExpression(@"^.*nm(\d{5}|\d{7})$")]
        public string ActorId { get; set; }
    }
}
